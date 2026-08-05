namespace UrlShortener.Application.Contracts;

public sealed record ShortUrlResponse(string OriginalUrl, string ShortCode, string ShortUrl);
