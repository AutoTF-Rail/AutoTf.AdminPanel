using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class OutpostModel
{
    [JsonPropertyName("Pk")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Pk { get; set; } = null;
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("providers")] 
    [JsonConverter(typeof(StringListConverter))]
    public List<string> Providers { get; set; } = [];

    [JsonPropertyName("providers_obj")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? ProvidersObject { get; set; } = null;
    
    [JsonPropertyName("service_connection")]
    public required string ServiceConnection { get; set; }

    [JsonPropertyName("service_connection_obj")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ServiceConnectionObject { get; set; } = null;

    [JsonPropertyName("refresh_interval_s")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RefreshIntervalS { get; set; } = null;

    [JsonPropertyName("token_identifier")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenIdentifier { get; set; } = null;
    
    [JsonPropertyName("config")]
    public required OutpostConfig Config { get; set; }

    [JsonPropertyName("managed")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Managed { get; set; } = null;
}