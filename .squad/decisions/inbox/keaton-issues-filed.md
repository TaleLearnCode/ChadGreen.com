# GitHub Issues Filed - Content Extraction Sprint

**Decision by:** Keaton (Lead)  
**Date:** 2025-01-28  
**Status:** ✅ Complete

---

## DECISION: Filed 10 GitHub Issues

All 10 approved content extraction issues have been filed in the TaleLearnCode/ChadGreen.com repository.

### Issues Created

| Issue # | Title | Priority | Estimated Effort | Labels |
|---------|-------|----------|------------------|--------|
| [#4](https://github.com/TaleLearnCode/ChadGreen.com/issues/4) | [CLUSTER] Extract Messaging Patterns presentation series | HIGH | 5-6 hours | squad:dallas, content:presentation, content:engagementPresentations, priority:high, cluster |
| [#5](https://github.com/TaleLearnCode/ChadGreen.com/issues/5) | Extract presentation: Generative AI for .NET Developers | HIGH | 35-40 min | squad:dallas, content:presentation, priority:high |
| [#6](https://github.com/TaleLearnCode/ChadGreen.com/issues/6) | Extract presentation: Azure Container Apps in Practice | HIGH | 35-40 min | squad:dallas, content:presentation, priority:high |
| [#7](https://github.com/TaleLearnCode/ChadGreen.com/issues/7) | Extract workshop: Advanced Serverless Workshop | HIGH | 70-80 min | squad:dallas, content:presentation, type:workshop, priority:high |
| [#8](https://github.com/TaleLearnCode/ChadGreen.com/issues/8) | Extract workshop: Event-Driven Architecture & Microservices Workshop | HIGH | 70-80 min | squad:dallas, content:presentation, type:workshop, priority:high |
| [#9](https://github.com/TaleLearnCode/ChadGreen.com/issues/9) | Map BuildingServerlessSolutions to events | HIGH | 25-30 min | squad:dallas, content:engagementPresentations, priority:high, depends-on-presentations |
| [#10](https://github.com/TaleLearnCode/ChadGreen.com/issues/10) | Extract presentation: Building Serverless Solutions (base) | HIGH | 35-40 min | squad:dallas, content:presentation, priority:high |
| [#11](https://github.com/TaleLearnCode/ChadGreen.com/issues/11) | Extract presentation: Design and Develop Serverless Event-Driven Microservice | MEDIUM | 40-45 min | squad:dallas, content:presentation, priority:medium |
| [#12](https://github.com/TaleLearnCode/ChadGreen.com/issues/12) | Extract presentation: Building Scalable, Resilient, Cloud-Native App | MEDIUM | 35-40 min | squad:dallas, content:presentation, priority:medium |
| [#13](https://github.com/TaleLearnCode/ChadGreen.com/issues/13) | Extract presentation: Going Schema-less with Azure Cosmos DB | MEDIUM | 35-40 min | squad:dallas, content:presentation, priority:medium |

---

## Labels Created

Created 8 custom labels for issue categorization:
- `squad:dallas` - Assigned to Dallas (Content Engineer)
- `content:presentation` - Presentation content extraction
- `content:engagementPresentations` - Event-to-presentation mapping
- `priority:high` - High priority task
- `priority:medium` - Medium priority task
- `type:workshop` - Workshop content
- `cluster` - Multi-item cluster task
- `depends-on-presentations` - Depends on presentation content being created first

---

## Dependencies

**Critical Path:**
- Issue #10 (Building Serverless Solutions base) MUST be completed before Issue #9 (event mapping)
- Dependency comment added to Issue #9

All other issues can be executed in parallel.

---

## Next Steps

1. ✅ Issues filed and labeled
2. ✅ Dependencies documented
3. 🔄 Dallas can now start execution in priority order
4. 📋 Scribe will log this decision

**Total Estimated Effort:** 11.5-13 hours across 10 issues

**Execution Order Recommendation:**
1. Priority 1 (HIGH): Issues #4-10 (6 issues)
2. Priority 2 (MEDIUM): Issues #11-13 (4 issues)
3. Within HIGH priority, execute #10 before #9 due to dependency

---

## Repository

All issues filed in: **TaleLearnCode/ChadGreen.com**

Issue range: #4-#13 (10 issues total)
