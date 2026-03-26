# Sample Consolidation Mapping
**Purpose:** Demonstrate expected output format once Chad provides full 79-repo list  
**Status:** EXAMPLE ONLY - Replace with actual data after receiving repo inventory

---

## Consolidation Mapping Table

| Public Repo | Private Equivalent | Archived | Consolidated Slug | Workshop Version | Type | Notes |
|-------------|-------------------|----------|-------------------|------------------|------|-------|
| GenerativeAIForDotNetDevelopers | GenerativeAIForDotNetDevelopers-private | - | gen-ai-dotnet | No | session | Private is source of truth |
| AspNetCoreFromScratch | - | AspNetCoreFromScratch-delete | aspnet-core-scratch | No | session | Consolidate archived with public |
| BuildingModernWebApps-v1 | - | - | modern-webapps-v1 | Yes (v1) | workshop | Keep version separate |
| BuildingModernWebApps-v2 | - | - | modern-webapps-v2 | Yes (v2) | workshop | Keep version separate |
| SecuringYourApps | SecuringYourApps-private | - | securing-apps | No | session | Private is source of truth |
| IntroToAzure-NDC | - | - | intro-azure | No | session | Event variant of IntroToAzure |
| IntroToAzure-CodeMash | - | - | intro-azure | No | session | Event variant (same presentation) |
| DockerForDevelopers | - | - | docker-developers | No | session | Public only |
| KubernetesBasics-private | - | - | kubernetes-basics | No | session | Private only |
| aspnetrazor-azure-blob | - | - | EXCLUDED | - | utility | Not a presentation (per Chad) |
| ...continuing for all 79... | | | | | | |

**Consolidation Summary:**
- **Total Repos Analyzed:** 79
- **Unique Presentations After Consolidation:** ~45 (estimated)
- **Public-Private Pairs:** ~20 (estimated)
- **Private-Only Presentations:** ~7 (estimated)
- **Workshops (Versioned Separately):** ~8 (estimated)
- **Event Variants (Consolidated to Base):** ~12 (estimated)
- **Excluded (Utilities):** 1 confirmed (aspnetrazor-azure-blob)

---

## Priority Ranking (Top 10 - Sample)

| Rank | Presentation Slug | Source Repos | Type | Priority Reason |
|------|------------------|--------------|------|-----------------|
| 1 | gen-ai-dotnet | Public + Private | Session | Recent, high-demand topic, flagship talk |
| 2 | modern-webapps-v2 | Public | Workshop | Latest version, complex labs, frequently delivered |
| 3 | securing-apps | Public + Private | Session | Evergreen topic, private has updated content |
| 4 | kubernetes-basics | Private only | Session | Strategic topic, private-only (NDA check needed) |
| 5 | docker-developers | Public | Session | Popular, frequently requested |
| 6 | modern-webapps-v1 | Public | Workshop | Previous version, track evolution |
| 7 | intro-azure | Public (+ event variants) | Session | Multiple deliveries, map to events |
| 8 | aspnet-core-scratch | Public + Archived | Session | Foundational, consolidate historical |
| 9 | api-design-best-practices | Public + Private | Session | Professional dev focus |
| 10 | cloud-migration-strategies | Private only | Session | Corporate audience, high value |

**Priority Criteria:**
1. **Recency** - Topics from 2023-2024 prioritized
2. **Frequency** - Talks delivered 5+ times rank higher
3. **Strategic Value** - Flagship/signature talks prioritized
4. **Complexity** - Workshops prioritized due to lab tracking needs
5. **Private Content** - Private-only or private-public pairs ranked higher (authoritative content)

---

## Workshop Version Tracking (Sample)

| Workshop Base | Version | Repo | Labs Count | Notes |
|--------------|---------|------|------------|-------|
| Building Modern Web Apps | v1 | BuildingModernWebApps-v1 | 5 labs | Initial version, 2021 |
| Building Modern Web Apps | v2 | BuildingModernWebApps-v2 | 7 labs | Updated for .NET 6, 2023 |
| Building Modern Web Apps | v3 | BuildingModernWebApps-v3 | 7 labs | .NET 8 migration, 2024 |
| Kubernetes for Developers | v1 | K8s-Developers-v1 | 4 labs | Basic cluster operations |
| Kubernetes for Developers | v2 | K8s-Developers-v2 | 6 labs | Added service mesh, Helm |

**Per Chad's Directive:** Each version gets separate presentation entry. Track lab evolution in description.

---

## Event Mapping Examples (Sample)

### Example 1: Single Presentation, Multiple Events
**Base Presentation:** `intro-azure`  
**Source Repo:** `IntroToAzure` (public)

**Event Mappings:**
- **Event 1:** NDC London 2023 → Create `engagementPresentations/intro-azure-ndc-2023.md`
- **Event 2:** CodeMash 2024 → Create `engagementPresentations/intro-azure-codemash-2024.md`
- **Event 3:** Stir Trek 2024 → Create `engagementPresentations/intro-azure-stirtrek-2024.md`

**Dallas Action:** Create ONE `presentations/intro-azure.md` + THREE `engagementPresentations/` entries linking to respective events.

### Example 2: Workshop Delivered Multiple Times
**Base Presentation:** `modern-webapps-v2`  
**Source Repo:** `BuildingModernWebApps-v2` (public)

**Event Mappings:**
- **Event 1:** TechBash 2023 Workshop → `engagementPresentations/modern-webapps-v2-techbash-2023.md`
- **Event 2:** Music City Tech 2024 Workshop → `engagementPresentations/modern-webapps-v2-musiccitytech-2024.md`

**Dallas Action:** Create ONE `presentations/modern-webapps-v2.md` (workshop type) + TWO `engagementPresentations/` entries.

---

## Public-Private Consolidation Examples

### Example 1: Both Exist, Content Differs
**Public Repo:** `GenerativeAIForDotNetDevelopers`  
- Abstract: "Learn to build AI-powered .NET apps with OpenAI and Azure"
- Last updated: 2023-11

**Private Repo:** `GenerativeAIForDotNetDevelopers-private`  
- Abstract: "Build production-ready AI apps in .NET using Azure OpenAI, Semantic Kernel, and RAG patterns"
- Last updated: 2024-02

**Dallas Action:**  
1. Use **private abstract** (more detailed, more recent)
2. Cross-check public repo for demos/sample code
3. Create `presentations/gen-ai-dotnet.md` with private content as base
4. Note both repos in frontmatter: `sourceRepos: [private-url, public-url]`

### Example 2: Private Exists, Public is Minimal
**Public Repo:** `SecuringYourApps` (basic README only)  
**Private Repo:** `SecuringYourApps-private` (full abstract, slides, notes)

**Dallas Action:**  
1. Extract from private (authoritative)
2. Note public exists but has minimal content
3. Single consolidated entry with private as source

### Example 3: Private Only (No Public Match)
**Private Repo:** `KubernetesBasics-private` (no public equivalent)

**Dallas Action:**  
1. Extract from private repo
2. **Privacy check:** Review for NDA/corporate-sensitive content before publishing to public site
3. Flag for Chad review if uncertain
4. Create `presentations/kubernetes-basics.md` with `sourceRepos: [private-url]`

---

## GitHub Issue Examples (Revised Templates Applied)

### Issue #1: Extract Public-Private Consolidated
```markdown
**Title:** Extract: Generative AI for .NET Developers (Public-Private Consolidated)

**Type:** Content Extraction  
**Assigned To:** Dallas  
**Priority:** High

**Source Repos:**
- Private (authoritative): `GenerativeAIForDotNetDevelopers-private`
- Public (supplemental): `GenerativeAIForDotNetDevelopers`

**Tasks:**
- [ ] Clone both repos
- [ ] Extract abstract, description, level, type from **private** repo
- [ ] Cross-check public repo for demo code, additional resources
- [ ] Resolve conflicts (private wins)
- [ ] Create `src/content/presentations/gen-ai-dotnet.md`
- [ ] Set frontmatter: `sourceRepos: [private-url, public-url]`
- [ ] Add tags: `AI`, `.NET`, `Azure OpenAI`, `Semantic Kernel`
- [ ] Set status: `active`

**Consolidation Notes:**
- Private repo has more recent abstract (2024-02 vs 2023-11)
- Use private content as base
- Public repo may have sample code to reference

**Acceptance Criteria:**
- Single consolidated markdown file created
- Private content is authoritative
- Both repos noted in frontmatter
- Passes schema validation
```

### Issue #2: Extract Workshop with Version Tracking
```markdown
**Title:** Extract: Building Modern Web Apps - v2 (Workshop)

**Type:** Content Extraction (Workshop)  
**Assigned To:** Dallas  
**Priority:** High

**Source Repo:** `BuildingModernWebApps-v2` (public)  
**Version:** v2  
**Lab Count:** 7 labs

**Tasks:**
- [ ] Clone repo
- [ ] Extract workshop abstract (high-level, not lab-by-lab details)
- [ ] Document lab structure in description (Lab 1: Setup, Lab 2: Components, etc.)
- [ ] Note version-specific features (.NET 6, updated for 2023)
- [ ] Create `src/content/presentations/modern-webapps-v2.md`
- [ ] Set `type: workshop`, `version: 2`
- [ ] Add related versions in notes: v1 (deprecated), v3 (if exists)

**Version Tracking Notes:**
- Do NOT consolidate with v1 or v3
- Keep as separate presentation entry
- Track lab evolution in description: "Updated from v1 with 2 additional labs covering Docker deployment and CI/CD"

**Acceptance Criteria:**
- Workshop entry created with version indicator
- Lab structure documented in description
- Related versions noted (for context)
- Passes schema validation
```

### Issue #3: Map Presentation to Multiple Events
```markdown
**Title:** Map: Intro to Azure → 3 Event Variants

**Type:** Event Mapping  
**Assigned To:** Dallas  
**Priority:** Medium

**Base Presentation:** `intro-azure` (already extracted)

**Events to Map:**
1. **NDC London 2023** (2023-05-15)
   - Venue: QE II Centre, London
   - Event slug: `ndc-london-2023`
   
2. **CodeMash 2024** (2024-01-11)
   - Venue: Kalahari Resort, Sandusky OH
   - Event slug: `codemash-2024`
   
3. **Stir Trek 2024** (2024-05-03)
   - Venue: Marcus Crosswoods Cinema, Columbus OH
   - Event slug: `stirtrek-2024`

**Tasks:**
- [ ] Ensure base presentation exists: `src/content/presentations/intro-azure.md`
- [ ] Create event entries (if not exist):
  - `src/content/events/ndc-london-2023.md`
  - `src/content/events/codemash-2024.md`
  - `src/content/events/stirtrek-2024.md`
- [ ] Create engagement presentations:
  - `src/content/engagementPresentations/intro-azure-ndc-2023.md`
  - `src/content/engagementPresentations/intro-azure-codemash-2024.md`
  - `src/content/engagementPresentations/intro-azure-stirtrek-2024.md`
- [ ] Link via `presentationId: intro-azure` and `eventId: [event-slug]`
- [ ] Note any event-specific customizations (time changes, audience level adjustments)

**Acceptance Criteria:**
- Base presentation exists (no duplicates)
- All 3 events have entries
- All 3 engagement presentations link correctly
- Schema validation passes
```

---

## Edge Cases Identified (Require Chad Input)

### 1. Private Repo with No README or Minimal Content
**Example:** Private repo exists but only has slides (no abstract)  
**Question:** Should Dallas:
- Skip it (wait for Chad to add content)?
- Extract what exists and flag as incomplete?
- Use public version if more complete?

**Recommended:** Extract what exists, flag as incomplete, note in GitHub issue for Chad review.

---

### 2. Archived Repo Newer Than Public
**Example:** `PresentationName-delete` has 2024 content, `PresentationName` (public) has 2022 content  
**Question:** Which is authoritative?  
**Assumption:** Public is current, archived is historical (despite dates). Consolidate into public, note archived had updates.

**Chad Input Needed:** Confirm assumption or clarify rule.

---

### 3. Workshop Without Clear Version Indicator
**Example:** `DockerWorkshop` and `DockerWorkshop-Updated` exist  
**Question:** Are these versions or separate workshops?  
**Assumption:** Treat as versions (v1 and v2), keep separate per workshop rule.

**Chad Input Needed:** Provide version identification guidance for ambiguous cases.

---

### 4. Event-Specific Repo with Substantially Different Content
**Example:** `IntroToKubernetes-NDC` has different abstract than `IntroToKubernetes`  
**Question:** Consolidate as event variant or separate presentation?  
**Threshold:** If abstract differs by >50%, treat as separate presentation.

**Chad Input Needed:** Confirm threshold or provide examples of true variants vs. separate talks.

---

### 5. Utility Repos Beyond `aspnetrazor-azure-blob`
**Question:** Are there other repos to exclude?  
**Examples to Check:**
- Repos with names like `scripts`, `tools`, `utilities`, `config`
- Repos that are GitHub Actions workflows
- Repos that are website/portfolio code (not presentation content)

**Chad Input Needed:** List any other non-presentation repos to exclude.

---

## Next Actions

### Immediate (Once Chad Provides Repo List)
1. **Keaton:** Apply consolidation rules to full 79-repo list
2. **Keaton:** Generate actual mapping table (replacing this sample)
3. **Keaton:** Calculate precise scope numbers
4. **Keaton:** Produce priority ranking (top 10, then full list)
5. **Keaton:** Create GitHub issues for Dallas (using revised templates)

### Post-Analysis
6. **Chad:** Review consolidation mapping for accuracy
7. **Chad:** Clarify edge cases (as flagged)
8. **Keaton:** Finalize backlog based on Chad's feedback
9. **Dallas:** Begin extraction per prioritized issues

---

**Status:** SAMPLE ONLY - Replace with actual data  
**Blocking Item:** Need full 79-repo list from Chad  
**ETA:** 1-2 hours after receiving list
