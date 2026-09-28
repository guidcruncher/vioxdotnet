using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Tests.Spotify;

public class MediaUriParserTests
{
    [Theory]
    [InlineData("spotify:track:123abc", "spotify", "track", "123abc", null)]
    [InlineData("tunein:station:5678", "tunein", "station", "5678", null)]
    [InlineData("file:track:abc-hash", "file", "track", "abc-hash", null)]
    public void ParseMediaUri_WithValidFormats_ParsesCorrectly(
        string uri,
        string expectedSource,
        string expectedType,
        string expectedId,
        string? expectedSecondaryId)
    {
        var result = uri.ParseMediaUri();

        Assert.NotNull(result);
        Assert.Equal(expectedSource, result!.Source);
        Assert.Equal(expectedType, result.Type);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal(expectedSecondaryId, result.SecondaryId);
    }

    [Theory]
    [InlineData("spotify:track:123:secondary-456")]
    [InlineData("podverse:episode:999:episode-12345")]
    [InlineData("radiobrowser:station:xyz:station-abc")]
    public void ParseMediaUri_WithSecondaryId_ParsesAndRoundTrips(string uri)
    {
        var parsed = uri.ParseMediaUri();

        Assert.NotNull(parsed);
        Assert.NotNull(parsed!.SecondaryId);
        Assert.Equal(uri, parsed.ToString());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("spotify:track")]
    [InlineData("spotify:track:123:secondary:extra")]
    [InlineData("")]
    public void ParseMediaUri_WithInvalidFormats_ReturnsNull(string uri)
    {
        var result = uri.ParseMediaUri();

        Assert.Null(result);
    }

    [Theory]
    [InlineData("unknown-source:track:123")]
    [InlineData("spotify:invalid-type:123")]
    [InlineData("spotify:track:123:secondary:extra")]
    public void ParseMediaUri_WithInvalidSourceOrType_ReturnsNull(string uri)
    {
        var result = uri.ParseMediaUri();

        Assert.Null(result);
    }

    [Theory]
    [InlineData("spotify:track:abc123")]
    [InlineData("podverse:episode:episode-123:ep-456")]
    [InlineData("radiobrowser:station:station-789")]
    public void ParseMediaUriValue_ProducesIdenticalResult(string uri)
    {
        var result1 = uri.ParseMediaUri();
        var result2 = MediaUriParser.ParseMediaUriValue(uri);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Source, result2.Source);
        Assert.Equal(result1.Type, result2.Type);
        Assert.Equal(result1.Id, result2.Id);
        Assert.Equal(result1.SecondaryId, result2.SecondaryId);
    }

    [Fact]
    public void MediaUri_ToString_FormatsCorrectly()
    {
        var uri = new MediaUri
        {
            Source = "spotify",
            Type = "track",
            Id = "abc123"
        };

        Assert.Equal("spotify:track:abc123", uri.ToString());
    }

    [Fact]
    public void MediaUri_ToString_WithSecondaryId_FormatsCorrectly()
    {
        var uri = new MediaUri
        {
            Source = "podverse",
            Type = "episode",
            Id = "episode-999",
            SecondaryId = "season-5"
        };

        Assert.Equal("podverse:episode:episode-999:season-5", uri.ToString());
    }
}
