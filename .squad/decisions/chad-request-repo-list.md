# Scope Analysis Request for Chad

**Date:** 2026-03-26  
**From:** Keaton (Squad Lead)  
**Re:** 79-Repo Consolidation - Need Full Repository List

---

## Context

You've requested a revised decomposition of the 79-repo inventory based on new consolidation rules:
1. Private repos as source of truth
2. Consolidate `-delete` archived with public equivalents
3. Event variants map to single presentations
4. Workshops keep versions separate
5. Exclude `aspnetrazor-azure-blob`

---

## Current Blocker

I need the **full list of 79 repositories** to perform the consolidation analysis. The GitHub API search returned only 2 public repos for `user:chadgreen`, which suggests:

1. The majority of repos may be under an **organization** (not your personal account)
2. Private repos require **different access** or aren't visible via public API
3. There may be a **pre-existing list** from a previous analysis I don't have access to

---

## What I Need from You

### Option 1: Provide the Full Repo List
If you have the 79-repo list from the original analysis, please provide it in any format:
- CSV/Excel export
- Text list
- GitHub organization URL (if repos are under an org)
- Previous analysis document

**Minimum info per repo:**
- Repository name
- Visibility (public/private)
- Type (presentation/workshop/utility/other)
- Any version indicators

### Option 2: Point Me to the Organization
If repos are under a GitHub organization:
- Organization name (e.g., `github.com/YourOrg`)
- I can query the org's repos (if I have access)

### Option 3: Grant Access to Previous Analysis
If there's an existing document with the 79-repo breakdown:
- File location or URL
- I'll apply the new consolidation rules to it

---

## What I've Prepared (Ready to Execute Once I Have the List)

1. **Consolidation Strategy Document** (`.squad/decisions/repo-consolidation-strategy.md`)
   - Detailed consolidation rules based on your Q&A
   - Workflow for categorizing and mapping repos
   - Edge case handling

2. **GitHub Issue Templates** (5 templates ready)
   - Public-private consolidated extraction
   - Workshop version tracking
   - Event mapping
   - Private-only extraction
   - Archived consolidation

3. **Analysis Framework** (ready to populate)
   - Consolidation mapping table
   - Scope calculation (unique presentations, workshops, event variants)
   - Priority ranking methodology
   - Data conflict resolution strategy

---

## Once You Provide the List, I Will Deliver:

### 1. Consolidation Mapping Table
```
Public Repo | Private Equivalent | Consolidated Into | Workshop Version? | Notes
------------|-------------------|-------------------|-------------------|-------
[repo1]     | [repo1-private]   | [slug]           | No                | Private source of truth
[workshop1] | -                 | [slug-v1]        | Yes (v1)          | Keep version separate
...
```

### 2. Updated Scope Numbers
- Total unique presentations (after consolidation)
- Workshop count (versioned separately)
- Event variant mappings
- Private-only presentations

### 3. Priority Ranking (Top 10)
Based on: Recency, delivery frequency, strategic value, complexity

### 4. Backlog for Dallas
- Sequenced GitHub issues for content extraction
- Accounts for consolidation, versioning, source-of-truth handling

### 5. Edge Cases & Questions
- Any ambiguities requiring your input
- Conflicts discovered during mapping

---

## Current Status

**Blocker:** Need 79-repo list to proceed  
**ETA After Receiving List:** 1-2 hours for full analysis  
**Squad Waiting:** Dallas (ready to extract once backlog is prioritized)

---

## How to Respond

Reply with:
1. The repo list (CSV, text, URL to org, or file path), OR
2. Instructions on how to access the original 79-repo analysis, OR
3. Confirmation that repos are in an organization (and the org name)

I'll immediately perform the consolidation analysis and deliver the revised plan.

**- Keaton**
