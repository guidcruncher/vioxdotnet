namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Server.Abstraction;
using Viox.Server.Models;
using Viox.Snapcast.Models;
using Viox.Snapcast.Services;

/// <summary>
/// API endpoints for controlling media playback through the composite control surface.
/// </summary>
[ApiController]
[Route("api/v1/media-player")]
[Tags("Media Player")]
[Produces("application/json")]
public sealed class MediaPlayerController : ControllerBase
{
    private readonly IMediaPlayerControlSurface _controlSurface;
    private readonly ILogger<MediaPlayerController> _logger;
    private readonly ISnapcastClient _snapcastClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPlayerController"/> class.
    /// </summary>
    /// <param name="controlSurface">The composite media player control surface instance.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="snapcastClient">The injected Snapcast client instance.</param>
    public MediaPlayerController(
        IMediaPlayerControlSurface controlSurface,
        ILogger<MediaPlayerController> logger,
        ISnapcastClient snapcastClient)
    {
        _controlSurface = controlSurface ?? throw new ArgumentNullException(nameof(controlSurface));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _snapcastClient = snapcastClient ?? throw new ArgumentNullException(nameof(snapcastClient));
    }

    /// <summary>
    /// Initiates playback for a given media URI.
    /// </summary>
    /// <param name="request">Playback payload details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("play")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Play([FromBody] PlayRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _controlSurface.PlayAsync(request.Uri, cancellationToken);
            return Ok(new { Message = $"Playback started for '{request.Uri}'." });
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Playback request failed due to unsupported URI scheme.");
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Pauses current playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Pause(CancellationToken cancellationToken)
    {
        await _controlSurface.PauseAsync(cancellationToken);
        return Ok(new { Message = "Playback paused." });
    }

    /// <summary>
    /// Resumes current playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Resume(CancellationToken cancellationToken)
    {
        await _controlSurface.ResumeAsync(cancellationToken);
        return Ok(new { Message = "Playback resumed." });
    }

    /// <summary>
    /// Stops current playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Stop(CancellationToken cancellationToken)
    {
        await _controlSurface.StopAsync(cancellationToken);
        return Ok(new { Message = "Playback stopped." });
    }

    /// <summary>
    /// Seeks to a specific position in the currently playing item.
    /// </summary>
    /// <param name="request">Target seek position.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("seek")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seek([FromBody] SeekRequest request, CancellationToken cancellationToken)
    {
        await _controlSurface.SeekAsync(request.Position, cancellationToken);
        return Ok(new { Message = $"Seeked to position {request.Position}." });
    }

    /// <summary>
    /// Skips to the next track.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("next")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Next(CancellationToken cancellationToken)
    {
        await _controlSurface.NextAsync(cancellationToken);
        return Ok(new { Message = "Skipped to next track." });
    }

    /// <summary>
    /// Returns to the previous track.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("previous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Previous(CancellationToken cancellationToken)
    {
        await _controlSurface.PreviousAsync(cancellationToken);
        return Ok(new { Message = "Returned to previous track." });
    }

    /// <summary>
    /// Updates volume and mute state for all clients concurrently.
    /// </summary>
    [HttpPut("volume")]
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
    /// Gets current volume level percentage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("volume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVolume(CancellationToken cancellationToken)
    {
        var clients = await _snapcastClient.GetAllClientsAsync(cancellationToken);
        var client = clients.Find(a => a.Config.Name == "viox-net");

        if (client is null)
        {
            client = clients.FirstOrDefault();
        }

        if (client is null)
        {
            return NotFound();
        }

        return Ok(new { VolumePercent = client.Config.Volume.Percent, Muted = client.Config.Volume.Muted });
    }

    /// <summary>
    /// Gets the status of the currently active player engine adapter.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePlayer(CancellationToken cancellationToken)
    {
        IMediaPlayerAdapter? activePlayer = await _controlSurface.GetActivePlayerAsync(cancellationToken);
        if (activePlayer == null)
        {
            return Ok(new { Active = false, PlayerName = (string?)null });
        }

        return Ok(new { Active = true, PlayerName = activePlayer.Name });
    }

    /// <summary>
    /// Gets the current active Music Metadata
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("current")]
    [ProducesResponseType(typeof(PlaybackState), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentTrack(CancellationToken cancellationToken)
    {
        PlaybackState state = new();
        MediaMetaData? metaData = _controlSurface.GetCurrentTrack();
        IMediaPlayerAdapter? activePlayer = await _controlSurface.GetActivePlayerAsync(cancellationToken);

        if (activePlayer is not null)
        {
            state.ActiveBackend = activePlayer.Name;
            state.Playing = true;
            if (metaData is not null)
            {
                state.Track = metaData;
                double? position = await activePlayer.GetPlaybackPositionAsync(cancellationToken);
                state.Position = position ?? 0;
                state.IsLive = state.Track.Duration is not null;
            }
        }

        return Ok(state);
    }
}
