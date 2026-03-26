# 79-Repo Consolidation Analysis - Status Summary

**Date:** 2026-03-26  
**Lead:** Keaton  
**Status:** ⚠️ **BLOCKED** - Awaiting 79-repo list from Chad

---

## What You Asked For

Revise the 79-repo decomposition based on consolidation rules:
1. Private repos are source of truth
2. Consolidate `-delete` archived with public equivalents
3. Event variants → single base presentation + event mappings
4. Workshops → keep versions separate (don't consolidate)
5. Exclude `aspnetrazor-azure-blob` (utility, not presentation)

---

## What I've Delivered (Ready to Execute)

### ✅ Documents Created

1. **`repo-consolidation-strategy.md`**  
   - Detailed consolidation workflow
   - 5 revised GitHub issue templates (public-private, workshop versioning, event mapping, private-only, archived)
   - Edge case handling
   - Data conflict resolution rules

2. **`sample-consolidation-mapping.md`**  
   - EXAMPLE of what the final deliverable will look like
   - Sample mapping table (public-private pairs, workshop versions, event variants)
   - Sample priority ranking (top 10)
   - Sample GitHub issues using revised templates
   - Edge cases that may require your input

3. **`chad-request-repo-list.md`**  
   - Explains what I need from you to unblock
   - Options for how to provide the repo list

---

## What I Need from You to Proceed

**The full list of 79 repositories** in ANY format:

### Option 1: CSV/Excel/Text List
Minimum columns:
- Repo name
- Visibility (public/private)
- Type (presentation/workshop/utility)
- Version indicator (if workshop)

### Option 2: GitHub Organization Name
If repos are under an org (not your personal account):
- Provide org name → I'll query it directly

### Option 3: Previous Analysis Document
If there's an existing 79-repo breakdown:
- File path or link → I'll apply the new consolidation rules

---

## What You'll Get (1-2 Hours After I Receive the List)

### 1. Consolidation Mapping Table
```
Public Repo | Private Equivalent | Consolidated Slug | Workshop Version | Notes
------------|-------------------|-------------------|------------------|-------
[actual data for all 79 repos]
```

### 2. Updated Scope Numbers
- Total unique presentations (after consolidation)
- Workshop count (versioned separately)
- Event variant count
- Private-only presentations
- Public-private pairs

### 3. Priority Ranking
- Top 10 presentations for backfill
- Full prioritized list
- Rationale (recency, frequency, complexity, strategic value)

### 4. Backlog for Dallas
- GitHub issues ready to assign
- Uses revised templates (accounts for consolidation, source-of-truth, versioning)

### 5. Edge Cases for Your Review
- Any ambiguities discovered during mapping
- Repos that don't fit standard patterns
- Content conflicts (private vs. public differences)

---

## Current Squad Status

| Agent | Status | Next Action |
|-------|--------|-------------|
| **Keaton** | ⏸️ Blocked | Waiting for repo list |
| **Dallas** | ⏸️ Ready | Will execute once backlog is prioritized |
| **Fenster** | ✅ Idle | No deployment changes needed yet |
| **Hockney** | ⏸️ Ready | Will review content once Dallas extracts |
| **Ralph** | ⏸️ Ready | Will track issues once created |

---

## How to Unblock

Reply with the 79-repo list in **any format** (CSV, text, org name, or file path).

I'll immediately:
1. Apply consolidation rules
2. Generate mapping table
3. Calculate scope
4. Prioritize backlog
5. Create GitHub issues for Dallas

**ETA:** 1-2 hours from receiving the list.

---

**Keaton**  
Squad Lead, chadgreen.com
