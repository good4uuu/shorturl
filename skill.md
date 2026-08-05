# URL Shortener Development Skill

## Purpose

This skill defines how to build, test, document, and deploy a production-quality URL shortener for the technical assignment.

The application must allow a user to enter a valid long URL, generate a shorter URL, display the result, and redirect the short URL to the original destination.

## Recommended Technology Stack

- Backend language: C#
- Backend framework: ASP.NET Core Web API
- Frontend: React with TypeScript
- Styling: Tailwind CSS
- Database: PostgreSQL
- ORM: Entity Framework Core
- Validation: FluentValidation or built-in ASP.NET Core validation
- Unit and integration tests: xUnit
- Mocking: Moq
- Assertions: FluentAssertions
- End-to-end tests: Playwright
- API documentation: Swagger / OpenAPI
- Hosting: Azure App Service, AWS, Render, Railway, or Docker
- Database hosting: Azure Database for PostgreSQL, AWS RDS, Neon, Supabase, or another managed PostgreSQL service

This stack is recommended because it matches a .NET developer profile while still demonstrating full-stack capability.

## Architecture

```text
React + TypeScript Frontend
          |
          v
ASP.NET Core Web API
          |
          v
Application Service
          |
          v
Entity Framework Core
          |
          v
PostgreSQL
```

Recommended solution structure:

```text
UrlShortener/
├── src/
│   ├── UrlShortener.Api/
│   ├── UrlShortener.Application/
│   ├── UrlShortener.Domain/
│   ├── UrlShortener.Infrastructure/
│   └── UrlShortener.Web/
├── tests/
│   ├── UrlShortener.UnitTests/
│   ├── UrlShortener.IntegrationTests/
│   └── UrlShortener.E2ETests/
├── README.md
├── feature.md
├── skill.md
└── agent.md
```

For a smaller assignment, the API, application logic, infrastructure, and domain can be placed in fewer projects. Avoid unnecessary overengineering.

## Core Data Model

```csharp
public sealed class ShortenedUrl
{
    public long Id { get; set; }

    public required string OriginalUrl { get; set; }

    public required string ShortCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastAccessedAtUtc { get; set; }

    public long VisitCount { get; set; }
}
```

Entity Framework Core configuration:

```csharp
public sealed class ShortenedUrlConfiguration
    : IEntityTypeConfiguration<ShortenedUrl>
{
    public void Configure(
        EntityTypeBuilder<ShortenedUrl> builder)
    {
        builder.ToTable("shortened_urls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalUrl)
            .HasColumnName("original_url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.ShortCode)
            .HasColumnName("short_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.LastAccessedAtUtc)
            .HasColumnName("last_accessed_at_utc");

        builder.Property(x => x.VisitCount)
            .HasColumnName("visit_count")
            .IsRequired();

        builder.HasIndex(x => x.ShortCode)
            .IsUnique();

        builder.HasIndex(x => x.OriginalUrl);
    }
}
```

## URL Validation

Accept only absolute HTTP and HTTPS URLs.

```csharp
public static bool IsValidUrl(string value)
{
    if (!Uri.TryCreate(
            value,
            UriKind.Absolute,
            out var uri))
    {
        return false;
    }

    return uri.Scheme == Uri.UriSchemeHttp
        || uri.Scheme == Uri.UriSchemeHttps;
}
```

Reject:

- Empty input
- Relative URLs
- `javascript:` URLs
- `ftp:` URLs
- Malformed URLs
- URLs longer than 2,048 characters

Example request validator:

```csharp
public sealed class CreateShortUrlRequestValidator
    : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Please enter a URL.")
            .MaximumLength(2048)
            .WithMessage("The URL is too long.")
            .Must(IsValidUrl)
            .WithMessage(
                "Please enter a valid HTTP or HTTPS URL.");
    }

    private static bool IsValidUrl(string value)
    {
        return Uri.TryCreate(
                   value,
                   UriKind.Absolute,
                   out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps);
    }
}
```

## Short-Code Generation

Use Base62 characters:

```text
abcdefghijklmnopqrstuvwxyz
ABCDEFGHIJKLMNOPQRSTUVWXYZ
0123456789
```

Recommended code length: 7 characters.

```csharp
using System.Security.Cryptography;

public sealed class ShortCodeGenerator
{
    private const string Alphabet =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 7)
    {
        var result = new char[length];

        for (var index = 0; index < length; index++)
        {
            result[index] =
                Alphabet[
                    RandomNumberGenerator.GetInt32(
                        Alphabet.Length)];
        }

        return new string(result);
    }
}
```

Generation process:

1. Generate a short code.
2. Attempt to insert the mapping.
3. Let the database unique constraint detect a collision.
4. Catch the unique-constraint exception.
5. Generate another code.
6. Retry a limited number of times.

Do not rely only on a prior existence check because concurrent requests may still generate the same code.

## Shortening Workflow

```text
Receive URL
    |
Validate request
    |
Trim and normalize URL
    |
Check existing mapping
    |
Generate Base62 code
    |
Build complete short URL
    |
Confirm it is shorter
    |
Store mapping
    |
Return result
```

Recommended duplicate behaviour:

- Return the existing short URL when the same normalized original URL already exists.

## Short URL Length Rule

The assignment requires the generated URL to be shorter than the original.

```csharp
var shortUrl =
    $"{baseUrl.TrimEnd('/')}/{shortCode}";

if (shortUrl.Length >= originalUrl.Length)
{
    throw new ValidationException(
        "This URL is already too short to shorten.");
}
```

The comparison must use the complete generated URL, not only the code.

## API Design

### Create Short URL

```http
POST /api/urls
Content-Type: application/json
```

Request:

```json
{
  "url": "https://www.example.com/products/category/item?id=12345"
}
```

Success response:

```json
{
  "originalUrl": "https://www.example.com/products/category/item?id=12345",
  "shortCode": "aB3xY7",
  "shortUrl": "https://sho.rt/aB3xY7"
}
```

Recommended status:

```http
201 Created
```

Validation response:

```json
{
  "error": "Please enter a valid HTTP or HTTPS URL."
}
```

Recommended status:

```http
400 Bad Request
```

### Redirect

```http
GET /aB3xY7
```

Response:

```http
302 Found
Location: https://www.example.com/products/category/item?id=12345
```

Use `302 Found` because the destination may change in a future version.

## Controller Example

```csharp
[ApiController]
[Route("api/urls")]
public sealed class UrlsController : ControllerBase
{
    private readonly IUrlShortenerService _service;

    public UrlsController(
        IUrlShortenerService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<CreateShortUrlResponse>>
        CreateAsync(
            CreateShortUrlRequest request,
            CancellationToken cancellationToken)
    {
        var baseUrl =
            $"{Request.Scheme}://{Request.Host}";

        var result =
            await _service.CreateAsync(
                request.Url,
                baseUrl,
                cancellationToken);

        return Created(
            result.ShortUrl,
            result);
    }
}
```

Redirect endpoint:

```csharp
[ApiController]
public sealed class RedirectController : ControllerBase
{
    private readonly IUrlShortenerService _service;

    public RedirectController(
        IUrlShortenerService service)
    {
        _service = service;
    }

    [HttpGet("/{shortCode}")]
    public async Task<IActionResult> RedirectAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var destination =
            await _service.ResolveAsync(
                shortCode,
                cancellationToken);

        if (destination is null)
        {
            return NotFound();
        }

        return Redirect(destination);
    }
}
```

## Frontend Requirements

The React frontend should include:

- Application title
- Short explanation
- Labelled URL input
- Shorten button
- Inline validation
- Loading state
- Success result
- Copy button
- Open-link action
- Responsive layout

Recommended UI states:

```text
Idle
Typing
Invalid
Submitting
Success
Server error
```

Example frontend API call:

```tsx
const response = await fetch(
  `${apiBaseUrl}/api/urls`,
  {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ url }),
  },
);
```

## Tailwind CSS Guidance

Desktop:

```text
[ Long URL input                         ] [ Shorten URL ]
```

Mobile:

```text
[ Long URL input                                      ]
[ Shorten URL                                         ]
```

Test at:

- 375 px
- 768 px
- 1,280 px

## Accessibility

- Use a visible `<label>`
- Support Enter to submit
- Maintain visible focus states
- Use `aria-live` for feedback
- Use sufficient contrast
- Do not communicate state by colour alone
- Ensure buttons have clear names

## Testing Strategy

### xUnit Unit Tests

Test:

- Valid HTTP URL
- Valid HTTPS URL
- Empty URL
- Invalid URL
- Unsupported protocol
- Maximum URL length
- Base62 output
- Short-code length
- Collision retry
- Duplicate URL reuse
- Shorter-length rule

Example:

```csharp
public sealed class UrlValidatorTests
{
    [Theory]
    [InlineData("https://example.com/path")]
    [InlineData("http://example.com/path")]
    public void ValidUrl_ShouldBeAccepted(
        string value)
    {
        var result =
            Uri.TryCreate(
                value,
                UriKind.Absolute,
                out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps);

        result.Should().BeTrue();
    }
}
```

### Integration Tests

Use `WebApplicationFactory<Program>`.

Test:

- Valid request returns 201
- Invalid request returns 400
- Duplicate URL returns existing mapping
- Unknown short code returns 404
- Existing code redirects correctly
- Database failure returns a controlled error

Use a disposable PostgreSQL test database or Testcontainers when possible.

### Playwright

Create at least one full flow:

```text
Open landing page
Enter a valid long URL
Submit
See generated short URL
Open short URL
Confirm redirect
```

## Security

At minimum:

- Accept only HTTP and HTTPS
- Validate all server-side input
- Use HTTPS
- Do not expose connection strings
- Use EF Core parameterization
- Limit input length
- Configure CORS narrowly
- Escape frontend output
- Add rate limiting if time permits
- Do not log sensitive query parameters unnecessarily

Document malicious-link scanning as a future enhancement.

## Performance

- Add a unique index on `ShortCode`
- Add an index on `OriginalUrl`
- Use async EF Core calls
- Perform one indexed redirect lookup
- Increment visit count atomically
- Avoid loading unnecessary columns
- Keep redirect logic minimal

For production scale:

- Add Redis caching
- Process analytics asynchronously
- Add rate limiting
- Use multiple application instances
- Add a branded short domain

## Deployment

Recommended options:

```text
ASP.NET Core API -> Azure App Service or Docker
React frontend -> Azure Static Web Apps, Vercel, or Netlify
PostgreSQL -> Azure Database for PostgreSQL, AWS RDS, Neon, or Supabase
```

Simpler single-host option:

```text
ASP.NET Core serves the built React application
PostgreSQL is hosted separately
```

## Required Documentation

README should include:

- Overview
- Screenshots
- Technology stack
- Architecture
- Setup
- Environment variables
- Database migrations
- Development commands
- Build commands
- Test commands
- Deployment notes
- API examples
- Assumptions
- Trade-offs
- Future improvements

## Completion Checklist

- [ ] Valid URL can be shortened
- [ ] Invalid URL is rejected
- [ ] Generated URL is shorter
- [ ] Short code is unique
- [ ] Mapping is stored in PostgreSQL
- [ ] Redirect works
- [ ] Result is displayed clearly
- [ ] Copy action works
- [ ] Mobile layout works
- [ ] xUnit tests pass
- [ ] Playwright flow passes
- [ ] EF Core migrations are included
- [ ] README is complete
- [ ] Application is pushed to GitHub
- [ ] Optional deployment works
