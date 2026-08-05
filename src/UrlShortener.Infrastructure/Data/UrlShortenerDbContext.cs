using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Models;

namespace UrlShortener.Infrastructure.Data;

public sealed class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
    : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls => Set<ShortenedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ShortenedUrl>();
        entity.ToTable("shortened_urls");
        entity.HasKey(x => x.Id);
        entity
            .Property(x => x.OriginalUrl)
            .HasColumnName("original_url")
            .HasMaxLength(2048)
            .IsRequired();
        entity.Property(x => x.ShortCode).HasColumnName("short_code").HasMaxLength(10).IsRequired();
        entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        entity.Property(x => x.LastAccessedAtUtc).HasColumnName("last_accessed_at_utc");
        entity.Property(x => x.VisitCount).HasColumnName("visit_count").IsRequired();
        entity.HasIndex(x => x.ShortCode).IsUnique();
        entity.HasIndex(x => x.OriginalUrl);
    }
}
