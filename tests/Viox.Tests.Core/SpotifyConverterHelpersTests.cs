using Viox.Client.Spotify.Models;
using Viox.Client.Spotify.Services;

namespace Viox.Tests.Core;

public class SpotifyConverterHelpersTests
{
    [Theory]
    [InlineData("2024", "year", "2024-01-01")]
    [InlineData("2024-03", "month", "2024-03-01")]
    [InlineData("2024-03-19", "day", "2024-03-19")]
    [InlineData("2024-03-19", "DAY", "2024-03-19")]
    public void ParseSpotifyDate_ParsesSupportedPrecision(string input, string precision, string expectedDate)
    {
        DateTimeOffset? actual = SpotifyConverterHelpers.ParseSpotifyDate(input, precision);

        Assert.Equal(DateTimeOffset.Parse($"{expectedDate}T00:00:00+00:00"), actual);
    }

    [Theory]
    [InlineData("2024-02-30", "day")]
    [InlineData("not-a-date", "year")]
    [InlineData("2024", "decade")]
    public void ParseSpotifyDate_ReturnsNullForInvalidDateOrPrecision(string input, string precision)
    {
        Assert.Null(SpotifyConverterHelpers.ParseSpotifyDate(input, precision));
    }

    [Fact]
    public void GetSpotifyImageUrl_SelectsImageWithLargestArea()
    {
        SpotifyImage[] images =
        [
            new() { Url = "square", Width = 300, Height = 300 },
            new() { Url = "landscape", Width = 600, Height = 200 },
            new() { Url = "small", Width = 64, Height = 64 }
        ];

        Assert.Equal("landscape", SpotifyConverterHelpers.GetSpotifyImageUrl(images));
    }

    [Fact]
    public void GetSpotifyImageUrl_HandlesMissingDimensionsAndBlankUrls()
    {
        SpotifyImage[] images =
        [
            new() { Url = "", Width = 900, Height = 900 },
            new() { Url = "width-only", Width = 160 },
            new() { Url = "height-only", Height = 320 }
        ];

        Assert.Equal("height-only", SpotifyConverterHelpers.GetSpotifyImageUrl(images));
        Assert.Empty(SpotifyConverterHelpers.GetSpotifyImageUrl(null));
    }

    [Fact]
    public void GetArtists_JoinsNonEmptyNames()
    {
        SpotifyArtist[] artists =
        [
            new() { Name = "First" },
            new() { Name = "" },
            new() { Name = "Second" }
        ];

        Assert.Equal("First, Second", SpotifyConverterHelpers.GetArtists(artists));
        Assert.Empty(SpotifyConverterHelpers.GetArtists(null));
    }
}
