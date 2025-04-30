using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models;

public class StringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt32().ToString(),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}