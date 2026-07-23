# Project Context

- **Project:** chadgreen.com
- **Created:** 2026-03-26

## Core Context

Agent Scribe initialized and ready for work.

## Recent Updates

📌 Team initialized on 2026-03-26

## Learnings

Initial setup complete.

## Recent Updates

📌 Team update (2026-05-31T13:59:13.344-04:00): Website fixes batch started. Canonical social links were aligned to Chad's LinkedIn profile, and the contact form remains a client-side POST to `/api/contact` during deployment/runtime verification.

📌 Team update (2026-05-31T15:32:04.586-04:00): Recorded the social media plan batch and captured the current channel priorities for McManus.

📌 Team update (2026-07-03T09:18:28.411-04:00): Logged the Blazor WASM + local API content management planning batch with Keaton, Rusty, Linus, and Hockney; phased plan draft and open questions were captured.

📌 Team update (2026-07-03T09:23:27.017-04:00): Confirmed v1 decisions logged — archive retention/purge set to 90 days, markdown editing remains plain textarea, and optional git commit integration is in scope.

📌 Team update (2026-07-22T20:44:07.8720255-04:00): Clarified that engagement presentations can override base presentation resources: `EngagementPresentationPage.astro` uses `engagement.data.links` when present, otherwise falls back to `presentation.data.resources`. Updated `src/content/engagementPresentations/cincy-deliver-2026-navigating-maze-communicating-architecture.md` links with Slides, ADR Examples, and Cincy Deliver.

📌 Team update (2026-07-22T20:44:07.8720255-04:00): Logged route-resolution fix for engagement presentations. Root cause was duplicate files sharing the same eventSlug/sessionSlug, where an outdated file without links won route resolution. Action taken: deleted `src/content/engagementPresentations/cincy-deliver-2026-navigating-maze-architecture-decisions.md`. Outcome: build succeeded and the canonical engagement file now drives route resources.
