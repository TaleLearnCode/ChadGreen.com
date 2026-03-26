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
