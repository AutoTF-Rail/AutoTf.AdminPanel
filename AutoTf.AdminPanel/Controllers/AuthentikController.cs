using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using AutoTf.AdminPanel.Statics;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/authentik")]
public class AuthentikController : ControllerBase
{
    private readonly AuthManager _auth;

    public AuthentikController(AuthManager auth)
    {
        _auth = auth;
    }

    [HttpPost("create")]
    public async Task<ActionResult<string>> Create([FromBody, Required] CreateProxyRequest request)
    {
        string? result = await _auth.CreateProxy(request);
        
        if (result == null)
            return Problem(result);

        return result;
    }

    [HttpGet("outposts")]
    public async Task<ActionResult<string>> Outposts()
    {
        string? result = await _auth.GetOutposts();
        
        if (result == null)
            return Problem(result);

        return result;
    }

    [HttpGet("outpost/{id}")]
    public async Task<ActionResult<string>> Outpost(string id)
    {
        string? result = await _auth.GetOutpost(id);
        
        if (result == null)
            return Problem(result);

        return result;
    }

    [HttpPut("outpost/{id}")]
    public async Task<ActionResult<string>> UpdateOutpost(string id, [FromBody, Required] OutpostModel config)
    {
        string? result = await _auth.UpdateOutpost(id, config);
        
        if (result == null)
            return Problem(result);

        return result;
    }

    [HttpGet("providers")]
    public async Task<ActionResult<string>> Providers()
    {
        string? result = await _auth.GetProviders();
        
        if (result == null)
            return Problem(result);

        return result;
    }
}