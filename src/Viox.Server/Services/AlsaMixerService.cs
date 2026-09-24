using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using Viox.Server.Configuration;

namespace Viox.Server.Services;

/// <summary>
/// Service for managing ALSA mixer controls via amixer.
/// </summary>
public partial class AlsaMixerService : IAlsaMixerService
{
    private readonly IAlsaProcessExecutor _executor;
    private readonly AlsaControlOptions _options;
    private readonly ILogger<AlsaMixerService> _logger;

    public AlsaMixerService(
        IAlsaProcessExecutor executor,
        IOptions<AlsaControlOptions> options,
        ILogger<AlsaMixerService> logger)
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _executor = executor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> GetVolumeAsync(string? controlName = null, CancellationToken cancellationToken = default)
    {
        string target = controlName ?? _options.MasterVolumeControlName;
        string output = await GetControlOutputAsync(target, cancellationToken);

        var match = VolumeRegex().Match(output);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int volume))
        {
            _logger.LogInformation("Control '{Control}' volume is at {Volume}%", target, volume);
            return volume;
        }

        _logger.LogWarning("Unable to parse volume level from output for control '{Control}'", target);
        throw new InvalidOperationException($"Could not determine volume for control '{target}'.");
    }

    public async Task SetVolumeAsync(int percentage, string? controlName = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(percentage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);

        string target = controlName ?? _options.MasterVolumeControlName;
        string args = $"-c {_options.Card} sset '{target}' {percentage}%";

        await _executor.ExecuteAsync("amixer", args, cancellationToken);
        _logger.LogInformation("Set volume for control '{Control}' to {Percentage}%", target, percentage);
    }

    public async Task<bool> IsMutedAsync(string? controlName = null, CancellationToken cancellationToken = default)
    {
        string target = controlName ?? _options.MasterVolumeControlName;
        string output = await GetControlOutputAsync(target, cancellationToken);

        var match = MuteRegex().Match(output);
        if (match.Success)
        {
            bool isMuted = match.Groups[1].Value.Equals("off", StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation("Control '{Control}' mute status: {Muted}", target, isMuted);
            return isMuted;
        }

        _logger.LogWarning("Unable to parse mute state from output for control '{Control}'", target);
        throw new InvalidOperationException($"Could not determine mute state for control '{target}'.");
    }

    public async Task SetMuteAsync(bool mute, string? controlName = null, CancellationToken cancellationToken = default)
    {
        string target = controlName ?? _options.MasterVolumeControlName;
        string stateArg = mute ? "mute" : "unmute";
        string args = $"-c {_options.Card} sset '{target}' {stateArg}";

        await _executor.ExecuteAsync("amixer", args, cancellationToken);
        _logger.LogInformation("Set mute for control '{Control}' to {MuteState}", target, stateArg);
    }

    public async Task ToggleMuteAsync(string? controlName = null, CancellationToken cancellationToken = default)
    {
        string target = controlName ?? _options.MasterVolumeControlName;
        string args = $"-c {_options.Card} sset '{target}' toggle";

        await _executor.ExecuteAsync("amixer", args, cancellationToken);
        _logger.LogInformation("Toggled mute state for control '{Control}'", target);
    }

    private async Task<string> GetControlOutputAsync(string controlName, CancellationToken cancellationToken)
    {
        string args = $"-c {_options.Card} sget '{controlName}'";
        return await _executor.ExecuteAsync("amixer", args, cancellationToken);
    }

    [GeneratedRegex(@"\[(\d+)%\]")]
    private static partial Regex VolumeRegex();

    [GeneratedRegex(@"\[(on|off)\]")]
    private static partial Regex MuteRegex();
}
