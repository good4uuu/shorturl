# Vercel deployment

Vercel hosts the React frontend. The ASP.NET Core API must be deployed separately because it needs a persistent server process and secure database credentials.

## 1. Deploy the API

Deploy `src/UrlShortener.Api` to a .NET host such as Render, Railway, or Azure App Service. Set these environment variables on the API host:

```text
ConnectionStrings__UrlShortener=Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require
PublicBaseUrl=https://shorturl-api-aalt.onrender.com
Cors__AllowedOrigins__0=https://YOUR_VERCEL_DOMAIN
```

`PublicBaseUrl` ensures newly created short links point to the deployed API, not `localhost`.

## 2. Deploy the frontend on Vercel

1. Push the repository to GitHub.
2. In Vercel, choose **Add New → Project** and import the repository.
3. Set **Root Directory** to `src/UrlShortener.Web`.
4. Add `VITE_API_URL` with the API URL from step 1, for example `https://shorturl-api-aalt.onrender.com`.
5. Deploy. Environment-variable changes require a redeploy.

The Vercel configuration is at `src/UrlShortener.Web/vercel.json`. Never add the Supabase database password to Vercel because only the API needs it.