using System.Security.Cryptography;

using Viox.Core.Utilities;

namespace Viox.Tests.Core;

public class CoreUtilitiesTests
{
    [Fact]
    public void Sha256Hex_MatchesKnownDigest()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", "abc".ToSha256Hex());
    }

    [Fact]
    public void Sha256Base64_MatchesKnownDigest()
    {
        Assert.Equal("ungWv48Bz+pBQUDeXa4iI7ADYaOWF3qctBD/YfIAFa0=", "abc".ToSha256Base64());
    }

    [Fact]
    public void XxHash64_IsDeterministicAndUsesExpectedFormat()
    {
        string first = "media".ToXxHash64Hex();
        string second = "media".ToXxHash64Hex();

        Assert.Equal(first, second);
        Assert.Matches("^[0-9a-f]{16}$", first);
    }

    [Fact]
    public void HashExtensions_RejectNullInput()
    {
        string? input = null;

        Assert.Throws<ArgumentNullException>(() => input!.ToSha256Hex());
        Assert.Throws<ArgumentNullException>(() => input!.ToSha256Base64());
        Assert.Throws<ArgumentNullException>(() => input!.ToXxHash64Hex());
    }

    [Theory]
    [InlineData(false, "7 8 9 10 11 *")]
    [InlineData(true, "7 8 9 * * *")]
    public void ToCron_FormatsExpectedFields(bool excludeDate, string expected)
    {
        var value = new DateTimeOffset(2024, 11, 10, 9, 8, 7, TimeSpan.Zero);

        Assert.Equal(expected, value.ToCron(excludeDate));
    }

    [Theory]
    [InlineData(true, 1_700_000_000_000L, "2023-11-14T22:13:20+00:00")]
    [InlineData(false, 1_700_000_000L, "2023-11-14T22:13:20+00:00")]
    public void FromEpoch_UsesSelectedUnit(bool useMilliseconds, long epoch, string expected)
    {
        Assert.Equal(DateTimeOffset.Parse(expected), epoch.FromEpoch(useMilliseconds));
    }

    [Fact]
    public void FromEpoch_DoubleRoundsToNearestInteger()
    {
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(2), 1.6.FromEpoch(useMilliseconds: false));
    }

    [Theory]
    [InlineData("")]
    [InlineData("A playlist name")]
    [InlineData("♫ 東京 — café")]
    public void CompactStringCipher_RoundTripsText(string original)
    {
        CompactStringCipher cipher = new();

        Assert.Equal(original, cipher.Decrypt(cipher.Encrypt(original)));
    }

    [Fact]
    public void CompactStringCipher_UsesRandomIvForEachEncryption()
    {
        CompactStringCipher cipher = new();

        Assert.NotEqual(cipher.Encrypt("same input"), cipher.Encrypt("same input"));
    }

    [Fact]
    public void CompactStringCipher_RejectsMalformedCiphertext()
    {
        CompactStringCipher cipher = new();

        Assert.Throws<FormatException>(() => cipher.Decrypt("not-base62!"));
        Assert.Throws<ArgumentException>(() => cipher.Decrypt("0"));
    }

    [Fact]
    public void UriGenerator_RoundTripsEmptyAndUnicodeText()
    {
        UriGenerator generator = new();

        Assert.Equal(string.Empty, generator.Decode(generator.Create(string.Empty)));
        Assert.Equal("♫ 東京 — café", generator.Decode(generator.Create("♫ 東京 — café")));
    }

    [Fact]
    public void UriGenerator_RejectsTamperedCiphertext()
    {
        UriGenerator generator = new();
        byte[] payload = Convert.FromBase64String(generator.Create("authenticated"));
        payload[12] ^= 1;

        Assert.ThrowsAny<CryptographicException>(() => generator.Decode(Convert.ToBase64String(payload)));
    }

    [Fact]
    public void UriGenerator_RejectsTooShortCiphertext()
    {
        UriGenerator generator = new();

        Assert.Throws<CryptographicException>(() => generator.Decode(Convert.ToBase64String(new byte[27])));
    }
}
