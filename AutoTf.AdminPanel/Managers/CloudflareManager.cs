using System.Text;
using System.Text.Json;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Requests;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class CloudflareManager : IHostedService
{
    private Timer _timer = new Timer(TimeSpan.FromMinutes(10));
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

    public async Task<bool> CreateNewEntry(CreateDnsRecord record)
    {
        try
        {
            record.Comment = "Managed by AutoTF Admin Panel." + record.Comment;
            
            HttpContent content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
            
            await HttpHelper.SendPostCloudflare<bool>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", content, _credentials.CloudflareKey);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when configuring the DNS entry for {record.Name}:");
            Console.WriteLine(e.ToString());
            return false;
        }
    }

    public async Task<bool> DeleteEntry(string id)
    {
        try
        {
            return await HttpHelper.SendCloudflareDelete($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", _credentials.CloudflareKey);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when configuring the DNS entry {id}:");
            Console.WriteLine(e.ToString());
            return false;
        }
    }

    public bool DoesEntryExistByName(string name)
    {
        return Records.Any(x => x.Name == name);
    }

    public bool DoesEntryExist(string id)
    {
        return Records.Any(x => x.Id == id);
    }

    public DnsRecord? GetRecordByName(string name)
    {
        return Records.FirstOrDefault(x => x.Name == name);
    }

    public DnsRecord? GetRecord(string id)
    {
        return Records.FirstOrDefault(x => x.Id == id);
    }

    private async Task UpdateCache()
    {
        try
        {
            Console.WriteLine("Updating cloudflare records cache.");
            
            DnsRecords? dnsRecords = await HttpHelper.SendGetCloudflare<DnsRecords>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", _credentials.CloudflareKey, true);

            if (dnsRecords == null)
                throw new Exception("Empty return from Cloudflare API.");

            Records = dnsRecords.Records.Where(x => x.Comment != null && x.Comment.Contains("Managed by AutoTF Admin Panel.")).ToList();
            
            Console.WriteLine($"Finished updating cloudflare records cache with {Records.Count} records.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when updating the DNS cache:");
            Console.WriteLine(e.ToString());
            Environment.Exit(1);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return Task.CompletedTask;
    }
}