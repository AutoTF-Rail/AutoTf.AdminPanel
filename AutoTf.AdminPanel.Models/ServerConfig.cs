namespace AutoTf.AdminPanel.Models;

public class ServerConfig
{
    public required string DefaultDnsType { get; set; }
    public required string DefaultTarget { get; set; }
    
    public bool DefaultProxySetting { get; set; }
    public int DefaultTtl { get; set; }
    
    public required string DefaultNetwork { get; set; }
    public required string DefaultAdditionalNetwork { get; set; }
    public required string DefaultImage { get; set; }
    
    public required string DefaultAuthorizationFlow { get; set; }
    public required string DefaultInvalidationFlow { get; set; }

    public required string DefaultOutpost { get; set; }
    
    public required string DefaultCertificateEmail { get; set; }
    
    public required string DefaultAuthentikHost { get; set; }
}