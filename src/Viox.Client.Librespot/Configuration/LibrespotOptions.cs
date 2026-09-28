namespace Viox.Client.Librespot.Configuration;

/// <summary>
/// Configuration options for the go-librespot client integration.
/// </summary>
public sealed class LibrespotOptions
{
    public const string SectionName = "Librespot";

    /// <summary>
    /// Gets or sets the base URI for the go-librespot REST API.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:3678";

    /// <summary>
    /// Gets or sets the WebSocket endpoint URI for event streams.
    /// </summary>
    public string WebSocketUrl { get; set; } = "ws://localhost:3678/events";

    /// <summary>
    /// Gets or sets the REST client request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether the WebSocket client should auto-reconnect on drop.
    /// </summary>
    public bool EnableWebSocketAutoReconnect { get; set; } = true;
}
