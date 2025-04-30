using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class CreateAppWithProviderModel
{
    [JsonPropertyName("app")] 
    public AppModel App { get; set; } = new AppModel();

    [JsonPropertyName("provider_model")] 
    public string ProviderModel { get; set; } = "authentik_providers_proxy.proxyprovider";

    [JsonPropertyName("provider")] 
    public AppProviderModel Provider { get; set; } = new AppProviderModel();

    [JsonPropertyName("policy_bindings")] 
    public List<PolicyBinding> PolicyBindings { get; set; } = new List<PolicyBinding>();
}