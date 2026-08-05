using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Contracts;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/urls")]
public sealed class UrlsController(IUrlShortenerService service, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShortUrlResponse>> CreateAsync(CreateShortUrlRequest request, CancellationToken cancellationToken)
    {
        var error = UrlShortenerService.Validate(request.Url);
        if (error is not null) return BadRequest(new { error });

        try
        {
            var result = await service.CreateAsync(request.Url!, BaseUrl(), cancellationToken);
            return CreatedAtAction(nameof(GetAsync), new { shortCode = result.ShortCode }, result);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch { return Problem("Unable to create a short link. Please try again."); }
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LinkDetailResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LinkDetailResponse>>> GetRecentAsync(int limit = 10, CancellationToken cancellationToken = default) =>
        Ok(await service.GetRecentAsync(limit, BaseUrl(), cancellationToken));

    [HttpGet("{shortCode}")]
    [ProducesResponseType<LinkDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkDetailResponse>> GetAsync(string shortCode, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(shortCode, BaseUrl(), cancellationToken);
        return result is null ? NotFound(new { error = "The requested short link does not exist." }) : Ok(result);
    }

    private string BaseUrl() => configuration["PublicBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";
}