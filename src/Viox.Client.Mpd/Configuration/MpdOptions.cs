namespace Viox.Client.Mpd.Configuration;

/// <summary>
/// Configuration options for connecting to a Music Player Daemon (MPD) server.
/// </summary>
public sealed class MpdOptions
{
    /// <summary>
    /// The default configuration section name used in configuration providers.
    /// </summary>
    public const string SectionName = "Mpd";

    /// <summary>
    /// Gets or sets the host address or IP of the MPD server.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port number of the MPD server. Default is 6600.
    /// </summary>
    public int Port { get; set; } = 6600;

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the optional password for MPD authentication.
    /// </summary>
    public string? Password { get; set; }
}
