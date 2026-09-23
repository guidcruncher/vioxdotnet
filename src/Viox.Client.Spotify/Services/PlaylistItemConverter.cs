namespace Viox.Client.Spotify.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

using Viox.Client.Spotify.Models;

public class PlaylistItemConverter : JsonConverter<IPlaylistItem>
{
    public override IPlaylistItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.GetProperty("type").GetString();
        if (type is null)
        {
            throw new JsonException("Cannot find property 'type' or its value is null.");
        }

        IPlaylistItem? result = type switch
        {
            "track" => JsonSerializer.Deserialize<SpotifyTrack>(root, options),
            "episode" => JsonSerializer.Deserialize<SpotifyEpisode>(root, options),
            _ => throw new JsonException($"Unknown playlist item type: {type}")
        };

        return result ?? throw new JsonException($"Failed to deserialize JSON into {type}. The result was null.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        IPlaylistItem value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
