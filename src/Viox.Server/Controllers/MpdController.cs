using Microsoft.AspNetCore.Mvc;

using Viox.Client.Mpd.Services;
using Viox.Server.Models;

namespace Viox.Server.Controllers;

/// <summary>
/// Provides RESTful API endpoints for controlling the Music Player Daemon (MPD) instance.
/// </summary>
[ApiController]
[Route("api/v1/mpd")]
[Tags("MPD")]
[Produces("application/json")]
public class MpdController : ControllerBase
{
    private readonly IMpdClient _mpdClient;
    private readonly ILogger<MpdController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdController"/> class.
    /// </summary>
    /// <param name="mpdClient">The MPD client service instance.</param>
    /// <param name="logger">The logger instance.</param>
    public MpdController(IMpdClient mpdClient, ILogger<MpdController> logger)
    {
        _mpdClient = mpdClient;
        _logger = logger;
    }

    /// <summary>
    /// Establishes a TCP connection to the configured MPD server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating connection success.</returns>
    [HttpPost("connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConnectAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.ConnectAsync(cancellationToken);
        return Ok(new { Message = "Connected to MPD server successfully." });
    }

    /// <summary>
    /// Gracefully closes the connection to the MPD server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating disconnection success.</returns>
    [HttpPost("disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DisconnectAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.DisconnectAsync(cancellationToken);
        return Ok(new { Message = "Disconnected from MPD server." });
    }

    /// <summary>
    /// Starts or resumes audio playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating playback start.</returns>
    [HttpPost("play")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PlayAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.PlayAsync(cancellationToken);
        return Ok(new { Message = "Playback started." });
    }

    /// <summary>
    /// Clears current playlist, appends the specified media file or network stream URL, and begins playback.
    /// </summary>
    /// <param name="request">The request payload containing target media string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating media playback start.</returns>
    [HttpPost("play-file")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PlayFileOrUrlAsync([FromBody] MpdPlayRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileOrUrl))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Media Request",
                Detail = "The target FileOrUrl path cannot be null or empty."
            });
        }

        await _mpdClient.PlayFileOrUrlAsync(request.FileOrUrl, cancellationToken);
        return Ok(new { Message = $"Playing media: {request.FileOrUrl}" });
    }

    /// <summary>
    /// Pauses active playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating playback pause.</returns>
    [HttpPost("pause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PauseAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.PauseAsync(cancellationToken);
        return Ok(new { Message = "Playback paused." });
    }

    /// <summary>
    /// Stops active playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating playback stop.</returns>
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StopAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.StopAsync(cancellationToken);
        return Ok(new { Message = "Playback stopped." });
    }

    /// <summary>
    /// Advances playback to the next track in the queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating skip operation success.</returns>
    [HttpPost("next")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> NextAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.NextAsync(cancellationToken);
        return Ok(new { Message = "Skipped to next track." });
    }

    /// <summary>
    /// Returns playback to the previous track in the queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating skip operation success.</returns>
    [HttpPost("previous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PreviousAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.PreviousAsync(cancellationToken);
        return Ok(new { Message = "Skipped to previous track." });
    }

    /// <summary>
    /// Removes all songs from the active MPD queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A status response indicating playlist clear success.</returns>
    [HttpDelete("playlist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearPlaylistAsync(CancellationToken cancellationToken)
    {
        await _mpdClient.ClearPlaylistAsync(cancellationToken);
        return Ok(new { Message = "Active playlist cleared." });
    }

    /// <summary>
    /// Sends a raw text command to the MPD server and returns the unparsed output.
    /// </summary>
    /// <param name="request">The request payload containing command string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw text string returned by the MPD server.</returns>
    [HttpPost("command")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendCommandAsync([FromBody] MpdCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Command))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Command",
                Detail = "The Command parameter cannot be null or empty."
            });
        }

        string rawResponse = await _mpdClient.SendCommandAsync(request.Command, cancellationToken);
        return Ok(new { Command = request.Command, Response = rawResponse });
    }
}
