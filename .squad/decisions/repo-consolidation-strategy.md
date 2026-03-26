# Repository Consolidation Strategy
**Date:** 2026-03-26  
**Decision:** Revised 79-Repo Decomposition for Consolidated Public-Private Strategy  
**Scope Clarifications by:** Chad Green

---

## Consolidation Rules (Chad's Directives)

### 1. Private Repos as Source of Truth
- **Rule:** Private repos are ALWAYS the authoritative source
- **Consolidation:** Private repos MAY consolidate with public repos of similar name
- **Conflict Resolution:** If public and private differ, private content wins
- **Matching Logic:** Look for `-private` or `_private` suffix, or exact name match

### 2. Archived Repos (-delete suffix)
- **Rule:** Consolidate `-delete` archived repos WITH public equivalents if they exist
- **Fallback:** If no public equivalent exists, include the archived repo
- **Example:** `PresentationName-delete` consolidates with `PresentationName` (public)

### 3. Event-Specific Variants
- **Strategy:** Single base presentation + multiple event mappings
- **Implementation:** One presentation record, multiple `engagementPresentations` linking to different events
- **Rationale:** Avoid duplicate content for same talk at different venues

### 4. Workshop Versioning
- **Rule:** Keep workshop versions SEPARATE (do NOT consolidate)
- **Reason:** Labs evolve; versions must be tracked independently
- **Example:** `WorkshopName-v1`, `WorkshopName-v2`, `WorkshopName-v3` remain distinct
- **Identification:** Look for version indicators (`-v1`, `-v2`, version numbers in name/description)

### 5. Exclusions
- **Excluded:** `aspnetrazor-azure-blob` (not a presentation, utility repo)
- **Check for similar:** Other utility/tooling repos that aren't presentations

---

## Consolidation Workflow

### Phase 1: Categorization
1. **Identify Private Repos** (27 expected from original count)
2. **Match Public-Private Pairs** by name similarity
3. **Flag Workshops** for version tracking (keep separate)
4. **Identify Archived Repos** (`-delete` suffix)
5. **Remove Utility Repos** (aspnetrazor-azure-blob, similar)

### Phase 2: Consolidation Mapping
For each repo:
- **Public only?** → Extract as-is
- **Private only?** → Extract as-is (authoritative)
- **Public + Private pair?** → Consolidate (private is source of truth)
- **Archived + Public?** → Consolidate into public
- **Workshop?** → Flag for version tracking, DON'T consolidate versions
- **Event variant?** → Map to single base presentation + event links

### Phase 3: Priority Ranking
After consolidation:
- Count unique presentations
- Identify high-value content (recent, frequently delivered, flagship talks)
- Prioritize workshops (complex, require lab tracking)
- Rank by: Recency, Frequency, Complexity, Strategic Value

---

## Data Structure After Consolidation

### Consolidated Presentation Record
```
Presentation ID: [base-name]
Source Repos: 
  - Private: [private-repo-url] (AUTHORITATIVE)
  - Public: [public-repo-url] (supplemental)
  - Archived: [archived-repo-url] (historical)

Content Priority: Private > Public > Archived

Workshop Versioning: 
  - If workshop: Track versions separately
  - Version indicators: [list version markers]

Event Mappings:
  - Event 1: [event-id] → [engagement-presentation-id]
  - Event 2: [event-id] → [engagement-presentation-id]
```

---

## GitHub Issue Templates (Revised)

### Template 1: Extract Public-Private Consolidated Presentation
```markdown
**Title:** Extract: [Presentation Name] (Public-Private Consolidated)

**Description:**
Extract presentation content from both public and private repos, using private as source of truth.

**Source Repos:**
- Private (authoritative): `[private-repo-name]`
- Public (supplemental): `[public-repo-name]`

**Consolidation Notes:**
- [ ] Extract from private repo first (README, abstract, slides metadata)
- [ ] Check public repo for additional content (demos, sample code)
- [ ] Resolve conflicts (private wins)
- [ ] Create single `presentations/[slug].md` entry
- [ ] Note both repos in frontmatter `sourceRepos` field

**Dallas Tasks:**
1. Clone both repos
2. Extract abstract, description, level, type from private
3. Cross-check public for supplemental info
4. Create consolidated markdown file
5. Mark as complete

**Priority:** [High/Medium/Low]
```

### Template 2: Extract Workshop with Version Tracking
```markdown
**Title:** Extract: [Workshop Name] - Version [X]

**Description:**
Extract workshop content with version tracking. Keep versions separate per Chad's directive.

**Source Repo:** `[workshop-repo-name-v{X}]`
**Version:** [X]
**Lab Count:** [N labs]

**Workshop-Specific Tasks:**
- [ ] Extract workshop abstract (session-level content)
- [ ] Document lab structure (lab names, order, dependencies)
- [ ] Note version-specific differences (if comparing to other versions)
- [ ] Create `presentations/[workshop-slug-vX].md`
- [ ] Tag with `type: workshop` and `version: X`

**Dallas Notes:**
- Do NOT consolidate with other versions
- Track lab evolution in description
- Link related versions in `relatedPresentations` field (optional)

**Priority:** [High/Medium/Low]
```

### Template 3: Map Consolidated Presentation to Multiple Events
```markdown
**Title:** Map: [Presentation Name] to Event Variants

**Description:**
Create event mappings for a single presentation delivered at multiple venues.

**Base Presentation:** `[presentation-slug]`

**Events:**
1. [Event 1 Name] - [Date] - [Venue]
2. [Event 2 Name] - [Date] - [Venue]
3. [Event 3 Name] - [Date] - [Venue]

**Dallas Tasks:**
1. Ensure base presentation exists in `presentations/`
2. Create event entries in `events/` (if not exist)
3. Create `engagementPresentations/` entries for each event-presentation pairing
4. Link via `presentationId` and `eventId` fields
5. Note any event-specific customizations in `engagementPresentations` notes

**Priority:** [Medium]
```

### Template 4: Extract Private-Only Presentation
```markdown
**Title:** Extract: [Presentation Name] (Private Only)

**Description:**
Extract presentation from private repo (no public equivalent).

**Source Repo (Private):** `[private-repo-name]`

**Dallas Tasks:**
- [ ] Clone private repo
- [ ] Extract abstract, description, metadata
- [ ] Create `presentations/[slug].md`
- [ ] Mark `sourceRepos: [private-repo-url]` in frontmatter
- [ ] Flag if content is sensitive/NDA (shouldn't be public on site)

**Privacy Check:**
- Review for corporate/NDA content before publishing
- Confirm with Chad if uncertain

**Priority:** [High/Medium/Low]
```

### Template 5: Consolidate Archived Repo with Public
```markdown
**Title:** Consolidate: [Presentation Name] (Archived + Public)

**Description:**
Consolidate archived `-delete` repo with active public repo.

**Source Repos:**
- Public (current): `[public-repo-name]`
- Archived: `[archived-repo-name-delete]`

**Dallas Tasks:**
- [ ] Extract from public repo (primary)
- [ ] Check archived repo for historical context
- [ ] Note retirement date or status changes
- [ ] Create single consolidated `presentations/[slug].md`
- [ ] Set `status: retired` if appropriate

**Priority:** [Low]
```

---

## Edge Cases & Ambiguities

### 1. Conflicting Content Between Public and Private
**Question:** If private and public repos have significantly different abstracts/descriptions, how should Dallas handle it?
**Answer:** Private is authoritative. Use private content, note discrepancies in comments.

### 2. Partial Private Repos (No README, minimal content)
**Question:** If a private repo exists but has less content than the public version?
**Answer:** Still use private as base, supplement with public content. Note in frontmatter.

### 3. Workshop Version Identification
**Question:** How to identify versions if not explicitly named `-v1`, `-v2`?
**Chad Input Needed:** Provide version identification strategy (date ranges, major updates, etc.)

### 4. Event-Specific Content That Differs Significantly
**Question:** If an "event variant" has substantially different content (not just venue change)?
**Answer:** Treat as separate presentation. Map as related, not consolidated.

### 5. Utility Repos vs. Presentation Repos
**Question:** Besides `aspnetrazor-azure-blob`, are there other utility repos to exclude?
**Chad Input Needed:** List any other non-presentation repos.

---

## Next Steps

1. **Chad to Provide:** Full 79-repo list with visibility (public/private) and basic metadata
2. **Keaton to Analyze:** Apply consolidation rules, generate mapping table
3. **Keaton to Produce:**
   - Consolidated repo mapping (table format)
   - Updated scope numbers (unique presentations, workshops, event variants)
   - Priority ranking (top 10)
   - Revised issue templates (ready for Dallas)
4. **Dallas to Execute:** Once approved, begin extraction per prioritized backlog

---

## Repository List Template (Chad to Fill)

```csv
repo_name,visibility,type,version,notes
GenerativeAIForDotNetDevelopers,public,presentation,,,
GenerativeAIForDotNetDevelopers-private,private,presentation,,private-source
AspNetCoreFromScratch,public,presentation,,,
AspNetCoreFromScratch-delete,public,presentation,,archived
BuildingModernWebApps-v1,public,workshop,1,,
BuildingModernWebApps-v2,public,workshop,2,,
aspnetrazor-azure-blob,public,utility,,EXCLUDE
...
```

**Expected Count:** 79 total
- Public: ~50
- Private: ~27
- Utilities: ~1-2

---

## Decision Log

**By:** Keaton (Lead)  
**Status:** Awaiting Chad's 79-Repo List  
**Next Review:** After consolidation mapping complete
