using System.Buffers;
using System.Net.WebSockets;
using System.Text;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Librespot.Configuration;

namespace Viox.Client.Librespot.Events;

public class LibrespotWebSocketWorker : BackgroundService
{
    private readonly ILogger<LibrespotWebSocketWorker> _logger;
    private readonly LibrespotOptions _options;
    private readonly ILibrespotEventService _eventService;

    public LibrespotWebSocketWorker(
        ILogger<LibrespotWebSocketWorker> logger,
        IOptions<LibrespotOptions> options,
        ILibrespotEventService eventService)
    {
        _logger = logger;
        _options = options.Value;
        _eventService = eventService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Librespot WebSocket background worker starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var clientWebSocket = new ClientWebSocket();
                var uri = new Uri(_options.WebSocketUri);

                _logger.LogInformation("Connecting to go-librespot WebSocket at {Uri}", uri);
                await clientWebSocket.ConnectAsync(uri, stoppingToken);
                _logger.LogInformation("Successfully connected to go-librespot event stream.");

                await ReceiveLoopAsync(clientWebSocket, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "WebSocket connection error. Reconnecting in {Seconds} seconds...", _options.ReconnectDelaySeconds);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken stoppingToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                using var messageStream = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket close frame received from server.");
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return;
                    }

                    messageStream.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageJson = Encoding.UTF8.GetString(messageStream.ToArray());
                    _eventService.DispatchEvent(messageJson);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
