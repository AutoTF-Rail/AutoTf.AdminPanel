using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/docker")]
public class DockerController : ControllerBase
{
    private readonly DockerManager _docker;

    public DockerController(DockerManager docker)
    {
        _docker = docker;
    }
    
    [HttpGet("getAllContainers")]
    public async Task<ActionResult<List<ContainerListResponse>>> GetAllContainers()
    {
        return await _docker.GetAll();
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateContainerResponse>> CreateContainer([FromBody] CreateContainer parameters)
    {
        return await _docker.CreateContainer(parameters);
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

    [HttpPost("kill")]
    public async Task<ActionResult<bool>> KillContainer([FromBody, Required] string id)
    {
        return await _docker.KillContainer(id);
    }

    [HttpPost("exists")]
    public async Task<ActionResult<bool>> Exists([FromBody, Required] string id)
    {
        return await _docker.ContainerExists(id);
    }

    [HttpPost("running")]
    public async Task<ActionResult<bool>> Running([FromBody, Required] string id)
    {
        return await _docker.ContainerRunning(id);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteContainer([FromBody, Required] string id)
    {
        if (await _docker.DeleteContainer(id))
            return Ok();

        return Problem("Could not delete container because it was either not found or is still running.");
    }

    [HttpGet("getByName")]
    public async Task<ActionResult<ContainerListResponse>> GetByName([FromBody, Required] string name)
    {
        ContainerListResponse? containerListResponse = await _docker.GetContainerByName(name);
        
        if (containerListResponse == null)
            return Problem("Could not find container.");
        
        return containerListResponse;
    }

    [HttpGet("getById/{id}")]
    public async Task<ActionResult<ContainerListResponse>> GetById(string id)
    {
        ContainerListResponse? containerListResponse = await _docker.GetContainerById(id);
        
        if (containerListResponse == null)
            return Problem("Could not find container.");
        
        return containerListResponse;
    }

    [HttpGet("{id}/inspect")]
    public async Task<ActionResult<ContainerInspectResponse>> Inspect(string id)
    {
        ContainerInspectResponse? response = await _docker.InspectContainerById(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        return response;
    }

    [HttpGet("{id}/size")]
    public async Task<ActionResult<float>> GetSize(string id)
    {
        return MathF.Round((float)(await _docker.GetContainerSize(id) / (1024.0 * 1024.0 * 1024.0)), 2);
    }

    [HttpGet("{id}/trainCount")]
    public async Task<ActionResult<int>> TrainCount(string id)
    {
        return await _docker.GetTrainCount(id);
    }

    [HttpGet("{id}/allowedTrainsCount")]
    public async Task<ActionResult<int>> AllowedTrainsCount(string id)
    {
        return await _docker.GetAllowedTrainsCount(id);
    }
    
    [HttpPost("{id}/updateAllowedTrains")]
    public async Task<ActionResult> UpdateAllowedTrains(string id, [FromBody, Required] int allowedTrains)
    {
        return await _docker.UpdateAllowedTrains(id, allowedTrains);
    }

    [HttpGet("networks")]
    public async Task<ActionResult<List<string>>> GetAllNetworks()
    {
        return await _docker.GetNetworks();
    }
}