# Add site monitoring & maintenance automation

## What was added

- Scheduled workflows: monthly-maintenance, nightly-quickchecks
- Lighthouse, Lychee, pa11y-ci, SEO and SSL checks
- Artifacts upload and monthly report generation
- Dependabot weekly updates and dependency review

## How to run locally

```bash
npm ci
npm run audit:all
```

## Override URL samples

- Update `lighthouserc.json` and `pa11yci.json`
- Or pass env vars SITE_URL/SITEMAP_URL in CI

## Artifacts & reports

- Workflow artifacts: `lhci/`, `links/`, `artifacts/`
- Monthly reports: `maintenance/reports/YYYY-MM.md`

## Auto-fixes

- Scoped to Pug layouts and `docs/staticwebapp.config.json`
- See workflow logs for applied changes
