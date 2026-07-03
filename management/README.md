# ChadGreen Management App (Phase 0)

This folder contains the initial Blazor WebAssembly + ASP.NET Core API scaffold.

## Projects
- `src/ChadGreen.Management.Client` - Blazor WASM client
- `src/ChadGreen.Management.Api` - local management API
- `src/ChadGreen.Management.Shared` - shared contracts/enums

## Run locally

### Recommended (from repo root)
1. `npm run build:management`
2. In terminal A: `npm run dev:management:api`
3. In terminal B: `npm run dev:management:client`

Use `npm run dev:management` to print the quick-start guidance at any time.

### Windows launcher (opens three terminals)
- From anywhere: `C:\Repos\ChadGreen.com\run-management.cmd`
- Optional preflight check (no terminals opened): `C:\Repos\ChadGreen.com\run-management.cmd --check`

Launcher windows:
- Management API (`dotnet run --project .\management\src\ChadGreen.Management.Api --launch-profile http --no-build`)
- Management Client (`dotnet run --project .\management\src\ChadGreen.Management.Client --launch-profile http --no-build`)
- Astro site (`npm run dev`)

Default local endpoints:
- Client: `http://localhost:5506`
- API: `http://localhost:5508`

### Direct .NET commands (from `management\`)
1. `dotnet restore .\ChadGreen.Management.slnx`
2. Start API: `dotnet run --project .\src\ChadGreen.Management.Api --launch-profile http`
3. Start client: `dotnet run --project .\src\ChadGreen.Management.Client --launch-profile http`

Quick health check (API running):
- `curl http://localhost:5508/api/health`

## Notes
- Archive root: `<repo>\\.archive` (default)
- Retention purge: 90 days, applied during archive/restore operations
- Archive purge is best-effort and skips locked/inaccessible files so archive/restore operations can still complete.
- API CORS origins are configured at `Management:ClientOrigins` in `src/ChadGreen.Management.Api/appsettings.json`
- Client API base URL default is `ManagementApi:BaseUrl` in `src/ChadGreen.Management.Client/wwwroot/appsettings.json`
- Git capability endpoint is feature-flagged via `Management:Features:GitIntegration` (default `false`)
- Current git scope is manual local "Save + Commit" with conventional commits only
- Integrity validation endpoint: `POST /api/integrity/validate`
  - Blocking findings (`blocksSave: true`) gate save
  - External link findings stay warning-only (`blocksSave: false`)
  - External URL checks use short (2s) request timeouts to keep validation responsive.
  - Content file scans ignore inaccessible directories for safer utility operations in shared worktrees.
  - Slug mutation requests can return/apply cascade updates for speaking entities
- Management coverage includes:
  - Presentations + speaking engagements
  - Meetup groups + meetup events
  - Blog content
  - About profile (`src/content/authors`)
  - Media library (`public/images`) with upload, replace, browse, and archive
