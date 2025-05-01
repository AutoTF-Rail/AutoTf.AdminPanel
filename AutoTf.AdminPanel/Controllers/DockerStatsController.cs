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

    [HttpGet("memory")]
    public async Task<ActionResult<MemoryStats>> Memory()
    {
        return await _docker.Memory();
    }
}