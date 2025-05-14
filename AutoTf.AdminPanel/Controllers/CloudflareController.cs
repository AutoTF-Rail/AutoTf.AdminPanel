using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
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
    public async Task<Result<object>> CreateRecord([FromBody] CreateDnsRecord record)
    {
        // TODO: Check that the new values don't already exist
        return await _cloudflare.CreateNewEntry(record);
    }

    [HttpDelete("{id}")]
    public async Task<Result<object>> DeleteRecord(string id)
    {
        // This not only ensures that nothing is deleted that doesn't exist, but it also ensures that nothing is deleted that isn't a Admin Panel managed record.
        if (!_cloudflare.DoesEntryExist(id))
            return Result.Fail(ResultCode.NotFound, $"Could not find DNS entry {id}.");
        
        return await _cloudflare.DeleteEntry(id);
    }

    [HttpGet("{id}")]
    public Result<DnsRecord> GetRecord(string id)
    {
        if (!_cloudflare.DoesEntryExist(id))
            return Result.Fail<DnsRecord>(ResultCode.NotFound, $"Could not find DNS entry {id}.");

        return Result.Ok(_cloudflare.GetRecord(id)!);
    }

    [HttpPatch("{id}")]
    public async Task<Result<object>> UpdateRecord(string id, [FromBody] CreateDnsRecord record)
    {
        if (!_cloudflare.DoesEntryExist(id))
            return Result.Fail(ResultCode.NotFound, $"Could not find DNS entry {id}.");
        
        // TODO: Check that the new values don't already exist
        return await _cloudflare.UpdateRecord(id, record);
    }

    [HttpGet("all")]
    public ActionResult<List<DnsRecord>> GetAll()
    {
        return _cloudflare.Records;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update()
    {
        await _cloudflare.UpdateCache();
        return Ok();
    }
}