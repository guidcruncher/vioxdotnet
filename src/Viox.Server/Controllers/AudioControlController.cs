using Microsoft.AspNetCore.Mvc;

using Viox.Server.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Controller for managing ALSA audio equalizer bands and mixer volume settings.
/// </summary>
[ApiController]
[Route("api/v1/mixer/eq")]
[Produces("application/json")]
public sealed class AudioControlController : ControllerBase
{
    private readonly IAlsaEqualizerService _equalizerService;
    private readonly IAlsaMixerService _mixerService;
    private readonly ILogger<AudioControlController> _logger;

    public AudioControlController(
        IAlsaEqualizerService equalizerService,
        IAlsaMixerService mixerService,
        ILogger<AudioControlController> logger)
    {
        ArgumentNullException.ThrowIfNull(equalizerService);
        ArgumentNullException.ThrowIfNull(mixerService);
        ArgumentNullException.ThrowIfNull(logger);

        _equalizerService = equalizerService;
        _mixerService = mixerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets current equalizer band levels as percentage values (0-100).
    /// </summary>
    [HttpGet("equalizer/bands")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<int, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEqualizerBands(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET request received for equalizer band levels.");
        IReadOnlyDictionary<int, int> bands = await _equalizerService.GetBandLevelsAsync(cancellationToken);
        return Ok(bands);
    }

    /// <summary>
    /// Sets a specific equalizer band level by band index.
    /// </summary>
    /// <param name="bandIndex">Index of the equalizer band (0-indexed).</param>
    /// <param name="request">Request body containing percentage level (0-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("equalizer/bands/{bandIndex:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetEqualizerBand(
        [FromRoute] int bandIndex,
        [FromBody] SetLevelRequest request,
        CancellationToken cancellationToken)
    {
        if (bandIndex < 0)
        {
            return BadRequest("Band index must be non-negative.");
        }

        if (request.Percentage is < 0 or > 100)
        {
            return BadRequest("Percentage must be between 0 and 100.");
        }

        _logger.LogInformation("HTTP PUT request received for equalizer band {BandIndex} to {Percentage}%.", bandIndex, request.Percentage);
        await _equalizerService.SetBandLevelAsync(bandIndex, request.Percentage, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets all equalizer bands simultaneously.
    /// </summary>
    /// <param name="request">Request body containing sequential band level percentages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("equalizer/bands")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAllEqualizerBands(
        [FromBody] SetAllBandsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Percentages is null || !request.Percentages.Any())
        {
            return BadRequest("Percentages list cannot be empty.");
        }

        if (request.Percentages.Any(p => p is < 0 or > 100))
        {
            return BadRequest("All band percentages must be between 0 and 100.");
        }

        _logger.LogInformation("HTTP PUT request received for updating all equalizer bands.");
        await _equalizerService.SetAllBandsAsync(request.Percentages, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets current volume level percentage for the specified mixer control.
    /// </summary>
    /// <param name="controlName">Optional ALSA mixer control name (e.g. Master). Defaults to configured control.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("mixer/volume")]
    [ProducesResponseType(typeof(VolumeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVolume([FromQuery] string? controlName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET request received for mixer volume level.");
        int volume = await _mixerService.GetVolumeAsync(controlName, cancellationToken);
        return Ok(new VolumeResponse(controlName, volume));
    }

    /// <summary>
    /// Sets volume percentage for the specified mixer control.
    /// </summary>
    /// <param name="request">Request body containing level percentage and optional control name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("mixer/volume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetVolume(
        [FromBody] SetMixerVolumeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Percentage is < 0 or > 100)
        {
            return BadRequest("Percentage must be between 0 and 100.");
        }

        _logger.LogInformation("HTTP PUT request received for volume setting to {Percentage}%.", request.Percentage);
        await _mixerService.SetVolumeAsync(request.Percentage, request.ControlName, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets current mute status for the specified mixer control.
    /// </summary>
    /// <param name="controlName">Optional ALSA mixer control name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("mixer/mute")]
    [ProducesResponseType(typeof(MuteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMuteStatus([FromQuery] string? controlName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP GET request received for mute status.");
        bool isMuted = await _mixerService.IsMutedAsync(controlName, cancellationToken);
        return Ok(new MuteResponse(controlName, isMuted));
    }

    /// <summary>
    /// Sets mute status for the specified mixer control.
    /// </summary>
    /// <param name="request">Request body containing mute boolean and optional control name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("mixer/mute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetMute(
        [FromBody] SetMuteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP PUT request received to set mute state to {IsMuted}.", request.IsMuted);
        await _mixerService.SetMuteAsync(request.IsMuted, request.ControlName, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toggles the current mute state for the specified mixer control.
    /// </summary>
    /// <param name="request">Request body containing optional control name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("mixer/mute/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleMute(
        [FromBody] ToggleMuteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP POST request received to toggle mute state.");
        await _mixerService.ToggleMuteAsync(request.ControlName, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request DTO for setting a percentage level.
/// </summary>
public sealed record SetLevelRequest(int Percentage);

/// <summary>
/// Request DTO for setting all equalizer bands.
/// </summary>
public sealed record SetAllBandsRequest(IEnumerable<int> Percentages);

/// <summary>
/// Request DTO for setting volume level.
/// </summary>
public sealed record SetMixerVolumeRequest(int Percentage, string? ControlName = null);

/// <summary>
/// Request DTO for setting mute status.
/// </summary>
public sealed record SetMuteRequest(bool IsMuted, string? ControlName = null);

/// <summary>
/// Request DTO for toggling mute status.
/// </summary>
public sealed record ToggleMuteRequest(string? ControlName = null);

/// <summary>
/// Response DTO for volume GET requests.
/// </summary>
public sealed record VolumeResponse(string? ControlName, int Percentage);

/// <summary>
/// Response DTO for mute GET requests.
/// </summary>
public sealed record MuteResponse(string? ControlName, bool IsMuted);
