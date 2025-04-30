using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models;

public class StringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string result = string.Empty;

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString()!;
                    break;
                case JsonTokenType.Number:
                    return reader.GetInt32().ToString();
                    break;
                default:
                    throw new JsonException("Unsupported type in array");
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value);
        writer.WriteEndArray();
    }
}