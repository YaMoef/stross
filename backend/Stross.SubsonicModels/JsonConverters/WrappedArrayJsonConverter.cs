using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stross.SubsonicModels.JsonConverters;

/// <summary>
/// Attribute for wrapping array serialization with a specified item name.
/// Usage: [WrappedArrayJsonConverter("genre")]
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class WrappedArrayJsonConverterAttribute : Attribute
{
    public WrappedArrayJsonConverterAttribute(string itemName)
    {
        ItemName = itemName;
    }

    public string ItemName { get; }
}

/// <summary>
/// Factory for creating wrapped array converters with a specified item name.
/// </summary>
public class WrappedArrayJsonConverterFactory : JsonConverterFactory
{
    private readonly string _itemName;

    public WrappedArrayJsonConverterFactory(string itemName)
    {
        _itemName = itemName;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(List<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(WrappedArrayJsonConverterImpl<>).MakeGenericType(elementType);
        return (JsonConverter?)Activator.CreateInstance(converterType, _itemName);
    }

    public class WrappedArrayJsonConverterImpl<T> : JsonConverter<List<T>>
    {
        private readonly string _itemName;

        public WrappedArrayJsonConverterImpl(string itemName)
        {
            _itemName = itemName;
        }

        public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var list = new List<T>();

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object");

            reader.Read(); // Move to property name
            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != _itemName)
                throw new JsonException($"Expected property name '{_itemName}'");

            reader.Read(); // Move to array start
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                if (item != null)
                    list.Add(item);
            }

            reader.Read(); // Move past end object

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(_itemName);
            writer.WriteStartArray();

            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}