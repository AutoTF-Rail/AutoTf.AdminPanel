using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
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
        return _plesk.CreateSubdomain(request.SubDomain, request.RootDomain.ToLower(), request.Email, request.AuthentikHost);
    }

    [HttpDelete("{rootDomain}/{subDomain}")]
    public ActionResult<bool> Delete(string rootDomain, string subDomain)
    {
        return _plesk.DeleteSubDomain(rootDomain, subDomain);
    }

    [HttpGet("all")]
    public ActionResult<List<string>> All()
    {
        return _plesk.GetAll();
    }
}