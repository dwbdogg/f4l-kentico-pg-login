# Fit4Less • Kentico drop‑in login (PerfectGym API)

This starter gives you a **Kentico‑friendly front end** and a tiny **.NET proxy** that talks to PerfectGym with the required headers — keeping your **Client Secret off the browser**.

> ⚠️ You must replace the placeholder endpoints with the correct PerfectGym login and profile endpoints for your tenant.

## Contents
- `web/` — Static site (HTML/CSS/JS). Drop this into a Kentico page via a static file app, widget, or CMS page template.
- `server/` — Minimal API (.NET 8). Host alongside Kentico (IIS) or as a small microservice. Exposes:
  - `POST /api/login` `{ email, password }` → forwards to PG login, stores `pg_token` httpOnly cookie.
  - `GET /api/me` → fetches current member details (using `Bearer {token}`).
  - `POST /api/logout` → clears cookie.

## Configure server
Set environment variables (IIS or web.config):
```
PG_BASE_URL=https://fit4less.perfectgym.pl
PG_CLIENT_ID=***
PG_CLIENT_SECRET=***
```
> Per PerfectGym docs, use `X-Client-Id` and `X-Client-Secret` headers **server‑side only**. Do not embed the secret in client code.
> Docs: https://presentation.perfectgym.com/Api/Docs/api/authentication.html and https://kb.perfectgym.com/article/api-authentication

### Endpoints to confirm
Update `Program.cs` to match the real endpoints for your PG version, for example:
- Login: `/Api/v2.2/customers/login` (placeholder in code — change to what your instance uses)
- Current profile: `/Api/v2.2/customers/me` (placeholder)

If you prefer, swap these for Client Portal auth endpoints and just return the portal session.

## Run locally
1. **Server (requires .NET 8):**
   ```bash
   dotnet new web -o server
   # replace Program.cs with the provided file
   dotnet run --project server
   ```
2. **Front end:**
   Serve `web/` via any static server (IIS / Kentico / `npx serve web`). When hosted behind the same domain as the API, requests go to `/api/*` directly.

## Security notes
- Keep `PG_CLIENT_SECRET` only on the server. Never ship it to the browser.
- Set `Secure`, `HttpOnly`, and `SameSite` on cookies (already done).
- Consider CSRF protection if you host API and site on the same domain.
- Enforce CORS only for your Kentico origin if hosting API separately.

## Styling
The login page uses the Fit4Less theme: buttercup yellow `#F6B221`, charcoal surfaces, Montserrat, bold uppercase headings.
