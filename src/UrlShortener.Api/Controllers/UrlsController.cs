using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Contracts;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/urls")]
public sealed class UrlsController(
    IUrlShortenerService service,
    IConfiguration configuration,
    ILogger<UrlsController> logger
) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ShortUrlResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShortUrlResponse>> CreateAsync(
        CreateShortUrlRequest request,
        CancellationToken cancellationToken
    )
    {
        var error = UrlShortenerService.Validate(request.Url);
        if (error is not null)
        {
            logger.LogWarning("Short-link creation rejected: {Error}", error);
            return BadRequest(new { error });
        }

        try
        {
            var result = await service.CreateAsync(request.Url!, BaseUrl(), cancellationToken);
            logger.LogInformation("Short link created: {ShortCode}", result.ShortCode);
            return Created($"/api/urls/{result.ShortCode}", result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Short-link creation rejected by the service");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Short-link creation failed");
            return Problem("Unable to create a short link. Please try again.");
        }
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LinkDetailResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LinkDetailResponse>>> GetRecentAsync(
        int limit = 10,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Loading recent links. Limit: {Limit}", limit);
        return Ok(await service.GetRecentAsync(limit, BaseUrl(), cancellationToken));
    }

    [HttpGet("{shortCode}")]
    [ProducesResponseType<LinkDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LinkDetailResponse>> GetAsync(
        string shortCode,
        CancellationToken cancellationToken
    )
    {
        var result = await service.GetAsync(shortCode, BaseUrl(), cancellationToken);
        if (result is null)
        {
            logger.LogInformation("Short link not found: {ShortCode}", shortCode);
            return NotFound(new { error = "The requested short link does not exist." });
        }

        return Ok(result);
    }

    private string BaseUrl()
    {
        var configuredBaseUrl = configuration["PublicBaseUrl"];
        return string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? $"{Request.Scheme}://{Request.Host}"
            : configuredBaseUrl.TrimEnd('/');
    }
}
