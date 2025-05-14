using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers.Authentik;

[ApiController]
[Route("/api/authentik/outpost")]
public class OutpostController : ControllerBase
{
    private readonly IAuthManager _auth;

    public OutpostController(IAuthManager auth)
    {
        _auth = auth;
    }
    // TODO: Delete delete endpoint?

    [HttpGet("{id}")]
    public async Task<Result<OutpostModel>> Outpost(string id)
    {
        return await _auth.GetOutpost(id);
    }
    
    [HttpPut("{id}")]
    public async Task<Result<string>> UpdateOutpost(string id, [FromBody, Required] OutpostModel config)
    {
        return await _auth.UpdateOutpost(id, config);
    }
    
    [HttpPost("{id}/assign")]
    public async Task<Result<string>> AssignToOutpost(string id, [FromBody, Required] string providerPk)
    {
        return await _auth.AssignToOutpost(id, providerPk);
    }

    [HttpPost("{id}/unassign")]
    public async Task<Result<string>> UnassignFromOutpost(string id, [FromBody, Required] string providerPk)
    {
        return await _auth.UnassignFromOutpost(id, providerPk);
    }

    [HttpGet("all")]
    public async Task<Result<string>> All()
    {
        return await _auth.GetOutposts();
    }
}