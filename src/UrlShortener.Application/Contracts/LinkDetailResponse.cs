namespace UrlShortener.Application.Contracts;

public sealed record LinkDetailResponse(
    string OriginalUrl,
    string ShortCode,
    string ShortUrl,
    DateTime CreatedAtUtc,
    DateTime? LastAccessedAtUtc,
    long VisitCount
);
