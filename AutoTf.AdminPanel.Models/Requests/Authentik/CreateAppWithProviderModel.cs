using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class CreateAppWithProviderModel
{
    public CreateAppWithProviderModel(AppModel app, Provider provider, List<PolicyBinding> policyBindings)
    {
        App = app;
        Provider = provider;
        PolicyBindings = policyBindings;
    }

    [JsonPropertyName("app")] 
    public AppModel App { get; set; }

    [JsonPropertyName("provider_model")] 
    public string ProviderModel { get; set; } = "authentik_providers_proxy.proxyprovider";

    [JsonPropertyName("provider")] 
    public Provider Provider { get; set; }

    [JsonPropertyName("policy_bindings")] 
    public List<PolicyBinding> PolicyBindings { get; set; }
}