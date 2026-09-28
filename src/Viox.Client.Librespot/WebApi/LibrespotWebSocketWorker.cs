using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Librespot.Events;
using Viox.Client.Librespot.Services;

namespace Viox.Client.Librespot.WebApi;

/// <summary>
/// Background worker service managing real-time event streaming into state storage.
/// </summary>
public sealed class LibrespotWebSocketWorker : BackgroundService
{
    private readonly ILibrespotWebSocketClient _webSocketClient;
    private readonly LibrespotEventState _eventState;
    private readonly ILogger<LibrespotWebSocketWorker> _logger;

    public LibrespotWebSocketWorker(
        ILibrespotWebSocketClient webSocketClient,
        LibrespotEventState eventState,
        ILogger<LibrespotWebSocketWorker> logger)
    {
        _webSocketClient = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));
        _eventState = eventState ?? throw new ArgumentNullException(nameof(eventState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _webSocketClient.OnEventReceived += HandleEventAsync;
        _webSocketClient.OnError += HandleErrorAsync;

        _logger.LogInformation("Starting Librespot WebSocket background worker...");
        await _webSocketClient.StartAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task HandleEventAsync(LibrespotEvent @event)
    {
        if (@event is UnknownLibrespotEvent unknown)
        {
            _logger.LogWarning("Unrecognized WebSocket event received: {EventType}", unknown.EventType);
        }
        else
        {
            _logger.LogDebug("Processing WebSocket event: {EventType}", @event.EventType);
        }

        _eventState.UpdateState(@event);
        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(Exception exception)
    {
        _logger.LogError(exception, "Librespot WebSocket connection error.");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _webSocketClient.OnEventReceived -= HandleEventAsync;
        _webSocketClient.OnError -= HandleErrorAsync;

        await _webSocketClient.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
