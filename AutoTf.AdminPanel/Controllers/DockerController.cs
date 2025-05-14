using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
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
    public async Task<Result<CreateContainerResponse>> CreateContainer([FromBody] CreateContainer parameters)
    {
        return await _docker.CreateContainer(parameters);
    }

    [HttpPost("start")]
    public async Task<Result<object>> StartContainer([FromBody, Required] string id)
    {
        return await _docker.StartContainer(id);
    }

    [HttpPost("stop")]
    public async Task<Result<object>> StopContainer([FromBody, Required] string id)
    {
        return await _docker.StopContainer(id);
    }

    [HttpPost("kill")]
    public async Task<Result<object>> KillContainer([FromBody, Required] string id)
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
    public async Task<Result<object>> DeleteContainer([FromBody, Required] string id)
    {
        return await _docker.DeleteContainer(id);
    }

    [HttpGet("getByName")]
    public async Task<Result<ContainerListResponse>> GetByName([FromBody, Required] string name)
    {
        return await _docker.GetContainerByName(name);
    }

    [HttpGet("getById/{id}")]
    public async Task<Result<ContainerListResponse>> GetById(string id)
    {
        return await _docker.GetContainerById(id);
    }

    [HttpGet("{id}/inspect")]
    public async Task<Result<ContainerInspectResponse>> Inspect(string id)
    {
        return await _docker.InspectContainerById(id);
    }

    [HttpGet("{id}/size")]
    public async Task<Result<float>> GetSize(string id)
    {
        return await _docker.GetContainerSizeGb(id);
    }

    [HttpGet("{id}/trainCount")]
    public async Task<Result<int>> TrainCount(string id)
    {
        return await _docker.GetTrainCount(id);
    }

    [HttpGet("{id}/allowedTrainsCount")]
    public async Task<Result<int>> AllowedTrainsCount(string id)
    {
        return await _docker.GetAllowedTrainsCount(id);
    }
    
    [HttpPost("{id}/updateAllowedTrains")]
    public async Task<Result<object>> UpdateAllowedTrains(string id, [FromBody, Required] int allowedTrains)
    {
        return await _docker.UpdateAllowedTrains(id, allowedTrains);
    }

    [HttpGet("networks")]
    public async Task<ActionResult<List<string>>> GetAllNetworks()
    {
        return await _docker.GetNetworks();
    }
}