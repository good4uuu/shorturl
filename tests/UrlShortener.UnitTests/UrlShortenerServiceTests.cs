using FluentAssertions;
using Moq;
using UrlShortener.Application.Models;
using UrlShortener.Application.Services;
using Xunit;

namespace UrlShortener.UnitTests;

public sealed class UrlShortenerServiceTests
{
    private readonly Mock<IShortenedUrlRepository> _repository = new();

    [Fact]
    public async Task CreateAsync_WhenUrlAlreadyExists_ReturnsTheExistingShortLink()
    {
        var existing = new ShortenedUrl
        {
            OriginalUrl = "https://example.com/a/long-path",
            ShortCode = "abc123X",
            CreatedAtUtc = DateTime.UtcNow,
            VisitCount = 3,
        };
        _repository
            .Setup(repository =>
                repository.FindByOriginalUrlAsync(
                    existing.OriginalUrl,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(existing);
        var service = CreateService();

        var result = await service.CreateAsync(
            existing.OriginalUrl,
            "https://sho.rt",
            CancellationToken.None
        );

        result.ShortCode.Should().Be(existing.ShortCode);
        result.ShortUrl.Should().Be("https://sho.rt/abc123X");
        _repository.Verify(
            repository =>
                repository.TryCreateAsync(It.IsAny<ShortenedUrl>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryAcceptsNewLink_ReturnsCreatedLink()
    {
        _repository
            .Setup(repository =>
                repository.FindByOriginalUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((ShortenedUrl?)null);
        _repository
            .Setup(repository =>
                repository.TryCreateAsync(It.IsAny<ShortenedUrl>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);
        var service = CreateService();

        var result = await service.CreateAsync(
            "https://example.com/a/very/long/path?item=12345",
            "https://sho.rt",
            CancellationToken.None
        );

        result.OriginalUrl.Should().Be("https://example.com/a/very/long/path?item=12345");
        result.ShortCode.Should().MatchRegex("^[a-zA-Z0-9]{7}$");
        result.ShortUrl.Should().StartWith("https://sho.rt/");
        _repository.Verify(
            repository =>
                repository.TryCreateAsync(
                    It.Is<ShortenedUrl>(link => link.OriginalUrl == result.OriginalUrl),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolveAsync_WhenLinkExists_RecordsItsVisitAndReturnsDestination()
    {
        var link = new ShortenedUrl
        {
            OriginalUrl = "https://example.com/destination",
            ShortCode = "abc123X",
            CreatedAtUtc = DateTime.UtcNow,
        };
        _repository
            .Setup(repository =>
                repository.FindByShortCodeAsync(link.ShortCode, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(link);
        _repository
            .Setup(repository => repository.RecordVisitAsync(link, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var destination = await service.ResolveAsync(link.ShortCode, CancellationToken.None);

        destination.Should().Be(link.OriginalUrl);
        _repository.Verify(
            repository => repository.RecordVisitAsync(link, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private UrlShortenerService CreateService() =>
        new(_repository.Object, new ShortCodeGenerator());
}
