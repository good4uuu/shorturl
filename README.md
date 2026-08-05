# ShortUrl

A responsive URL shortener using ASP.NET Core, Supabase PostgreSQL, EF Core, React, and TypeScript.

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
dotnet test tests/UrlShortener.UnitTests/UrlShortener.UnitTests.csproj
```

```powershell
cd src/UrlShortener.Web
npm run build
npm run test:e2e
```

The end-to-end test expects the API and Vite frontend to be running locally. Install its browser once with `npx playwright install`.

## API

- `POST /api/urls` with `{ "url": "https://example.com/long-path" }`
- `GET /api/urls?limit=10` for recent links
- `GET /api/urls/{shortCode}` for link details
- `GET /{shortCode}` to redirect and increment its visit count

## Notes

Supabase provides the managed PostgreSQL database. The database password and connection string remain server-side. Malicious-link scanning, authentication, billing, and team workspaces are deliberately out of scope for this assignment.