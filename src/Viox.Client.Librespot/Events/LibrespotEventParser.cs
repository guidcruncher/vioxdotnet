using System.Text.Json;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Parses raw JSON WebSocket messages from go-librespot into strongly typed LibrespotEvent instances.
/// </summary>
public static class LibrespotEventParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static LibrespotEvent? Parse(string jsonMessage)
    {
        if (string.IsNullOrWhiteSpace(jsonMessage))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonMessage);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeElement))
            {
                return null;
            }

            var eventType = typeElement.GetString();
            if (string.IsNullOrEmpty(eventType))
            {
                return null;
            }

            var hasData = root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind != JsonValueKind.Null;
            var rawData = hasData ? dataElement.GetRawText() : "{}";

            return eventType switch
            {
                "active" => new ActiveEvent(),
                "inactive" => new InactiveEvent(),
                "metadata" => JsonSerializer.Deserialize<MetadataEvent>(rawData, SerializerOptions),
                "will_play" => JsonSerializer.Deserialize<WillPlayEvent>(rawData, SerializerOptions),
                "playing" => JsonSerializer.Deserialize<PlayingEvent>(rawData, SerializerOptions),
                "not_playing" => JsonSerializer.Deserialize<NotPlayingEvent>(rawData, SerializerOptions),
                "paused" => JsonSerializer.Deserialize<PausedEvent>(rawData, SerializerOptions),
                "stopped" => JsonSerializer.Deserialize<StoppedEvent>(rawData, SerializerOptions),
                "seek" => JsonSerializer.Deserialize<SeekEvent>(rawData, SerializerOptions),
                "volume" => JsonSerializer.Deserialize<VolumeEvent>(rawData, SerializerOptions),
                "shuffle_context" => JsonSerializer.Deserialize<ShuffleContextEvent>(rawData, SerializerOptions),
                "repeat_context" => JsonSerializer.Deserialize<RepeatContextEvent>(rawData, SerializerOptions),
                "repeat_track" => JsonSerializer.Deserialize<RepeatTrackEvent>(rawData, SerializerOptions),
                _ => new UnknownLibrespotEvent
                {
                    EventType = eventType,
                    RawData = hasData ? dataElement.Clone() : default
                }
            };
        }
        catch
        {
            return null;
        }
    }
}
