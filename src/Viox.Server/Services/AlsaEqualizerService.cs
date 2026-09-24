using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Viox.Server.Configuration;
using Viox.Server.Models;

namespace Viox.Server.Services;

/// <summary>
/// Service for managing ALSA equalizer (alsaequal) controls via amixer.
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

    public async Task<IReadOnlyList<EqualizerBand>> GetBandsAsync(CancellationToken cancellationToken = default)
    {
        string args = $"-D {_options.EqualizerControlName} scontents";
        string output = await _executor.ExecuteAsync("amixer", args, cancellationToken);

        var bands = new List<EqualizerBand>();
        string[] blocks = output.Split("Simple mixer control", StringSplitOptions.RemoveEmptyEntries);

        int index = 0;
        foreach (string block in blocks)
        {
            var nameMatch = ControlNameRegex().Match(block);
            if (!nameMatch.Success)
            {
                continue;
            }

            string fullControlName = nameMatch.Groups[1].Value;
            string frequencyLabel = nameMatch.Groups[2].Value.Trim();

            var percentMatches = PercentageRegex().Matches(block);
            if (percentMatches.Count == 0)
            {
                continue;
            }

            int leftPercent = int.Parse(percentMatches[0].Groups[1].Value);
            int rightPercent = percentMatches.Count > 1 
                ? int.Parse(percentMatches[1].Groups[1].Value) 
                : leftPercent;

            bands.Add(new EqualizerBand(index++, fullControlName, frequencyLabel, leftPercent, rightPercent));
        }

        _logger.LogInformation("Parsed {Count} equalizer bands from device '{Device}'.", bands.Count, _options.EqualizerControlName);
        return bands;
    }

    public async Task SetBandLevelAsync(int bandIndex, int percentage, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bandIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(percentage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);

        var bands = await GetBandsAsync(cancellationToken);
        if (bandIndex >= bands.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(bandIndex), $"Band index {bandIndex} exceeds available bands count ({bands.Count}).");
        }

        string controlName = bands[bandIndex].ControlName;
        await SetBandLevelByControlNameAsync(controlName, percentage, cancellationToken);
    }

    public async Task SetBandLevelByControlNameAsync(string controlName, int percentage, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(controlName);
        ArgumentOutOfRangeException.ThrowIfLessThan(percentage, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentage, 100);

        string args = $"-D {_options.EqualizerControlName} sset '{controlName}' {percentage}%";
        await _executor.ExecuteAsync("amixer", args, cancellationToken);

        _logger.LogInformation("Set equalizer control '{ControlName}' to {Percentage}%", controlName, percentage);
    }

    public async Task SetAllBandsAsync(IEnumerable<int> percentages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(percentages);

        var bandList = percentages.ToList();
        var currentBands = await GetBandsAsync(cancellationToken);

        if (bandList.Count > currentBands.Count)
        {
            throw new ArgumentException($"Provided percentage count ({bandList.Count}) exceeds available bands ({currentBands.Count}).", nameof(percentages));
        }

        for (int i = 0; i < bandList.Count; i++)
        {
            await SetBandLevelByControlNameAsync(currentBands[i].ControlName, bandList[i], cancellationToken);
        }
    }

    [GeneratedRegex(@"'((?:\d+\.\s*)?([^']+))',0")]
    private static partial Regex ControlNameRegex();

    [GeneratedRegex(@"\[(\d+)%\]")]
    private static partial Regex PercentageRegex();
}
