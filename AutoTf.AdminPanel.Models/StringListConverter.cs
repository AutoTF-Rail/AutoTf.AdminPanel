using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models;

public class StringListConverter : JsonConverter<List<string>>
{
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        List<string> result = new List<string>();

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
                    result.Add(reader.GetString()!);
                    break;
                case JsonTokenType.Number:
                    result.Add(reader.GetInt32().ToString());
                    break;
                default:
                    throw new JsonException("Unsupported type in array");
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}