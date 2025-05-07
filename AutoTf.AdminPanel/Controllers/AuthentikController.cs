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
    public async Task<ActionResult<TransactionalCreationResponse>> Create([FromBody, Required] CreateProxyRequest request)
    {
        TransactionalCreationResponse? result = await _auth.CreateProxy(request);
        
        if (result == null)
            return Problem("Something went wrong when creating the proxy.");

        return result;
    }

    [HttpPost("outpost/{id}/assign")]
    public async Task<ActionResult<string>> AssignToOutpost(string id, [FromBody, Required] string providerPk)
    {
        string? result = await _auth.AssignToOutpost(id, providerPk);
        
        if (result == null)
            return Problem("Could not update outpost.");

        return result;
    }

    [HttpPost("outpost/{id}/unassign")]
    public async Task<ActionResult<string>> UnassignFromOutpost(string id, [FromBody, Required] string providerPk)
    {
        string? result = await _auth.UnassignFromOutpost(id, providerPk);
        
        if (result == null)
            return Problem("Could not update outpost.");

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
    public async Task<ActionResult<OutpostModel>> Outpost(string id)
    {
        OutpostModel? result = await _auth.GetOutpost(id);
        
        if (result == null)
            return Problem("Could not find outpost.");

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
    public async Task<ActionResult<ProviderPaginationResult>> Providers()
    {
        ProviderPaginationResult? result = await _auth.GetProviders();
        
        if (result == null)
            return Problem("Failed to send the request to authentik.");

        return result;
    }

    [HttpGet("providerId")]
    public async Task<ActionResult<string>> ProviderByExternalHost([FromBody, Required] string externalHost)
    {
        string? result = await _auth.GetProviderIdByExternalHost(externalHost);
        
        if (result == null)
            return Problem("Failed to find the provider.");

        return result;
    }

    [HttpDelete("provider/{id}")]
    public async Task<ActionResult<bool>> DeleteProvider(string id)
    {
        // TODO: Check for existance
        return await _auth.DeleteProvider(id);
    }

    [HttpDelete("application/{id}")]
    public async Task<ActionResult<bool>> DeleteApplication(string id)
    {
        // TODO: Check for existance
        return await _auth.DeleteApplication(id);
    }

    [HttpGet("flows/authorization")]
    public async Task<ActionResult<List<Flow>>> AuthorizationFlows()
    {
        return await _auth.GetAuthorizationFlows();
    }

    [HttpGet("flows/invalidation")]
    public async Task<ActionResult<List<Flow>>> InvalidationFlows()
    {
        return await _auth.GetInvalidationFlows();
    }

    [HttpGet("groups")]
    public async Task<ActionResult<List<Group>>> Groups()
    {
        return await _auth.GetGroups();
    }
}