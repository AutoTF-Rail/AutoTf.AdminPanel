using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/manage")]
public class ManageController : ControllerBase
{
    private readonly DockerManager _docker;

    public ManageController(DockerManager docker)
    {
        _docker = docker;
    }
    
    [HttpGet("getAllContainers")]
    public async Task<ActionResult<List<ContainerListResponse>>> GetAllContainers()
    {
        return await _docker.GetContainers();
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateContainerResponse>> CreateContainer([FromBody] CreateContainer parameters)
    {
        // TODO: Get default values if any are empty
        Dictionary<string,EndpointSettings> endpoints = DockerHelper.AssembleEndpoints(parameters);

        if (string.IsNullOrEmpty(parameters.ContainerName))
            parameters.ContainerName = parameters.EvuName;

        return await _docker.CreateContainer(parameters, endpoints);
    }
}