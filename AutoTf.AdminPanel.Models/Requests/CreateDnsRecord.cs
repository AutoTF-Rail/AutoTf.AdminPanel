using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateDnsRecord
{
    public CreateDnsRecord(string type, string name, string content, int ttl, bool proxied, string comment)
    {
        Type = type;
        Name = name;
        Content = content;
        Ttl = ttl;
        Proxied = proxied;
        Comment = comment;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("content")]
    public string Content { get; set; }
    
    [JsonPropertyName("ttl")]
    public int Ttl { get; set; }
    
    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; }
    
    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}