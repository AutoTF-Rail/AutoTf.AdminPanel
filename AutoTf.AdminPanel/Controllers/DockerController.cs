using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;

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
        return await _docker.GetContainers();
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
        await _docker.DeleteContainer(id);
        return Ok();
    }

    [HttpGet("getByName")]
    public async Task<ActionResult<ContainerListResponse>> GetByName([FromBody, Required] string name)
    {
        ContainerListResponse? containerListResponse = await _docker.GetContainerByName(name);
        
        if (containerListResponse == null)
            return Problem("Could not find container.");
        
        return containerListResponse;
    }

    [HttpGet("stats/{id}")]
    public async Task<ActionResult<ContainerStatsResponse>> Stats(string id)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        return response;
    }

    [HttpGet("stats/{id}/memory")]
    public async Task<ActionResult<MemoryStats>> MemoryStats(string id)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        double memoryUsageBytes = response.MemoryStats.Usage;
        double memoryLimitBytes = response.MemoryStats.Limit;
        
        double memoryUsageMb = memoryUsageBytes / (1024 * 1024);
        double memoryLimitMb = memoryLimitBytes / (1024 * 1024);
        double memoryPercentage = (memoryUsageBytes / memoryLimitBytes) * 100;

        MemoryStats stats = new MemoryStats()
        {
            MemoryUsageMb = memoryUsageMb,
            MemoryLimitMb = memoryLimitMb,
            MemoryPercentage = memoryPercentage
        };
        
        return stats;
    }
}