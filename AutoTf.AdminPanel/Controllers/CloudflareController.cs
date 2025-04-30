using AutoTf.AdminPanel.Managers;
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

    [HttpPost("create")]
    public async Task<ActionResult<bool>> CreateRecord([FromBody] CreateDnsRecord record)
    {
        // TODO: Check that the new values don't already exist
        return await _cloudflare.CreateNewEntry(record);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteRecord(string id)
    {
        // This not only ensures that nothing is deleted that doesn't exist, but it also ensures that nothing is deleted that isnt a Admin Panel managed record.
        if (!_cloudflare.DoesEntryExist(id))
            return NotFound("Could not find DNS entry.");
        
        return await _cloudflare.DeleteEntry(id);
    }

    [HttpGet("{id}")]
    public ActionResult<DnsRecord> GetRecord(string id)
    {
        if (!_cloudflare.DoesEntryExist(id))
            return NotFound("Could not find DNS entry.");
        
        return _cloudflare.GetRecord(id)!;
    }

    [HttpPost("{id}")]
    public async Task<ActionResult<string>> UpdateRecord(string id, [FromBody] CreateDnsRecord record)
    {
        if (!_cloudflare.DoesEntryExist(id))
            return NotFound("Could not find DNS entry.");
        
        // TODO: Check that the new values don't already exist
        return await _cloudflare.UpdateRecord(id, record);
    }

    [HttpGet("all")]
    public ActionResult<List<DnsRecord>> GetAll()
    {
        return _cloudflare.Records;
    }
}