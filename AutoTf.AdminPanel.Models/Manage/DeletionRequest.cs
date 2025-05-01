using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Manage;

public class DeletionRequest
{
    [JsonPropertyName("recordId")] 
    public string? RecordId { get; set; } = null;
    
    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; } = null;
    
    [JsonPropertyName("externalHost")]
    public string? ExternalHost { get; set; } = null;
}