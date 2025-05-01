using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Manage;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

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

    private async Task<ActionResult> RevertChanges(string error, string? recordId = null, string? containerId = null, string? externalHost = null)
    {
        bool entryDeletionSuccess = false;
        bool containerKillSuccess = false;
        bool proxyDeletionSuccess = false;
        
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
            error += entryDeletionSuccess ? " Deleted proxy." : "";

        return Problem(error);
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
            return await RevertChanges("A container with this name already exists.");
        
        // Cloudflare
        if (!await _cloudflare.CreateNewEntry(request.DnsRecord))
            return await RevertChanges("Failed to create DNS entry.");

        DnsRecord? record = _cloudflare.GetRecordByName(request.DnsRecord.Name, request.DnsRecord.Type);

        if (record == null)
            return Problem("Failed to create DNS record.");

        // Docker
        await _docker.CreateContainer(request.Container);

        ContainerListResponse? container = await _docker.GetContainerByName(request.Container.ContainerName);

        if (container == null)
            return await RevertChanges("The created container could not be found.", record.Id);

        await _docker.StartContainer(container.ID);
        container = await _docker.GetContainerByName(request.Container.ContainerName);
        
        if (container == null)
            return await RevertChanges("The created container could not be found after it was started.", record.Id);

        if (!container.NetworkSettings.Networks.TryGetValue(request.Container.DefaultNetwork, out EndpointSettings? endpoint))
            return await RevertChanges("Something went wrong during the network creation.", record.Id, container.ID);

        // Authentik
        CreateProxyRequest proxy = request.Proxy.ConvertToRequest(endpoint.IPAddress);

        TransactionalCreationResponse? proxyResult = await _auth.CreateProxy(proxy);
        
        if (proxyResult == null)
            return await RevertChanges("Something went wrong when creating the provider.", record.Id, container.ID);

        if (proxyResult.Applied == false)
            return await RevertChanges("Something went wrong when creating the provider. The operation was not successful.", record.Id, container.ID);

        string? providerId = await _auth.GetProviderIdByExternalHost(request.Proxy.ExternalHost);

        if (providerId == null)
            return await RevertChanges("The created provider could not be found.", record.Id, container.ID);

        // idk how to validate this properly yet
        string? assignResult = await _auth.AssignToOutpost(request.Proxy.OutpostId, providerId);
        
        if (assignResult == null)
            return await RevertChanges("Failed while assigning the provider to the outpost.", record.Id, container.ID, request.Proxy.ExternalHost);
        
        // Plesk
        if (!_plesk.CreateSubdomain(request.Plesk.SubDomain, request.Plesk.RootDomain, request.Plesk.Email, request.Plesk.AuthentikHost))
            return await RevertChanges("Something went wrong when creating the subdomain in plesk.", record.Id, container.ID, request.Proxy.ExternalHost);

        return Ok();
    }
}