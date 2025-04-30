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

    [HttpPost("create")]
    public async Task<ActionResult<bool>> Create([FromBody, Required] TotalCreationRequest request)
    {
        await _cloudflare.CreateNewEntry(request.DnsRecord);

        await _docker.CreateContainer(request.Container);
        
        if (request.Container.ContainerName == "")
            request.Container.ContainerName = request.Container.EvuName;

        ContainerListResponse? containerId = await _docker.GetContainerByName(request.Container.ContainerName);

        if (containerId == null)
            return Problem("The created container could not be found.");

        if (!containerId.NetworkSettings.Networks.TryGetValue(request.Container.DefaultNetwork, out EndpointSettings? endpoint))
            return Problem("Something went wrong during the network creation.");

        CreateProxyRequest proxy = request.Proxy.ConvertToRequest(endpoint.IPAddress);

        TransactionalCreationResponse? proxyResult = await _auth.CreateProxy(proxy);
        
        if (proxyResult == null)
            return Problem("Failed while creating the proxy.");

        if (proxyResult.Applied == false)
        {
            return Problem(proxyResult.Logs.Any() ? $"Failed while creating the proxy. Logs: {string.Join(Environment.NewLine, proxyResult.Logs)}" : "Failed while creating the proxy. No logs");
        }

        string? providerId = await _auth.GetProviderIdByExternalHost(request.Proxy.ExternalHost);

        if (providerId == null)
            return Problem("Failed to find the created provider.");

        // idk how to validate this properly yet
        string? assignResult = await _auth.AssignToOutpost(request.Proxy.OutpostId, providerId);
        
        if (assignResult == null)
            return Problem("Failed while assigning the provider to the outpost");

        return _plesk.CreateSubdomain(request.Plesk.SubDomain, request.Plesk.RootDomain, request.Plesk.Email, request.Plesk.AuthentikHost);
    }
}