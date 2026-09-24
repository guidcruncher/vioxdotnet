namespace Viox.Server.Services;

/// <summary>
/// Service contract for controlling ALSA mixer volume and mute states.
/// </summary>
public interface IAlsaMixerService
{
    /// <summary>
    /// Gets current volume level percentage for a specified control.
    /// </summary>
    Task<int> GetVolumeAsync(string? controlName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets volume percentage for a specified control.
    /// </summary>
    Task SetVolumeAsync(int percentage, string? controlName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current mute state for a specified control.
    /// </summary>
    Task<bool> IsMutedAsync(string? controlName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets mute status for a specified control.
    /// </summary>
    Task SetMuteAsync(bool mute, string? controlName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles the current mute state for a specified control.
    /// </summary>
    Task ToggleMuteAsync(string? controlName = null, CancellationToken cancellationToken = default);
}
