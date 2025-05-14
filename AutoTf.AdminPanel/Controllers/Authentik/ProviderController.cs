using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers.Authentik;

[ApiController]
[Route("/api/authentik/provider")]
public class ProviderController : ControllerBase
{
    private readonly IAuthManager _auth;

    public ProviderController(IAuthManager auth)
    {
        _auth = auth;
    }

    [HttpGet("{id}")]
    public async Task<Result<Provider>> GetProvider(string id)
    {
        return await _auth.GetProvider(id);
    }

    [HttpDelete("{id}")]
    public async Task<Result<string>> DeleteProvider(string id)
    {
        return await _auth.DeleteProvider(id);
    }

    [HttpGet("id")]
    public async Task<Result<string>> ProviderByExternalHost([FromBody, Required] string externalHost)
    {
        return await _auth.GetProviderIdByExternalHost(externalHost);
    }

    [HttpGet("all")]
    public async Task<Result<ProviderPaginationResult>> All()
    {
        return await _auth.GetProviders();
    }
}