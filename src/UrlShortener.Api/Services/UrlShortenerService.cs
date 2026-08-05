using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public sealed class UrlShortenerService(UrlShortenerDbContext db, ShortCodeGenerator generator)
{
    public static string? Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Please enter a URL.";
        if (value.Trim().Length > 2048) return "The URL is too long.";
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? null : "Please enter a valid HTTP or HTTPS URL.";
    }

    public async Task<ShortUrlResponse> CreateAsync(string rawUrl, string baseUrl, CancellationToken ct)
    {
        var originalUrl = rawUrl.Trim();
        var existing = await db.ShortenedUrls.AsNoTracking().FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl, ct);
        if (existing is not null) return ToResponse(existing, baseUrl);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = generator.Generate();
            var shortUrl = $"{baseUrl.TrimEnd('/')}/{code}";
            if (shortUrl.Length >= originalUrl.Length) throw new ArgumentException("This URL is already too short to shorten.");
            db.ShortenedUrls.Add(new ShortenedUrl { OriginalUrl = originalUrl, ShortCode = code, CreatedAtUtc = DateTime.UtcNow, VisitCount = 0 });
            try { await db.SaveChangesAsync(ct); return new ShortUrlResponse(originalUrl, code, shortUrl); }
            catch (DbUpdateException) { db.ChangeTracker.Clear(); }
        }
        throw new InvalidOperationException("Unable to create a short link. Please try again.");
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken ct)
    {
        var link = await db.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortCode == code, ct);
        if (link is null) return null;
        link.VisitCount++;
        link.LastAccessedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return link.OriginalUrl;
    }

    public async Task<LinkDetailResponse?> GetAsync(string code, string baseUrl, CancellationToken ct) =>
        (await db.ShortenedUrls.AsNoTracking().FirstOrDefaultAsync(x => x.ShortCode == code, ct)) is { } link ? ToDetail(link, baseUrl) : null;

    public async Task<IReadOnlyList<LinkDetailResponse>> GetRecentAsync(int limit, string baseUrl, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 50);
        var links = await db.ShortenedUrls.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(limit).ToListAsync(ct);
        return links.Select(x => ToDetail(x, baseUrl)).ToList();
    }

    private static ShortUrlResponse ToResponse(ShortenedUrl link, string baseUrl) => new(link.OriginalUrl, link.ShortCode, $"{baseUrl.TrimEnd('/')}/{link.ShortCode}");
    private static LinkDetailResponse ToDetail(ShortenedUrl link, string baseUrl) => new(link.OriginalUrl, link.ShortCode, $"{baseUrl.TrimEnd('/')}/{link.ShortCode}", link.CreatedAtUtc, link.LastAccessedAtUtc, link.VisitCount);
}