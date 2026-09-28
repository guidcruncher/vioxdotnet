using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Viox.Client.Mpd.Services;

/// <summary>
/// Background service responsible for establishing an initial connection to the MPD server upon application startup.
/// </summary>
public sealed class MpdConnectionHostedService : IHostedService
{
    private readonly IMpdClient _mpdClient;
    private readonly ILogger<MpdConnectionHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdConnectionHostedService"/> class.
    /// </summary>
    /// <param name="mpdClient">The MPD client instance.</param>
    /// <param name="logger">The logger instance.</param>
    public MpdConnectionHostedService(IMpdClient mpdClient, ILogger<MpdConnectionHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(mpdClient);
        ArgumentNullException.ThrowIfNull(logger);

        _mpdClient = mpdClient;
        _logger = logger;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A task representing the startup connection attempt.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting automatic connection to MPD server on application startup...");

        try
        {
            await _mpdClient.ConnectAsync(cancellationToken);
            _logger.LogInformation("Successfully auto-connected to MPD server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-connect to MPD server during startup.");
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A task representing the disconnection attempt.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting from MPD server during application shutdown...");
        await _mpdClient.DisconnectAsync(cancellationToken);
    }
}
