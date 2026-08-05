using UrlShortener.Api;
using UrlShortener.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await DatabaseInitializer.InitializeAsync(
        scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>()
    );
}

startup.Configure(app);
app.Run();

public partial class Program { }
