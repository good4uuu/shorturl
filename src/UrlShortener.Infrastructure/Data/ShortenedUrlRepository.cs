using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Models;
using UrlShortener.Application.Services;

namespace UrlShortener.Infrastructure.Data;

public sealed class ShortenedUrlRepository(UrlShortenerDbContext db) : IShortenedUrlRepository
{
    public Task<ShortenedUrl?> FindByOriginalUrlAsync(string originalUrl, CancellationToken cancellationToken) =>
        db.ShortenedUrls.AsNoTracking().FirstOrDefaultAsync(link => link.OriginalUrl == originalUrl, cancellationToken);

    public Task<ShortenedUrl?> FindByShortCodeAsync(string shortCode, CancellationToken cancellationToken) =>
        db.ShortenedUrls.FirstOrDefaultAsync(link => link.ShortCode == shortCode, cancellationToken);

    public async Task<IReadOnlyList<ShortenedUrl>> GetRecentAsync(int limit, CancellationToken cancellationToken) =>
        await db.ShortenedUrls.AsNoTracking().OrderByDescending(link => link.CreatedAtUtc).Take(limit).ToListAsync(cancellationToken);

    public async Task<bool> TryCreateAsync(ShortenedUrl link, CancellationToken cancellationToken)
    {
        db.ShortenedUrls.Add(link);
        try { await db.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return false; }
    }

    public async Task RecordVisitAsync(ShortenedUrl link, CancellationToken cancellationToken)
    {
        link.VisitCount++;
        link.LastAccessedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}