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

---

# Session: 79-Repo Consolidation Strategy (2026-03-26)

## Task Received
Chad requested revision of 79-repo decomposition based on new consolidation rules:
1. Private repos are source of truth (consolidate with public where both exist)
2. Archived repos (`-delete` suffix) consolidate with public equivalents
3. Event-specific variants → single base presentation + event mappings
4. Workshops → keep versions SEPARATE (don't consolidate)
5. Exclude `aspnetrazor-azure-blob` (utility, not presentation)

## Analysis & Decisions

### Blocker Identified
- Cannot perform consolidation analysis without the full 79-repo list
- GitHub API search only returned 2 public repos (chadgreen user account)
- Repos may be under an organization or private (not accessible via public API)

### Work Completed
Created comprehensive framework documents for consolidation strategy:

1. **`.squad/decisions/repo-consolidation-strategy.md`**
   - Detailed consolidation rules based on Chad's Q&A
   - Workflow for categorizing repos (public, private, archived, workshops, event-variants, utilities)
   - 5 revised GitHub issue templates:
     - Public-Private consolidated extraction
     - Workshop version tracking
     - Event mapping (single presentation → multiple events)
     - Private-only extraction
     - Archived consolidation
   - Edge case handling (content conflicts, privacy checks, version identification)
   - Repository list template (CSV format Chad can fill)

2. **`.squad/decisions/sample-consolidation-mapping.md`**
   - EXAMPLE of final deliverables (to be replaced with actual data)
   - Sample mapping table (public-private pairs, workshop versions, event variants)
   - Sample scope calculations (unique presentations, workshops, event mappings)
   - Sample priority ranking (top 10)
   - Example GitHub issues using revised templates
   - Edge cases flagged for Chad's input

3. **`.squad/decisions/consolidation-walkthrough-example.md`**
   - Step-by-step walkthrough using 10 hypothetical repos
   - Demonstrates how each consolidation rule is applied
   - Shows reduction from 10 repos → 6 unique presentations (40% consolidation)
   - Detailed Dallas tasks for each presentation type
   - GitHub issue examples for each template

4. **`.squad/decisions/chad-request-repo-list.md`**
   - Explains blocker and what's needed to proceed
   - Options for Chad to provide repo list (CSV, org name, previous analysis)
   - ETA after receiving list: 1-2 hours

5. **`.squad/decisions/SUMMARY-consolidation-status.md`**
   - Executive summary for Chad (quick-read format)
   - Current status, blockers, deliverables ready, next actions

### Consolidation Logic Defined

**Public-Private Pairs:**
- Match by name similarity (`-private` or `_private` suffix, or exact match)
- Private repo is authoritative
- Dallas extracts from private first, cross-checks public for supplemental content
- Single consolidated `presentations/*.md` entry

**Archived Repos:**
- Match `-delete` suffix repos with public equivalents by name
- Consolidate into public version (public is current, archived is historical)
- Note archived content in frontmatter

**Workshop Versioning:**
- Identify versions by `-v1`, `-v2`, date ranges, or explicit version indicators
- Keep each version as SEPARATE presentation entry
- Track lab evolution in description field
- Do NOT consolidate workshop versions

**Event Variants:**
- Single base presentation entry
- Multiple `engagementPresentations/` entries (one per event)
- Link via `presentationId` and `eventId`
- Avoid duplicate presentation content for same talk at different venues

**Exclusions:**
- Utility repos (confirmed: `aspnetrazor-azure-blob`)
- Non-presentation content (scripts, tools, config, website code)

### Templates Created for Dallas

All templates account for:
- Source-of-truth handling (private wins conflicts)
- Consolidation notes (which repos merged, which is authoritative)
- Privacy checks (flag private-only content for Chad review)
- Version tracking (workshops only)
- Event mapping (single presentation → N events)

### Next Actions (Once Chad Provides List)

1. Apply consolidation rules to full 79-repo inventory
2. Generate actual mapping table (replace sample)
3. Calculate precise scope:
   - Unique presentations after consolidation
   - Workshop count (versioned separately)
   - Event variant mappings
   - Public-private pairs
   - Private-only content
4. Produce priority ranking (top 10, then full list)
5. Create GitHub issues for Dallas using revised templates
6. Flag edge cases for Chad's review
7. Hand off backlog to Dallas for execution

### Current Status
**Blocked:** Waiting for full 79-repo list from Chad  
**Framework Ready:** All consolidation logic, templates, and workflows defined  
**Squad Ready:** Dallas, Ralph, Hockney ready to execute once backlog is prioritized  
**ETA:** 1-2 hours after receiving repo list

## My Role as Lead

- Defined scope and consolidation strategy without full data (framework-first approach)
- Created reusable templates for Dallas to execute consistently
- Identified blocker and communicated clearly to Chad
- Prepared all deliverables in advance (mapping, priority, issues) for rapid execution
- Anticipated edge cases and provided resolution strategies
