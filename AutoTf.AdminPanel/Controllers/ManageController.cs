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
    private readonly DockerController _docker;
    private readonly AuthManager _auth;
    private readonly CloudflareController _cloudflare;
    private readonly PleskController _plesk;
    private readonly DockerManager _dockerManager;

    public ManageController(DockerController docker, AuthManager auth, CloudflareController cloudflare, PleskController plesk, DockerManager dockerManager)
    {
        _docker = docker;
        _auth = auth;
        _cloudflare = cloudflare;
        _plesk = plesk;
        _dockerManager = dockerManager;
    }

    [HttpPost("create")]
    public async Task<ActionResult<bool>> Create([FromBody, Required] TotalCreationRequest request)
    {
        await _cloudflare.CreateRecord(request.DnsRecord);

        await _docker.CreateContainer(request.Container);
        
        if (request.Container.ContainerName == "")
            request.Container.ContainerName = request.Container.EvuName;

        ContainerListResponse? containerId = await _dockerManager.GetContainerByName(request.Container.ContainerName);

        if (containerId == null)
            return Problem("The created container could not be found.");

        if (!containerId.NetworkSettings.Networks.TryGetValue(request.Container.DefaultNetwork, out EndpointSettings? endpoint))
            return Problem("Something went wrong during the network creation.");

        CreateProxyRequest proxy = request.Proxy.ConvertToRequest(endpoint.IPAddress);

        TransactionalCreationResponse? proxyResult = await _auth.CreateProxy(proxy);
        
        if (proxyResult == null)
            return Problem($"Failed while creating the proxy.");
        else if (proxyResult.Applied == false)
            return Problem($"Failed while creating the proxy. Logs: {string.Join(Environment.NewLine, proxyResult.Logs)}");
        
        return _plesk.Create(request.Plesk);
    }
}