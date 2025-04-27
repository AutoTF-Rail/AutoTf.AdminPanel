using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class CreateDnsRecordResult
{
    [JsonPropertyName("result")] public DnsRecord Result { get; set; }

    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errors")]
    public List<object> Errors { get; set; }
    
    [JsonPropertyName("messages")]
    public List<object> Messages { get; set; }

}