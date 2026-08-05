using Xunit;
using UrlShortener.Api.Services;

namespace UrlShortener.UnitTests;

public sealed class UrlRulesTests
{
    [Theory]
    [InlineData("https://example.com/path")]
    [InlineData("http://example.com/path")]
    public void ValidHttpUrls_AreAccepted(string url) => Assert.Null(UrlShortenerService.Validate(url));

    [Theory]
    [InlineData("")]
    [InlineData("example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    public void InvalidUrls_AreRejected(string url) => Assert.NotNull(UrlShortenerService.Validate(url));

    [Fact]
    public void OverlongUrl_IsRejected() => Assert.Equal("The URL is too long.", UrlShortenerService.Validate("https://example.com/" + new string('a', 2040)));

    [Fact]
    public void ShortCode_IsSevenBase62Characters()
    {
        var code = new ShortCodeGenerator().Generate();
        Assert.Matches("^[a-zA-Z0-9]{7}$", code);
    }
}