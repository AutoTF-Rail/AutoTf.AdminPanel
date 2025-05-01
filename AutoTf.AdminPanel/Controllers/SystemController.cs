using AutoTf.AdminPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/system")]
public class SystemController : ControllerBase
{
    private readonly Credentials _credentials;
    
    public SystemController(IOptions<Credentials> options)
    {
        _credentials = options.Value;
    }

    [HttpGet("Config")]
    public ActionResult<ServerConfig> Config()
    {
        return _credentials.DefaultConfig;
    }
}