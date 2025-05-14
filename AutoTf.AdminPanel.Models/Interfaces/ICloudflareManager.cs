using AutoTf.AdminPanel.Models.Requests;
using Microsoft.Extensions.Hosting;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface ICloudflareManager : IHostedService
{
    public List<DnsRecord> Records { get; }

    public Task<Result<object>> CreateNewEntry(CreateDnsRecord record);
    
    public Task<Result<object>> DeleteEntry(string id);
    
    public bool DoesEntryExistByName(string name);
    
    public bool DoesEntryExist(string id);
    
    public DnsRecord? GetRecordByName(string name, string type);
    
    public DnsRecord? GetRecord(string id);
    
    public Task<Result<object>> UpdateRecord(string id, CreateDnsRecord record);
    
    public Task UpdateCache();
}