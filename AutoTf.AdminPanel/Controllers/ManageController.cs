using AutoTf.AdminPanel.Managers;
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
}