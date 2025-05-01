using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class GroupPaginationRequest
{
    [JsonPropertyName("pagination")]
    public object Pagination { get; set; }
    
    [JsonPropertyName("results")]
    public List<Group> Results { get; set; }
}