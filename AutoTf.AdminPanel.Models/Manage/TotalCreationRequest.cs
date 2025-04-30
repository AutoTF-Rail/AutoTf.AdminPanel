using AutoTf.AdminPanel.Models.Requests;

namespace AutoTf.AdminPanel.Models.Manage;

public class TotalCreationRequest
{
    public CreateDnsRecord DnsRecord { get; set; }
    
    public CreateContainer Container { get; set; }
    
    public Proxy Proxy { get; set; }
    
    public CreateSubdomainRequest Plesk { get; set; }
}