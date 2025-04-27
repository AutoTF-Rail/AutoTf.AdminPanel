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
    
    public async Task<ActionResult<List<ContainerListResponse>>> GetAllContainers()
    {
        return await _docker.GetContainers();
    }
}