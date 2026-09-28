using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// Reads Radio Browser's mixed boolean encodings: JSON booleans, <c>true</c>/<c>false</c> strings,
/// and the integer flags <c>0</c>/<c>1</c> used by fields such as <c>hls</c> and <c>lastcheckok</c>.
/// </summary>
public sealed class FlexibleBooleanConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt64(out var number) => number != 0,
            JsonTokenType.String => ParseString(reader.GetString()),
            JsonTokenType.Null => false,
            _ => throw new JsonException($"Cannot convert token {reader.TokenType} to Boolean.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);

    private static bool ParseString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            return number != 0;
        }

        return false;
    }
}

/// <summary>
/// Reads Radio Browser nullable boolean fields that may arrive as booleans, integers, or strings.
/// </summary>
public sealed class FlexibleNullableBooleanConverter : JsonConverter<bool?>
{
    private static readonly FlexibleBooleanConverter Inner = new();

    /// <inheritdoc />
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        return Inner.Read(ref reader, typeof(bool), options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteBooleanValue(value.Value);
    }
}

/// <summary>
/// Reads integer values that the API sometimes encodes as strings.
/// </summary>
public sealed class FlexibleInt32Converter : JsonConverter<int>
{
    /// <inheritdoc />
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out var number) => number,
            JsonTokenType.Number => Convert.ToInt32(reader.GetDouble()),
            JsonTokenType.String when int.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            JsonTokenType.Null => 0,
            _ => throw new JsonException($"Cannot convert token {reader.TokenType} to Int32.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

/// <summary>
/// Reads nullable doubles that the API may omit, null out, or encode as strings.
/// </summary>
public sealed class FlexibleNullableDoubleConverter : JsonConverter<double?>
{
    /// <inheritdoc />
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String when string.IsNullOrWhiteSpace(reader.GetString()) => null,
            JsonTokenType.String when double.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => throw new JsonException($"Cannot convert token {reader.TokenType} to Double.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value);
    }
}
