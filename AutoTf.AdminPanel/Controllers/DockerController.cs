using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;
using NetworkStats = AutoTf.AdminPanel.Models.Requests.NetworkStats;

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
        MemoryStats? response = await _docker.GetMemoryStats(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        return response;
    }

    [HttpGet("stats/{id}/cpu")]
    public async Task<ActionResult<double>> CpuStats(string id)
    {
        double? response = await _docker.GetCpuUsage(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        return response;
    }

    [HttpGet("stats/{id}/network")]
    public async Task<ActionResult<NetworkStats>> NetworkStats(string id)
    {
        NetworkStats? response = await _docker.GetNetworkStats(id);
        
        if (response == null)
            return Problem("Could not find container.");
        
        return response;
    }

    [HttpGet("stats/memory")]
    public async Task<ActionResult<MemoryStats>> StatsMemory()
    {
        List<ContainerListResponse> containers = await _docker.GetContainers();

        ConcurrentBag<MemoryStats> statsBag = new ConcurrentBag<MemoryStats>();
        
        await Parallel.ForEachAsync(containers, async (container, token) =>
        {
            statsBag.Add((await _docker.GetMemoryStats(container.ID))!);
        });

        float memoryUsageMb = 0.0f;
        float memoryPercentage = 0.0f;
        float memoryLimitMb = statsBag.Any() ? statsBag.First().MemoryLimitMb : 0.0f;
        
        foreach (MemoryStats stat in statsBag)
        {
            memoryUsageMb += Safe(stat.MemoryUsageMb);
            memoryPercentage += Safe(stat.MemoryPercentage);
        }

        return new MemoryStats()
        {
            MemoryPercentage = MathF.Round(Safe(memoryPercentage), 2),
            MemoryLimitMb = MathF.Round(Safe(memoryLimitMb), 2),
            MemoryUsageMb = MathF.Round(Safe(memoryUsageMb), 2)
        };

        float Safe(float value) =>
            float.IsNaN(value) || float.IsInfinity(value) ? 0.0f : value;
    }

    [HttpGet("stats/cpu")]
    public async Task<ActionResult<double>> StatsCpu()
    {
        List<ContainerListResponse> containers = await _docker.GetContainers();

        double stats = 0.0f;
        
        await Parallel.ForEachAsync(containers, async (container, token) =>
        {
            double? cpuUsage = await _docker.GetCpuUsage(container.ID);
            if(cpuUsage != null)
                stats += (double)cpuUsage;
        });

        return stats;
    }

    [HttpGet("stats/network")]
    public async Task<ActionResult<NetworkStats>> StatsNetwork()
    {
        List<ContainerListResponse> containers = await _docker.GetContainers();

        ConcurrentBag<NetworkStats> statsBag = new ConcurrentBag<NetworkStats>();

        await Parallel.ForEachAsync(containers,
            async (container, token) => { statsBag.Add((await _docker.GetNetworkStats(container.ID))!); });

        float totalReceived = 0.0f;
        float totalSend = 0.0f;

        foreach (NetworkStats stat in statsBag)
        {
            totalReceived += stat.TotalReceived;
            totalSend += stat.TotalSend;
        }

        return new NetworkStats()
        {
            TotalReceived = MathF.Round(totalReceived, 2),
            TotalSend = MathF.Round(totalSend, 2)
        };
    }
}