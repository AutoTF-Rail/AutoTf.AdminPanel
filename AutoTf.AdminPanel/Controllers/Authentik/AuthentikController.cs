using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers.Authentik;

[ApiController]
[Route("/api/authentik")]
public class AuthentikController : ControllerBase
{
    private readonly IAuthManager _auth;

    public AuthentikController(IAuthManager auth)
    {
        _auth = auth;
    }

    [HttpPost("create")]
    public async Task<Result<TransactionalCreationResponse>> Create([FromBody, Required] CreateProxyRequest request)
    {
        return await _auth.CreateProxy(request);
    }

    [HttpGet("flows/authorization")]
    public async Task<Result<List<Flow>>> AuthorizationFlows()
    {
        return await _auth.GetAuthorizationFlows();
    }

    [HttpGet("flows/invalidation")]
    public async Task<Result<List<Flow>>> InvalidationFlows()
    {
        return await _auth.GetInvalidationFlows();
    }

    [HttpGet("groups")]
    public async Task<Result<List<Group>>> Groups()
    {
        return await _auth.GetGroups();
    }
}