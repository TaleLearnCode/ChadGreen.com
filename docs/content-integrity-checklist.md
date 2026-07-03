# Content Integrity Checklist (Phase 2)

Use this checklist before accepting content saves:

- [ ] **Schema/frontmatter valid**: required fields and enum values pass collection schema checks.
- [ ] **Internal/local references valid**: cross-collection slugs/IDs and local paths resolve.
- [ ] **Media paths exist**: referenced `/images/...` or other local assets exist in repo.
- [ ] **External links checked**: unreachable external URLs are recorded as warnings only.
- [ ] **Slug edits cascaded**: when a slug changes, dependent references are auto-updated and reported.
- [ ] **Meetup content checks**: `meetupEvents.meetupGroup` resolves to `meetupGroups.slug`; required meetup fields are present.
- [ ] **Blog checks**: required `title`, `description`, `pubDate` fields are valid and category values stay in schema enum.
- [ ] **Profile/about checks**: `authors` and `siteData` required profile fields remain non-empty scalar strings.

## Save policy

- Block save on broken **internal/local** references or schema/media errors.
- Allow save (with warnings) for broken **external** links.

## Developer usage (Phase 1 readiness)

- Canonical TypeScript integrity contracts and DTOs live in `src/lib/integrity`.
- Import from the barrel (`src/lib/integrity/index.ts`) using:
  - `import type { IntegrityValidationRequest, IntegrityValidationResult } from '@/lib/integrity';`
  - `import { integrityRuleDefinitions, integrityRuleDefinitionsById } from '@/lib/integrity';`
- `*Dto` names remain exported for backwards compatibility; non-`Dto` aliases are provided for Phase 1 management app alignment.
