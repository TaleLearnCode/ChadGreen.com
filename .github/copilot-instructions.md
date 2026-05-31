# Copilot Instructions for chadgreen.com

## Purpose

This repo powers Chad Green's personal website and social presence across speaking, meetups, blog posts, and profile/contact pages.

## Agent Priorities

1. Treat markdown content as the source of truth for site updates.
2. Preserve existing information architecture (presentations, speaking events, meetups, blog, about, contact).
3. Prefer small, content-first changes over broad refactors.
4. Validate content/schema compatibility before proposing or making large edits.

## Fast Start

Use these commands first:

```bash
npm run dev
npm run build
npm run preview
```

## Source-of-Truth Files

- Content schemas and enum constraints: [`src/content.config.ts`](../src/content.config.ts)
- Global profile/tagline/site identity: [`src/content/siteData/site.md`](../src/content/siteData/site.md)
- Author social links/profile metadata: [`src/content/authors/chad-green.md`](../src/content/authors/chad-green.md)
- Astro runtime/build settings: [`astro.config.mjs`](../astro.config.mjs)
- Azure Static Web Apps routing/cache/security config: [`staticwebapp.config.json`](../staticwebapp.config.json)
- Contact API function: [`api/src/functions/contact.js`](../api/src/functions/contact.js)
- Deployment workflows: [`.github/workflows/deploy-site.yml`](../.github/workflows/deploy-site.yml), [`.github/workflows/nightly-rebuild.yml`](../.github/workflows/nightly-rebuild.yml)

## High-Value Update Patterns

### Content and Social Presence Updates

- Add or edit content in `src/content/*` collections (blog, events, presentations, meetup groups/events, authors, siteData).
- Keep filenames kebab-case and frontmatter aligned to the schema.
- For social/profile changes, update `authors` and `siteData` first, then verify pages rendering that data.

### UI and Route Updates

- Routes are file-based under `src/pages/`.
- Reusable UI stays in `src/components/`; keep existing organization by folder (`ui`, `cards`, `forms`, `layout`, `presentations`).
- Keep layout-level concerns in `src/layouts/`.

## Guardrails

- Do not invent schema fields; extend [`src/content.config.ts`](../src/content.config.ts) first if needed.
- Respect status/visibility flags (`featured`, `status`, `draft`) since they control what is shown.
- Keep image references under `/images/...` backed by files in `public/images/`.
- Treat the root [`readme.md`](../readme.md) as legacy starter text; rely on this file and in-repo configs for current behavior.

## Validation Checklist

Before finishing non-trivial changes:

1. Run `npm run build` to catch Astro content/schema issues.
2. If UI/content changed, run `npm run dev` and verify affected routes.
3. If contact/API behavior changed, verify `/api/contact` assumptions still match [`staticwebapp.config.json`](../staticwebapp.config.json).

## Notes

- Stack: Astro 5 + TypeScript + markdown content collections + Azure Static Web Apps + Azure Functions.
- No formal test suite is configured; build + manual route checks are required.
