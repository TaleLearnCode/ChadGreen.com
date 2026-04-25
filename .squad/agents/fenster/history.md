# Day 1 Context: chadgreen.com

## Project Summary
Personal portfolio and speaking engagement site for Chad Green. Showcases presentations, speaking events, community involvement (meetups), and blog content.

## Deployment Setup
- **Hosting:** Azure Static Web Apps
- **Build:** `npm run build` → static output to `dist/`
- **Config:** `staticwebapp.config.json` - routing, headers, API routes
- **API:** `/api/contact` (Azure Functions, Node.js 20)
- **CI/CD:** Triggered on push (GitHub Actions workflow)

## Build Commands
- `npm run build` - Production build
- `npm run dev` - Local dev server (hot reload)
- `npm run preview` - Preview production build locally
- `npm run astro` - Astro CLI for advanced ops

## Key Configuration
- **astro.config.mjs:** Defines build output, asset paths, markdown plugins (Shiki, rehype-mermaid)
- **staticwebapp.config.json:** Defines routes, cache headers, API endpoints, trailing slash behavior
- **tsconfig.json:** Path aliases (@/*, @components/*, etc.)

## Deployment Considerations
- Assets in `_astro/` get immutable long-term cache (31536000 seconds)
- Images get 24-hour cache
- Global headers include security policies (X-Frame-Options, CSP, etc.)
- Build output goes to `dist/` (git-ignored)

## My Role (DevOps)
- Ensure builds succeed
- Monitor deployment pipeline
- Check performance and caching
- Coordinate with Hockney on pre-deployment verification

## Learnings

### Nightly Rebuild Workflow (Day 2)
**Date:** 2026-03-27  
**Task:** Create automated nightly rebuild pipeline for static site refresh

**Decision:** Implemented `.github/workflows/nightly-rebuild.yml` with:
- **Schedule:** Cron `0 5 * * *` (5:00 AM UTC daily)
- **Trigger:** Both scheduled and manual (`workflow_dispatch`)
- **Pipeline:** Checkout → Setup Node → Cache Playwright → Install browsers → Install deps → Build → Deploy to Azure Static Web Apps

**Key Observations:**
1. Reused existing `deploy-site.yml` structure but simplified (no PR logic, no close job)
2. Secret `AZURE_STATIC_WEB_APPS_API_TOKEN_POLITE_DUNE_01D25BA0F` is the deployment token from original workflow
3. Included Playwright browser caching for consistency with primary workflow
4. Used `npm ci` instead of `npm install` for clean, reproducible builds in CI
5. No git commit needed - workflow is pure deploy (dates/status fields filter content at build time)

**Why This Works:**
- Astro collections already have `status` and date filtering in templates (events/blog posts marked as "past" get filtered at build time)
- Nightly rebuild ensures content visibility changes are reflected without code changes
- Playwright cache keeps CI job times acceptable

**Azure Static Web Apps Config:**
- Build output: `dist/` (Astro default)
- API location: `api/` (Azure Functions for contact endpoint)
- App location: `.` (root)
- No special environment variables needed; all content is markdown-based
