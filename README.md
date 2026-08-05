# ShortUrl

A responsive URL shortener using ASP.NET Core, Supabase PostgreSQL, EF Core, React, and TypeScript.

## Live API

- API: https://shorturl-api-aalt.onrender.com
- Health check: https://shorturl-api-aalt.onrender.com/health
- Swagger UI: https://shorturl-api-aalt.onrender.com/swagger/index.html
- Frontend: https://shorten-url-blue.vercel.app
## Deployed application

The frontend at `https://shorten-url-blue.vercel.app` calls the Render API. Render must allow this exact Vercel origin through CORS:

```text
Cors__AllowedOrigins__0=https://shorten-url-blue.vercel.app
PublicBaseUrl=https://shorturl-api-aalt.onrender.com
``` 

After changing Render environment variables, save them and deploy the latest commit again.

## Architecture

```text
UrlShortener.Api            Controllers, Startup, HTTP concerns
UrlShortener.Application    Business rules, contracts, interfaces
UrlShortener.Infrastructure EF Core, repository, Supabase access
```

## Features

- HTTP/HTTPS validation with a 2,048-character limit
- Secure 7-character Base62 codes, collision retry, and duplicate-link reuse
- Supabase persistence, unique short codes, 302 redirects, and visit tracking
- Recent-link dashboard, copy action, open action, and local QR-code generation
- Unit tests, Playwright flow, and GitHub Actions build verification

## Setup

Set the Supabase connection string only in your terminal. Never commit the password.

```powershell
$env:ConnectionStrings__UrlShortener = 'Host=aws-0-ap-northeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.zkhrxgoqjthofhuosrhz;Password=YOUR_DATABASE_PASSWORD;SSL Mode=Require'
dotnet run --project src/UrlShortener.Api --urls http://localhost:5000
```

The API creates the `shortened_urls` table and indexes automatically on first successful connection.

In a second terminal:

```powershell
cd src/UrlShortener.Web
npm install
npm run dev
```

Open `http://localhost:5173`.

## Commands

```powershell
dotnet build src/UrlShortener.Api/UrlShortener.Api.csproj
dotnet test UrlShortener.sln
```

```powershell
cd src/UrlShortener.Web
npm run build
npm run test:e2e
```

The Playwright UI tests start Vite automatically and mock API responses. Install its browser once with `npx playwright install`.

## API documentation

When the API is running, open `http://localhost:5000/swagger` for Swagger UI and `http://localhost:5000/swagger/v1/swagger.json` for the OpenAPI document.

Database initialization uses an idempotent SQL script rather than EF Core migrations; see [database-initialization.md](database-initialization.md) for the rationale and trade-off.

## API

- `POST /api/urls` with `{ "url": "https://example.com/long-path" }`
- `GET /api/urls?limit=10` for recent links
- `GET /api/urls/{shortCode}` for link details
- `GET /{shortCode}` to redirect and increment its visit count

## Notes

Supabase provides the managed PostgreSQL database. The database password and connection string remain server-side. Malicious-link scanning, authentication, billing, and team workspaces are deliberately out of scope for this assignment.

