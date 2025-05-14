using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("api/plesk")]
public class PleskController : ControllerBase
{
    private readonly IPleskManager _plesk;

    public PleskController(IPleskManager plesk)
    {
        _plesk = plesk;
    }

    [HttpPost("create")]
    public Result<object> Create([FromBody] CreateSubdomainRequest request)
    {
        return _plesk.CreateSubdomain(request.SubDomain.ToLower(), request.RootDomain.ToLower(), request.Email, request.AuthentikHost);
    }

    [HttpDelete("{rootDomain}/{subDomain}")]
    public Result<object> Delete(string rootDomain, string subDomain)
    {
        return _plesk.DeleteSubDomain(rootDomain, subDomain);
    }

    [HttpGet("all")]
    public ActionResult<List<string>> All()
    {
        return _plesk.Records;
    }

    [HttpPost("updateCache")]
    public IActionResult UpdateCache()
    {
        _plesk.UpdateCache();
        return Ok();
    }

    [HttpPost("validateHost")]
    public ActionResult<bool> ValidateHost([FromBody, Required] string newAuthHost)
    {
        return RegexHelper.ValidateAuthHost(newAuthHost);
    }

    [HttpPost("{rootDomain}/{subDomain}/updateAuthHost")]
    public Result<object> UpdateAuthHost(string rootDomain, string subDomain, [FromBody, Required] string newAuthHost)
    {
        Result<object> result = _plesk.UpdateAuthHost(rootDomain, subDomain, newAuthHost);
        
        if(result.IsSuccess)
        {
            _plesk.ReloadNginx();
        }

        return result;
    }

    [HttpGet("{rootDomain}/{subDomain}/authHost")]
    public Result<string> GetAuthHost(string rootDomain, string subDomain)
    {
        return _plesk.GetAuthHost(rootDomain, subDomain);
    }

    [HttpGet("{domain}/extract")]
    public ActionResult<KeyValuePair<string, string>> ExtractDomains(string domain)
    {
        KeyValuePair<string, string>? domains = RegexHelper.ExtractDomains(domain);

        if (domains == null)
            return Problem("Could not extract the domains.");

        return domains;
    }
}