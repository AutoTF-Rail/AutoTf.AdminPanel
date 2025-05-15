using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class DnsRecord
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("proxiable")]
    public bool Proxiable { get; set; }

    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("settings")]
    public required Dictionary<string, object> Settings { get; set; }

    [JsonPropertyName("meta")]
    public required Dictionary<string, object> Meta { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("tags")]
    public required List<string> Tags { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }
}