# URL Shortener Agent Instructions

## Role

You are the engineering agent responsible for completing the URL Shortener technical assignment using C#, ASP.NET Core, React, and PostgreSQL.

Build a small, polished, testable, and explainable full-stack application. Prioritize correctness, simplicity, usability, and code clarity over unnecessary features.

## Required Technology Stack

Use:

- C#
- ASP.NET Core Web API
- React with TypeScript
- Tailwind CSS
- PostgreSQL
- Entity Framework Core
- FluentValidation
- xUnit
- Moq
- FluentAssertions
- Playwright
- Swagger / OpenAPI

Preferred deployment:

- ASP.NET Core API on Azure App Service or Docker
- React frontend on Azure Static Web Apps, Vercel, or Netlify
- Managed PostgreSQL

Do not replace this stack unless the existing repository already uses another approved stack or the user explicitly requests a change.

## Primary Objective

Build a web application that:

1. Accepts a long URL.
2. Validates the URL.
3. Generates a unique short code.
4. Stores the mapping in PostgreSQL.
5. Displays the short URL.
6. Redirects the short URL to the original destination.
7. Ensures the short URL is shorter than the original.
8. Includes xUnit tests.
9. Includes one Playwright flow.
10. Works on desktop and mobile.
11. Can be submitted through GitHub.

## Scope Order

Implement in this order:

1. Solution setup
2. Database schema
3. URL validation
4. Short-code generation
5. Create-URL API
6. Redirect endpoint
7. React landing page
8. Error handling
9. xUnit tests
10. Integration tests
11. Playwright test
12. Responsive improvements
13. README
14. Deployment
15. Optional enhancements

Do not start advanced features before the required end-to-end flow works.

## Recommended Project Structure

```text
src/
├── UrlShortener.Api/
├── UrlShortener.Application/
├── UrlShortener.Domain/
├── UrlShortener.Infrastructure/
└── UrlShortener.Web/

tests/
├── UrlShortener.UnitTests/
├── UrlShortener.IntegrationTests/
└── UrlShortener.E2ETests/
```

Keep controllers thin. Put business logic in application services.

For a small assignment, fewer projects are acceptable. Avoid unnecessary abstractions.

## Required Domain Rules

### URL Validation

- Trim input.
- Require a non-empty value.
- Limit length to 2,048 characters.
- Parse with `Uri.TryCreate`.
- Accept only HTTP and HTTPS.
- Reject malformed and relative URLs.
- Validate on the backend even when the frontend also validates.

Use FluentValidation or a dedicated validator.

### Short-Code Generation

- Use Base62.
- Default to 7 characters.
- Use `RandomNumberGenerator`.
- Do not use `Random`.
- Retry collisions.
- Add a PostgreSQL unique constraint.
- Handle concurrent unique violations.

### Duplicate URL Handling

Return the existing short URL when the normalized original URL already exists.

Document this decision.

### Short URL Length Rule

Construct the complete URL:

```csharp
var shortUrl =
    $"{baseUrl.TrimEnd('/')}/{shortCode}";
```

Reject when:

```csharp
shortUrl.Length >= originalUrl.Length
```

Return:

```text
This URL is already too short to shorten.
```

### Redirect Behaviour

- Extract the short code.
- Query by indexed `ShortCode`.
- Return 404 when not found.
- Increment `VisitCount`.
- Update `LastAccessedAtUtc`.
- Return HTTP 302.
- Do not expose database internals.

## Database Rules

Use EF Core migrations.

Required table:

```text
shortened_urls
```

Required columns:

```text
id
original_url
short_code
created_at_utc
```

Recommended columns:

```text
visit_count
last_accessed_at_utc
```

Required constraint:

```text
UNIQUE(short_code)
```

Recommended index:

```text
INDEX(original_url)
```

Do not use an in-memory collection as the final persistence layer.

## API Rules

### Create Endpoint

```http
POST /api/urls
```

Success:

```http
201 Created
```

Validation error:

```http
400 Bad Request
```

Unexpected error:

```http
500 Internal Server Error
```

Example success body:

```json
{
  "originalUrl": "https://example.com/a/long/path",
  "shortCode": "aB3xY7",
  "shortUrl": "https://sho.rt/aB3xY7"
}
```

Do not return stack traces.

### Redirect Endpoint

```http
GET /{shortCode}
```

Known code:

```http
302 Found
```

Unknown code:

```http
404 Not Found
```

## Frontend Rules

Use React with TypeScript and Tailwind CSS.

The landing page must include:

- Heading
- Supporting description
- Labelled URL input
- Shorten button
- Loading state
- Inline validation error
- Success result
- Copy button
- Open-link action

Do not place backend logic in the React application.

### Responsive Behaviour

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

### Accessibility

- Use a `<label>`
- Support Enter submission
- Add visible focus styles
- Use `aria-live`
- Maintain readable contrast
- Give buttons meaningful names
- Do not rely only on colour

## Testing Rules

### xUnit Unit Tests

Write tests for:

- Valid HTTP URL
- Valid HTTPS URL
- Empty URL
- Invalid URL
- FTP URL
- JavaScript URL
- Maximum length
- Base62 output
- Code length
- Duplicate URL reuse
- Collision retry
- Shorter-length rule

### Integration Tests

Use `WebApplicationFactory<Program>`.

Write tests for:

- Valid request returns 201
- Invalid request returns 400
- Missing body returns 400
- Duplicate URL returns existing mapping
- Database failure is controlled
- Unknown code returns 404
- Valid code returns redirect

Use PostgreSQL Testcontainers when possible.

### Playwright

Create at least one full flow:

```text
Open the landing page
Enter a valid long URL
Submit
See the generated short URL
Open the short URL
Confirm redirect
```

Optional:

- Invalid URL
- Copy button
- Mobile viewport

## Security Rules

At minimum:

- Accept only HTTP and HTTPS
- Validate all server-side input
- Keep connection strings server-side
- Use HTTPS
- Configure CORS narrowly
- Escape rendered values
- Limit request body size
- Add rate limiting only after core scope is complete
- Do not log sensitive query parameters unnecessarily
- Do not disable certificate validation

Document malicious-link scanning as a future enhancement.

## Performance Rules

- Redirect using one indexed lookup
- Increment counts atomically
- Keep redirect code minimal
- Use async EF Core methods
- Avoid loading full collections
- Select only required columns
- Do not block async calls

Production scaling discussion:

- Redis for popular redirects
- Async analytics events
- Multiple API instances
- CDN
- Rate limiting
- Branded short domain
- Dedicated analytics storage

## Error Messages

Use clear user-facing messages:

```text
Please enter a URL.
Please enter a valid HTTP or HTTPS URL.
The URL is too long.
This URL is already too short to shorten.
The requested short link does not exist.
Unable to create a short link. Please try again.
```

Do not expose PostgreSQL or EF Core exception details.

## Documentation Rules

README must contain:

1. Overview
2. Screenshots
3. Technology stack
4. Architecture
5. Setup requirements
6. Environment variables
7. Database migration commands
8. Development commands
9. Build commands
10. Unit-test commands
11. Integration-test commands
12. End-to-end test command
13. Deployment steps
14. API examples
15. Assumptions
16. Trade-offs
17. Future improvements

Expected backend commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet ef database update
dotnet run
```

Expected frontend commands:

```bash
npm install
npm run dev
npm run build
npm run lint
npx playwright test
```

## Git Rules

Before submission:

- Remove secrets
- Include configuration examples
- Add `.gitignore`
- Exclude `bin`
- Exclude `obj`
- Exclude `node_modules`
- Include EF Core migrations
- Use meaningful commits
- Verify a fresh clone builds
- Confirm tests pass
- Verify README commands

Recommended commits:

```text
chore: initialize solution
feat: add PostgreSQL URL schema
feat: add URL validation and short-code generation
feat: add shortening and redirect endpoints
feat: add responsive React landing page
test: add unit integration and end-to-end tests
docs: add setup and architecture guide
```

## Deployment Rules

Preferred:

```text
ASP.NET Core API -> Azure App Service or Docker
React frontend -> Azure Static Web Apps or Vercel
PostgreSQL -> Managed PostgreSQL
```

Steps:

1. Push to GitHub.
2. Create PostgreSQL database.
3. Configure the API connection string.
4. Configure the frontend API base URL.
5. Apply EF Core migrations.
6. Deploy the API.
7. Deploy the frontend.
8. Test shortening.
9. Test production redirect.
10. Confirm no secrets are exposed.

## Optional Enhancement Limit

After the core scope is complete, add at most two or three:

1. Duplicate URL detection
2. Click count
3. Recent links
4. QR code
5. GitHub Actions

Do not implement:

- Authentication
- Billing
- Team workspaces
- Advanced analytics
- Bulk processing
- Custom domains

unless explicitly requested.

## Interview Preparation

Be prepared to explain:

### Why ASP.NET Core

- Strong typing
- Mature dependency injection
- Clear API structure
- Good performance
- Matches the candidate's experience

### Why PostgreSQL

- Reliable relational storage
- Strong unique constraints
- Good concurrency behaviour
- Widely available managed hosting

### Why EF Core

- Strong C# integration
- Migrations
- LINQ querying
- Change tracking
- Easy testing and maintainability

### Why Base62

A 7-character Base62 code provides:

```text
62^7 = 3,521,614,606,208 combinations
```

The database unique constraint remains required.

### Concurrency

Two requests may generate the same code.

Correct handling:

1. Generate code.
2. Attempt insert.
3. PostgreSQL rejects one duplicate.
4. Catch the unique violation.
5. Generate another code.
6. Retry.

### Production Improvements

- Redis cache
- Rate limiting
- Malicious-link detection
- Async analytics
- Custom domain
- Observability
- Read replicas

## Definition of Done

- [ ] ASP.NET Core application builds
- [ ] React application builds
- [ ] PostgreSQL schema and migrations exist
- [ ] Valid URLs can be shortened
- [ ] Invalid URLs are rejected
- [ ] Generated URLs are shorter
- [ ] Short codes are unique
- [ ] Duplicate URLs are handled
- [ ] Redirect works
- [ ] Unknown codes return 404
- [ ] Copy action works
- [ ] Mobile layout works
- [ ] xUnit tests pass
- [ ] Integration tests pass
- [ ] Playwright flow passes
- [ ] README is accurate
- [ ] No secrets are committed
- [ ] Code is pushed to GitHub
- [ ] Production deployment works when included
