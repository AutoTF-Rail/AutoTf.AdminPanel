using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class GroupPaginationRequest
{
    [JsonPropertyName("pagination")]
    public required object Pagination { get; set; }
    
    [JsonPropertyName("results")]
    public required List<Group> Results { get; set; }
}