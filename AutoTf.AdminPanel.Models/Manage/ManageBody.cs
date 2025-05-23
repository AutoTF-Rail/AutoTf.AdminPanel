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
    
    [JsonPropertyName("rootDomain")]
    public string? RootDomain { get; set; } = null;
    
    [JsonPropertyName("evuName")]
    public string? EvuName { get; set; } = null;

    [JsonPropertyName("id")] 
    public string Id { get; set; } = null!;
}