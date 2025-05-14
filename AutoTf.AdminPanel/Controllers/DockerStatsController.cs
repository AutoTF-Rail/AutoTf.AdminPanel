using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/docker/stats")]
public class DockerStatsController : ControllerBase
{
    private readonly IDockerStatsManager _docker;

    public DockerStatsController(IDockerStatsManager docker)
    {
        _docker = docker;
    }

    [HttpGet]
    public async Task<ActionResult<ContainerStats>> Stats()
    {
        return await _docker.Stats();
    }

    [HttpGet("memory")]
    public async Task<ActionResult<MemoryStats>> Memory()
    {
        return await _docker.Memory();
    }

    [HttpGet("cpu")]
    public async Task<ActionResult<double>> Cpu()
    {
        return await _docker.Cpu();
    }

    [HttpGet("network")]
    public async Task<ActionResult<NetworkStats>> Network()
    {
        return await _docker.Network();
    }
    
    #region Container

    [HttpGet("{containerId}")]
    public Result<ContainerStats> Stats(string containerId)
    {
        return _docker.Stats(containerId);
    }

    [HttpGet("{containerId}/memory")]
    public Result<MemoryStats> Memory(string containerId)
    {
        return _docker.Memory(containerId);
    }

    [HttpGet("{containerId}/network")]
    public Result<NetworkStats> Network(string containerId)
    {
        return _docker.Network(containerId);
    }

    [HttpGet("{containerId}/cpu")]
    public Result<double> Cpu(string containerId)
    {
        return _docker.Cpu(containerId);
    }
    
    #endregion
}