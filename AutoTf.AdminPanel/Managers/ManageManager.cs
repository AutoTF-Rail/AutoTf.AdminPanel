using System.Text;
using System.Text.Json;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Manage;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Managers;

public class ManageManager
{
    private readonly IAuthManager _auth;
    private readonly ICloudflareManager _cloudflare;
    private readonly PleskManager _plesk;
    private readonly IDockerManager _docker;

    public ManageManager(IDockerManager docker, IAuthManager auth, ICloudflareManager cloudflare, PleskManager plesk)
    {
        _auth = auth;
        _cloudflare = cloudflare;
        _plesk = plesk;
        _docker = docker;
    }

    public async Task<List<string>> AllWithHost(string authHost)
    {
        List<string> containers = await AllPlesk();
        List<string> sameHost = new List<string>();

        foreach (string domain in containers)
        {
            string? host = _plesk.GetAuthHost(domain);
            if (host == null)
                continue;
            
            if(host == authHost)
                sameHost.Add(domain);
        }

        return sameHost;
    }
    
    public async Task<List<ManageBody>> All()
    {
        List<ManageBody> managedContainers = new List<ManageBody>();
        
        List<ContainerListResponse> containers = await _docker.GetAll();
        List<string> pleskDomains = _plesk.Records;
        
        Result<ProviderPaginationResult> authResult = await _auth.GetProviders();

        if (!authResult.IsSuccess || authResult.Value == null)
            return new List<ManageBody>();
        
        List<Provider> providers = authResult.Value.Results;
        
        foreach (ContainerListResponse container in containers)
        {
            string name = container.Names.First().Replace("/autotf-", "");
            
            Result<ContainerInspectResponse> inspectedContainer = await _docker.InspectContainerById(container.ID);
            
            if (!inspectedContainer.IsSuccess || inspectedContainer.Value == null)
                continue;

            string? evuName = inspectedContainer.Value.Config.Env.FirstOrDefault(x => x.StartsWith("evuName="));
            
            if (evuName == null)
                continue;

            evuName = evuName.Replace("evuName=", "");
            
            string? pleskDomain = pleskDomains.FirstOrDefault(x => x.StartsWith(name));
            
            if (pleskDomain == null)
                continue;
            
            Provider? authProvider = providers.FirstOrDefault(x => x.Name.ToLower().Replace("managed provider for ", "").StartsWith(name));

            if (authProvider == null)
                continue;

            DnsRecord? cloudflareEntry = _cloudflare.Records.FirstOrDefault(x => x.Name.StartsWith(name));
            
            if (cloudflareEntry == null)
                continue;

            KeyValuePair<string,string>? domains = RegexHelper.ExtractDomains(pleskDomain);

            ManageBody body = new ManageBody()
            {
                RecordId = cloudflareEntry.Id,
                ContainerId = container.ID,
                ExternalHost = authProvider.ExternalHost,
                RootDomain = domains!.Value.Value,
                SubDomain = domains.Value.Key,
                EvuName = evuName
            };
            body.Id = EncodeManagedDomain(body);
            managedContainers.Add(body);
        }

        return managedContainers;
    }
    
    public string EncodeManagedDomain(ManageBody parts)
    {
        string combined = JsonSerializer.Serialize(parts);
        byte[] bytes = Encoding.UTF8.GetBytes(combined);
        return Convert.ToBase64String(bytes);
    }

    public ManageBody DecodeManagedDomain(string encoded)
    {
        byte[] bytes = Convert.FromBase64String(encoded);
        string decoded = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<ManageBody>(decoded)!;
    }
    
    public async Task<List<ContainerListResponse>> AllDocker()
    {
        List<ContainerListResponse> managedContainers = new List<ContainerListResponse>();
        
        List<ContainerListResponse> containers = await _docker.GetAll();
        List<string> pleskDomains = _plesk.Records;
        
        Result<ProviderPaginationResult> authResult = await _auth.GetProviders();

        if (!authResult.IsSuccess || authResult.Value == null)
            return new List<ContainerListResponse>();
        
        List<Provider> providers = authResult.Value.Results;
        
        
        foreach (ContainerListResponse container in containers)
        {
            string name = container.Names.First().Replace("/autotf-", "");
            if (!pleskDomains.Any(x => x.StartsWith(name)))
                continue;
            
            if (!providers.Any(x => x.Name.ToLower().Replace("managed provider for ", "").StartsWith(name)))
                continue;
            
            if (!_cloudflare.Records.Any(x => x.Name.StartsWith(name)))
                continue;
            
            managedContainers.Add(container);
        }

        return managedContainers;
    }
    
    public async Task<List<string>> AllPlesk()
    {
        List<string> managedContainers = new List<string>();
        
        List<ContainerListResponse> containers = await _docker.GetAll();
        List<string> pleskDomains = _plesk.Records;
        
        Result<ProviderPaginationResult> authResult = await _auth.GetProviders();

        if (!authResult.IsSuccess || authResult.Value == null)
            return new List<string>();
        
        List<Provider> providers = authResult.Value.Results;
        
        foreach (ContainerListResponse container in containers)
        {
            string name = container.Names.First().Replace("/autotf-", "");
            if (!pleskDomains.Any(x => x.StartsWith(name)))
                continue;
            
            if (!providers.Any(x => x.Name.ToLower().Replace("managed provider for ", "").StartsWith(name)))
                continue;
            
            if (!_cloudflare.Records.Any(x => x.Name.StartsWith(name)))
                continue;
            
            managedContainers.Add(pleskDomains.First(x => x.StartsWith(name)));
        }

        return managedContainers;
    }

    public async Task<string?> Create(TotalCreationRequest request)
    {
        
        // Pre checks
        if (_cloudflare.GetRecordByName(request.DnsRecord.Name, request.DnsRecord.Type) != null)
            return "A DNS entry with this name already exists.";
        
        if (request.Container.ContainerName == "")
            request.Container.ContainerName = request.Container.EvuName;

        if (!request.Container.ContainerName.ToLower().StartsWith("autotf-"))
            request.Container.ContainerName = "autotf-" + request.Plesk.SubDomain.ToLower();

        Result<ContainerListResponse> containerByNameResult = await _docker.GetContainerByName(request.Container.ContainerName);
        
        if (containerByNameResult.IsSuccess)
            return await AssembleProblem("A container with this name already exists.");
        
        // Cloudflare

        Result<object> entryCreation = await _cloudflare.CreateNewEntry(request.DnsRecord);
        if (!entryCreation.IsSuccess)
            return await AssembleProblem($"Failed to create DNS entry. {entryCreation.Error}");

        DnsRecord? record = _cloudflare.GetRecordByName(request.DnsRecord.Name, request.DnsRecord.Type);

        if (record == null)
            return "Failed to create DNS record.";

        // Docker
        await _docker.CreateContainer(request.Container);

        Result<ContainerListResponse> containerResult = await _docker.GetContainerByName(request.Container.ContainerName);

        if (!containerResult.IsSuccess || containerResult.Value == null)
            return await AssembleProblem($"The created container could not be found. {containerResult.Error}", record.Id);

        ContainerListResponse container = containerResult.Value;

        await _docker.StartContainer(container.ID);
        containerResult = await _docker.GetContainerByName(request.Container.ContainerName);
        
        if (!containerResult.IsSuccess)
            return await AssembleProblem("The created container could not be found after it was started.", record.Id);

        if (!container.NetworkSettings.Networks.TryGetValue(request.Container.DefaultNetwork, out EndpointSettings? endpoint))
            return await AssembleProblem("Something went wrong during the network creation.", record.Id, container.ID);

        // Authentik
        CreateProxyRequest proxy = request.Proxy.ConvertToRequest(endpoint.IPAddress, request.Plesk.SubDomain);

        Result<TransactionalCreationResponse> proxyResult = await _auth.CreateProxy(proxy);
        
        if (proxyResult.ResultCode != ResultCode.Success)
            return await AssembleProblem(proxyResult.Error, record.Id, container.ID);

        TransactionalCreationResponse proxyResultValue = proxyResult.Value!;

        if (proxyResultValue.Applied == false)
        {
            if(proxyResultValue.Logs != null && proxyResultValue.Logs.Any())
                return await AssembleProblem($"Something went wrong when creating the provider. Logs: {string.Join(Environment.NewLine, proxyResultValue.Logs)}", record.Id,
                    container.ID);
            return await AssembleProblem("Something went wrong when creating the provider. The operation was not successful.", record.Id,
                container.ID);
        }

        Result<string> providerId = await _auth.GetProviderIdByExternalHost(request.Proxy.ExternalHost);

        if (!providerId.IsSuccess || providerId.Value == null)
            return await AssembleProblem($"The created provider could not be found. {providerId.Error}", record.Id, container.ID);

        // idk how to validate this properly yet
        Result<string> assignResult = await _auth.AssignToOutpost(request.Proxy.OutpostId, providerId.Value);
        
        if (!assignResult.IsSuccess || assignResult.Value == null)
            return await AssembleProblem($"Failed while assigning the provider to the outpost. {assignResult.Error}", record.Id, container.ID, request.Proxy.ExternalHost);
        
        // Plesk
        if (!_plesk.CreateSubdomain(request.Plesk.SubDomain, request.Plesk.RootDomain, request.Plesk.Email, request.Plesk.AuthentikHost))
            return await AssembleProblem("Something went wrong when creating the subdomain in plesk.", record.Id, container.ID, request.Proxy.ExternalHost);

        return null;
    }

    public async Task<string> RevertChangesById(string error, string manageId)
    {
        ManageBody body = DecodeManagedDomain(manageId);

        return await RevertChanges(error, body.RecordId, body.ContainerId, body.ExternalHost, body.SubDomain, body.RootDomain);
    }

    public async Task<string> RevertChanges(string error, string? recordId = null, string? containerId = null, string? externalHost = null, string? subDomain = null, string? rootDomain = null)
    {
        bool entryDeletionSuccess = false;
        bool proxyDeletionSuccess = false;
        bool applicationDeletionSuccess = false;
        bool pleskDeletionSuccess = false;
        
        if (recordId != null)
            entryDeletionSuccess = (await _cloudflare.DeleteEntry(recordId)).IsSuccess;

        if (entryDeletionSuccess)
            error += " Deleted Dns Entry.";


        if (containerId != null)
        {
            bool containerKillSuccess = (await _docker.KillContainer(containerId)).IsSuccess;

            if (containerKillSuccess)
            {
                if((await _docker.DeleteContainer(containerId)).IsSuccess)
                    error += containerKillSuccess ? " Deleted container." : "";
            }
        }

        if (externalHost != null)
        {
            Result<string> providerId = await _auth.GetProviderIdByExternalHost(externalHost);
            Result<string> applicationSlug = await _auth.GetApplicationSlugByLaunchUrl(externalHost);

            // TODO: Maybe save the entire result instead so that we can add possible error messages
            if (providerId.IsSuccess && providerId.Value != null)
                proxyDeletionSuccess = (await _auth.DeleteProvider(providerId.Value)).IsSuccess;
            else
                error += $" Could not find provider by external host {externalHost}.";
            
            if(applicationSlug.IsSuccess && applicationSlug.Value != null)
                applicationDeletionSuccess = (await _auth.DeleteApplication(applicationSlug.Value)).IsSuccess;
            else
                error += $" Could not find application Slug by launch url {externalHost}.";
        }
        
        if (proxyDeletionSuccess)
            error += " Deleted proxy.";
        if (applicationDeletionSuccess)
            error += " Deleted Application.";

        
        if (subDomain != null && rootDomain != null)
            pleskDeletionSuccess = _plesk.DeleteSubDomain(rootDomain, subDomain);
        
        if (pleskDeletionSuccess)
            error += " Deleted plesk site.";

        return error;
    }

    private async Task<string> AssembleProblem(string error, string? recordId = null, string? containerId = null, string? externalHost = null)
    {
        return await RevertChanges(error, recordId, containerId, externalHost);
    }

    public async Task<float> GetTotalSizeGb()
    {
        List<ManageBody> managedContainers = await All();
        float final = 0;
        
        foreach (ManageBody container in managedContainers)
        {
            Result<float> result = await _docker.GetContainerSize(container.ContainerId!);
            
            if(!result.IsSuccess)
                continue;
            
            final += result.Value;
        }

        return MathF.Round((float)(final / (1024.0 * 1024.0 * 1024.0)), 2);
    }

    public async Task<ActionResult<int>> GetTotalTrainCount()
    {
        List<ManageBody> managedContainers = await All();
        int final = 0;
        
        foreach (ManageBody container in managedContainers)
        {
            Result<int> result = await _docker.GetTrainCount(container.ContainerId!);
            
            if(!result.IsSuccess)
                continue;
            final += result.Value;
        }

        return final;
    }

    public async Task<ActionResult<int>> GetAllowedTrainsCount()
    {
        List<ManageBody> managedContainers = await All();
        int final = 0;
        
        foreach (ManageBody container in managedContainers)
        {
            Result<int> result = await _docker.GetAllowedTrainsCount(container.ContainerId!);
            
            if(!result.IsSuccess)
                continue;
            
            final += result.Value;
        }

        return final;
    }
}