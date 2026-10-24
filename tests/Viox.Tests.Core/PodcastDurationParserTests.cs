using Viox.Client.Podverse.Services;

namespace Viox.Tests.Core;

public class PodcastDurationParserTests
{
    [Theory]
    [InlineData("75", 75)]
    [InlineData("01:15", 75)]
    [InlineData("1:02:03", 3723)]
    [InlineData("2:01:02:03", 176523)]
    [InlineData("1.25", 1.25)]
    [InlineData(" 00:01:30 ", 90)]
    public void ParseToSeconds_ParsesSupportedFormats(string input, double expected)
    {
        Assert.Equal(expected, RssDurationParser.ParseToSeconds(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-1")]
    [InlineData("1:2:3:4:5")]
    [InlineData("1:abc")]
    [InlineData("not a duration")]
    public void TryParseToSeconds_RejectsInvalidFormats(string? input)
    {
        Assert.False(RssDurationParser.TryParseToSeconds(input, out double seconds));
        Assert.Equal(0, seconds);
        Assert.Equal(0, RssDurationParser.ParseToSeconds(input));
    }
}
