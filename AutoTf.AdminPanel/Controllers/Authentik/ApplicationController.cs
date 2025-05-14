using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers.Authentik;
    
[ApiController]
[Route("/api/authentik/application")]
public class ApplicationController : ControllerBase
{
    private readonly IAuthManager _auth;

    public ApplicationController(IAuthManager auth)
    {
        _auth = auth;
    }

    [HttpDelete("{slug}")]
    public async Task<Result<string>> DeleteApplication(string slug)
    {
        // TODO: Check for existance
        return await _auth.DeleteApplication(slug);
    }

    [HttpGet("all")]
    public async Task<Result<ApplicationPaginationResult>> All()
    {
        return await _auth.GetApplications();
    }
}