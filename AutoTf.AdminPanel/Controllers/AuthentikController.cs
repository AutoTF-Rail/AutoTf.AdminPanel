using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

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

    [HttpPost("outpost/{id}/assign")]
    public async Task<Result<string>> AssignToOutpost(string id, [FromBody, Required] string providerPk)
    {
       return await _auth.AssignToOutpost(id, providerPk);
    }

    [HttpPost("outpost/{id}/unassign")]
    public async Task<Result<string>> UnassignFromOutpost(string id, [FromBody, Required] string providerPk)
    {
        return await _auth.UnassignFromOutpost(id, providerPk);
    }

    [HttpGet("outposts")]
    public async Task<Result<string>> Outposts()
    {
        return await _auth.GetOutposts();
    }

    [HttpGet("outpost/{id}")]
    public async Task<Result<OutpostModel>> Outpost(string id)
    {
        return await _auth.GetOutpost(id);
    }

    [HttpPut("outpost/{id}")]
    public async Task<Result<string>> UpdateOutpost(string id, [FromBody, Required] OutpostModel config)
    {
        return await _auth.UpdateOutpost(id, config);
    }

    [HttpGet("providers")]
    public async Task<Result<ProviderPaginationResult>> Providers()
    {
        return await _auth.GetProviders();
    }

    [HttpGet("applications")]
    public async Task<Result<ApplicationPaginationResult>> Applications()
    {
        return await _auth.GetApplications();
    }

    [HttpGet("providerId")]
    public async Task<Result<string>> ProviderByExternalHost([FromBody, Required] string externalHost)
    {
        return await _auth.GetProviderIdByExternalHost(externalHost);
    }

    [HttpGet("provider/{pk}")]
    public async Task<Result<Provider>> GetProvider(string pk)
    {
        return await _auth.GetProvider(pk);
    }

    [HttpDelete("provider/{id}")]
    public async Task<Result<string>> DeleteProvider(string id)
    {
        return await _auth.DeleteProvider(id);
    }

    [HttpDelete("application/{slug}")]
    public async Task<Result<string>> DeleteApplication(string slug)
    {
        // TODO: Check for existance
        return await _auth.DeleteApplication(slug);
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