namespace Viox.Client.Files.Configuration;

using System;
using System.Collections.Generic;

public class MediaScannerOptions
{
    public const string SectionName = "MediaScanner";

    public string DefaultDirectoryPath { get; set; } = "/music/files";

    public bool SearchSubdirectories { get; set; } = true;

    public HashSet<string> SupportedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv",
        ".mp3", ".flac", ".wav", ".aac",
        ".jpg", ".jpeg", ".png", ".webp"
    };
}
