using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Group
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("pk")]
    public string Pk { get; set; }
}