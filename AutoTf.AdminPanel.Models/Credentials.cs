namespace AutoTf.AdminPanel.Models;

public class Credentials
{
    public string ClientId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string AuthUrl { get; set; }
    public string CloudflareZone { get; set; }
    public string CloudflareKey { get; set; }
    
    public ServerConfig DefaultConfig { get; set; }
    
    public string AuthServerContainerId { get; set; }
    public string AuthDefaultNetworkId { get; set; }
}