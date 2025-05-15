using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class AppModel
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("slug")]
    public required string Slug { get; set; }
    
    [JsonPropertyName("open_in_new_tab")]
    public bool OpenInNewTab { get; set; }
    
    [JsonPropertyName("meta_launch_url")]
    public required string MetaLaunchUrl { get; set; }
    
    [JsonPropertyName("policy_engine_mode")]
    public required string PolicyEngineMode { get; set; }
    
    [JsonPropertyName("group")]
    public required string Group { get; set; }
}