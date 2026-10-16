using System.Text.Json;
using System.Text.Json.Serialization;

using Viox.Core.Models;

namespace Viox.Core.Entities;

public sealed class HistoryItem
{

    public long Id { get; set; } = 0;
    public string Uri { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;

    public long CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static HistoryItem Create(MediaMetaData item)
    {
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        HistoryItem res = new()
        {
            Id = 0,
            Uri = item.RawUri,
            Metadata = JsonSerializer.Serialize(item, jsonOptions),
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        return res;
    }

}


