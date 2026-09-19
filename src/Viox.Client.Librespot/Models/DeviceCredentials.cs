namespace Viox.Client.Librespot.Models;

using System.Text.Json.Serialization;

public sealed class DeviceCredentials
{
    [property: JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [property: JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}
