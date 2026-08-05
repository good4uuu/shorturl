using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"url-shortener-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<UrlShortenerDbContext>>();
            services.RemoveAll<UrlShortenerDbContext>();
            services.AddDbContext<UrlShortenerDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
            );
            services.AddScoped<IShortenedUrlRepository, ShortenedUrlRepository>();
        });
    }
}
