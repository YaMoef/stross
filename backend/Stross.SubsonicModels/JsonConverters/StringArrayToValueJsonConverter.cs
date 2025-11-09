using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stross.SubsonicModels.JsonConverters;

/// <summary>
/// Converts a string array to a single string value containing concatenated content.
/// Example: ["foo", "bar"] => "foobar"
/// </summary>
public class StringArrayToValueJsonConverter : JsonConverter<string[]>
{
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString() ?? string.Empty;
            return new[] { value };
        }

        throw new JsonException("Expected string value");
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Concat(value));
    }
}
