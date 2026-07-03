# Day 1 Context: chadgreen.com

## Project Summary
Personal portfolio and speaking engagement site for Chad Green. Showcases presentations, speaking events, community involvement (meetups), and blog content.

## Content Collections Overview
1. **presentations** - Reusable talks with metadata (status: active/retired/in-development, featured: bool)
2. **events** - Speaking events (conference, meetup, webinar, etc.)
3. **engagementPresentations** - Event-specific sessions linking events → presentations
4. **meetupGroups** - Meetup group info and role
5. **meetupEvents** - Individual meetup session details
6. **blog** - Blog posts (category: Technical/Speaking/Community/etc., draft: bool)
7. **authors** - Author profiles
8. **siteData** - Global site config

## QA Checklist
- Frontmatter matches schema (esp. enums: type, status, eventType, category)
- Required fields are present
- Dates are valid (use ISO format in frontmatter: 2026-03-26)
- External links are active and correct
- Internal links resolve (e.g., event → presentation references)
- No typos or inconsistencies in naming
- Featured/status fields control visibility correctly

## Testing Process
1. Run locally: `npm run build && npm run preview`
2. Browse the site; click through navigation
3. Check all new/updated content renders correctly
4. Verify links work (both internal and external)
5. Check that featured items appear in lists
6. Verify draft posts don't show up in production

## My Role (QA)
- Review content before merge
- Validate schema compliance
- Test links and rendering
- Sign off on quality

## Phase 0 QA Acceptance Criteria + Smoke Checks (2026-07-03)

### Objective Pass/Fail Checklist
| Area | Check | Pass Criteria | Status |
|---|---|---|---|
| Scaffold | Linus and Rusty agent scaffolding files exist | `.squad/agents/{linus,rusty}/{charter.md,history.md}` present | ✅ Pass |
| Contracts | Team + routing contracts include new roles/lanes | `team.md` and `routing.md` explicitly include Linus + Rusty | ✅ Pass |
| Build | Site compiles from current scaffold state | `npm run build` exits `0` | ✅ Pass (with warnings) |
| Endpoint | Contact endpoint basic behavior holds | Invalid payloads return `400`; valid general payload returns `200`; honeypot returns `200` | ✅ Pass |
| Contracts | Product scope contract is documented | `.squad/decisions.md` includes Product Decisions section | ✅ Pass |

### Smoke Checks Run + Outcomes
1. `npm run build` → **PASS** (`exit 0`, 996 pages built).  
   - Observed multiple Vite/esbuild CSS minify warnings (`css-syntax-error`) during build.
2. Mocked endpoint smoke harness against `api/src/functions/contact.js` (no external services) → **PASS**  
   - Invalid inquiry type: `400`  
   - Missing required fields: `400`  
   - Valid general inquiry in dev mode: `200`  
   - Honeypot payload: `200`
3. Scaffold/contract presence checks (PowerShell assertions) → **PASS**  
   - Agent files, team roster entries, and routing lanes verified.

### Immediate Quality Risks
- ⚠️ **Build warning debt:** CSS minify `css-syntax-error` warnings are present in `npm run build`. Build passes, but warning volume can mask real regressions.  
- ⚠️ **Endpoint coverage gap:** No native automated test suite exists for `api/src/functions/contact.js`; current validation uses a mock harness only.

📌 Team update (2026-07-03T13:34:41.336-04:00): Redesign strategy aligned on phased rollout sequencing (Keaton), EasyMDE pilot recommendation (Rusty), accessibility + light/dark QA gates as DoD (Hockney), and content-authoring/editor UX standards (Dallas). — decided by Keaton, Rusty, Hockney, Dallas
