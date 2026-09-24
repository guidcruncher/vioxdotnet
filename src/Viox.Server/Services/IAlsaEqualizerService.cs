namespace Viox.Server.Services;

/// <summary>
/// Service contract for controlling ALSA equalizer (alsaequal) settings.
/// </summary>
public interface IAlsaEqualizerService
{
    /// <summary>
    /// Gets the current equalizer band levels as percentage values (0 to 100).
    /// </summary>
    Task<IReadOnlyDictionary<int, int>> GetBandLevelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a specific equalizer band level.
    /// </summary>
    /// <param name="bandIndex">Index of the band to modify (0-indexed).</param>
    /// <param name="percentage">Level percentage (0 to 100).</param>
    Task SetBandLevelAsync(int bandIndex, int percentage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets all equalizer bands simultaneously.
    /// </summary>
    /// <param name="percentages">List of percentages corresponding to each band sequentially.</param>
    Task SetAllBandsAsync(IEnumerable<int> percentages, CancellationToken cancellationToken = default);
}
