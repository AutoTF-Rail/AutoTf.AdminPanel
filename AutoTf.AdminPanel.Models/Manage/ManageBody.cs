using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Manage;

public class ManageBody
{
    [JsonPropertyName("recordId")] 
    public string? RecordId { get; set; } = null;
    
    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; } = null;
    
    [JsonPropertyName("externalHost")]
    public string? ExternalHost { get; set; } = null;
    
    [JsonPropertyName("subDomain")]
    public string? SubDomain { get; set; } = null;
    
    [JsonPropertyName("RootDomain")]
    public string? RootDomain { get; set; } = null;
}