using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class PolicyBinding
{
    [JsonPropertyName("group")]
    public required string Group { get; set; }
    
    [JsonPropertyName("negate")]
    public bool Negate { get; set; }

    [JsonPropertyName("enabled")] 
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("order")] 
    public required string Order { get; set; }

    [JsonPropertyName("timeout")] 
    public required string Timeout { get; set; }
    
    [JsonPropertyName("failure_result")]
    public bool FailureResult { get; set; }
}