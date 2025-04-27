using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateSubdomainRequest
{
    [JsonPropertyName("subDomain")]
    public string SubDomain { get; set; }
    
    [JsonPropertyName("rootDomain")]
    public string RootDomain { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("authentikHost")]
    public string AuthentikHost { get; set; }
}