using AutoTf.AdminPanel.Models.Requests;

namespace AutoTf.AdminPanel.Models.Manage;

public class TotalCreationRequest
{
    public required CreateDnsRecord DnsRecord { get; set; }
    
    public required CreateContainer Container { get; set; }
    
    public required Proxy Proxy { get; set; }
    
    public required CreateSubdomainRequest Plesk { get; set; }
}