using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UrlShortener.Application.Contracts;
using Xunit;

namespace UrlShortener.IntegrationTests;

public sealed class UrlsApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public UrlsApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAsync_WithValidUrl_Returns201AndShortLink()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/urls",
            new CreateShortUrlRequest("https://example.com/a/very/long/path?item=12345")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ShortUrlResponse>();
        result.Should().NotBeNull();
        result!.ShortCode.Should().MatchRegex("^[a-zA-Z0-9]{7}$");
        result.ShortUrl.Should().EndWith($"/{result.ShortCode}");
    }

    [Fact]
    public async Task CreateAsync_WithUnsupportedScheme_Returns400()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/urls",
            new CreateShortUrlRequest("ftp://example.com/file")
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("valid HTTP or HTTPS");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateUrl_ReusesTheSameShortCode()
    {
        using var client = _factory.CreateClient();
        var request = new CreateShortUrlRequest(
            "https://example.com/a/very/long/path?duplicate=true"
        );

        var first = await client.PostAsJsonAsync("/api/urls", request);
        var second = await client.PostAsJsonAsync("/api/urls", request);

        var firstResult = await first.Content.ReadFromJsonAsync<ShortUrlResponse>();
        var secondResult = await second.Content.ReadFromJsonAsync<ShortUrlResponse>();
        secondResult!.ShortCode.Should().Be(firstResult!.ShortCode);
    }

    [Fact]
    public async Task RedirectAsync_WithKnownCode_Returns302AndDestination()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );
        var originalUrl = "https://example.com/a/very/long/path?redirect=true";
        var create = await client.PostAsJsonAsync(
            "/api/urls",
            new CreateShortUrlRequest(originalUrl)
        );
        var created = await create.Content.ReadFromJsonAsync<ShortUrlResponse>();

        var redirect = await client.GetAsync($"/{created!.ShortCode}");

        redirect.StatusCode.Should().Be(HttpStatusCode.Found);
        redirect.Headers.Location!.ToString().Should().Be(originalUrl);
    }

    [Fact]
    public async Task RedirectAsync_WithUnknownCode_Returns404()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

        var response = await client.GetAsync("/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
