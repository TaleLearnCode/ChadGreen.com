# 79-Repo Consolidation Analysis - Document Index

**Date:** 2026-03-26  
**Lead:** Keaton  
**Status:** Framework Complete, Awaiting Repo List from Chad

---

## 📖 How to Navigate This Work

### 🚀 START HERE
1. **`CHAD-READ-THIS-FIRST.md`** (root directory)
   - 2-minute executive summary
   - Current blocker and what I need
   - What you'll get once unblocked
   - Squad status

### 📋 Quick Reference
2. **`.squad/decisions/QUICK-REFERENCE-consolidation-rules.md`**
   - One-page decision table
   - Consolidation logic flow diagram
   - Conflict resolution rules
   - Dallas extraction checklists

### 📚 Detailed Documents

#### Strategy & Framework
3. **`.squad/decisions/repo-consolidation-strategy.md`**
   - Complete consolidation workflow
   - 5 GitHub issue templates (revised for consolidation)
   - Edge case handling
   - Repository list template (CSV format)

#### Examples & Walkthroughs
4. **`.squad/decisions/consolidation-walkthrough-example.md`**
   - Step-by-step walkthrough using 10 sample repos
   - Demonstrates consolidation in action
   - Shows 10 repos → 6 unique presentations (40% reduction)
   - Detailed Dallas tasks for each presentation type

5. **`.squad/decisions/sample-consolidation-mapping.md`**
   - Example of final deliverables
   - Sample mapping table, scope calculations, priority ranking
   - Sample GitHub issues using revised templates
   - Edge cases flagged for review

#### Status & Requests
6. **`.squad/decisions/SUMMARY-consolidation-status.md`**
   - Executive summary (quick-read format)
   - Current status, blockers, deliverables ready
   - Next actions and ETA

7. **`.squad/decisions/chad-request-repo-list.md`**
   - Detailed explanation of blocker
   - Options for providing the repo list
   - What happens after I receive it

---

## 📊 Deliverables Status

| Deliverable | Status | Location |
|-------------|--------|----------|
| **Consolidation Strategy** | ✅ Complete | `repo-consolidation-strategy.md` |
| **GitHub Issue Templates** | ✅ Complete (5 templates) | `repo-consolidation-strategy.md` |
| **Quick Reference Guide** | ✅ Complete | `QUICK-REFERENCE-consolidation-rules.md` |
| **Example Walkthrough** | ✅ Complete | `consolidation-walkthrough-example.md` |
| **Sample Deliverables** | ✅ Complete | `sample-consolidation-mapping.md` |
| **Consolidation Mapping Table** | ⏸️ Awaiting Repo List | N/A |
| **Scope Calculations** | ⏸️ Awaiting Repo List | N/A |
| **Priority Ranking** | ⏸️ Awaiting Repo List | N/A |
| **GitHub Issues for Dallas** | ⏸️ Awaiting Repo List | N/A |

---

## 🎯 Consolidation Rules Summary

1. **Private Repos = Source of Truth**
   - When public and private versions exist, use private content
   - Consolidate into single presentation

2. **Archived Repos Consolidate with Public**
   - `-delete` suffix repos merge with public equivalents
   - Public is current, archived is historical

3. **Event Variants → Base Presentation**
   - Single base presentation entry
   - Multiple `engagementPresentations` linking to events
   - Avoid duplicate content

4. **Workshop Versions Stay Separate**
   - Each version (v1, v2, v3) is a distinct presentation
   - Track lab evolution independently
   - **Do NOT consolidate workshop versions**

5. **Exclude Utilities**
   - `aspnetrazor-azure-blob` confirmed excluded
   - Other non-presentation repos (scripts, tools, config)

---

## 🔄 Workflow Once Unblocked

```
1. Chad provides 79-repo list
   ↓
2. Keaton applies consolidation rules
   ↓
3. Keaton generates mapping table
   ↓
4. Keaton calculates scope (unique presentations, workshops, event variants)
   ↓
5. Keaton produces priority ranking (top 10, then full list)
   ↓
6. Keaton creates GitHub issues for Dallas
   ↓
7. Keaton flags edge cases for Chad review
   ↓
8. Chad reviews and approves
   ↓
9. Dallas executes extraction per prioritized backlog
   ↓
10. Hockney validates content quality
   ↓
11. Ralph tracks progress
```

**ETA:** Steps 2-7 take 1-2 hours after receiving repo list

---

## 📝 What I Need from Chad

**The full 79-repo list** in ANY format:

### Option 1: CSV/Excel/Text
Minimum columns:
- Repo name
- Visibility (public/private)
- Type (presentation/workshop/utility)
- Version indicator (if workshop)

### Option 2: GitHub Organization
If repos are under an org:
- Organization name → I'll query it

### Option 3: Previous Analysis
If there's an existing breakdown:
- File path or URL → I'll apply new rules

---

## 🚧 Current Blocker

Cannot proceed without the full repo list because:
- GitHub API search only returned 2 public repos from `chadgreen` user account
- Other 77 repos are likely under an organization or private
- Need complete inventory to apply consolidation rules and calculate scope

---

## 📅 Timeline

| Milestone | Status | ETA |
|-----------|--------|-----|
| Consolidation framework created | ✅ Complete | Done (2026-03-26) |
| Issue templates designed | ✅ Complete | Done (2026-03-26) |
| Example deliverables documented | ✅ Complete | Done (2026-03-26) |
| Receive 79-repo list from Chad | ⏳ Pending | TBD |
| Apply consolidation rules | ⏸️ Waiting | 30 mins after list |
| Generate mapping table | ⏸️ Waiting | 1 hour after list |
| Produce priority ranking | ⏸️ Waiting | 1.5 hours after list |
| Create GitHub issues | ⏸️ Waiting | 2 hours after list |
| Chad review & approval | ⏸️ Waiting | TBD |
| Dallas begins extraction | ⏸️ Waiting | After approval |

---

## 👥 Squad Roles

| Agent | Responsibility | Current Status |
|-------|---------------|----------------|
| **Keaton** | Lead, scope decisions, consolidation analysis | ⏸️ Blocked (awaiting repo list) |
| **Dallas** | Content extraction from repos | ⏸️ Ready (awaiting backlog) |
| **Fenster** | Deployment, CI/CD | ✅ Idle (no deployment changes) |
| **Hockney** | Content quality validation | ⏸️ Ready (awaiting content) |
| **Ralph** | Backlog tracking | ⏸️ Ready (awaiting issues) |
| **Scribe** | Decision documentation | ✅ Active (logging this work) |

---

## 🔗 Related Decisions

See `.squad/decisions.md` for:
- Team responsibilities
- Content-first phase directive
- Astro collections as source of truth

---

## 📞 Contact

**Ready to Unblock?**  
Send the 79-repo list to Keaton in any format (CSV, org name, or previous analysis).

**Questions?**  
Review the detailed documents above or ask Keaton for clarification.

---

**Last Updated:** 2026-03-26  
**By:** Keaton (Squad Lead)
