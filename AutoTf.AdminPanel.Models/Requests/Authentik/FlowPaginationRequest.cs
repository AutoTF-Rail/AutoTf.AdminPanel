using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class FlowPaginationRequest
{
    [JsonPropertyName("pagination")]
    public object Pagination { get; set; }
    
    [JsonPropertyName("results")]
    public List<Flow> Results { get; set; }
}