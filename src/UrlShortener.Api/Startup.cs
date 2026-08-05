using UrlShortener.Application.Services;
using UrlShortener.Infrastructure;

namespace UrlShortener.Api;

public sealed class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure(configuration);
        services.AddScoped<IUrlShortenerService, UrlShortenerService>();
        services.AddSingleton<ShortCodeGenerator>();
        services.AddControllers();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
    }

    public void Configure(WebApplication app)
    {
        app.UseCors();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();
    }
}