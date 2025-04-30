using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class AppModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("slug")]
    public string Slug { get; set; }
    
    [JsonPropertyName("open_in_new_tab")]
    public bool OpenInNewTab { get; set; }
    
    [JsonPropertyName("meta_launch_url")]
    public string MetaLaunchUrl { get; set; }
    
    [JsonPropertyName("policy_engine_mode")]
    public string PolicyEngineMode { get; set; }
    
    [JsonPropertyName("group")]
    public string Group { get; set; }
}