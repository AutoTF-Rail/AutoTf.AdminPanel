using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateSubdomainRequest
{
    [JsonPropertyName("subDomain")]
    public required string SubDomain { get; set; }
    
    [JsonPropertyName("rootDomain")]
    public required string RootDomain { get; set; }
    
    [JsonPropertyName("email")]
    public required string Email { get; set; }
    
    [JsonPropertyName("authentikHost")]
    public required string AuthentikHost { get; set; }
}