namespace AutoTf.AdminPanel.Models;

public class Credentials
{
    public required string ClientId { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string AuthUrl { get; set; }
    public required string CloudflareZone { get; set; }
    public required string CloudflareKey { get; set; }
    
    public required ServerConfig DefaultConfig { get; set; }
    
    public required string AuthServerContainerId { get; set; }
    public required string AuthDefaultNetworkId { get; set; }
}