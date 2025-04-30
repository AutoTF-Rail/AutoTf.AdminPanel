using System.Text.Json.Serialization;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;

namespace AutoTf.AdminPanel.Models.Manage;

public class Proxy
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("launchUrl")]
    public string LaunchUrl { get; set; }
    
    [JsonPropertyName("authorizationFlow")]
    public string AuthorizationFlow { get; set; }
    
    [JsonPropertyName("invalidationFlow")]
    public string InvalidationFlow { get; set; }
    
    [JsonPropertyName("externalHost")]
    public string ExternalHost { get; set; }
    
    [JsonPropertyName("policyBindings")]
    public List<PolicyBinding> PolicyBindings { get; set; }
    
    [JsonPropertyName("outpostId")]
    public string OutpostId { get; set; }

    public CreateProxyRequest ConvertToRequest(string ipAddress)
    {
        return new CreateProxyRequest()
        {
            Name = Name,
            LaunchUrl = LaunchUrl,
            AuthorizationFlow = AuthorizationFlow,
            ExternalHost = ExternalHost,
            InternalHost = $"http://{ipAddress}:8080",
            InvalidationFlow = InvalidationFlow,
            PolicyBindings = PolicyBindings
        };
    }
}