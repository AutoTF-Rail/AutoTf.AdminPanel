using System.Text.Json.Serialization;
using AutoTf.AdminPanel.Models.Requests.Authentik;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateProxyRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("launchUrl")]
    public required string LaunchUrl { get; set; }
    
    [JsonPropertyName("authorizationFlow")]
    public required string AuthorizationFlow { get; set; }
    
    [JsonPropertyName("invalidationFlow")]
    public required string InvalidationFlow { get; set; }
    
    [JsonPropertyName("internalHost")]
    public required string InternalHost { get; set; }
    
    [JsonPropertyName("externalHost")]
    public required string ExternalHost { get; set; }
    
    [JsonPropertyName("policyBindings")]
    public required List<PolicyBinding> PolicyBindings { get; set; }
}