namespace Viox.Server.Configuration;

/// <summary>
/// Options for configuring ALSA equalizer and mixer controls based on system configuration.
/// </summary>
public sealed class AlsaControlOptions
{
    /// <summary>
    /// Configuration section name in application settings.
    /// </summary>
    public const string SectionName = "AlsaControl";

    /// <summary>
    /// Gets or sets the ALSA hardware card identifier (e.g., "hw:0,0" or "0").
    /// </summary>
    public string Card { get; set; } = "hw:0,0";

    /// <summary>
    /// Gets or sets the ALSA control device name used for alsaequal (e.g., "equal").
    /// </summary>
    public string EqualizerControlName { get; set; } = "equal";

    /// <summary>
    /// Gets or sets the ALSA mixer control name used for master volume (e.g., "Master").
    /// </summary>
    public string MasterVolumeControlName { get; set; } = "Master";

    /// <summary>
    /// Gets or sets the path to the alsaequal controls state binary file.
    /// </summary>
    public string EqualizerStateFile { get; set; } = "/data/alsaequal.bin";

    /// <summary>
    /// Gets or sets the timeout in milliseconds for executing system commands.
    /// </summary>
    public int CommandTimeoutMs { get; set; } = 3000;
}
