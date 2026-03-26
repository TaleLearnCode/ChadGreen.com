# Consolidation Rules Quick Reference

**Purpose:** One-page summary of how repos consolidate  
**Date:** 2026-03-26

---

## Decision Table

| Repo Pattern | Action | Result | Source of Truth | Example |
|--------------|--------|--------|-----------------|---------|
| **Public + Private (same name)** | Consolidate | 1 presentation | Private repo | `GenAI` (pub) + `GenAI-private` (priv) → `gen-ai` |
| **Public + Archived (-delete)** | Consolidate | 1 presentation | Public repo | `AspNetCore` (pub) + `AspNetCore-delete` (arch) → `aspnet-core` |
| **Workshop v1 + Workshop v2** | Keep Separate | 2 presentations | Each repo independently | `Workshop-v1` + `Workshop-v2` → `workshop-v1` + `workshop-v2` |
| **Base + Event Variant** | Consolidate to Base | 1 presentation + event mapping | Base repo | `IntroAzure` + `IntroAzure-NDC` → `intro-azure` (map to NDC event) |
| **Private Only (no public)** | Extract As-Is | 1 presentation | Private repo | `K8sBasics-private` → `k8s-basics` |
| **Public Only (no private)** | Extract As-Is | 1 presentation | Public repo | `DockerIntro` → `docker-intro` |
| **Utility Repo** | Exclude | 0 presentations | N/A | `aspnetrazor-azure-blob` → EXCLUDED |

---

## Consolidation Logic Flow

```
START: Repo from 79-repo list
  ↓
Is it a utility? (aspnetrazor-azure-blob, etc.)
  → YES → EXCLUDE (0 presentations)
  → NO → Continue
  ↓
Is there a matching private repo?
  → YES → Consolidate (Private is source of truth)
  → NO → Continue
  ↓
Is there a matching -delete archived repo?
  → YES → Consolidate (Public is source of truth)
  → NO → Continue
  ↓
Is it a workshop with version indicator?
  → YES → Keep Separate (Don't consolidate versions)
  → NO → Continue
  ↓
Is it an event-specific variant? (e.g., PresentationName-EventName)
  → YES → Consolidate to Base (Map to event)
  → NO → Continue
  ↓
Extract As-Is (Public-only or Private-only)
  → Create 1 presentation
END
```

---

## Conflict Resolution

| Conflict Type | Resolution |
|---------------|------------|
| Public vs. Private abstract differs | Use Private (source of truth) |
| Public has more content than Private | Use Private as base, supplement with Public |
| Archived has newer date than Public | Use Public (archived is historical) |
| Event variant has different abstract | Treat as separate presentation (not a variant) |
| Workshop version unclear | Flag for Chad input |
| Privacy/NDA concern in Private repo | Flag for Chad review before publishing |

---

## Dallas Extraction Checklist

For each consolidated presentation:

**Public-Private Pair:**
- [ ] Clone both repos
- [ ] Extract from Private first (abstract, description, level, type)
- [ ] Cross-check Public for demos, sample code
- [ ] Resolve conflicts (Private wins)
- [ ] Note both repos in `sourceRepos` frontmatter

**Archived-Public Pair:**
- [ ] Clone Public repo (primary)
- [ ] Review Archived for historical context
- [ ] Extract from Public
- [ ] Note archived content in frontmatter
- [ ] Set `status: retired` if appropriate

**Workshop (Any Version):**
- [ ] Clone repo
- [ ] Extract abstract (high-level, not lab details)
- [ ] Document lab structure in description
- [ ] Create separate file per version (don't consolidate)
- [ ] Set `type: workshop`, `version: N`
- [ ] Link related versions in `relatedPresentations`

**Event Variant:**
- [ ] Clone base repo only (ignore event-specific variant repos)
- [ ] Extract abstract from base
- [ ] Create base presentation entry
- [ ] Create event entries for each delivery
- [ ] Create `engagementPresentations` linking presentation to events

**Private-Only:**
- [ ] Clone private repo
- [ ] Extract abstract, description, level, type
- [ ] **Privacy check:** Review for NDA/corporate content
- [ ] Flag for Chad review if uncertain
- [ ] Note `sourceRepos: [private-url]` in frontmatter

---

## Frontmatter Fields for Consolidated Content

```yaml
# Standard fields
title: "Presentation Title"
slug: "presentation-slug"
type: "session" | "workshop" | "lightning-talk" | "keynote" | "panel" | "webinar"
level: "introductory" | "intermediate" | "advanced" | "all"
status: "active" | "retired" | "in-development"

# Consolidation-specific fields
sourceRepos:
  - "https://github.com/chadgreen/RepoName-private"  # Private (authoritative)
  - "https://github.com/chadgreen/RepoName"          # Public (supplemental)
  - "https://github.com/chadgreen/RepoName-delete"   # Archived (historical)

# Workshop-specific fields
version: 1                    # For workshops only
labCount: 7                   # Number of hands-on labs
relatedPresentations:
  - "workshop-slug-v2"        # Link to other versions

# Tags
tags: ["AI", ".NET", "Azure"]

# Notes (for consolidation context)
notes: |
  Consolidated from public and private repos. Private repo has updated 2024 content.
  Public repo has polished demo code referenced in labs.
```

---

## Scope Calculation Formula

```
Unique Presentations = 
  Public-Only Repos
  + Private-Only Repos
  + Public-Private Pairs (count as 1)
  + Archived-Public Pairs (count as 1)
  + Workshop Versions (each version counts as 1)
  + Event Base Presentations (variants don't count)
  - Excluded Utilities

Example:
  10 Public-Only
  + 7 Private-Only
  + 20 Public-Private Pairs (= 20, not 40)
  + 2 Archived-Public Pairs (= 2, not 4)
  + 8 Workshop Versions (kept separate)
  + 5 Event Base Presentations (variants consolidated)
  - 1 Excluded Utility
  = 51 Unique Presentations
```

---

## Priority Ranking Criteria

| Criteria | Weight | Rationale |
|----------|--------|-----------|
| **Recency** | High | 2023-2024 content prioritized (most relevant) |
| **Frequency** | High | Talks delivered 5+ times = high-value |
| **Strategic Value** | High | Flagship/signature talks represent Chad's brand |
| **Complexity** | Medium | Workshops prioritized (require lab tracking) |
| **Private Content** | Medium | Private-only or private-public pairs have authoritative content |
| **Event Mappings** | Low | Multiple deliveries tracked, but not priority driver |

---

## Edge Cases (Flag for Chad Review)

1. **Private repo exists but has less content than public**
   - Resolution: Use private as base, supplement with public
   - Flag: Note in GitHub issue for Chad review

2. **Archived repo has newer date than public**
   - Resolution: Public is current, archived is historical (despite dates)
   - Flag: Confirm with Chad if archived should supersede public

3. **Workshop without clear version indicator**
   - Resolution: Treat as versions if named similarly (e.g., `Workshop` vs. `Workshop-Updated`)
   - Flag: Request Chad input on version identification

4. **Event variant has substantially different content (>50% abstract difference)**
   - Resolution: Treat as separate presentation, not a variant
   - Flag: Confirm threshold with Chad

5. **Utility repos beyond aspnetrazor-azure-blob**
   - Resolution: Exclude repos named: `scripts`, `tools`, `utilities`, `config`, website code
   - Flag: Confirm exclusions with Chad

---

**Reference:** See `.squad/decisions/repo-consolidation-strategy.md` for full details
