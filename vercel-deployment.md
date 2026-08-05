# Vercel deployment

Vercel hosts the React frontend. The ASP.NET Core API is deployed separately on Render because it needs a persistent server process and secure database credentials.

## Live application

- Frontend: https://shorten-url-blue.vercel.app
- API: https://shorturl-api-aalt.onrender.com
- Swagger UI: https://shorturl-api-aalt.onrender.com/swagger/index.html

## 1. Configure Render API

In Render, open the `shorturl-api` service, select **Environment**, and add these values:

```text
ConnectionStrings__UrlShortener=Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require
PublicBaseUrl=https://shorturl-api-aalt.onrender.com
Cors__AllowedOrigins__0=https://shorten-url-blue.vercel.app
```

Do not add a trailing `/` to either URL. The database connection string stays in Render only; never commit it or add it to Vercel.

After saving environment variables, use **Manual Deploy → Deploy latest commit**. Render must redeploy before the CORS change is active.

### CORS error

If the browser console says `blocked by CORS policy` or `No Access-Control-Allow-Origin header`, the Vercel origin does not exactly match `Cors__AllowedOrigins__0`.

1. Copy the deployed Vercel URL from the browser address bar.
2. Set `Cors__AllowedOrigins__0` to that exact origin, for example `https://shorten-url-blue.vercel.app`.
3. Save the variable and redeploy Render.
4. Refresh the frontend and retry.

## 2. Deploy frontend on Vercel

1. Push the repository to GitHub.
2. In Vercel, choose **Add New → Project** and import the repository.
3. Set **Framework Preset** to **Vite**.
4. Set **Root Directory** to `src/UrlShortener.Web`.
5. Add this environment variable:

   ```text
   VITE_API_URL=https://shorturl-api-aalt.onrender.com
   ```

6. Deploy. Environment-variable changes require another deployment.

Vercel automatically detects the Vite `package.json`; no `vercel.json` file is required.

## Diagnostics

- Browser logs: press `F12` → **Console**. Frontend messages start with `[ShortUrl]`.
- API logs: in Render, open the service → **Logs**. They include creation, validation, and server-error events.
