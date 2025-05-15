using System.Text.Json.Serialization;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;

namespace AutoTf.AdminPanel.Models.Manage;

public class Proxy
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("launchUrl")]
    public required string LaunchUrl { get; set; }
    
    [JsonPropertyName("authorizationFlow")]
    public required string AuthorizationFlow { get; set; }
    
    [JsonPropertyName("invalidationFlow")]
    public required string InvalidationFlow { get; set; }
    
    [JsonPropertyName("externalHost")]
    public required string ExternalHost { get; set; }
    
    [JsonPropertyName("policyBindings")]
    public required List<PolicyBinding> PolicyBindings { get; set; }
    
    [JsonPropertyName("outpostId")]
    public required string OutpostId { get; set; }

    public CreateProxyRequest ConvertToRequest(string ipAddress, string subDomain)
    {
        return new CreateProxyRequest()
        {
            Name = subDomain,
            LaunchUrl = LaunchUrl,
            AuthorizationFlow = AuthorizationFlow,
            ExternalHost = ExternalHost,
            InternalHost = $"http://{ipAddress}:8080",
            InvalidationFlow = InvalidationFlow,
            PolicyBindings = PolicyBindings
        };
    }
}