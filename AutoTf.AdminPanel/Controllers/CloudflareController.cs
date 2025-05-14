using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/cloudflare")]
public class CloudflareController : ControllerBase
{
    private readonly CloudflareManager _cloudflare;

    public CloudflareController(CloudflareManager cloudflare)
    {
        _cloudflare = cloudflare;
    }

    // TODO: Check that the new values don't already exist
    [HttpPost("create")]
    public async Task<Result<object>> CreateRecord([FromBody] CreateDnsRecord record)
    {
        return await _cloudflare.CreateNewEntry(record);
    }

    [HttpDelete("{id}")]
    public async Task<Result<object>> DeleteRecord(string id)
    {
        return await _cloudflare.DeleteEntry(id);
    }

    [HttpGet("{id}")]
    public Result<DnsRecord> GetRecord(string id)
    {
        return _cloudflare.GetRecord(id);
    }
    
    // TODO: Check that the new values don't already exist
    [HttpPatch("{id}")]
    public async Task<Result<object>> UpdateRecord(string id, [FromBody] CreateDnsRecord record)
    {
        return await _cloudflare.UpdateRecord(id, record);
    }

    [HttpGet("all")]
    public ActionResult<List<DnsRecord>> GetAll()
    {
        return _cloudflare.Records;
    }

    [HttpPost("update")]
    public Task Update()
    {
        return _cloudflare.UpdateCache();
    }
}