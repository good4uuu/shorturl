using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<UrlShortenerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UrlShortener"))
        );
        services.AddScoped<IShortenedUrlRepository, ShortenedUrlRepository>();
        return services;
    }
}
