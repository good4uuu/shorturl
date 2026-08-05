using UrlShortener.Application.Models;

namespace UrlShortener.Application.Services;

public interface IShortenedUrlRepository
{
    Task<ShortenedUrl?> FindByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken);
    Task<ShortenedUrl?> FindByShortCodeAsync(string shortCode, CancellationToken cancellationToken);
    Task<IReadOnlyList<ShortenedUrl>> GetRecentAsync(int limit, CancellationToken cancellationToken);
    Task<bool> TryCreateAsync(ShortenedUrl link, CancellationToken cancellationToken);
    Task RecordVisitAsync(ShortenedUrl link, CancellationToken cancellationToken);
}