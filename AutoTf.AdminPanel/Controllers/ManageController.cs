using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Manage;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/manage")]
public class ManageController : ControllerBase
{
    private readonly AuthManager _auth;
    private readonly CloudflareManager _cloudflare;
    private readonly PleskManager _plesk;
    private readonly DockerManager _docker;

    public ManageController(DockerManager docker, AuthManager auth, CloudflareManager cloudflare, PleskManager plesk)
    {
        _auth = auth;
        _cloudflare = cloudflare;
        _plesk = plesk;
        _docker = docker;
    }

    [HttpGet("stats/memory")]
    public async Task<ActionResult<MemoryStats>> StatsMemory()
    {
        List<ContainerListResponse> containers = await _docker.GetContainers();

        ConcurrentBag<MemoryStats> statsBag = new ConcurrentBag<MemoryStats>();
        
        await Parallel.ForEachAsync(containers, async (container, token) =>
        {
            statsBag.Add((await _docker.GetMemoryStats(container.ID))!);
        });

        float memoryUsageMb = 0.0f;
        float memoryPercentage = 0.0f;
        float memoryLimitMb = statsBag.Any() ? statsBag.First().MemoryLimitMb : 0.0f;
        
        foreach (MemoryStats stat in statsBag)
        {
            memoryUsageMb += stat.MemoryUsageMb;
            memoryPercentage += stat.MemoryPercentage;
        }

        return new MemoryStats()
        {
            MemoryPercentage = MathF.Round(memoryPercentage, 2),
            MemoryLimitMb = MathF.Round(memoryLimitMb, 2),
            MemoryUsageMb = MathF.Round(memoryUsageMb, 2)
        };
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody, Required] DeletionRequest request)
    {
        return Ok(await RevertChanges(string.Empty, request.RecordId, request.ContainerId, request.ExternalHost, request.SubDomain, request.RootDomain));
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody, Required] TotalCreationRequest request)
    {
        // Pre checks
        if (_cloudflare.GetRecordByName(request.DnsRecord.Name, request.DnsRecord.Type) != null)
            return Problem("A DNS entry with this name already exists.");
        
        if (request.Container.ContainerName == "")
            request.Container.ContainerName = request.Container.EvuName;
        
        if (await _docker.GetContainerByName(request.Container.ContainerName) != null)
            return await AssembleProblem("A container with this name already exists.");
        
        // Cloudflare
        if (!await _cloudflare.CreateNewEntry(request.DnsRecord))
            return await AssembleProblem("Failed to create DNS entry.");

        DnsRecord? record = _cloudflare.GetRecordByName(request.DnsRecord.Name, request.DnsRecord.Type);

        if (record == null)
            return Problem("Failed to create DNS record.");

        // Docker
        await _docker.CreateContainer(request.Container);

        ContainerListResponse? container = await _docker.GetContainerByName(request.Container.ContainerName);

        if (container == null)
            return await AssembleProblem("The created container could not be found.", record.Id);

        await _docker.StartContainer(container.ID);
        container = await _docker.GetContainerByName(request.Container.ContainerName);
        
        if (container == null)
            return await AssembleProblem("The created container could not be found after it was started.", record.Id);

        if (!container.NetworkSettings.Networks.TryGetValue(request.Container.DefaultNetwork, out EndpointSettings? endpoint))
            return await AssembleProblem("Something went wrong during the network creation.", record.Id, container.ID);

        // Authentik
        CreateProxyRequest proxy = request.Proxy.ConvertToRequest(endpoint.IPAddress);

        TransactionalCreationResponse? proxyResult = await _auth.CreateProxy(proxy);
        
        if (proxyResult == null)
            return await AssembleProblem("Something went wrong when creating the provider.", record.Id, container.ID);

        if (proxyResult.Applied == false)
            return await AssembleProblem("Something went wrong when creating the provider. The operation was not successful.", record.Id, container.ID);

        string? providerId = await _auth.GetProviderIdByExternalHost(request.Proxy.ExternalHost);

        if (providerId == null)
            return await AssembleProblem("The created provider could not be found.", record.Id, container.ID);

        // idk how to validate this properly yet
        string? assignResult = await _auth.AssignToOutpost(request.Proxy.OutpostId, providerId);
        
        if (assignResult == null)
            return await AssembleProblem("Failed while assigning the provider to the outpost.", record.Id, container.ID, request.Proxy.ExternalHost);
        
        // Plesk
        if (!_plesk.CreateSubdomain(request.Plesk.SubDomain, request.Plesk.RootDomain, request.Plesk.Email, request.Plesk.AuthentikHost))
            return await AssembleProblem("Something went wrong when creating the subdomain in plesk.", record.Id, container.ID, request.Proxy.ExternalHost);

        return Ok();
    }

    private async Task<string> RevertChanges(string error, string? recordId = null, string? containerId = null, string? externalHost = null, string? subDomain = null, string? rootDomain = null)
    {
        bool entryDeletionSuccess = false;
        bool containerKillSuccess = false;
        bool proxyDeletionSuccess = false;
        bool pleskDeletionSuccess = false;
        
        if (recordId != null)
            entryDeletionSuccess = await _cloudflare.DeleteEntry(recordId);

        if (entryDeletionSuccess)
            error += entryDeletionSuccess ? " Deleted Dns Entry." : "";


        if (containerId != null)
        {
            containerKillSuccess = await _docker.KillContainer(containerId);

            if (containerKillSuccess)
            {
                await _docker.DeleteContainer(containerId);
                error += containerKillSuccess ? " Deleted container." : "";
            }
        }

        if (externalHost != null)
        {
            string? providerId = await _auth.GetProviderIdByExternalHost(externalHost);
            
            if (providerId != null)
                proxyDeletionSuccess = await _auth.DeleteProvider(providerId);
        }
        
        if (proxyDeletionSuccess)
            error += proxyDeletionSuccess ? " Deleted proxy." : "";

        
        if (subDomain != null && rootDomain != null)
            pleskDeletionSuccess = _plesk.DeleteSubDomain(rootDomain, subDomain);
        
        if (pleskDeletionSuccess)
            error += pleskDeletionSuccess ? " Deleted plesk site." : "";

        return error;
    }

    private async Task<ActionResult> AssembleProblem(string error, string? recordId = null, string? containerId = null, string? externalHost = null)
    {
        return Problem(await RevertChanges(error, recordId, containerId, externalHost));
    }
}