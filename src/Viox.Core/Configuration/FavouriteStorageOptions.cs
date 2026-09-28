namespace Viox.Core.Configuration;

/// <summary>
/// Configures options for the persistent favorites storage engine.
/// </summary>
public class FavoritesStorageOptions
{
    public const string SectionName = "FavoritesStorage";

    /// <summary>
    /// Gets or sets the file path where favorites are persisted on the Core.
    /// </summary>
    public string FilePath { get; set; } = "/data/favorites.json";

    /// <summary>
    /// Gets or sets a value indicating whether changes are saved to disk automatically.
    /// </summary>
    public bool AutoSave { get; set; } = true;
}
