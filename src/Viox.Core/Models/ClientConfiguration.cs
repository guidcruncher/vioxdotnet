namespace Viox.Core.Models;

using System.Text.Json.Serialization;

public class ClientConfiguration
{
    [JsonPropertyName("defaultCountry")]
    public string DefaultCountry { get; set; } = string.Empty;

    [JsonPropertyName("tuneInRegion")]
    public string TuneInRegion { get; set; } = "r101309";

    [JsonPropertyName("playLists")]
    public Dictionary<string, string> Playlists { get; set; } = new();
}
