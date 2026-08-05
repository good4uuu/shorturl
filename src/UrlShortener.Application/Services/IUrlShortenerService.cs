using UrlShortener.Application.Contracts;

namespace UrlShortener.Application.Services;

public interface IUrlShortenerService
{
    Task<ShortUrlResponse> CreateAsync(
        string rawUrl,
        string baseUrl,
        CancellationToken cancellationToken
    );
    Task<string?> ResolveAsync(string code, CancellationToken cancellationToken);
    Task<LinkDetailResponse?> GetAsync(
        string code,
        string baseUrl,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyList<LinkDetailResponse>> GetRecentAsync(
        int limit,
        string baseUrl,
        CancellationToken cancellationToken
    );
}
