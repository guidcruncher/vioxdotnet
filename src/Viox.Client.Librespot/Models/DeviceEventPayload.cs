namespace Viox.Client.Librespot.Models;

using System.Text.Json.Serialization;


public sealed class DeviceEventPayload
{
    [property: JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [property: JsonPropertyName("event_manager")]
    public string? EventManager { get; set; }

    [property: JsonPropertyName("credentials")]
    public DeviceCredentials Credentials { get; set; } = new();

    [property: JsonPropertyName("last_volume")]
    public int? LastVolume { get; set; }
}
