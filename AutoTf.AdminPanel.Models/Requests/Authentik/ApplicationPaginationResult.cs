using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class ApplicationPaginationResult
{
    [JsonPropertyName("pagination")]
    public object? Pagination { get; set; }

    [JsonPropertyName("results")] 
    public List<Application> Results { get; set; } = [];
}