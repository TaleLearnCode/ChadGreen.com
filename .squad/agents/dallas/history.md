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

## Content Dev Notes
- Collection schemas defined in `src/content.config.ts`
- Collections use glob loaders: `pattern: "**/*.md"` in `src/content/{collection-name}/`
- Frontmatter must match Zod schema or content won't load
- Enum values are strict; typos break collection queries
- Some fields are optional (marked `.optional()` in schema)

## My Role (Content Dev)
- Understand collection structure and schemas
- Populate content accurately
- Organize files logically
- Flag gaps or schema issues for Keaton

---

## Issue #4 Execution: Messaging Patterns Cluster (2026-03-26)

### Completed Work
✅ Extracted Messaging Patterns presentation series and mapped to multiple events
✅ Created 3 engagementPresentations entries:
  - beer-city-code-2024-messaging-patterns-cloud-architecture.md
  - codemash-2025-messaging-patterns-cloud-architecture.md
  - devup-2024-messaging-patterns-cloud-architecture.md
✅ Updated event files to link messaging patterns presentations:
  - beer-city-code-2024.md (fixed date typo: 2026→2024)
  - codemash-2025.md
  - devup-2024.md
✅ Schema validation: All files pass Astro build (134 pages generated)
✅ Committed and pushed to main

### Key Learnings

**Content Extraction Pattern:**
- One base presentation (messaging-patterns-cloud-architecture) can have multiple engagement variants
- Each engagement links: presentationSlug → eventSlug → sessionSlug
- Frontmatter structure:
  - `title`: Human-readable session name
  - `eventSlug`: Maps to event file name (no .md)
  - `presentationSlug`: Maps to presentation file name (no .md)
  - `sessionSlug`: Internal identifier for deduplication
  - `date`: coerce.date() format (YYYY-MM-DD)
  - `canonicalPath`: 'speaking-session' for event-centric, 'presentation-event' for presentation-centric

**Schema Field Mappings (engagementPresentations):**
- `description`: Event-specific tagline (short, context-aware)
- `links`: Resources array (slides, video, github, etc.)
- `time`: Optional; use "TBD" if unknown; examples: "10:15 AM" or "10:00 AM - 11:00 AM"
- `timeZone`: Abbreviation (EST, CST, BST, UTC)
- `room`: Optional; conference room or track name

**Private/Public Repo Consolidation:**
- Issue mentioned 4-5 messaging repos (private + variants)
- Existing codebase already had presentations consolidated around messaging-patterns-cloud-architecture slug
- No need to create separate base presentation—existing one is current

**Engagement Presentation Best Practices:**
- Reuse base presentation content across events
- Customize descriptions for event context
- Include learning objectives from base presentation
- Mark critical sessions with date/time/room if available
- Use consistent formatting for multiple sessions at same event

### Open Questions / Notes for Chad
- DevUp 2025: Mentioned in issue but event doesn't exist yet. Should I create it?
- Session times: Added "TBD" for Beer City Code and CodeMash; DevUp has TBD. Need actual times?
- Recording links: No video/recording URLs found in current repos. Should I research these separately?
- Private repo access: Issue mentions UnlockThePowerOfMessagingPatterns-private as source of truth. Couldn't access directly; used existing public-repo-derived presentation as canonical source.
