# Consolidation Process Walkthrough (Example)

**Purpose:** Demonstrate how consolidation rules will be applied in practice  
**Audience:** Chad Green  
**Status:** Hypothetical example (will be replaced with actual data)

---

## Scenario: 10 Sample Repos from the 79 Total

Assume these 10 repos exist in Chad's GitHub inventory:

```
1. GenerativeAIForDotNetDevelopers (public)
2. GenerativeAIForDotNetDevelopers-private (private)
3. AspNetCoreFromScratch (public)
4. AspNetCoreFromScratch-delete (public, archived)
5. BuildingModernWebApps-v1 (public)
6. BuildingModernWebApps-v2 (public)
7. IntroToAzure (public)
8. IntroToAzure-NDC (public)
9. KubernetesBasics-private (private)
10. aspnetrazor-azure-blob (public, utility)
```

---

## Step 1: Apply Consolidation Rules

### Rule 1: Match Public-Private Pairs
**Repos 1 & 2:** `GenerativeAIForDotNetDevelopers` (public) + `GenerativeAIForDotNetDevelopers-private` (private)
- **Action:** Consolidate into single presentation
- **Source of Truth:** Private repo
- **Result:** 1 presentation, not 2

### Rule 2: Consolidate Archived with Public
**Repos 3 & 4:** `AspNetCoreFromScratch` (public) + `AspNetCoreFromScratch-delete` (archived)
- **Action:** Consolidate into single presentation
- **Source of Truth:** Public repo (archived is historical)
- **Result:** 1 presentation, note archived content

### Rule 3: Workshop Versions Stay Separate
**Repos 5 & 6:** `BuildingModernWebApps-v1` and `BuildingModernWebApps-v2`
- **Action:** Keep as separate presentations
- **Reason:** Labs evolve, versions must be tracked
- **Result:** 2 presentations (v1 and v2)

### Rule 4: Event Variants Consolidate to Base
**Repos 7 & 8:** `IntroToAzure` (public) + `IntroToAzure-NDC` (public)
- **Action:** Single base presentation + event mapping
- **Source of Truth:** Base repo (`IntroToAzure`)
- **Result:** 1 presentation, multiple event links

### Rule 5: Private-Only Stays As-Is
**Repo 9:** `KubernetesBasics-private` (no public equivalent)
- **Action:** Extract as-is from private repo
- **Privacy Check:** Review for NDA/corporate content
- **Result:** 1 presentation (private-only)

### Rule 6: Exclude Utilities
**Repo 10:** `aspnetrazor-azure-blob` (utility)
- **Action:** Exclude entirely
- **Reason:** Not a presentation (per Chad's directive)
- **Result:** 0 presentations

---

## Step 2: Consolidation Mapping

| # | Public Repo | Private Repo | Archived | Consolidated Slug | Count | Type | Notes |
|---|-------------|--------------|----------|-------------------|-------|------|-------|
| 1 | GenerativeAIForDotNetDevelopers | GenerativeAIForDotNetDevelopers-private | - | gen-ai-dotnet | 1 | session | Private source of truth |
| 2 | AspNetCoreFromScratch | - | AspNetCoreFromScratch-delete | aspnet-core-scratch | 1 | session | Archived consolidated |
| 3 | BuildingModernWebApps-v1 | - | - | modern-webapps-v1 | 1 | workshop | Keep version separate |
| 4 | BuildingModernWebApps-v2 | - | - | modern-webapps-v2 | 1 | workshop | Keep version separate |
| 5 | IntroToAzure | - | - | intro-azure | 1 | session | Event variants consolidated |
| 6 | IntroToAzure-NDC | - | - | intro-azure | 0 | event-variant | Maps to base presentation |
| 7 | KubernetesBasics-private | - | - | kubernetes-basics | 1 | session | Private-only |
| 8 | aspnetrazor-azure-blob | - | - | EXCLUDED | 0 | utility | Not a presentation |

**Original Repo Count:** 10  
**Unique Presentations After Consolidation:** 6  
**Reduction:** 40% (10 → 6)

---

## Step 3: Content Extraction Plan

### Presentation 1: gen-ai-dotnet
**Dallas Tasks:**
1. Clone both `GenerativeAIForDotNetDevelopers` (public) and `GenerativeAIForDotNetDevelopers-private` (private)
2. Extract from **private** repo (authoritative):
   - Abstract from README or slides
   - Description, level (intermediate/advanced), type (session)
   - Tags: AI, .NET, Azure OpenAI, Semantic Kernel
3. Cross-check **public** repo for:
   - Demo code (may be more polished in public)
   - Sample projects (reference in frontmatter)
4. Create `src/content/presentations/gen-ai-dotnet.md`
5. Set frontmatter:
   ```yaml
   title: "Generative AI for .NET Developers"
   slug: "gen-ai-dotnet"
   type: "session"
   level: "intermediate"
   status: "active"
   sourceRepos:
     - "https://github.com/chadgreen/GenerativeAIForDotNetDevelopers-private"
     - "https://github.com/chadgreen/GenerativeAIForDotNetDevelopers"
   tags: ["AI", ".NET", "Azure OpenAI", "Semantic Kernel"]
   ```

**Expected Outcome:** Single consolidated markdown file with private content as base, public resources noted.

---

### Presentation 2: aspnet-core-scratch
**Dallas Tasks:**
1. Clone `AspNetCoreFromScratch` (public)
2. Review `AspNetCoreFromScratch-delete` (archived) for historical context
3. Extract from **public** repo (current):
   - Abstract, description, level, type
4. Note archived content in frontmatter:
   ```yaml
   sourceRepos:
     - "https://github.com/chadgreen/AspNetCoreFromScratch"
     - "https://github.com/chadgreen/AspNetCoreFromScratch-delete" (archived)
   status: "retired"  # or "active" if still delivered
   ```
5. If archived has updates public doesn't, flag for Chad review

**Expected Outcome:** Single consolidated entry, archived content noted.

---

### Presentations 3 & 4: modern-webapps-v1 and modern-webapps-v2
**Dallas Tasks (SEPARATE for each version):**

**v1:**
1. Clone `BuildingModernWebApps-v1`
2. Extract abstract (not lab-by-lab details)
3. Document lab structure in description:
   - "Workshop consists of 5 hands-on labs covering..."
4. Create `src/content/presentations/modern-webapps-v1.md`
5. Set frontmatter:
   ```yaml
   type: "workshop"
   version: 1
   labCount: 5
   relatedPresentations: ["modern-webapps-v2"]
   status: "retired"  # if superseded by v2
   ```

**v2:**
1. Clone `BuildingModernWebApps-v2`
2. Extract abstract
3. Document lab structure (7 labs, updated for .NET 6)
4. Create `src/content/presentations/modern-webapps-v2.md` (SEPARATE FILE)
5. Set frontmatter:
   ```yaml
   type: "workshop"
   version: 2
   labCount: 7
   relatedPresentations: ["modern-webapps-v1"]
   status: "active"
   ```

**Expected Outcome:** TWO separate presentation files (do NOT consolidate workshop versions).

---

### Presentation 5: intro-azure + Event Mappings
**Dallas Tasks:**
1. Clone `IntroToAzure` (base repo)
2. Extract abstract from base (ignore `IntroToAzure-NDC`, that's just event variant)
3. Create `src/content/presentations/intro-azure.md`
4. **Event Mapping:** Create event entries and engagement presentations:
   - Identify events where this talk was delivered (NDC, others)
   - Create `src/content/events/ndc-london-2023.md` (if not exist)
   - Create `src/content/engagementPresentations/intro-azure-ndc-2023.md`
   - Link via `presentationId: intro-azure` and `eventId: ndc-london-2023`

**Expected Outcome:** 
- 1 base presentation
- N event entries
- N engagement presentations (one per event delivery)

---

### Presentation 6: kubernetes-basics (Private-Only)
**Dallas Tasks:**
1. Clone `KubernetesBasics-private`
2. Extract abstract, description, level, type
3. **Privacy Check:**
   - Review for corporate/NDA content
   - If sensitive, flag for Chad review before publishing
4. Create `src/content/presentations/kubernetes-basics.md`
5. Set frontmatter:
   ```yaml
   sourceRepos: ["https://github.com/chadgreen/KubernetesBasics-private"]
   visibility: "private-source"  # custom field to note origin
   ```

**Expected Outcome:** Single presentation from private repo, privacy-checked.

---

## Step 4: Scope Calculation

### Original Input
- 10 repos analyzed

### Consolidation Results
| Category | Count | Notes |
|----------|-------|-------|
| Public-Private Pairs | 1 | GenerativeAI (consolidated) |
| Archived-Public Pairs | 1 | AspNetCore (consolidated) |
| Workshop Versions | 2 | v1 and v2 (kept separate) |
| Event Variants | 1 | IntroToAzure-NDC (consolidated to base) |
| Private-Only | 1 | KubernetesBasics |
| Excluded | 1 | aspnetrazor-azure-blob |

### Final Scope
- **Unique Presentations:** 6
- **Workshops (Versioned):** 2
- **Sessions:** 4
- **Private-Only:** 1
- **Public-Private Consolidated:** 1
- **Event Mappings:** 1 base → N events

---

## Step 5: Priority Ranking (Sample Top 6)

| Rank | Presentation Slug | Reason |
|------|------------------|--------|
| 1 | gen-ai-dotnet | Recent, high-demand topic, private content (authoritative) |
| 2 | modern-webapps-v2 | Latest workshop version, complex labs |
| 3 | kubernetes-basics | Private-only, strategic topic, needs privacy review |
| 4 | intro-azure | Multiple event deliveries, map to events |
| 5 | modern-webapps-v1 | Track workshop evolution |
| 6 | aspnet-core-scratch | Foundational topic, archived content noted |

---

## Step 6: GitHub Issues Created

### Issue #1
```
Title: Extract: Generative AI for .NET Developers (Public-Private Consolidated)
Assigned: Dallas
Priority: High
Template: Public-Private Consolidated Extraction
Source Repos: GenerativeAIForDotNetDevelopers (public), GenerativeAIForDotNetDevelopers-private (private)
Tasks: [See template in repo-consolidation-strategy.md]
```

### Issue #2
```
Title: Extract: Building Modern Web Apps - v1 (Workshop)
Assigned: Dallas
Priority: Medium
Template: Workshop Version Tracking
Source Repo: BuildingModernWebApps-v1
Tasks: [See template in repo-consolidation-strategy.md]
```

### Issue #3
```
Title: Extract: Building Modern Web Apps - v2 (Workshop)
Assigned: Dallas
Priority: High
Template: Workshop Version Tracking
Source Repo: BuildingModernWebApps-v2
Tasks: [See template in repo-consolidation-strategy.md]
```

### Issue #4
```
Title: Map: Intro to Azure → Event Variants
Assigned: Dallas
Priority: Medium
Template: Event Mapping
Base Presentation: intro-azure
Events: [NDC London 2023, others TBD]
Tasks: [See template in repo-consolidation-strategy.md]
```

### Issue #5
```
Title: Extract: Kubernetes Basics (Private-Only)
Assigned: Dallas
Priority: High
Template: Private-Only Extraction
Source Repo: KubernetesBasics-private
Tasks: [See template in repo-consolidation-strategy.md]
Special: Privacy review required
```

### Issue #6
```
Title: Consolidate: ASP.NET Core From Scratch (Archived + Public)
Assigned: Dallas
Priority: Low
Template: Archived Consolidation
Source Repos: AspNetCoreFromScratch (public), AspNetCoreFromScratch-delete (archived)
Tasks: [See template in repo-consolidation-strategy.md]
```

---

## Key Takeaways from This Example

1. **10 repos → 6 presentations** (40% reduction through consolidation)
2. **Workshop versions stay separate** (v1 and v2 are distinct presentations)
3. **Event variants consolidate** (IntroToAzure-NDC maps to IntroToAzure base)
4. **Private repos are authoritative** (GenerativeAI uses private content)
5. **Utilities excluded** (aspnetrazor-azure-blob doesn't create a presentation)
6. **Privacy checks flagged** (KubernetesBasics-private needs review before publish)

---

## Applying This to All 79 Repos

Once Chad provides the full list:
1. **Categorize each repo** (public/private/archived/workshop/event-variant/utility)
2. **Match pairs** (public-private, archived-public)
3. **Apply consolidation rules** (as demonstrated above)
4. **Generate mapping table** (79 rows → N unique presentations)
5. **Calculate scope** (unique count, workshops, event variants, private-only)
6. **Prioritize** (top 10, then full list)
7. **Create GitHub issues** (using revised templates)
8. **Hand off to Dallas** for execution

**ETA:** 1-2 hours after receiving the 79-repo list.

---

**Status:** Example walkthrough complete  
**Next:** Waiting for full repo list from Chad  
**Prepared By:** Keaton (Squad Lead)
