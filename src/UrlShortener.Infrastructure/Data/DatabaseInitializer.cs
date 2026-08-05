using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Infrastructure.Data;

public static class DatabaseInitializer
{
    private const string ScriptResourceName =
        "UrlShortener.Infrastructure.Data.Scripts.initialize_database.sql";

    public static async Task InitializeAsync(
        UrlShortenerDbContext context,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream =
            typeof(DatabaseInitializer).Assembly.GetManifestResourceStream(ScriptResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded SQL script '{ScriptResourceName}' was not found."
            );
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(cancellationToken);
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
