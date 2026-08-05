using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
public sealed class RedirectController(IUrlShortenerService service) : ControllerBase
{
    [HttpGet("/{shortCode}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectAsync(
        string shortCode,
        CancellationToken cancellationToken
    )
    {
        var destination = await service.ResolveAsync(shortCode, cancellationToken);
        return destination is null
            ? NotFound(new { error = "The requested short link does not exist." })
            : Redirect(destination);
    }
}
