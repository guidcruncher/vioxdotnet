namespace Viox.Snapcast.Services;

using Viox.Snapcast.Models;

/// <summary>
/// Defines the asynchronous JSON-RPC client operations and events for a Snapcast server.
/// </summary>
public interface ISnapcastClient : IAsyncDisposable
{
    /// <summary>
    /// Connects to the configured Snapcast TCP server endpoint.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the Snapcast TCP server endpoint.
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the JSON-RPC version supported by the Snapcast server.
    /// </summary>
    Task<RpcVersion> GetRpcVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets full snapshot status of the Snapserver state including groups, clients, and streams.
    /// </summary>
    Task<SnapServer> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a flat list of all registered clients across all groups on the server.
    /// </summary>
    Task<List<SnapClient>> GetAllClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the volume percentage and mute state for a specified client ID.
    /// </summary>
    Task<VolumeState> SetClientVolumeAsync(string clientId, int percent, bool muted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the volume percentage and mute state for all clients across all groups concurrently.
    /// </summary>
    Task<Dictionary<string, VolumeState>> SetAllClientVolumesAsync(int percent, bool muted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the friendly display name of a client.
    /// </summary>
    Task<string> SetClientNameAsync(string clientId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the latency offset in milliseconds for a client.
    /// </summary>
    Task<int> SetClientLatencyAsync(string clientId, int latency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles mute status for a target client group.
    /// </summary>
    Task<bool> SetGroupMuteAsync(string groupId, bool mute, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns an active audio stream to a client group.
    /// </summary>
    Task<string> SetGroupStreamAsync(string groupId, string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns specific clients to a group.
    /// </summary>
    Task<List<string>> SetGroupClientsAsync(string groupId, IEnumerable<string> clientIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a disconnected client entry from the server inventory.
    /// </summary>
    Task DeleteClientAsync(string clientId, CancellationToken cancellationToken = default);

    // Notifications
    event EventHandler<ClientConnectEventArgs>? ClientConnected;
    event EventHandler<ClientDisconnectEventArgs>? ClientDisconnected;
    event EventHandler<ClientVolumeChangedEventArgs>? ClientVolumeChanged;
    event EventHandler<GroupMuteEventArgs>? GroupMuted;
    event EventHandler<GroupStreamChangedEventArgs>? GroupStreamChanged;
    event EventHandler<ServerUpdateEventArgs>? ServerUpdated;
}
