using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/docker")]
public class DockerController : ControllerBase
{
    private readonly IDockerManager _docker;

    public DockerController(IDockerManager docker)
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

    [HttpPost("{id}/start")]
    public async Task<Result> StartContainer(string id)
    {
        return await _docker.StartContainer(id);
    }

    [HttpPost("{id}/stop")]
    public async Task<Result> StopContainer(string id)
    {
        return await _docker.StopContainer(id);
    }

    [HttpPost("{id}/kill")]
    public async Task<Result> KillContainer(string id)
    {
        return await _docker.KillContainer(id);
    }

    [HttpPost("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(string id)
    {
        return await _docker.ContainerExists(id);
    }

    [HttpPost("{id}/running")]
    public async Task<ActionResult<bool>> Running(string id)
    {
        return await _docker.ContainerRunning(id);
    }

    [HttpPost("{id}/delete")]
    public async Task<Result> DeleteContainer(string id)
    {
        return await _docker.DeleteContainer(id);
    }

    [HttpGet("name/{name}")]
    public async Task<Result<ContainerListResponse>> GetByName(string name)
    {
        return await _docker.GetContainerByName(name);
    }

    [HttpGet("{id}")]
    public async Task<Result<ContainerListResponse>> GetContainer(string id)
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
    public async Task<Result> UpdateAllowedTrains(string id, [FromBody, Required] int allowedTrains)
    {
        return await _docker.UpdateAllowedTrains(id, allowedTrains);
    }

    [HttpGet("networks")]
    public async Task<ActionResult<List<string>>> GetAllNetworks()
    {
        return await _docker.GetNetworks();
    }
}