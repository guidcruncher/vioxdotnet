namespace Viox.Core.Models;

using System.Text.Json.Serialization;

public class ClientConfiguration
{
    [JsonPropertyName("defaultCountry")]
    public string DefaultCountry { get; set; } = string.Empty;

    [JsonPropertyName("playLists")]
    public Dictionary<string, string> Playlists { get; set; } = new();
}
