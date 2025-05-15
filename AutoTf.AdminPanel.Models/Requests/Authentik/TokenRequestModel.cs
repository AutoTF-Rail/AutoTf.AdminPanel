using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class TokenRequestModel
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("token_id")]
    public string? TokenId { get; set; }

    public static TokenRequestModel Empty()
    {
        return new TokenRequestModel()
        {
            AccessToken = "",
            ExpiresIn = 0,
        };
    }
}