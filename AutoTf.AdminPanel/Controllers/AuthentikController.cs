using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
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
}