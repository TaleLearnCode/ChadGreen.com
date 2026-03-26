# Copilot Instructions for chadgreen.com

## Project Overview

This is a personal portfolio and speaking website for Chad Green, built with **Astro 5** as a static site generator. The site showcases presentations, speaking events, blog content, meetup groups, and a contact form. It's deployed to **Azure Static Web Apps**.

## Build, Test & Run Commands

### Local Development
```bash
npm run dev      # Start dev server on http://localhost:3000 (with hot reload)
npm start        # Alias for npm run dev
npm run build    # Production build (output to dist/)
npm run preview  # Preview production build locally
npm run astro    # Run Astro CLI directly for advanced operations
```

### Project Structure
- **`src/pages/`** - Route-based pages (auto-generates URLs)
- **`src/components/`** - Reusable Astro components (organized by type: ui/, forms/, cards/, layout/, presentations/)
- **`src/layouts/`** - Page wrapper templates (BaseLayout, SidebarLayout, BlogLayout)
- **`src/content/`** - Content collections managed by Astro Content API (collections defined in `content.config.ts`)
- **`api/`** - Azure Functions for backend (Node.js 20 runtime, houses contact form endpoint)
- **`public/`** - Static assets (images, etc.) served at root
- **`dist/`** - Built output (git-ignored)

### Content Collections

The site uses **Astro Content Collections** with 8 collections (Zod schemas defined in `src/content.config.ts`):

1. **`presentations`** - Reusable talk abstracts with metadata (durations, level, learning objectives, resources, tags, status)
2. **`events`** - Speaking events (conferences, meetups, webinars) with venue details and linked presentations
3. **`engagementPresentations`** - Junction collection mapping presentations to specific event sessions
4. **`meetupGroups`** - Meetup group information and roles
5. **`meetupEvents`** - Individual meetup event sessions with speaker details and resources
6. **`blog`** - Blog posts with categories, tags, and featured status
7. **`authors`** - Author profiles with social links
8. **`siteData`** - Global site configuration (name, tagline, social links)

**Key patterns:**
- Collections use glob loaders: markdown files in `src/content/{collection-name}/`
- All content is markdown-based with frontmatter
- Use `.optional()` in schemas for fields that may be missing
- Dates use `z.coerce.date()` to auto-parse ISO strings
- `featured` and `status` fields control visibility on site

## High-Level Architecture

### Astro Framework Conventions
- **Astro is file-based routing:** Pages in `src/pages/` auto-generate routes (e.g., `src/pages/blog.astro` → `/blog`)
- **Components are `.astro` files:** Mix HTML, JavaScript, and other frameworks inline
- **Static output:** `output: 'static'` in `astro.config.mjs` means all pages pre-render at build time
- **Markdown support:** `.md` files are first-class; rehype plugins (e.g., `rehype-mermaid`) transform markdown after rendering

### Deployment Target
- **Azure Static Web Apps** via `staticwebapp.config.json`
- **Cache strategy:** Long-term cache for `_astro/` assets (immutable), short cache for images, rewrite 404s to index.html
- **Security headers:** `X-Frame-Options: DENY`, `X-XSS-Protection`, `X-Content-Type-Options: nosniff`
- **API route:** `/api/contact` (POST, anonymous) routes to Azure Function

### Data Fetching & Component Hierarchy
- **No REST API calls in components:** All data comes from markdown content files in `src/content/` loaded via Astro's Content API
- **Query content in pages:** Use `getCollection('presentations')` et al. to fetch, filter, and sort
- **Pass data to components:** Components receive data as props; they do NOT query content directly
- **Type safety:** Import and use generated types from `astro:content` (e.g., `type Presentation`)

### UI Component Layers
1. **Layout components** (`src/layouts/`) - Wrap full pages; define HTML structure, head metadata, nav/footer
2. **Page components** (`src/pages/`) - Route handlers; query collections, compose UI
3. **UI components** (`src/components/ui/`) - Primitives (Button, Badge, Hero, etc.); accept props
4. **Card components** (`src/components/cards/`) - Presentation, Event, Blog cards; display collection items
5. **Form components** (`src/components/forms/`) - Contact form (uses `<form>` + client-side validation, POSTs to `/api/contact`)

## Key Conventions

### TypeScript Path Aliases
All imports use path aliases (configured in `tsconfig.json`):
- `@/*` → `src/*`
- `@components/*` → `src/components/*`
- `@layouts/*` → `src/layouts/*`
- `@content/*` → `src/content/*`
- `@styles/*` → `src/styles/*`

**Use these in imports:** `import { Button } from '@components/ui/Button.astro'`

### Content Schema Enums
Content collections use strict enums for categorization to enable filtering and type safety:
- **Presentations:** `type` (session, workshop, lightning-talk, keynote, panel, webinar), `level` (introductory, intermediate, advanced, all), `status` (active, retired, in-development)
- **Events:** `eventType` (conference, meetup, webinar, podcast, workshop, user-group, corporate, other)
- **Blog:** `category` (Technical, Speaking, Community, Career, Tutorial, Announcement, Personal, Other), `status` via `draft` boolean
- **Meetup Events:** `status` (upcoming, past)

**When adding content:** Verify enum values match schema; typos break filtering.

### Markdown Syntax Highlighting & Mermaid
- **Shiki syntax highlighting** auto-applies to markdown code blocks
- **Mermaid diagrams:** Use ` ```mermaid ... ``` ` blocks in markdown; `rehype-mermaid` renders as SVG
- **Exclude Mermaid from Shiki:** Configured in `astro.config.mjs` so diagram syntax isn't highlighted

### Styles
- **CSS in components:** Astro scopes CSS in `<style>` blocks to the component automatically
- **Global styles:** Place in `src/styles/` and import in layout components
- **CSS minification:** Enabled in Vite build config (`cssMinify: true`)

### Image Handling
- **Static images:** Place in `public/images/` for fingerprinting and cache-busting
- **Image paths in markdown:** Use relative `/images/` paths (served from `public/`)
- **Default images:** Many collections have `default-*.svg` fallbacks (e.g., `heroImage: '/images/blog/default-hero.svg'`)

### API & Backend
- **Azure Functions location:** `api/src/` (not part of Astro build)
- **Contact form endpoint:** `POST /api/contact` (defined in `staticwebapp.config.json`)
- **No database:** Everything is markdown-based; no persistence beyond function invocations

### Featured & Status Fields
- **`featured: true`** → Pin to top of lists (used in blog, presentations, events, meetup groups)
- **`status` / `draft`** → Control visibility (e.g., `draft: true` hides blog post, `status: 'retired'` archives presentation)
- **Always check these fields** when filtering or iterating collections

### Build Output & Asset Paths
- **Build output:** `dist/` directory (recreated on each build)
- **Asset namespace:** `_astro/` for bundled CSS/JS; served with `max-age=31536000, immutable`
- **Asset path config:** Set in `astro.config.mjs` build options; do NOT change without understanding cache implications

## Additional Notes

- **No testing framework:** No test files or test runner configured; manual testing in dev server
- **Deployment:** Push to main branch; Azure Static Web Apps auto-builds via GitHub Actions
- **TypeScript:** Strict mode enabled; import types from `astro:content` for collections
- **Postinstall hook:** `playwright install` runs after npm install (for browser automation if needed in future)
