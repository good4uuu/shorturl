using UrlShortener.Application.Contracts;
using UrlShortener.Application.Models;

namespace UrlShortener.Application.Services;

public sealed class UrlShortenerService(IShortenedUrlRepository repository, ShortCodeGenerator generator) : IUrlShortenerService
{
    public static string? Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Please enter a URL.";
        if (value.Trim().Length > 2048) return "The URL is too long.";
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? null : "Please enter a valid HTTP or HTTPS URL.";
    }

    public async Task<ShortUrlResponse> CreateAsync(string rawUrl, string baseUrl, CancellationToken cancellationToken)
    {
        var originalUrl = rawUrl.Trim();
        var existing = await repository.FindByOriginalUrlAsync(originalUrl, cancellationToken);
        if (existing is not null) return ToResponse(existing, baseUrl);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = generator.Generate();
            var shortUrl = $"{baseUrl.TrimEnd('/')}/{code}";
            if (shortUrl.Length >= originalUrl.Length) throw new ArgumentException("This URL is already too short to shorten.");
            var link = new ShortenedUrl { OriginalUrl = originalUrl, ShortCode = code, CreatedAtUtc = DateTime.UtcNow, VisitCount = 0 };
            if (await repository.TryCreateAsync(link, cancellationToken)) return ToResponse(link, baseUrl);
        }
        throw new InvalidOperationException("Unable to create a short link. Please try again.");
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken cancellationToken)
    {
        var link = await repository.FindByShortCodeAsync(code, cancellationToken);
        if (link is null) return null;
        await repository.RecordVisitAsync(link, cancellationToken);
        return link.OriginalUrl;
    }

    public async Task<LinkDetailResponse?> GetAsync(string code, string baseUrl, CancellationToken cancellationToken) =>
        (await repository.FindByShortCodeAsync(code, cancellationToken)) is { } link ? ToDetail(link, baseUrl) : null;

    public async Task<IReadOnlyList<LinkDetailResponse>> GetRecentAsync(int limit, string baseUrl, CancellationToken cancellationToken) =>
        (await repository.GetRecentAsync(Math.Clamp(limit, 1, 50), cancellationToken)).Select(link => ToDetail(link, baseUrl)).ToList();

    private static ShortUrlResponse ToResponse(ShortenedUrl link, string baseUrl) => new(link.OriginalUrl, link.ShortCode, $"{baseUrl.TrimEnd('/')}/{link.ShortCode}");
    private static LinkDetailResponse ToDetail(ShortenedUrl link, string baseUrl) => new(link.OriginalUrl, link.ShortCode, $"{baseUrl.TrimEnd('/')}/{link.ShortCode}", link.CreatedAtUtc, link.LastAccessedAtUtc, link.VisitCount);
}