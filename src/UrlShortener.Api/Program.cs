using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Data;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<UrlShortenerDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortener")));
builder.Services.AddScoped<UrlShortenerService>();
builder.Services.AddSingleton<ShortCodeGenerator>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await DatabaseInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>());
}
app.UseCors();

string BaseUrl(HttpRequest request) => builder.Configuration["PublicBaseUrl"]?.TrimEnd('/') ?? $"{request.Scheme}://{request.Host}";
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/urls", async (CreateShortUrlRequest request, HttpRequest http, UrlShortenerService service, CancellationToken ct) =>
{
    var error = UrlShortenerService.Validate(request.Url);
    if (error is not null) return Results.BadRequest(new { error });
    try { return Results.Created("/api/urls", await service.CreateAsync(request.Url!, BaseUrl(http), ct)); }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch { return Results.Problem("Unable to create a short link. Please try again."); }
});
app.MapGet("/api/urls", async (int? limit, HttpRequest http, UrlShortenerService service, CancellationToken ct) => Results.Ok(await service.GetRecentAsync(limit ?? 10, BaseUrl(http), ct)));
app.MapGet("/api/urls/{shortCode}", async (string shortCode, HttpRequest http, UrlShortenerService service, CancellationToken ct) =>
    await service.GetAsync(shortCode, BaseUrl(http), ct) is { } result ? Results.Ok(result) : Results.NotFound(new { error = "The requested short link does not exist." }));
app.MapGet("/{shortCode}", async (string shortCode, UrlShortenerService service, CancellationToken ct) =>
{
    var destination = await service.ResolveAsync(shortCode, ct);
    return destination is null ? Results.NotFound(new { error = "The requested short link does not exist." }) : Results.Redirect(destination, permanent: false);
});
app.Run();
public partial class Program { }