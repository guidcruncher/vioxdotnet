// SnapcastServiceCollectionExtensions.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Viox.Snapcast.Services;

/// <summary>
/// Background service responsible for establishing and managing the lifecycle connection
/// to the Snapcast server on application startup.
/// </summary>
public sealed class SnapcastConnectionHostedService : IHostedService
{
    private readonly ISnapcastClient _client;
    private readonly ILogger<SnapcastConnectionHostedService> _logger;

    public SnapcastConnectionHostedService(
        ISnapcastClient client,
        ILogger<SnapcastConnectionHostedService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting to Snapcast server...");
        try
        {
            await _client.ConnectAsync(cancellationToken);
            _logger.LogInformation("Successfully connected to Snapcast server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Snapcast server during startup.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting from Snapcast server...");
        try
        {
            await _client.DisconnectAsync(cancellationToken);
            _logger.LogInformation("Disconnected from Snapcast server successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while disconnecting from Snapcast server.");
        }
    }
}
