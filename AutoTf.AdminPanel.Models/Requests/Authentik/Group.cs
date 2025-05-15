using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Group
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("pk")]
    public required string Pk { get; set; }
}