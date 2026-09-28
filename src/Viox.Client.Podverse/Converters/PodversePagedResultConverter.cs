using System.Text.Json;
using System.Text.Json.Serialization;

using Viox.Client.Podverse.Models;

namespace Viox.Client.Podverse.Converters;

/// <summary>
/// Custom converter to deserialize array tuples in the form of [ [items], totalCount ].
/// </summary>
/// <typeparam name="T">The item type contained within the inner array.</typeparam>
public sealed class PodversePagedResultConverter<T> : JsonConverter<PodversePagedResult<T>>
{
    /// <inheritdoc />
    public override PodversePagedResult<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray token but got {reader.TokenType}.");
        }

        reader.Read();

        List<T>? items = null;
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            items = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            items = [];
        }

        reader.Read();

        var totalCount = 0;
        if (reader.TokenType == JsonTokenType.Number)
        {
            totalCount = reader.GetInt32();
        }

        while (reader.TokenType != JsonTokenType.EndArray && reader.Read())
        {
        }

        return new PodversePagedResult<T>
        {
            Items = items ?? [],
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PodversePagedResult<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        JsonSerializer.Serialize(writer, value.Items, options);
        writer.WriteNumberValue(value.TotalCount);
        writer.WriteEndArray();
    }
}
