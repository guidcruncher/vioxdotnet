using Microsoft.AspNetCore.Mvc;

using Viox.Client.Librespot.Models;
using Viox.Client.Librespot.Services;
using Viox.Client.Librespot.WebApi;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// API Controller providing HTTP endpoints for go-librespot player control and WebSocket state monitoring.
/// </summary>
[ApiController]
[Route("api/v1/players/librespot")]
[Tags("Librespot")]
[Produces("application/json")]
public sealed class LibrespotController(
    ILibrespotRestClient restClient,
    MediaMetaDataConverterResolver resolver,
    LibrespotEventState state) : ControllerBase
{
    private readonly ILibrespotRestClient _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
    private readonly LibrespotEventState _state = state ?? throw new ArgumentNullException(nameof(state));
    private readonly MediaMetaDataConverterResolver _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

    /// <summary>
    /// Gets the aggregated real-time state derived from WebSocket events.
    /// </summary>
    [HttpGet("state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetState()
    {
        return Ok(new
        {
            _state.IsActive,
            _state.IsPlaying,
            _state.CurrentContextUri,
            _state.CurrentTrackUri,
            _state.LastPlayOrigin,
            Position = new
            {
                PositionMs = _state.CurrentPositionMs,
                DurationMs = _state.CurrentDurationMs
            },
            Volume = new
            {
                Current = _state.Volume,
                Max = _state.MaxVolume
            },
            Modes = new
            {
                _state.ShuffleContext,
                _state.RepeatContext,
                _state.RepeatTrack
            },
            _state.CurrentTrack,
            _state.PendingTrack
        });
    }

    /// <summary>
    /// Gets the recent rolling window of WebSocket events.
    /// </summary>
    [HttpGet("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEvents()
    {
        return Ok(_state.GetRecentEvents());
    }

    /// <summary>
    /// Begins track or context playback.
    /// </summary>
    [HttpPost("play")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Play([FromBody] ApiPlayRequest request, CancellationToken cancellationToken)
    {
        await _restClient.PlayAsync(request, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Pauses playback.
    /// </summary>
    [HttpPost("pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Pause(CancellationToken cancellationToken)
    {
        await _restClient.PauseAsync(cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Resumes playback.
    /// </summary>
    [HttpPost("resume")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Resume(CancellationToken cancellationToken)
    {
        await _restClient.ResumeAsync(cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Seeks to a specific position in the active track.
    /// </summary>
    [HttpPost("seek")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seek([FromBody] ApiSeekRequest request, CancellationToken cancellationToken)
    {
        await _restClient.SeekAsync(request, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Toggles context shuffling.
    /// </summary>
    [HttpPost("shuffle")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetShuffleContext([FromBody] ApiShuffleContextRequest request, CancellationToken cancellationToken)
    {
        await _restClient.SetShuffleContextAsync(request, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Toggles context repeating.
    /// </summary>
    [HttpPost("repeat-context")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetRepeatContext([FromBody] ApiRepeatContextRequest request, CancellationToken cancellationToken)
    {
        await _restClient.SetRepeatContextAsync(request, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Toggles single track repeating.
    /// </summary>
    [HttpPost("repeat-track")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetRepeatTrack([FromBody] ApiRepeatTrackRequest request, CancellationToken cancellationToken)
    {
        await _restClient.SetRepeatTrackAsync(request, cancellationToken);
        return Ok();
    }
}
