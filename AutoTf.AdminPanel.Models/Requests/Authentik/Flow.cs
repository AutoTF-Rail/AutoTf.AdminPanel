using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Flow
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; }
    
    [JsonPropertyName("pk")]
    public string Pk { get; set; }
}