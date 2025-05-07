using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Application
{
    [JsonPropertyName("pk")]
    public string Pk { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("launchUrl")]
    public string? LaunchUrl { get; set; }

    [JsonPropertyName("openInNewTab")]
    public bool OpenInNewTab { get; set; }

    [JsonPropertyName("metaLaunchUrl")]
    public string MetaLaunchUrl { get; set; }
}