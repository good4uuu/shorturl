namespace UrlShortener.Application.Models;

public sealed class ShortenedUrl
{
    public long Id { get; set; }
    public required string OriginalUrl { get; set; }
    public required string ShortCode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastAccessedAtUtc { get; set; }
    public long VisitCount { get; set; }
}
