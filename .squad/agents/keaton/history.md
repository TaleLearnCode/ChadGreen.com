# Day 1 Context: chadgreen.com

## Project Summary
Personal portfolio and speaking engagement site for Chad Green. Showcases presentations, speaking events, community involvement (meetups), and blog content.

## Tech Stack
- **Framework:** Astro 5 (static site generator)
- **Language:** TypeScript
- **Content:** Markdown files + Zod schemas (8 collections)
- **Hosting:** Azure Static Web Apps
- **Build:** `npm run build` → `dist/`
- **Dev:** `npm run dev` (hot reload)

## Key Paths
- **Content:** `src/content/` (presentations, events, meetups, blog, etc.)
- **Components:** `src/components/` (organized by ui, cards, forms, layout, presentations)
- **Layouts:** `src/layouts/` (BaseLayout, SidebarLayout, BlogLayout)
- **Config:** `astro.config.mjs`, `src/content.config.ts`, `staticwebapp.config.json`

## Current Phase
**Soft launched; content backfill mode**
- Need to add: more presentations, past speaking events, meetup updates, blog archives
- After backfill: establish regular blog cadence and maintenance

## Content Collections (8 total)
1. **presentations** - Reusable talk abstracts (status: active/retired/in-development)
2. **events** - Speaking events with venue, date, linked presentations
3. **engagementPresentations** - Event-specific session instances
4. **meetupGroups** - Meetup group info and roles
5. **meetupEvents** - Individual meetup sessions
6. **blog** - Blog posts (status: draft/published, featured: true/false)
7. **authors** - Author profiles
8. **siteData** - Global site config

## Enum Values to Know
- Presentation type: session, workshop, lightning-talk, keynote, panel, webinar
- Presentation level: introductory, intermediate, advanced, all
- Event type: conference, meetup, webinar, podcast, workshop, user-group, corporate, other
- Blog category: Technical, Speaking, Community, Career, Tutorial, Announcement, Personal, Other
- Status fields: active, retired, in-development (presentations); draft: boolean (blog)

## Initial Goals
1. Keaton leads decision-making and scope
2. Dallas populates content collections
3. Fenster ensures deployment is smooth
4. Hockney validates quality
5. Ralph tracks the backlog
