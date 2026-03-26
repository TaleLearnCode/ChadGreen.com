# Squad Decisions

## Initial Setup (2026-03-26)

### Team Hired
**By:** Coordinator (via Chad Green)
**Members:** Keaton (Lead), Dallas (Content Dev), Fenster (DevOps), Hockney (QA), Scribe, Ralph
**Why:** Managing a content-heavy portfolio site requires distributed expertise: content strategy, markdown/collection management, deployment, and quality assurance.

### User Directive: Content-First Phase
**By:** Chad Green
**What:** Prioritize content backfill. Site is soft-launched; focus on populating existing collections (presentations, events, meetups, blog archives) before moving to new content cadence.
**Why:** User has working site structure but needs content infrastructure populated.

### Architecture Decision: Astro Collections as Source of Truth
**By:** Team
**What:** All content lives in markdown files organized by collection (presentations, events, meetups, blog, authors, siteData). No database or CMS for this phase.
**Why:** Astro's content loader + Zod schemas provide type safety and straightforward structure for a portfolio site.

### Team Responsibilities Confirmed
- **Keaton** leads scope, decisions, code reviews
- **Dallas** owns content structure and markdown population
- **Fenster** manages Azure deployment and CI/CD
- **Hockney** ensures content quality before release
- **Ralph** tracks backlog and work queue
- **Scribe** maintains decisions, session logs, cross-team context
