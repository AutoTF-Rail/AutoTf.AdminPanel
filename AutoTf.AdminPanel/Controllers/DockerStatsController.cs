using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/docker/stats")]
public class DockerStatsController : ControllerBase
{
    private readonly DockerStatsManager _docker;

    public DockerStatsController(DockerStatsManager docker)
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
    public async Task<ActionResult<ContainerStats>> Stats(string containerId)
    {
        ContainerStats? stats = await _docker.Stats(containerId);

        if (stats == null)
            return NotFound("Could not find container.");

        return stats;
    }

    [HttpGet("{containerId}/memory")]
    public async Task<ActionResult<MemoryStats>> Memory(string containerId)
    {
        MemoryStats? stats = await _docker.Memory(containerId);

        if (stats == null)
            return NotFound("Could not find container.");

        return stats;
    }

    [HttpGet("{containerId}/network")]
    public async Task<ActionResult<NetworkStats>> Network(string containerId)
    {
        NetworkStats? stats = await _docker.Network(containerId);

        if (stats == null)
            return NotFound("Could not find container.");

        return stats;
    }

    [HttpGet("{containerId}/cpu")]
    public async Task<ActionResult<double>> Cpu(string containerId)
    {
        double? stats = await _docker.Cpu(containerId);

        if (stats == null)
            return NotFound("Could not find container.");

        return stats;
    }
    
    #endregion
}