using System.Text;
using System.Text.Json;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class CloudflareManager : IHostedService
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

    public async Task<bool> CreateNewEntry(CreateDnsRecord record)
    {
        try
        {
            record.Comment = "Managed by AutoTF Admin Panel. " + record.Comment;
            record.Name = record.Name.ToLower();
            
            HttpContent content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");
            
            CreateDnsRecordResult? result = await ApiHttpHelper.SendPost<CreateDnsRecordResult>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", content, _credentials.CloudflareKey, true);

            if (result == null)
            {
                Console.WriteLine("Failed to create DNS entry:");
                foreach (object error in result.Errors)
                {
                    Console.WriteLine(error.ToString());
                }

                return false;
            }
            
            Records.Add(result.Result);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when configuring the DNS entry for {record.Name}:");
            Console.WriteLine(e.ToString());
        }
        
        return false;
    }

    public async Task<bool> DeleteEntry(string id)
    {
        try
        {
            if (!await ApiHttpHelper.SendDelete($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records/{id}", _credentials.CloudflareKey)) 
                return false;
            
            Records.RemoveAll(x => x.Id == id);
            
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when deleting the DNS entry {id}:");
            Console.WriteLine(e.ToString());
            return false;
        }
    }

    public bool DoesEntryExistByName(string name)
    {
        return Records.Any(x => x.Name == name.ToLower());
    }

    public bool DoesEntryExist(string id)
    {
        return Records.Any(x => x.Id == id);
    }

    public DnsRecord? GetRecordByName(string name, string content)
    {
        return Records.FirstOrDefault(x => x.Name.StartsWith(name.ToLower()) && x.Content == content.ToLower());
    }

    public DnsRecord? GetRecord(string id)
    {
        return Records.FirstOrDefault(x => x.Id == id);
    }

    public async Task<string?> UpdateRecord(string id, CreateDnsRecord record)
    {
        try
        {
            if (!record.Comment.Contains("Managed by AutoTF Admin Panel."))
                record.Comment = "Managed by AutoTF Admin Panel. " + record.Comment;

            record.Name = record.Name.ToLower();
            
            HttpContent content = new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json");

            return await ApiHttpHelper.SendPatch(
                $"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records/{id}", content,
                _credentials.CloudflareKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when updating the DNS Record: ");
            Console.WriteLine(e.ToString());
        }
        
        return null;
    }

    public async Task UpdateCache()
    {
        try
        {
            Console.WriteLine("Updating cloudflare records cache.");
            
            DnsRecords? dnsRecords = await ApiHttpHelper.SendGet<DnsRecords>($"https://api.cloudflare.com/client/v4/zones/{_credentials.CloudflareZone}/dns_records", _credentials.CloudflareKey, true);

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