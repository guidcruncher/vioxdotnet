using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Represents a Spotify access token for the active session.
/// </summary>
public sealed record ApiToken
{
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;
}
