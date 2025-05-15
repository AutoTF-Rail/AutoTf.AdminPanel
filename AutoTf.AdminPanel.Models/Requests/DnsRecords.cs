using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests;

public class DnsRecords
{
    [JsonPropertyName("result")]
    public required List<DnsRecord> Records { get; set; }
}