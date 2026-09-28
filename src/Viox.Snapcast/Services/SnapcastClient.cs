using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Snapcast.Configuration;
using Viox.Snapcast.Models;

namespace Viox.Snapcast.Services;

public sealed class SnapcastClient : ISnapcastClient
{
    private readonly SnapcastOptions _options;
    private readonly ILogger<SnapcastClient> _logger;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonRpcResponse>> _pendingRequests = new();

    private TcpClient? _tcpClient;
    private Stream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private Task? _readLoopTask;
    private int _requestIdCounter;

    public event EventHandler<ClientConnectEventArgs>? ClientConnected;
    public event EventHandler<ClientDisconnectEventArgs>? ClientDisconnected;
    public event EventHandler<ClientVolumeChangedEventArgs>? ClientVolumeChanged;
    public event EventHandler<GroupMuteEventArgs>? GroupMuted;
    public event EventHandler<GroupStreamChangedEventArgs>? GroupStreamChanged;
    public event EventHandler<ServerUpdateEventArgs>? ServerUpdated;

    public SnapcastClient(IOptions<SnapcastOptions> options, ILogger<SnapcastClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_tcpClient is { Connected: true })
        {
            _logger.LogInformation("Snapcast client is already connected.");
            return;
        }

        _logger.LogInformation("Connecting to Snapcast server at {Host}:{Port}...", _options.Host, _options.Port);

        _tcpClient = new TcpClient();
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.TimeoutMilliseconds);

        await _tcpClient.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token).ConfigureAwait(false);

        _stream = _tcpClient.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true };

        _cts = new CancellationTokenSource();
        _readLoopTask = Task.Run(() => ListenLoopAsync(_cts.Token), _cts.Token);

        _logger.LogInformation("Successfully connected to Snapcast server.");
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disconnecting Snapcast client...");

        if (_cts != null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
        }

        _reader?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
        _tcpClient?.Dispose();

        if (_readLoopTask != null)
        {
            try { await _readLoopTask.ConfigureAwait(false); } catch (OperationCanceledException) { }
        }

        foreach (var tcs in _pendingRequests.Values)
        {
            tcs.TrySetCanceled(cancellationToken);
        }
        _pendingRequests.Clear();

        _logger.LogInformation("Snapcast client disconnected.");
    }

    public async Task<RpcVersion> GetRpcVersionAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync("Server.GetRPCVersion", null, cancellationToken).ConfigureAwait(false);
        return DeserializeResult<RpcVersion>(response);
    }

    public async Task<SnapServer> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync("Server.GetStatus", null, cancellationToken).ConfigureAwait(false);
        var container = DeserializeResult<ServerStatusContainer>(response);
        return container.Server;
    }

    public async Task<List<SnapClient>> GetAllClientsAsync(CancellationToken cancellationToken = default)
    {
        var server = await GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return server.Groups
            .SelectMany(group => group.Clients)
            .ToList();
    }

    public async Task<VolumeState> SetClientVolumeAsync(string clientId, int percent, bool muted, CancellationToken cancellationToken = default)
    {
        var payload = new { id = clientId, volume = new { percent, muted } };
        var response = await SendRequestAsync("Client.SetVolume", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("volume").Deserialize<VolumeState>()!;
    }

    public async Task<Dictionary<string, VolumeState>> SetAllClientVolumesAsync(int percent, bool muted, CancellationToken cancellationToken = default)
    {
        var clients = await GetAllClientsAsync(cancellationToken).ConfigureAwait(false);
        var tasks = clients.Select(async client =>
        {
            var volumeState = await SetClientVolumeAsync(client.Id, percent, muted, cancellationToken).ConfigureAwait(false);
            return (ClientId: client.Id, VolumeState: volumeState);
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToDictionary(r => r.ClientId, r => r.VolumeState);
    }

    public async Task<string> SetClientNameAsync(string clientId, string name, CancellationToken cancellationToken = default)
    {
        var payload = new { id = clientId, name };
        var response = await SendRequestAsync("Client.SetName", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("name").GetString()!;
    }

    public async Task<int> SetClientLatencyAsync(string clientId, int latency, CancellationToken cancellationToken = default)
    {
        var payload = new { id = clientId, latency };
        var response = await SendRequestAsync("Client.SetLatency", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("latency").GetInt32();
    }

    public async Task<bool> SetGroupMuteAsync(string groupId, bool mute, CancellationToken cancellationToken = default)
    {
        var payload = new { id = groupId, mute };
        var response = await SendRequestAsync("Group.SetMute", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("mute").GetBoolean();
    }

    public async Task<string> SetGroupStreamAsync(string groupId, string streamId, CancellationToken cancellationToken = default)
    {
        var payload = new { id = groupId, stream_id = streamId };
        var response = await SendRequestAsync("Group.SetStream", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("stream_id").GetString()!;
    }

    public async Task<List<string>> SetGroupClientsAsync(string groupId, IEnumerable<string> clientIds, CancellationToken cancellationToken = default)
    {
        var payload = new { id = groupId, clients = clientIds };
        var response = await SendRequestAsync("Group.SetClients", payload, cancellationToken).ConfigureAwait(false);
        var resultElement = response.Result ?? throw new InvalidOperationException("Missing RPC result payload.");
        return resultElement.GetProperty("clients").Deserialize<List<string>>()!;
    }

    public async Task DeleteClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var payload = new { id = clientId };
        await SendRequestAsync("Server.DeleteClient", payload, cancellationToken).ConfigureAwait(false);
    }

    private async Task<JsonRpcResponse> SendRequestAsync(string method, object? parameters, CancellationToken cancellationToken)
    {
        if (_writer == null || _tcpClient is not { Connected: true })
        {
            throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");
        }

        var id = Interlocked.Increment(ref _requestIdCounter);
        var request = new JsonRpcRequest
        {
            Id = id,
            Method = method,
            Params = parameters
        };

        var tcs = new TaskCompletionSource<JsonRpcResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[id] = tcs;

        var json = JsonSerializer.Serialize(request);
        _logger.LogDebug("Sending JSON-RPC request [{Id}]: {Json}", id, json);

        await _writer.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);

        using var ctr = cancellationToken.Register(() =>
        {
            if (_pendingRequests.TryRemove(id, out var pending))
            {
                pending.TrySetCanceled(cancellationToken);
            }
        });

        return await tcs.Task.ConfigureAwait(false);
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Snapcast read loop started.");
        try
        {
            while (!cancellationToken.IsCancellationRequested && _reader != null)
            {
                var line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line == null)
                {
                    _logger.LogWarning("Snapcast server closed the socket stream.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                _logger.LogDebug("Received JSON-RPC message: {Line}", line);

                try
                {
                    var response = JsonSerializer.Deserialize<JsonRpcResponse>(line);
                    if (response == null) continue;

                    if (response.Id.HasValue && _pendingRequests.TryRemove(response.Id.Value, out var tcs))
                    {
                        if (response.Error != null)
                        {
                            tcs.TrySetException(new InvalidOperationException($"Snapcast RPC Error ({response.Error.Code}): {response.Error.Message}"));
                        }
                        else
                        {
                            tcs.TrySetResult(response);
                        }
                    }
                    else if (!string.IsNullOrEmpty(response.Method))
                    {
                        DispatchNotification(response.Method, response.Params);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed parsing inbound JSON payload.");
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in Snapcast read loop.");
        }
        finally
        {
            _logger.LogInformation("Snapcast read loop stopped.");
        }
    }

    private void DispatchNotification(string method, JsonElement? parameters)
    {
        if (parameters == null) return;

        try
        {
            switch (method)
            {
                case "Client.OnConnect":
                    var conn = parameters.Value.Deserialize<ClientConnectEventArgs>();
                    if (conn != null) ClientConnected?.Invoke(this, conn);
                    break;
                case "Client.OnDisconnect":
                    var disconn = parameters.Value.Deserialize<ClientDisconnectEventArgs>();
                    if (disconn != null) ClientDisconnected?.Invoke(this, disconn);
                    break;
                case "Client.OnVolumeChanged":
                    var vol = parameters.Value.Deserialize<ClientVolumeChangedEventArgs>();
                    if (vol != null) ClientVolumeChanged?.Invoke(this, vol);
                    break;
                case "Group.OnMute":
                    var mute = parameters.Value.Deserialize<GroupMuteEventArgs>();
                    if (mute != null) GroupMuted?.Invoke(this, mute);
                    break;
                case "Group.OnStreamChanged":
                    var stream = parameters.Value.Deserialize<GroupStreamChangedEventArgs>();
                    if (stream != null) GroupStreamChanged?.Invoke(this, stream);
                    break;
                case "Server.OnUpdate":
                    var server = parameters.Value.Deserialize<ServerUpdateEventArgs>();
                    if (server != null) ServerUpdated?.Invoke(this, server);
                    break;
                default:
                    _logger.LogDebug("Unhandled Snapcast event method: {Method}", method);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed dispatching notification method {Method}", method);
        }
    }

    private static T DeserializeResult<T>(JsonRpcResponse response)
    {
        if (response.Result == null)
        {
            throw new InvalidOperationException("RPC result payload was empty.");
        }

        return response.Result.Value.Deserialize<T>()
               ?? throw new InvalidOperationException($"Failed to deserialize result into {typeof(T).Name}");
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}
