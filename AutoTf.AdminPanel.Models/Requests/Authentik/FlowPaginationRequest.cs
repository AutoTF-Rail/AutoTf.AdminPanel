using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class FlowPaginationRequest
{
    [JsonPropertyName("pagination")]
    public required object Pagination { get; set; }
    
    [JsonPropertyName("results")]
    public required List<Flow> Results { get; set; }
}