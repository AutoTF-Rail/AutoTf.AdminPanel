using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Application
{
    [JsonPropertyName("pk")]
    public required string Pk { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("slug")]
    public required string Slug { get; set; }

    [JsonPropertyName("launch_url")]
    public string? LaunchUrl { get; set; }

    [JsonPropertyName("open_in_new_tab")]
    public bool OpenInNewTab { get; set; }

    [JsonPropertyName("meta_launch_url")]
    public string? MetaLaunchUrl { get; set; }
}