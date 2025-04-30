using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class OutpostModel
{
    [JsonPropertyName("Pk")] 
    public string Pk { get; set; } = "";
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("providers")] 
    public List<string> Providers { get; set; } = [];

    [JsonPropertyName("providers_obj")] 
    public List<object> ProvidersObject { get; set; } = [];
    
    [JsonPropertyName("service_connection")]
    public string ServiceConnection { get; set; }

    [JsonPropertyName("service_connection_obj")]
    public object? ServiceConnectionObject { get; set; } = null;

    [JsonPropertyName("refresh_interval_s")]
    public int? RefreshIntervalS { get; set; } = null;

    [JsonPropertyName("token_identifier")] 
    public string? TokenIdentifier { get; set; } = null;
    
    [JsonPropertyName("config")]
    public OutpostConfig Config { get; set; }

    [JsonPropertyName("managed")] 
    public string? Managed { get; set; } = null;
}