using System.Text.Json;
using Viox.Core.Entities;

namespace Viox.Core.Models;

public sealed class History
{
    public long Id { get; set; } = 0;
    public string Uri { get; set; } = string.Empty;
    public MediaMetaData Metadata { get; set; } = new();
    public long CreatedTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static History Map(HistoryItem item)
    {
        History res = new()
        {
            Id = item.Id,
            Uri = item.Uri,
            Metadata = JsonSerializer.Deserialize<MediaMetaData>(item.Metadata) ?? new MediaMetaData(),
            CreatedTimestamp = item.CreatedTimestamp
        };

        return res;
    }
}
