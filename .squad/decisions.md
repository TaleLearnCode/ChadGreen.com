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

---

## Website Fix Batch (2026-05-31)

### Canonical Social Link Source
**By:** Keaton, Hockney  
**What:** Use `src/content/authors/chad-green.md` as the canonical source for Chad's social links. Mirror the LinkedIn URL everywhere it appears as `https://www.linkedin.com/in/chadwickgreen/`.
**Why:** Keeps footer, about/contact surfaces, and author metadata aligned so the profile URL does not drift across pages.

### Contact Form Behavior
**By:** Fenster  
**What:** Keep the contact form as a client-side `fetch('/api/contact')` flow with inline success/error messaging.
**Why:** Preserves the static-site UX while Fenster verifies runtime and deployment behavior around the Azure Functions endpoint.

---

## Repository Consolidation Strategy (2026-03-26)

### Decision: Revise 79-Repo Decomposition
**By:** Chad Green  
**Context:** Content backfill requires extracting presentation data from 79 GitHub repos (public + private + archived + workshops)

### Consolidation Rules (Chad's Directives)
1. **Private repos are source of truth** - When public and private versions exist, private is authoritative
2. **Consolidate archived repos** - Repos with `-delete` suffix merge with public equivalents (or include standalone if no match)
3. **Event-specific variants** - Single base presentation + multiple event mappings (avoid duplication)
4. **Workshop versioning** - Keep workshop versions SEPARATE (labs evolve, must track independently)
5. **Exclusions** - Remove `aspnetrazor-azure-blob` (utility, not a presentation)

### Current Status
**Blocker:** Need full 79-repo list to perform consolidation analysis  
**Keaton Action:** Prepared consolidation strategy framework + issue templates  
**Waiting On:** Chad to provide repo list (CSV, org name, or previous analysis document)

### Documents Created
- `.squad/decisions/repo-consolidation-strategy.md` - Detailed consolidation workflow, templates, edge cases
- `.squad/decisions/chad-request-repo-list.md` - Request for repo inventory to unblock analysis

### Next Steps (Once Repo List Received)
1. Keaton: Generate consolidation mapping table (public-private pairs, workshop versions, exclusions)
2. Keaton: Calculate updated scope (unique presentations, workshops, event variants)
3. Keaton: Produce priority ranking (top 10) for backfill
4. Keaton: Create GitHub issues for Dallas using revised templates
5. Dallas: Begin extraction per prioritized backlog

---

## Product Decisions — v1 Scope (2026-07-03)

### Archive Retention and Purge Window
**By:** User  
**What:** Archive retention/purge policy is **90 days**.

### Markdown Editing UX for v1
**By:** User  
**What:** v1 markdown editor is a **plain textarea** (no rich editor in v1).

### Git Commit Integration in v1
**By:** User  
**What:** **Optional** git commit integration is in scope for v1.

### Save+Commit Target
**By:** User  
**What:** Save+Commit targets a **local commit only** (no push).

### Commit Message Style
**By:** User  
**What:** Commit messages follow **Conventional Commits**.

### Archive Purge Enforcement
**By:** User  
**What:** Archive items are **automatically and permanently purged at 90 days**.

### Commit Trigger
**By:** User  
**What:** Commits run **only on manual explicit "Save + Commit"**.

---

## Execution Record — Phase 0 Batch (2026-07-03)

### Phase 0 Delivery Snapshot
**By:** Rusty, Linus, Fenster, Hockney  
**What:** Rusty scaffolded `management/` with Blazor WASM + API + shared contracts/foundational endpoints; Linus added integrity contracts/rules/DTOs under `src/lib/integrity` and `docs/content-integrity-checklist.md`; Fenster added local workflow scripts/instructions and validated build/run commands; Hockney captured QA phase-0 checklist outcomes and risks.

### Policy Confirmation
**By:** User (previously recorded), reconfirmed by Scribe  
**What:** No new policy decisions in this batch. Existing v1 policy remains: 90-day auto purge, plain textarea markdown editor, and local-only manual Save+Commit using Conventional Commits.
