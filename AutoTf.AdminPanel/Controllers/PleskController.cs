using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("api/plesk")]
public class PleskController : ControllerBase
{
    private readonly PleskManager _plesk;

    public PleskController(PleskManager plesk)
    {
        _plesk = plesk;
    }

    [HttpPost("create")]
    public ActionResult<bool> Create([FromBody] CreateSubdomainRequest request)
    {
        return _plesk.CreateSubdomain(request.SubDomain.ToLower(), request.RootDomain.ToLower(), request.Email, request.AuthentikHost);
    }

    [HttpDelete("{rootDomain}/{subDomain}")]
    public ActionResult<bool> Delete(string rootDomain, string subDomain)
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
    public IActionResult UpdateAuthHost(string rootDomain, string subDomain, [FromBody, Required] string newAuthHost)
    {
        if (_plesk.UpdateAuthHost(rootDomain, subDomain, newAuthHost))
        {
            _plesk.ReloadNginx();
            return Ok();
        }

        return Problem($"Something went wrong when updating to the new auth host {newAuthHost}.");
    }

    [HttpGet("{rootDomain}/{subDomain}/authHost")]
    public ActionResult<string> GetAuthHost(string rootDomain, string subDomain)
    {
        string? authHost = _plesk.GetAuthHost(rootDomain, subDomain);

        if (authHost == null)
            return Problem("Could not read auth host because the subdomain does not exist, or it's invalid.");

        return authHost;
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