using System.ComponentModel.DataAnnotations;
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
        try
        {
            // TODO: Get default values if any are empty
            Dictionary<string,EndpointSettings> networks = await DockerHelper.ConfigureNetwork(parameters, _docker);

            if (string.IsNullOrEmpty(parameters.ContainerName))
                parameters.ContainerName = parameters.EvuName;

            return await _docker.CreateContainer(parameters, networks);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("start")]
    public async Task<ActionResult<bool>> StartContainer([FromBody, Required] string id)
    {
        return await _docker.StartContainer(id);
    }

    [HttpPost("stop")]
    public async Task<ActionResult<bool>> StopContainer([FromBody, Required] string id)
    {
        return await _docker.StopContainer(id);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteContainer([FromBody, Required] string id)
    {
        await _docker.DeleteContainer(id);
        return Ok();
    }
}