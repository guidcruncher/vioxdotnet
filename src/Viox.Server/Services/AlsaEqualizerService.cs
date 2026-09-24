using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using Viox.Server.Configuration;

namespace Viox.Server.Services;

/// <summary>
/// Service for managing ALSA equalizer (alsaequal) settings via amixer.
/// </summary>
public partial class AlsaEqualizerService : IAlsaEqualizerService
{
    private readonly IAlsaProcessExecutor _executor;
    private readonly AlsaControlOptions _options;
    private readonly ILogger<AlsaEqualizerService> _logger;

    public AlsaEqualizerService(
        IAlsaProcessExecutor executor,
        IOptions<AlsaControlOptions> options,
        ILogger<AlsaEqualizerService> logger)
    {
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _executor = executor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetBandLevelsAsync(CancellationToken cancellationToken = default)
    {
        // Target the 'equal' control device defined in the alsa configuration
        string args = $"-D {_options.EqualizerControlName} sget '{_options.EqualizerControlName}'";
        string output = await _executor.ExecuteAsync("amixer", args, cancellationToken);

        var bandLevels = new Dictionary<int, int>();
        var matches = BandRegex().Matches(output);

        int bandIndex = 0;
        foreach (Match match in matches)
        {
            if (match.Success && int.TryParse(match.Groups[1].Value, out int percentage))
            {
                bandLevels[bandIndex++] = percentage;
            }
        }

        _logger.LogInformation("Retrieved {Count} equalizer band levels using device '{Device}'.", bandLevels.Count, _options.EqualizerControlName);
        return bandLevels;
    }

    public async Task SetBandLevelAsync(int bandIndex, int percentage, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bandIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(percentage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);

        string args = $"-D {_options.EqualizerControlName} sset '{_options.EqualizerControlName}' {bandIndex} {percentage}%";
        await _executor.ExecuteAsync("amixer", args, cancellationToken);

        _logger.LogInformation("Set equalizer band {BandIndex} to {Percentage}% on device '{Device}'", bandIndex, percentage, _options.EqualizerControlName);
    }

    public async Task SetAllBandsAsync(IEnumerable<int> percentages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(percentages);

        int index = 0;
        foreach (int percentage in percentages)
        {
            await SetBandLevelAsync(index++, percentage, cancellationToken);
        }
    }

    [GeneratedRegex(@"\[(\d+)%\]")]
    private static partial Regex BandRegex();
}
