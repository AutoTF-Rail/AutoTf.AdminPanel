using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class Provider
{
    [JsonPropertyName("pk")]
    [JsonConverter(typeof(StringConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Pk { get; set; } = null;

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("authentication_flow")]
    public object? AuthenticationFlow { get; set; }

    [JsonPropertyName("authorization_flow")]
    public required string AuthorizationFlow { get; set; }

    [JsonPropertyName("invalidation_flow")]
    public required string InvalidationFlow { get; set; }

    [JsonPropertyName("property_mappings")]
    public required List<string> PropertyMappings { get; set; }

    [JsonPropertyName("internal_host")]
    public required string InternalHost { get; set; }

    [JsonPropertyName("external_host")]
    public required string ExternalHost { get; set; }

    [JsonPropertyName("internal_host_ssl_validation")]
    public bool InternalHostSslValidation { get; set; }

    [JsonPropertyName("certificate")]
    public object? Certificate { get; set; }

    [JsonPropertyName("skip_path_regex")]
    public required string SkipPathRegex { get; set; }

    [JsonPropertyName("basic_auth_enabled")]
    public bool BasicAuthEnabled { get; set; }

    [JsonPropertyName("basic_auth_password_attribute")]
    public required string BasicAuthPasswordAttribute { get; set; }

    [JsonPropertyName("basic_auth_user_attribute")]
    public required string BasicAuthUserAttribute { get; set; }

    [JsonPropertyName("mode")]
    public required string Mode { get; set; }

    [JsonPropertyName("intercept_header_auth")]
    public bool InterceptHeaderAuth { get; set; }

    [JsonPropertyName("cookie_domain")]
    public required string CookieDomain { get; set; }

    [JsonPropertyName("jwt_federation_sources")]
    public required List<object> JwtFederationSources { get; set; }

    [JsonPropertyName("jwt_federation_providers")]
    public required List<object> JwtFederationProviders { get; set; }

    [JsonPropertyName("access_token_validity")]
    public required string AccessTokenValidity { get; set; }

    [JsonPropertyName("providerModel")]
    public required string ProviderModel { get; set; }
}