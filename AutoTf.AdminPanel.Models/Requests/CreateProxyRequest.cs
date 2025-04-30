using System.Text.Json.Serialization;
using AutoTf.AdminPanel.Models.Requests.Authentik;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateProxyRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("launchUrl")]
    public string LaunchUrl { get; set; }
    
    [JsonPropertyName("authorizationFlow")]
    public string AuthorizationFlow { get; set; }
    
    [JsonPropertyName("invalidationFlow")]
    public string InvalidationFlow { get; set; }
    
    [JsonPropertyName("internalHost")]
    public string InternalHost { get; set; }
    
    [JsonPropertyName("externalHost")]
    public string ExternalHost { get; set; }
    
    [JsonPropertyName("policyBindings")]
    public List<PolicyBinding> PolicyBindings { get; set; }
}