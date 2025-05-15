using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Flow
{
    [JsonPropertyName("slug")]
    public required string Slug { get; set; }
    
    [JsonPropertyName("pk")]
    public required string Pk { get; set; }
}