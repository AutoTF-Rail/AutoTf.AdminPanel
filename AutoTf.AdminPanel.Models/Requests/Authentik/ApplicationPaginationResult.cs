using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class ApplicationPaginationResult
{
    public object Pagination { get; set; }

    [JsonPropertyName("results")] 
    public List<Application> Results { get; set; } = [];
}