# Skill: GitHub Actions Scheduled Workflows

**Author:** Fenster (DevOps)  
**Last Updated:** 2026-03-27  
**Scope:** Building and deploying scheduled CI/CD tasks (nightly rebuilds, weekly checks, cleanup jobs)

## Pattern: Scheduled Rebuild Workflow

### When to Use
- **Rebuild static sites** on a schedule (e.g., content with date/status filtering)
- **Refresh cached data** without code changes
- **Run periodic tests** or health checks
- **Deploy without code push** (e.g., feature flags, config updates affecting visibility)

### Base Template

```yaml
name: Nightly [Task Name]

on:
  schedule:
    - cron: '0 2 * * *'  # 2:00 AM UTC daily (adjust as needed)
  workflow_dispatch:     # Manual trigger for testing

jobs:
  scheduled_job:
    runs-on: ubuntu-latest
    name: [Job Description]
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: true
      
      - name: Setup environment
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run task
        run: [your-command-here]
      
      - name: Deploy (if applicable)
        uses: [deployment-action]
        with:
          # credentials and config
```

### Cron Schedule Examples

| Schedule | Cron Pattern |
|----------|-------------|
| Daily at 2 AM UTC | `0 2 * * *` |
| Every 6 hours | `0 */6 * * *` |
| Daily at midnight UTC | `0 0 * * *` |
| Weekly on Monday, 9 AM UTC | `0 9 * * 1` |
| First day of month, 3 AM UTC | `0 3 1 * *` |

**Note:** Cron syntax is (minute hour day-of-month month day-of-week). See [cron.guru](https://cron.guru) for validation.

### Key Considerations

1. **workflow_dispatch required:** Always add manual trigger for testing before relying on schedule.
2. **No code commits:** Scheduled workflows should not modify git history (use `skip-ci` or avoid git push).
3. **Reuse existing secrets:** Inherit deployment credentials from main workflow (avoid duplicate secrets).
4. **Caching for CI speed:** Include step caching (Playwright, npm, etc.) for faster runs.
5. **Conditional jobs:** Use `if` clauses to skip deployment on failure or for dry-run scenarios.

### chadgreen.com Implementation

**File:** `.github/workflows/nightly-rebuild.yml`

- **Schedule:** 5:00 AM UTC (matches cron `0 5 * * *`; avoids peak traffic, runs during night hours globally)
- **Build:** `npm run build` (Astro static generation, 1-2 min typical)
- **Deploy:** Azure Static Web Apps deploy action (same token as primary workflow)
- **No code changes:** Content visibility filtered at build time by date/status fields

### Troubleshooting

| Issue | Solution |
|-------|----------|
| Workflow doesn't trigger on schedule | Check runner time zone; GitHub Actions uses UTC. Verify cron expression on [cron.guru](https://cron.guru). |
| Deploy fails silently | Add `workflow_dispatch` to test manually with same config. |
| Slow CI runtime | Add caching steps (npm, Playwright, Docker layers) before long-running tasks. |
| Unwanted git commits | Avoid `git push` in workflow. Use deploy action's native upload (no checkout needed post-build). |

---

**Related Files:**
- `.github/workflows/nightly-rebuild.yml` — Nightly build + deploy for chadgreen.com
- `.github/workflows/deploy-site.yml` — Primary workflow (push + PR based)
