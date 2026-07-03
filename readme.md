# ChadGreen.com

## Project overview

This repository contains Chad Green’s personal website plus a .NET management app scaffold used to manage supporting workflows/content operations.

## Tech stack

- **Static site:** Astro 5, TypeScript, Markdown content collections
- **Serverless API:** Azure Functions (Node.js) under `api/`
- **Management app scaffold:** .NET solution with:
  - `ChadGreen.Management.Api`
  - `ChadGreen.Management.Client`
  - `ChadGreen.Management.Shared`
- **Hosting/deploy target:** Azure Static Web Apps

## Local development (static site)

```bash
npm install
npm run dev
```

Site dev server runs via Astro (`astro dev`).

## Local development (Azure Functions API)

```bash
npm run setup:api
npm run dev:api
```

Notes:
- `setup:api` installs dependencies under `api/` (required before first API run).
- For clean local health checks, provide `AzureWebJobsStorage` (for example via Azurite in `api/local.settings.json`).

## Local development (management app)

> **Fastest way (recommended):** run `run-management.cmd` from the repo root.

```bat
run-management.cmd
```

This launches three processes in separate terminals:
- Management API (`management\src\ChadGreen.Management.Api`)
- Management Client (`management\src\ChadGreen.Management.Client`)
- Astro site (`npm run dev` from repo root)

The launcher performs a solution build first, then starts the two management apps with `--no-build` to avoid concurrent build file-lock conflicts, plus `npm run dev` for the Astro site.

Quick launcher validation:

```bat
run-management.cmd --check
```

Or via npm:

```bash
npm run check:management:launcher
```

Equivalent npm commands (manual split terminals):

```bash
npm run dev:management:prepare
npm run dev:management:api
npm run dev:management:client
npm run dev
```

## Build commands

```bash
# Site
npm run build

# Management solution
npm run build:management

# Optional targeted builds
npm run build:management:api
npm run build:management:client
```

## Repo structure (high-level)

- `src/` — Astro pages, components, layouts, and content collections
- `public/` — static assets (images, icons, downloadable files)
- `api/` — Azure Functions for site backend endpoints
- `management/` — .NET management solution and projects
- `.github/workflows/` — CI/CD and scheduled rebuild workflows
- `run-management.cmd` — one-command launcher for management API + client