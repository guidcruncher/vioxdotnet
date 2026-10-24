using System.Text;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Viox.Client.Files.Configuration;
using Viox.Client.Files.Services;
using Viox.Core.Utilities;

namespace Viox.Tests.Core;

public class M3uPlaylistParserTests
{
    [Fact]
    public async Task ParseAsync_MapsExtendedInfoAndTrackUri()
    {
        const string playlist = """
            #EXTM3U
            #EXTINF:123 tvg-name="Fallback title" group-title="Album" tvg-logo="https://example.test/cover.png",Artist - Track title
            https://example.test/audio.mp3
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(playlist));
        using HttpClient httpClient = new();
        M3uPlaylistParser parser = CreateParser(httpClient);

        var tracks = await parser.ParseAsync("playlist-id", stream);

        var track = Assert.Single(tracks);
        Assert.Equal("Track title", track.Title);
        Assert.Equal("Artist", track.Artist);
        Assert.Equal("Album", track.Album);
        Assert.Equal("https://example.test/cover.png", track.ImageUrl);
        Assert.Equal(123, track.Duration);
        Assert.Equal("https://example.test/audio.mp3", track.Url);
        Assert.Equal("playlist", track.Uri!.Source);
        Assert.Equal("media", track.Uri.Type);
        Assert.Equal("playlist-id".ToXxHash64Hex(), track.Uri.Id);
        Assert.Equal(track.Url.ToXxHash64Hex(), track.Uri.SecondaryId);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task ParseAsync_UsesFilenameWhenExtendedInfoIsMissing()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("# comment\nrelative/song.flac\n"));
        using HttpClient httpClient = new();
        M3uPlaylistParser parser = CreateParser(httpClient);

        var tracks = await parser.ParseAsync("playlist-id", stream);

        Assert.Equal("song", Assert.Single(tracks).Title);
    }

    [Fact]
    public async Task ParseAsync_SkipsRelativeUrisWhenRequiredAndResetsMetadata()
    {
        const string playlist = """
            #EXTINF:120,Artist - skipped
            relative.mp3
            #EXTINF:90,Artist - kept
            https://example.test/kept.mp3
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(playlist));
        using HttpClient httpClient = new();
        M3uPlaylistParser parser = CreateParser(httpClient, requireAbsoluteUri: true);

        var tracks = await parser.ParseAsync("playlist-id", stream);

        var track = Assert.Single(tracks);
        Assert.Equal("kept", track.Title);
        Assert.Equal(90, track.Duration);
    }

    [Fact]
    public async Task ParseAsync_RejectsUnreadableStream()
    {
        using MemoryStream stream = new();
        stream.Dispose();
        using HttpClient httpClient = new();
        M3uPlaylistParser parser = CreateParser(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() => parser.ParseAsync("playlist-id", stream));
    }

    private static M3uPlaylistParser CreateParser(HttpClient httpClient, bool requireAbsoluteUri = false) =>
        new(
            httpClient,
            Options.Create(new M3uPlaylistParserOptions { RequireAbsoluteUri = requireAbsoluteUri }),
            NullLogger<M3uPlaylistParser>.Instance);
}
