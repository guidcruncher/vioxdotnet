using System.Text.Json;
using System.Text.Json.Serialization;

using Viox.Client.Podverse.Models;

namespace Viox.Client.Podverse.Converters;

/// <summary>
/// Factory to instantiate PodversePagedResultConverter for generic item types.
/// </summary>
public sealed class PodversePagedResultConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeToConvert.GetGenericTypeDefinition() == typeof(PodversePagedResult<>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PodversePagedResultConverter<>).MakeGenericType(itemType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
