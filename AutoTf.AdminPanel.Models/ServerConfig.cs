namespace AutoTf.AdminPanel.Models;

public class ServerConfig
{
    public string DefaultDnsType { get; set; }
    public string DefaultTarget { get; set; }
    public bool DefaultProxySetting { get; set; }
    public int DefaultTtl { get; set; }
    
    public string DefaultNetwork { get; set; }
    public string DefaultAdditionalNetwork { get; set; }
    public string DefaultImage { get; set; }
    
    public string DefaultAuthorizationFlow { get; set; }
    public string DefaultInvalidationFlow { get; set; }

    public string DefaultOutpost { get; set; }
    
    public string DefaultCertificateEmail { get; set; }
    
    public string DefaultAuthentikHost { get; set; }
}