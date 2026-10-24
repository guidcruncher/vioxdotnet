using System.Text;

using Viox.Epg.Services;

namespace Viox.Tests.Core;

public class EpgParserTests
{
    [Theory]
    [InlineData("19700101000000 +0000", 0L)]
    [InlineData("19700101010000 +0100", 0L)]
    [InlineData("19700101000000", 0L)]
    [InlineData("not-a-date", 0L)]
    [InlineData("", 0L)]
    public void ToEpoch_ParsesXmltvDateOrReturnsZero(string input, long expected)
    {
        Assert.Equal(expected, EpgTimeHelper.ToEpoch(input));
    }

    [Fact]
    public async Task XmltvParser_ReadsChannelAndProgrammeFields()
    {
        const string xml = """
            <tv>
              <channel id="channel.one">
                <display-name>Channel One</display-name>
                <url>https://example.test/channel</url>
                <icon src="https://example.test/logo.png" />
              </channel>
              <programme start="19700101000000 +0000" stop="19700101003000 +0000" channel="channel.one">
                <title>News &amp; Weather</title>
                <sub-title>Evening</sub-title>
                <desc>Daily update</desc>
                <episode-num>season.1</episode-num>
                <icon src="https://example.test/programme.png" />
                <category>News</category>
                <category>Weather</category>
                <category> </category>
              </programme>
            </tv>
            """;
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
        List<Viox.Epg.Models.ChannelRecord> channels = [];
        List<Viox.Epg.Models.ProgrammeRecord> programmes = [];
        XmltvParser parser = new();

        await parser.ParseAsync(
            stream,
            channel =>
            {
                channels.Add(channel);
                return Task.CompletedTask;
            },
            programme =>
            {
                programmes.Add(programme);
                return Task.CompletedTask;
            });

        Viox.Epg.Models.ChannelRecord channel = Assert.Single(channels);
        Assert.Equal("channel.one", channel.Id);
        Assert.Equal("Channel One", channel.DisplayName);
        Assert.Equal("https://example.test/channel", channel.Url);
        Assert.Equal("https://example.test/logo.png", channel.IconUrl);

        Viox.Epg.Models.ProgrammeRecord programme = Assert.Single(programmes);
        Assert.Equal("channel.one", programme.ChannelId);
        Assert.Equal("News & Weather", programme.Title);
        Assert.Equal("Evening", programme.SubTitle);
        Assert.Equal("Daily update", programme.Description);
        Assert.Equal("season.1", programme.EpisodeNum);
        Assert.Equal("https://example.test/programme.png", programme.IconUrl);
        Assert.Equal(["News", "Weather"], programme.Categories);
        Assert.Equal(1800, programme.Stop - programme.Start);
    }

    [Fact]
    public async Task XmltvParser_PropagatesCancellation()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("<tv />"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        XmltvParser parser = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => parser.ParseAsync(
            stream,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            cancellation.Token));
    }
}
