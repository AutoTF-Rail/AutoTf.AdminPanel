using System.Text;
using System.Text.Json;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class CloudflareManager : ICloudflareManager
{
    private Timer _timer = new Timer(TimeSpan.FromMinutes(5));
    private Credentials _credentials;

    public List<DnsRecord> Records { get; private set; } = [];
    
    public CloudflareManager(IOptions<Credentials> options)
    {
        _credentials = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        StartCacheTimer();
        await UpdateCache();
    }

    private void StartCacheTimer()
    {
        _timer.AutoReset = true;
        _timer.Elapsed += (_, _) => _ = UpdateCache();
        _timer.Start();
    }

    public bool DoesEntryExistByName(string name) => Records.Any(x => x.Name == name.ToLower());

    public bool DoesEntryExist(string id) => Records.Any(x => x.Id == id);

    public DnsRecord? GetRecordByName(string name, string type) => Records.FirstOrDefault(x => x.Name.StartsWith(name.ToLower()) && x.Type == type);

    public Result<DnsRecord> GetRecord(string id)
    {
        if(!DoesEntryExist(id))
            return Result.Fail<DnsRecord>(ResultCode.NotFound, $"Could not find entry by id {id}.");
        
        return Result.Ok(Records.First(x => x.Id == id));
    }

    public async Task<Result<object>> CreateNewEntry(CreateDnsRecord record)
    {
        record.Comment = "Managed by AutoTF Admin Panel. " + record.Comment;
        record.Name = record.Name.ToLower();
        
        HttpContent content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
        
        Result<CreateDnsRecordResult> result = await ApiHttpHelper.SendPost<CreateDnsRecordResult>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", content, _credentials.CloudflareKey);

        if (!result.IsSuccess || result.Value == null)
        {
            return Result.Fail(result.ResultCode, result.Error);
        }
        
        Records.Add(result.Value.Result);

        return Result.Ok();
    }

    public async Task<Result<object>> DeleteEntry(string id)
    {
        if(!DoesEntryExist(id))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find entry by id {id}.");
        
        Result<string> result = await ApiHttpHelper.SendDelete($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records/{id}", _credentials.CloudflareKey);
        
        if (!result.IsSuccess || result.Value == null)
        {
            return Result.Fail(result.ResultCode, result.Error);
        }

        await UpdateCache();
        
        return Result.Ok();
    }

    public async Task<Result<object>> UpdateRecord(string id, CreateDnsRecord record)
    {
        if(!DoesEntryExist(id))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find entry by id {id}.");
        
        record.Name = record.Name.ToLower();
        
        HttpContent content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");

        Result<string> result = await ApiHttpHelper.SendPatch($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records/{id}", content, _credentials.CloudflareKey);
        
        if (!result.IsSuccess || result.Value == null)
        {
            return Result.Fail(result.ResultCode, result.Error);
        }
        
        await UpdateCache();
        
        return Result.Ok();
    }

    public async Task UpdateCache()
    {
        
        Console.WriteLine("Updating cloudflare records cache.");
        
        Result<DnsRecords> dnsRecords = await ApiHttpHelper.SendGet<DnsRecords>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", _credentials.CloudflareKey);

        if (!dnsRecords.IsSuccess || dnsRecords.Value == null)
        {
            Console.WriteLine($"Failed to update cloudflare cache. {dnsRecords.Error}");
            Environment.Exit(1);
        }

        Records = dnsRecords.Value.Records.Where(x => x.Comment != null && x.Comment.Contains("Managed by AutoTF Admin Panel.")).ToList();
        
        Console.WriteLine($"Finished updating cloudflare records cache with {Records.Count} records.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return Task.CompletedTask;
    }
}