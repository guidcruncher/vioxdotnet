using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Server.Models;
using Viox.Snapcast.Models;
using Viox.Snapcast.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Provides Web API endpoints to control and retrieve state from a Snapcast server.
/// </summary>
[ApiController]
[Route("api/v1/mixer/snapcast")]
[Tags("Snapcast")]
[Produces("application/json")]
public class SnapcastController : ControllerBase
{
    private readonly ISnapcastClient _snapcastClient;
    private readonly ILogger<SnapcastController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapcastController"/> class.
    /// </summary>
    /// <param name="snapcastClient">The injected Snapcast client instance.</param>
    /// <param name="logger">The injected logger instance.</param>
    public SnapcastController(
        ISnapcastClient snapcastClient,
        ILogger<SnapcastController> logger)
    {
        _snapcastClient = snapcastClient ?? throw new ArgumentNullException(nameof(snapcastClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates connection to the Snapcast server.
    /// </summary>
    [HttpPost("connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating connection to Snapcast server...");
        await _snapcastClient.ConnectAsync(cancellationToken);
        _logger.LogInformation("Connected to Snapcast server successfully.");
        return Ok(new { Message = "Connected successfully." });
    }

    /// <summary>
    /// Disconnects from the Snapcast server.
    /// </summary>
    [HttpPost("disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DisconnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting from Snapcast server...");
        await _snapcastClient.DisconnectAsync(cancellationToken);
        _logger.LogInformation("Disconnected from Snapcast server successfully.");
        return Ok(new { Message = "Disconnected successfully." });
    }

    /// <summary>
    /// Retrieves the JSON-RPC version supported by the server.
    /// </summary>
    [HttpGet("rpc-version")]
    [ProducesResponseType(typeof(RpcVersion), StatusCodes.Status200OK)]
    public async Task<ActionResult<RpcVersion>> GetRpcVersionAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving Snapcast RPC version...");
        var rpcVersion = await _snapcastClient.GetRpcVersionAsync(cancellationToken);
        return Ok(rpcVersion);
    }

    /// <summary>
    /// Retrieves the full snapshot status of the Snapserver state.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(SnapServer), StatusCodes.Status200OK)]
    public async Task<ActionResult<SnapServer>> GetStatusAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving full Snapcast server status...");
        var status = await _snapcastClient.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Retrieves all registered clients across all groups.
    /// </summary>
    [HttpGet("clients")]
    [ProducesResponseType(typeof(List<SnapClient>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SnapClient>>> GetAllClientsAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving all Snapcast clients...");
        var clients = await _snapcastClient.GetAllClientsAsync(cancellationToken);
        return Ok(clients);
    }

    /// <summary>
    /// Updates volume and mute state for a specific client.
    /// </summary>
    [HttpPut("clients/{clientId}/volume")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VolumeState), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VolumeState>> SetClientVolumeAsync(
        [FromRoute] string clientId,
        [FromBody] SetVolumeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating volume for client {ClientId} to {Percent}% (Muted: {Muted})...", clientId, request.VolumePercent, request.Muted);
        var volumeState = await _snapcastClient.SetClientVolumeAsync(clientId, request.VolumePercent, request.Muted, cancellationToken);
        return Ok(volumeState);
    }

    /// <summary>
    /// Updates volume and mute state for all clients concurrently.
    /// </summary>
    [HttpPut("clients/volume")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Dictionary<string, VolumeState>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dictionary<string, VolumeState>>> SetAllClientVolumesAsync(
        [FromBody] SetVolumeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating volume for all clients to {Percent}% (Muted: {Muted})...", request.VolumePercent, request.Muted);
        var result = await _snapcastClient.SetAllClientVolumesAsync(request.VolumePercent, request.Muted, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the friendly name of a client.
    /// </summary>
    [HttpPut("clients/{clientId}/name")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> SetClientNameAsync(
        [FromRoute] string clientId,
        [FromBody] SetClientNameRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting name for client {ClientId} to {Name}...", clientId, request.Name);
        var updatedName = await _snapcastClient.SetClientNameAsync(clientId, request.Name, cancellationToken);
        return Ok(updatedName);
    }

    /// <summary>
    /// Updates the latency offset in milliseconds for a client.
    /// </summary>
    [HttpPut("clients/{clientId}/latency")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> SetClientLatencyAsync(
        [FromRoute] string clientId,
        [FromBody] SetClientLatencyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting latency for client {ClientId} to {Latency}ms...", clientId, request.Latency);
        var updatedLatency = await _snapcastClient.SetClientLatencyAsync(clientId, request.Latency, cancellationToken);
        return Ok(updatedLatency);
    }

    /// <summary>
    /// Deletes a disconnected client from the server inventory.
    /// </summary>
    [HttpDelete("clients/{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteClientAsync(
        [FromRoute] string clientId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting client {ClientId} from inventory...", clientId);
        await _snapcastClient.DeleteClientAsync(clientId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toggles mute status for a target client group.
    /// </summary>
    [HttpPut("groups/{groupId}/mute")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> SetGroupMuteAsync(
        [FromRoute] string groupId,
        [FromBody] SetGroupMuteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting mute state for group {GroupId} to {Mute}...", groupId, request.Mute);
        var isMuted = await _snapcastClient.SetGroupMuteAsync(groupId, request.Mute, cancellationToken);
        return Ok(isMuted);
    }

    /// <summary>
    /// Assigns an active audio stream to a client group.
    /// </summary>
    [HttpPut("groups/{groupId}/stream")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> SetGroupStreamAsync(
        [FromRoute] string groupId,
        [FromBody] SetGroupStreamRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting stream for group {GroupId} to {StreamId}...", groupId, request.StreamId);
        var updatedStreamId = await _snapcastClient.SetGroupStreamAsync(groupId, request.StreamId, cancellationToken);
        return Ok(updatedStreamId);
    }

    /// <summary>
    /// Assigns a set of clients to a group.
    /// </summary>
    [HttpPut("groups/{groupId}/clients")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<string>>> SetGroupClientsAsync(
        [FromRoute] string groupId,
        [FromBody] SetGroupClientsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning clients to group {GroupId}...", groupId);
        var assignedClients = await _snapcastClient.SetGroupClientsAsync(groupId, request.ClientIds, cancellationToken);
        return Ok(assignedClients);
    }
}
