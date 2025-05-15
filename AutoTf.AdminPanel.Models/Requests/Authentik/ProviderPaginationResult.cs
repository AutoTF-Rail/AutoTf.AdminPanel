using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class ProviderPaginationResult
{
    public object? Pagination { get; set; }

    [JsonPropertyName("results")] 
    public List<Provider> Results { get; set; } = [];
}