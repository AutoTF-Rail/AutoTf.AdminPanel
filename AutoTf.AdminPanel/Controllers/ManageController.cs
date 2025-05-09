using System.ComponentModel.DataAnnotations;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Manage;
using AutoTf.AdminPanel.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Controllers;

[ApiController]
[Route("/api/manage")]
public class ManageController : ControllerBase
{
    private readonly ManageManager _manager;
    private readonly PleskManager _plesk;

    public ManageController(ManageManager manager, PleskManager plesk)
    {
        _manager = manager;
        _plesk = plesk;
    }

    [HttpPost("updateAuthHost")]
    public async Task<ActionResult> UpdateAuthHost([FromBody, Required] UpdateAuthHostRequest request)
    {
        List<string> allWithHost = await _manager.AllWithHost(request.CurrentHost);

        foreach (string host in allWithHost)
        {
            _plesk.UpdateAuthHost(host, request.NewHost);
        }

        return Ok();
    }

    [HttpPost("updateAllAuthHost")]
    public async Task<ActionResult> UpdateAuthHost([FromBody, Required] string newHost)
    {
        List<string> allPlesk = await _manager.AllPlesk();

        foreach (string host in allPlesk)
        {
            _plesk.UpdateAuthHost(host, newHost);
        }

        return Ok();
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<ManageBody>>> All()
    {
        return await _manager.All();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        return Ok(await _manager.RevertChangesById(string.Empty, id));
    }

    [HttpGet("size")]
    public async Task<ActionResult<float>> TotalSizeGb()
    {
        return await _manager.GetTotalSizeGb();
    }

    [HttpGet("trainCount")]
    public async Task<ActionResult<int>> TrainCount()
    {
        return await _manager.GetTotalTrainCount();
    }

    [HttpGet("allowedTrainsCount")]
    public async Task<ActionResult<int>> AllowedTrainsCount()
    {
        return await _manager.GetAllowedTrainsCount();
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody, Required] TotalCreationRequest request)
    {
        string? result = await _manager.Create(request);
        if (result == null)
            return Ok();

        return Problem(result);
    }
}