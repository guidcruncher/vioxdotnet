namespace Viox.Snapcast.Configuration;

/// <summary>
/// Configuration options for the Snapcast TCP JSON-RPC client.
/// </summary>
public sealed class SnapcastOptions
{
    public const string Position = "Snapcast";

    /// <summary>
    /// Snapserver IP address or hostname. Defaults to localhost.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Snapserver control TCP port. Defaults to 1705.
    /// </summary>
    public int Port { get; set; } = 1705;

    /// <summary>
    /// Connection request timeout in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 5000;
}
