namespace Viox.Client.Spotify.Configuration;

public sealed class FileAuthStoreOptions
{
    public const string SectionName = "FileAuthStore";

    public string FilePath { get; set; } = "auth_token.json";
    public bool CreateDirectoryIfNotExists { get; set; } = true;
}
