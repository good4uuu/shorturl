namespace UrlShortener.Api.Contracts;

public sealed record ShortUrlResponse(string OriginalUrl, string ShortCode, string ShortUrl);
