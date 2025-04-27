using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class DnsRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("proxiable")]
    public bool Proxiable { get; set; }

    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }

    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }

    [JsonPropertyName("settings")]
    public Dictionary<string, object> Settings { get; set; }

    [JsonPropertyName("meta")]
    public Dictionary<string, object> Meta { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }
}