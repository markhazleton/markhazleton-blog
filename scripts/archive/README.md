# PowerShell Scripts Archive

This folder contains PowerShell scripts that were used for one-time fixes and are no longer needed for regular site maintenance.

## Archived Scripts

### SEO & Twitter Card Fixes

- **`fix-twitter-descriptions.ps1`** - Comprehensive Twitter card description fixer with multiple fallback strategies
- **`fix-twitter-descriptions-quick.ps1`** - Quick version of Twitter description fixes
- **`fix-twitter-seo.ps1`** - Large comprehensive script for Twitter Card and Open Graph description issues

### Security Fixes

- **`fix-noopener.ps1`** - Original script to add `rel="noopener noreferrer"` to external links
- **`fix-noopener-updated.ps1`** - Enhanced version with more comprehensive pattern matching

## Why These Were Archived

These scripts were created to solve specific one-time issues:

1. **Twitter/SEO Description Issues**: Fixed inconsistent or missing descriptions across articles
2. **Security Attributes**: Added proper `rel="noopener noreferrer"` attributes to external links

Once these issues were resolved, the scripts became unnecessary for ongoing maintenance.

## If You Need to Use These Scripts

1. **Review the issue first**: Make sure the problem still exists
2. **Test in a safe environment**: These scripts modify content files
3. **Back up your data**: Especially `src/articles.json` and PUG template files
4. **Run with `-DryRun` parameter** when available to preview changes

## Current Maintenance Scripts

For ongoing site maintenance, use the scripts in the repository root:

- `maintenance-check.ps1` - Runs all essential maintenance checks
- `seo-audit.ps1` - Comprehensive SEO validation
- `check-missing-html.ps1` - Content integrity validation
- `test-seo-config.ps1` - Configuration validation

---
*Archived on: July 13, 2025*
*By: GitHub Copilot (automated organization)*
