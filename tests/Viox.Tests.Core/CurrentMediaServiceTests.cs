using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Tests.Spotify;

public class CurrentMediaServiceTests
{
    [Fact]
    public void CurrentMedia_InitiallyNull()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        Assert.Null(service.CurrentMedia);
    }

    [Fact]
    public void SetCurrentMedia_UpdatesCurrentMediaProperty()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        var media = new MediaMetaData
        {
            Album = "Test Album",
            Title = "Test Track",
            Artist = "Test Artist",
            Url = "/media/test.mp3",
            ImageUrl = "/images/test.png",
            Uri = new MediaUri { Source = "spotify", Type = "track", Id = "abc123" }
        };

        service.SetCurrentMedia(media);

        Assert.NotNull(service.CurrentMedia);
        Assert.Equal("Test Track", service.CurrentMedia!.Title);
        Assert.Equal("Test Artist", service.CurrentMedia.Artist);
    }

    [Fact]
    public void SetCurrentMedia_FiresMediaChangedEvent()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        var media = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image"
        };

        var eventFired = false;
        MediaMetaData? eventMedia = null;

        service.CurrentMediaChanged += (sender, args) =>
        {
            eventFired = true;
            eventMedia = args.CurrentMedia;
            return Task.CompletedTask;
        };

        service.SetCurrentMedia(media);

        Assert.True(eventFired);
        Assert.Equal(media, eventMedia);
    }

    [Fact]
    public void ClearCurrentMedia_SetsCurrentMediaToNull()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        var media = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image"
        };

        service.SetCurrentMedia(media);
        Assert.NotNull(service.CurrentMedia);

        service.ClearCurrentMedia();

        Assert.Null(service.CurrentMedia);
    }

    [Fact]
    public void ClearCurrentMedia_FiresMediaChangedEvent()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        var media = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image"
        };

        service.SetCurrentMedia(media);

        var clearEventFired = false;
        service.CurrentMediaChanged += (sender, args) =>
        {
            if (args.CurrentMedia is null)
            {
                clearEventFired = true;
            }
            return Task.CompletedTask;
        };

        service.ClearCurrentMedia();

        Assert.True(clearEventFired);
    }

    [Fact]
    public void SetCurrentMedia_WithNullValue_ClearsMedia()
    {
        var options = Options.Create(new CurrentMediaServiceOptions { EnableStateChangeLogging = false });
        var service = new CurrentMediaService(options, NullLogger<CurrentMediaService>.Instance);

        var media = new MediaMetaData
        {
            Album = "Album",
            Title = "Title",
            Artist = "Artist",
            Url = "/url",
            ImageUrl = "/image"
        };

        service.SetCurrentMedia(media);
        Assert.NotNull(service.CurrentMedia);

        service.SetCurrentMedia(null);

        Assert.Null(service.CurrentMedia);
    }
}
