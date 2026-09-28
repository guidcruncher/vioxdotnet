using System.Net.WebSockets;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Librespot.Configuration;
using Viox.Client.Librespot.Events;

namespace Viox.Client.Librespot.Services;

/// <summary>
/// Real-time WebSocket client implementation for go-librespot event stream.
/// </summary>
public sealed class LibrespotWebSocketClient : ILibrespotWebSocketClient
{
    private readonly LibrespotOptions _options;
    private readonly ILogger<LibrespotWebSocketClient> _logger;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;

    public event Func<LibrespotEvent, Task>? OnEventReceived;
    public event Func<string, Task>? OnRawMessageReceived;
    public event Func<Exception, Task>? OnError;

    public bool IsConnected => _webSocket is { State: WebSocketState.Open };

    public LibrespotWebSocketClient(IOptions<LibrespotOptions> options, ILogger<LibrespotWebSocketClient> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _listenTask = Task.Run(() => ConnectionLoopAsync(_cts.Token), _cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null)
        {
            return;
        }

        await _cts.CancelAsync();

        if (_webSocket is not null && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while closing WebSocket gracefully.");
            }
        }

        if (_listenTask is not null)
        {
            try
            {
                await _listenTask;
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation
            }
        }

        _webSocket?.Dispose();
        _webSocket = null;
        _cts.Dispose();
        _cts = null;
    }

    private async Task ConnectionLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _webSocket = new ClientWebSocket();
                _logger.LogInformation("Connecting to Librespot WebSocket at '{Uri}'...", _options.WebSocketUrl);

                await _webSocket.ConnectAsync(new Uri(_options.WebSocketUrl), cancellationToken);
                _logger.LogInformation("Librespot WebSocket connected successfully.");

                await ReceiveLoopAsync(_webSocket, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Librespot WebSocket connection error.");
                if (OnError is not null)
                {
                    await OnError.Invoke(ex);
                }
            }

            if (!_options.EnableWebSocketAutoReconnect || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _logger.LogInformation("Reconnecting to Librespot WebSocket in 5 seconds...");
            await Task.Delay(5000, cancellationToken);
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server requested closure", cancellationToken);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var jsonMessage = Encoding.UTF8.GetString(ms.ToArray());

                if (OnRawMessageReceived is not null)
                {
                    await OnRawMessageReceived.Invoke(jsonMessage);
                }

                var typedEvent = LibrespotEventParser.Parse(jsonMessage);
                if (typedEvent is not null && OnEventReceived is not null)
                {
                    await OnEventReceived.Invoke(typedEvent);
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
