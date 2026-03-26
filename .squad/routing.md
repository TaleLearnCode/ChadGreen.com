# Work Routing

## Primary Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Content decisions & scope | Keaton | What content to add, collection structure, priorities |
| Content creation & markdown | Dallas | Add presentations, events, blog posts, meetup info |
| Deployment & builds | Fenster | Deploy to Azure, fix build failures, CI/CD |
| Content QA & validation | Hockney | Review before merge, check links, validate schema |
| Session logging & decisions | Scribe | Automatic after work batches; never needs routing |
| Backlog monitoring | Ralph | Track work queue, alert to blocked items |

## Anticipatory Routing

- **Dallas content work** → automatically route to **Hockney** for QA review before merge
- **Any agent** → **Keaton** when major decisions or scope changes needed
- **Fenster deployments** → **Hockney** for post-deployment verification

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze, assign `squad:{member}` label | Keaton (Lead) |
| `squad:dallas` | Content backfill, markdown, collections | Dallas |
| `squad:fenster` | Deployment, build, infrastructure | Fenster |
| `squad:hockney` | Quality, testing, accuracy | Hockney |
| `squad:keaton` | Scope, decisions, reviews | Keaton |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Keaton** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Content-first approach:** Backfill content before new features or heavy refactoring.
2. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
3. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
4. **Quick facts → coordinator answers directly.** Don't spawn an agent for simple questions.
5. **When two agents could handle it**, pick the one whose domain is the primary concern.
6. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel.
7. **Anticipate downstream work.** Dallas adds content → Hockney reviews QA → Fenster builds → Scribe logs.
8. **Ralph monitoring:** Continuous backlog tracking; alerts to blocked work.
