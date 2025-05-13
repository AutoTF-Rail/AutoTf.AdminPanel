using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/system")]
public class SystemController : ControllerBase
{
    private readonly IpWatcher _ipWatcher;
    private readonly Credentials _credentials;
    
    public SystemController(IOptions<Credentials> options, IpWatcher ipWatcher)
    {
        _ipWatcher = ipWatcher;
        _credentials = options.Value;
    }

    [HttpGet("Config")]
    public ActionResult<ServerConfig> Config()
    {
        return _credentials.DefaultConfig;
    }

    [HttpGet("AuthIp")]
    public ActionResult<string> AuthIp()
    {
        return _ipWatcher.GetAuthIp();
    }
}