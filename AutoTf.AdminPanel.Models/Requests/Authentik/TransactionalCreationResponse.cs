using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class TransactionalCreationResponse
{
    [JsonPropertyName("applied")]
    public bool Applied { get; set; }
    
    [JsonPropertyName("logs")]
    public List<string>? Logs { get; set; }
}