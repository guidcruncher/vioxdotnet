using Viox.Core.Models;

namespace Viox.Tests.Spotify;

public class MediaModelTests
{
    [Fact]
    public void MediaUri_Record_SupportsEquality()
    {
        var uri1 = new MediaUri { Source = "spotify", Type = "track", Id = "abc123" };
        var uri2 = new MediaUri { Source = "spotify", Type = "track", Id = "abc123" };

        Assert.Equal(uri1, uri2);
    }

    [Fact]
    public void MediaUri_Record_DifferentIds_AreNotEqual()
    {
        var uri1 = new MediaUri { Source = "spotify", Type = "track", Id = "abc123" };
        var uri2 = new MediaUri { Source = "spotify", Type = "track", Id = "xyz789" };

        Assert.NotEqual(uri1, uri2);
    }

    [Fact]
    public void MediaUri_ToString_ProducesValidFormat()
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
    public void MediaUri_WithSecondaryId_ToString_IncludesSecondaryId()
    {
        var uri = new MediaUri
        {
            Source = "podverse",
            Type = "episode",
            Id = "episode-42",
            SecondaryId = "season-1"
        };

        Assert.Equal("podverse:episode:episode-42:season-1", uri.ToString());
    }

    [Fact]
    public void MediaMetaData_RawUri_ReturnsEmptyStringWhenUriIsNull()
    {
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image",
            Uri = null
        };

        Assert.Empty(metadata.RawUri);
    }

    [Fact]
    public void MediaMetaData_RawUri_ReturnsFormattedUriWithoutSecondaryId()
    {
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image",
            Uri = new MediaUri
            {
                Source = "spotify",
                Type = "track",
                Id = "abc123"
            }
        };

        Assert.Equal("spotify:track:abc123", metadata.RawUri);
    }

    [Fact]
    public void MediaMetaData_RawUri_ReturnsFormattedUriWithSecondaryId()
    {
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image",
            Uri = new MediaUri
            {
                Source = "podverse",
                Type = "episode",
                Id = "episode-42",
                SecondaryId = "season-1"
            }
        };

        Assert.Equal("podverse:episode:episode-42:season-1", metadata.RawUri);
    }

    [Fact]
    public void MediaMetaData_DefaultProperties_AreSet()
    {
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image"
        };

        Assert.False(metadata.Favourite);
        Assert.Null(metadata.Duration);
        Assert.Null(metadata.ReleaseDate);
        Assert.Null(metadata.Uri);
    }

    [Fact]
    public void MediaMetaData_FavouriteFlag_CanBeSet()
    {
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image",
            Favourite = true
        };

        Assert.True(metadata.Favourite);
    }

    [Fact]
    public void MediaMetaData_DurationAndReleaseDate_CanBeSet()
    {
        var releaseDate = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var metadata = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image",
            Duration = 180.5,
            ReleaseDate = releaseDate
        };

        Assert.Equal(180.5, metadata.Duration);
        Assert.Equal(releaseDate, metadata.ReleaseDate);
    }
}
