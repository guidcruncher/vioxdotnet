using System.Text.RegularExpressions;

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
        var args = new[] { "-D", _options.EqualizerControlName, "scontents" };
        string output = await _executor.ExecuteAsync("amixer", args, cancellationToken);

        var bands = new List<EqualizerBand>();
        string[] blocks = output.Split("Simple mixer control", StringSplitOptions.RemoveEmptyEntries);

        int index = 0;
        foreach (string block in blocks)
        {
            using var reader = new StringReader(block);
            string? firstLine = reader.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(firstLine))
            {
                continue;
            }

            // Extract the string enclosed in single quotes e.g. "09. 16 kHz" from "'09. 16 kHz',0"
            int firstQuote = firstLine.IndexOf('\'');
            int lastQuote = firstLine.LastIndexOf('\'');
            if (firstQuote == -1 || lastQuote == -1 || firstQuote == lastQuote)
            {
                continue;
            }

            string fullControlName = firstLine.Substring(firstQuote + 1, lastQuote - firstQuote - 1);

            // Derive frequency label (e.g., "16 kHz" from "09. 16 kHz")
            int periodIndex = fullControlName.IndexOf('.');
            string frequencyLabel = periodIndex != -1 && periodIndex + 1 < fullControlName.Length
                ? fullControlName[(periodIndex + 1)..].Trim()
                : fullControlName;

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

        // Pass controlName directly without surrounding shell quotes
        var args = new[]
        {
            "-D", _options.EqualizerControlName,
            "sset", controlName,
            $"{percentage}%"
        };

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

    [GeneratedRegex(@"\[(\d+)%\]")]
    private static partial Regex PercentageRegex();
}

