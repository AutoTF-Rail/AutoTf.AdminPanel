using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class AppProviderModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("authentication_flow")]
    public object? AuthenticationFlow { get; set; }

    [JsonPropertyName("authorization_flow")]
    public string AuthorizationFlow { get; set; }

    [JsonPropertyName("invalidation_flow")]
    public string InvalidationFlow { get; set; }

    [JsonPropertyName("property_mappings")]
    public List<object> PropertyMappings { get; set; }

    [JsonPropertyName("internal_host")]
    public string InternalHost { get; set; }

    [JsonPropertyName("external_host")]
    public string ExternalHost { get; set; }

    [JsonPropertyName("internal_host_ssl_validation")]
    public bool InternalHostSslValidation { get; set; }

    [JsonPropertyName("certificate")]
    public object? Certificate { get; set; }

    [JsonPropertyName("skip_path_regex")]
    public string SkipPathRegex { get; set; }

    [JsonPropertyName("basic_auth_enabled")]
    public bool BasicAuthEnabled { get; set; }

    [JsonPropertyName("basic_auth_password_attribute")]
    public string BasicAuthPasswordAttribute { get; set; }

    [JsonPropertyName("basic_auth_user_attribute")]
    public string BasicAuthUserAttribute { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    [JsonPropertyName("intercept_header_auth")]
    public bool InterceptHeaderAuth { get; set; }

    [JsonPropertyName("cookie_domain")]
    public string CookieDomain { get; set; }

    [JsonPropertyName("jwt_federation_sources")]
    public List<object> JwtFederationSources { get; set; }

    [JsonPropertyName("jwt_federation_providers")]
    public List<object> JwtFederationProviders { get; set; }

    [JsonPropertyName("access_token_validity")]
    public string AccessTokenValidity { get; set; }

    [JsonPropertyName("providerModel")]
    public string ProviderModel { get; set; }
}