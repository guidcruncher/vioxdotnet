using Viox.Server.Models;

namespace Viox.Server.Services;

/// <summary>
/// Service contract for controlling ALSA equalizer (alsaequal) settings.
/// </summary>
public interface IAlsaEqualizerService
{
    /// <summary>
    /// Gets all equalizer bands along with their control names, frequency labels, and channel percentages.
    /// </summary>
    Task<IReadOnlyList<EqualizerBand>> GetBandsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an equalizer band level by its 0-based band index.
    /// </summary>
    /// <param name="bandIndex">Index of the band to modify (0 to N-1).</param>
    /// <param name="percentage">Level percentage (0 to 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetBandLevelAsync(int bandIndex, int percentage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an equalizer band level directly by its ALSA control name (e.g., "00. 31 Hz").
    /// </summary>
    /// <param name="controlName">The full ALSA control name.</param>
    /// <param name="percentage">Level percentage (0 to 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetBandLevelByControlNameAsync(string controlName, int percentage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets all equalizer bands simultaneously.
    /// </summary>
    /// <param name="percentages">List of percentages corresponding to each band sequentially.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAllBandsAsync(IEnumerable<int> percentages, CancellationToken cancellationToken = default);
}
