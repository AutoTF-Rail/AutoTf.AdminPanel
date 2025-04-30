using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateContainer
{
    [Required]
    [JsonPropertyName("defaultNetwork")]
    public string DefaultNetwork { get; set; } = null!;
    
    [JsonPropertyName("defaultIp")]
    public string DefaultIp { get; set; } = null!;

    [JsonPropertyName("additionalNetwork")]
    public string AdditionalNetwork { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("image")]
    public string Image { get; set; } = null!;
    
    [JsonPropertyName("portMappings")]
    public List<KeyValuePair<string, string>> PortMappings { get; set; } = new List<KeyValuePair<string, string>>();

    [Required]
    [JsonPropertyName("evuName")]
    public string EvuName { get; set; } = string.Empty;

    [JsonPropertyName("containerName")]
    public string ContainerName { get; set; } = string.Empty;
}