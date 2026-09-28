using Viox.Client.Librespot.Events;

namespace Viox.Client.Librespot.Services;

/// <summary>
/// Contract for managing real-time WebSocket connection and events stream from go-librespot.
/// </summary>
public interface ILibrespotWebSocketClient : IAsyncDisposable
{
    event Func<LibrespotEvent, Task>? OnEventReceived;
    event Func<string, Task>? OnRawMessageReceived;
    event Func<Exception, Task>? OnError;

    bool IsConnected { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
