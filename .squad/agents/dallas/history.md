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

---

## Issue #5 Execution: Generative AI for .NET Developers (2026-03-26)

### Completed Work
✅ Extracted Generative AI for .NET Developers presentation
✅ Created 1 base presentation entry
✅ Created 1 event entry (Louisville .NET Meetup)
✅ Created 1 engagement presentation entry
✅ Schema validation: All files pass Astro build (150 pages generated)
✅ Committed and pushed to feature branch
✅ PR #15 created and ready for review

### Key Learnings

**GitHub API Access:**
- Both private and public repos accessible via GitHub MCP server
- Identical content in both repos (same README.md)
- Source of truth clearly marked in issue specification
- Can now extract from private repos directly without local cloning

**Event Creation Pattern:**
- New event required for Louisville .NET Meetup (first entry for this venue)
- No existing event entry for this meetup—created new event from scratch
- Event structure follows pattern: venue, date/time, location details
- Event links presentation through `presentations` array with session metadata

**Consolidation Workflow:**
- Private and public repos identical in this case
- Both link to private as authoritative source in resources
- Consolidation creates single presentation entry (not separate variants)
- Works perfectly for this public-private pair

**Metadata Extraction Best Practices:**
- Short abstract, full abstract, elevator pitch all captured from README
- Learning objectives extracted as array items
- Tags derived from content keywords
- Durations (45/60/75 min) captured from README table format
- Level: introductory (good starting point for AI topic for .NET devs)
- Status: active (currently being delivered)

**Engagement Presentation Structure:**
- Minimal but complete: links event and presentation with slug references
- sessionSlug unique identifier for deduplication (e.g., `louisville-dotnet-2024-08-22-generative-ai`)
- Date/time/room/timezone required for meetup context
- canonicalPath: 'speaking-session' for meetup context
- Learning objectives duplicated from base presentation for context

**Schema Field Mappings (All Collections):**
- **presentations:** type (session), durations (array), level, learningObjectives (array), tags, status, resources (github links work well)
- **events:** startDate (required), endDate (optional), eventType, location (object with venue/city/state/country), presentations (array with session details)
- **engagementPresentations:** eventSlug, presentationSlug, sessionSlug, date, time, timeZone, canonicalPath

### Naming Conventions Observed
- Presentation slug: `generative-ai-dotnet-developers` (hyphen-separated, lowercase)
- Event slug: `louisville-dotnet-meetup-2024-08-22` (includes date for uniqueness)
- File names: match slug names with .md extension
- sessionSlug: Similar format but may include event-specific identifiers

### Schema Validation Success
- All enum values must match exactly (e.g., 'introductory' not 'beginner')
- Date fields use coerce.date() format (YYYY-MM-DD)
- Required fields: title, description, eventSlug, presentationSlug, sessionSlug (engagementPresentations)
- Optional fields gracefully handled (no errors on omission)
- GitHub resource type works correctly in resources array

### Open Questions / Notes for Chad (Issue #5)
- None—full extraction completed successfully with all available repo data
- Louisville .NET Meetup appears to be a standing monthly meetup
- No future event dates documented in repo; used single 2024-08-22 date from README table

📌 Team update (2026-07-03T13:34:41.336-04:00): Redesign strategy aligned on phased rollout sequencing (Keaton), EasyMDE pilot recommendation (Rusty), accessibility + light/dark QA gates as DoD (Hockney), and content-authoring/editor UX standards (Dallas). — decided by Keaton, Rusty, Hockney, Dallas
