using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class OutpostModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("providers")] 
    public List<string> Providers { get; set; } = [];
    
    [JsonPropertyName("service_connection")]
    public string ServiceConnection { get; set; }
    
    [JsonPropertyName("config")]
    public OutpostConfig Config { get; set; }
}