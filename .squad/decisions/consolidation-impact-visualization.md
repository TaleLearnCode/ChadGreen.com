# Consolidation Impact Visualization

**Purpose:** Show expected reduction from 79 repos to unique presentations  
**Date:** 2026-03-26  
**Status:** Estimates (will update with actual numbers after analysis)

---

## Before Consolidation (Original 79 Repos)

```
PUBLIC REPOS (50)                    PRIVATE REPOS (27)
├─ Presentations                    ├─ Presentations (private)
├─ Workshops                        ├─ Workshops (private)
├─ Event Variants                   └─ Private-only content
└─ Archived (-delete)               

EVENT-SPECIFIC (5)                   WORKSHOPS (4)
├─ Event variant repos              ├─ Version 1
└─ Duplicate base presentations     ├─ Version 2
                                    ├─ Version 3
ARCHIVED (2)                         └─ Version 4
└─ -delete suffixed repos           
                                    UTILITIES (1)
                                    └─ aspnetrazor-azure-blob

TOTAL: 79 repos
```

---

## After Consolidation (Estimated)

```
UNIQUE PRESENTATIONS (~45-50)
├─ Public-Private Consolidated (20)    ← 40 repos → 20 presentations
├─ Public-Only (15)                    ← 15 repos → 15 presentations
├─ Private-Only (7)                    ← 7 repos → 7 presentations
├─ Workshops (8 versions)              ← 8 repos → 8 presentations (kept separate)
├─ Archived Consolidated (2)           ← 4 repos → 2 presentations
└─ Event Base Presentations (3)        ← 9 repos → 3 presentations

EXCLUDED (1)
└─ Utilities                           ← 1 repo → 0 presentations

TOTAL: ~45-50 unique presentations
```

---

## Consolidation Breakdown (Estimated)

| Category | Repos In | Presentations Out | Reduction |
|----------|---------|-------------------|-----------|
| **Public-Private Pairs** | 40 | 20 | 50% |
| **Public-Only** | 15 | 15 | 0% |
| **Private-Only** | 7 | 7 | 0% |
| **Workshops (Versioned)** | 8 | 8 | 0% (kept separate) |
| **Archived-Public Pairs** | 4 | 2 | 50% |
| **Event Variants** | 9 | 3 | 67% |
| **Utilities** | 1 | 0 | 100% (excluded) |
| **TOTAL** | **79** | **~45-50** | **36-43% reduction** |

---

## Visual Consolidation Flow

### Example 1: Public-Private Pair
```
GenerativeAIForDotNetDevelopers (public)     ─┐
                                              ├─→ gen-ai-dotnet.md (1 presentation)
GenerativeAIForDotNetDevelopers-private (priv)┘   
                                                  Source: Private (authoritative)
```

### Example 2: Event Variants
```
IntroToAzure (public, base)                  ─┐
IntroToAzure-NDC (event variant)             ─┤
IntroToAzure-CodeMash (event variant)        ─┼─→ intro-azure.md (1 presentation)
IntroToAzure-StirTrek (event variant)        ─┘   + 3 engagementPresentations entries
                                                  Source: Base repo
```

### Example 3: Workshop Versions (NOT Consolidated)
```
BuildingModernWebApps-v1 ───→ modern-webapps-v1.md (presentation 1)
BuildingModernWebApps-v2 ───→ modern-webapps-v2.md (presentation 2)
BuildingModernWebApps-v3 ───→ modern-webapps-v3.md (presentation 3)

Each version = separate presentation (per Chad's directive)
```

### Example 4: Archived-Public Consolidation
```
AspNetCoreFromScratch (public)               ─┐
                                              ├─→ aspnet-core-scratch.md (1 presentation)
AspNetCoreFromScratch-delete (archived)      ─┘   
                                                  Source: Public (archived is historical)
```

### Example 5: Private-Only
```
KubernetesBasics-private (no public match) ───→ kubernetes-basics.md (1 presentation)
                                                Source: Private (privacy check required)
```

### Example 6: Excluded
```
aspnetrazor-azure-blob (utility) ───→ [EXCLUDED - 0 presentations]
```

---

## Scope Impact Summary

### Original Scope
- **79 repos** to process
- Mix of public, private, archived, event variants, workshops, utilities

### Consolidated Scope (Estimated)
- **~45-50 unique presentations** to extract
- **36-43% reduction** through consolidation
- **8 workshops** (versioned separately, not consolidated)
- **20 public-private pairs** (private is authoritative)
- **3 event base presentations** (with N event mappings)
- **1 excluded utility** (aspnetrazor-azure-blob)

### Effort Savings for Dallas
- **Before consolidation:** 79 repos × 1 hour avg = 79 hours
- **After consolidation:** ~47 presentations × 1 hour avg = 47 hours
- **Time saved:** ~32 hours (40% reduction in extraction work)
- **Quality gain:** Private content is authoritative (no guesswork on which repo to use)

---

## Content Distribution (Estimated)

```
Presentation Types After Consolidation:

Sessions (Standard Talks)          ~35 presentations (75%)
├─ Public-Private Consolidated     ~18
├─ Public-Only                     ~12
└─ Private-Only                    ~5

Workshops (Multi-Lab Sessions)     ~8 presentations (17%)
├─ Version 1                       ~3
├─ Version 2                       ~3
└─ Version 3+                      ~2

Lightning Talks                    ~2 presentations (4%)

Keynotes                           ~1 presentation (2%)

Panels/Webinars                    ~1 presentation (2%)

TOTAL                              ~47 unique presentations
```

---

## Priority Distribution (Estimated)

```
High Priority (Top 10)             ~10 presentations (21%)
├─ Recent (2023-2024)              ~6
├─ Flagship/Signature Talks        ~3
└─ Complex Workshops               ~1

Medium Priority                    ~20 presentations (43%)
├─ Frequently Delivered            ~12
├─ Evergreen Topics                ~8

Low Priority                       ~17 presentations (36%)
├─ Older Content (pre-2023)        ~10
├─ One-time Deliveries             ~5
└─ Retired/Deprecated              ~2
```

---

## Event Mapping Impact (Estimated)

```
Event Variants Consolidated:

Base Presentation: intro-azure
├─ IntroToAzure-NDC              → Event: NDC London 2023
├─ IntroToAzure-CodeMash         → Event: CodeMash 2024
└─ IntroToAzure-StirTrek         → Event: Stir Trek 2024

Result: 1 presentation + 3 event mappings (not 4 separate presentations)

Estimated Total Event Mappings:
- ~3 base presentations with event variants
- ~9 event-specific repos consolidated
- ~15-20 total event mappings created (engagementPresentations)
```

---

## Data Structure After Consolidation

```
src/content/
├─ presentations/                  (~47 files)
│  ├─ gen-ai-dotnet.md            (Public-Private consolidated)
│  ├─ intro-azure.md              (Event variants consolidated)
│  ├─ modern-webapps-v1.md        (Workshop v1, kept separate)
│  ├─ modern-webapps-v2.md        (Workshop v2, kept separate)
│  ├─ kubernetes-basics.md        (Private-only)
│  └─ ...
│
├─ events/                         (existing + new)
│  ├─ ndc-london-2023.md
│  ├─ codemash-2024.md
│  └─ ...
│
└─ engagementPresentations/        (~15-20 new mappings)
   ├─ intro-azure-ndc-2023.md     (Links intro-azure → NDC event)
   ├─ intro-azure-codemash-2024.md
   └─ ...
```

---

## Quality Improvements from Consolidation

1. **Single Source of Truth**
   - Private repos are authoritative (no conflicting content)
   - Clear source hierarchy (private > public > archived)

2. **Reduced Duplication**
   - Event variants consolidated (avoid duplicate abstracts)
   - Public-private pairs merged (one presentation, not two)

3. **Versioning Clarity**
   - Workshop versions tracked separately (lab evolution documented)
   - Related versions linked in frontmatter

4. **Privacy Management**
   - Private-only content flagged for NDA review
   - Clear visibility on what's public vs. private source

5. **Content Accuracy**
   - Most recent content prioritized (private repos often newer)
   - Historical context preserved (archived repos noted)

---

## Next Steps (After Analysis)

Once Chad provides the 79-repo list:

1. ✅ Apply rules → Generate actual mapping table
2. ✅ Calculate precise scope (replace estimates)
3. ✅ Produce priority ranking (top 10 + full list)
4. ✅ Create GitHub issues for Dallas
5. ✅ Flag edge cases for Chad review
6. ✅ Hand off backlog to Dallas

**ETA:** 1-2 hours from receiving list

---

**Status:** Estimates based on consolidation rules  
**Will Update:** After analyzing actual 79-repo inventory  
**Prepared By:** Keaton
