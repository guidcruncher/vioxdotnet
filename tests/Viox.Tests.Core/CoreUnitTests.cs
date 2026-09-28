using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Core.Utilities;

namespace Viox.Tests.Core;

public class CoreUnitTests
{
    [Fact]
    public void ParseMediaUri_ReturnsExpectedValues()
    {
        MediaUri? result = "spotify:track:abc123".ParseMediaUri();

        Assert.NotNull(result);
        Assert.Equal("spotify", result!.Source);
        Assert.Equal("track", result.Type);
        Assert.Equal("abc123", result.Id);
        Assert.Null(result.SecondaryId);
    }

    [Fact]
    public void ParseMediaUri_WithSecondaryId_RoundTrips()
    {
        MediaUri? result = "podverse:episode:42:episode-9".ParseMediaUri();

        Assert.NotNull(result);
        Assert.Equal("podverse", result!.Source);
        Assert.Equal("episode", result.Type);
        Assert.Equal("42", result.Id);
        Assert.Equal("episode-9", result.SecondaryId);
        Assert.Equal("podverse:episode:42:episode-9", result.ToString());
    }

    [Fact]
    public void ParseMediaUri_WhenTypeIsInvalid_ReturnsNull()
    {
        Assert.Null("spotify:bogus:abc123".ParseMediaUri());
    }

    [Fact]
    public void UriGenerator_RoundTripsPlainText()
    {
        UriGenerator generator = new();
        const string original = "spotify:track:abc123";

        string protectedText = generator.Create(original);
        string decoded = generator.Decode(protectedText);

        Assert.NotEqual(original, protectedText);
        Assert.Equal(original, decoded);
    }

    [Fact]
    public void MediaMetaData_RawUri_UsesSecondaryIdWhenPresent()
    {
        MediaMetaData metadata = new()
        {
            Album = "Test Album",
            Title = "Nebula",
            Artist = "Sample Artist",
            Url = "/media/nebula.mp3",
            ImageUrl = "/images/nebula.png",
            Uri = new MediaUri
            {
                Source = "spotify",
                Type = "track",
                Id = "abc123",
                SecondaryId = "playlist-7"
            }
        };

        Assert.Equal("spotify:track:abc123:playlist-7", metadata.RawUri);
    }

    [Fact]
    public async Task FileScanner_ScansOnlySupportedExtensions()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"viox-core-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            string allowedMp3 = Path.Combine(tempDirectory, "track.mp3");
            string ignoredText = Path.Combine(tempDirectory, "notes.txt");
            string ignoredMarkdown = Path.Combine(tempDirectory, "readme.md");

            await File.WriteAllTextAsync(allowedMp3, "audio-bytes");
            await File.WriteAllTextAsync(ignoredText, "ignored");
            await File.WriteAllTextAsync(ignoredMarkdown, "ignored");

            FileScanner scanner = new(
                NullLogger<FileScanner>.Instance,
                Options.Create(new MediaScannerOptions
                {
                    DefaultDirectoryPath = tempDirectory,
                    SearchSubdirectories = false,
                    SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ".mp3"
                    }
                }));

            IReadOnlyList<MediaMetaData> results = await scanner.ScanDirectoryAsync();

            Assert.Single(results);
            Assert.Equal("track.mp3", results[0].Title);
            Assert.Equal(allowedMp3, results[0].Url);
            Assert.NotNull(results[0].Uri);
            Assert.Equal("file", results[0].Uri!.Source);
            Assert.Equal("track", results[0].Uri.Type);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task FileScanner_WhenDirectoryDoesNotExist_ReturnsEmptyList()
    {
        string missingDirectory = Path.Combine(Path.GetTempPath(), $"viox-core-missing-{Guid.NewGuid():N}");

        FileScanner scanner = new(
            NullLogger<FileScanner>.Instance,
            Options.Create(new MediaScannerOptions
            {
                DefaultDirectoryPath = missingDirectory,
                SearchSubdirectories = false,
                SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".mp3"
                }
            }));

        IReadOnlyList<MediaMetaData> results = await scanner.ScanDirectoryAsync();

        Assert.Empty(results);
    }
}
