# Font Awesome Audit Tool

## Overview

The Font Awesome Audit Tool is a standalone utility designed to scan PUG template files for Font Awesome class references and suggest Bootstrap Icons replacements. This tool helps maintain consistency when migrating from Font Awesome to Bootstrap Icons.

## Features

- 🔍 **Comprehensive Scanning**: Recursively scans all `.pug` files in the `src/` directory
- 📊 **Detailed Reporting**: Provides file-by-file breakdown of Font Awesome usage
- ✅ **Smart Mapping**: Includes 60+ pre-configured Font Awesome to Bootstrap Icons mappings
- 💾 **JSON Export**: Saves detailed audit results for further analysis
- 🚀 **Zero Dependencies**: Runs using only Node.js built-in modules

## Usage

### Command Line

```bash
# Run via npm script (recommended)
npm run audit:fontawesome

# Run directly
node tools/audit/fontawesome-audit-standalone.js
```

### Output

The tool provides:

1. **Summary Report**: Overview of files scanned and issues found
2. **Detailed Findings**: File-by-file breakdown with line numbers
3. **Suggestions**: Ready-to-use replacement mappings
4. **JSON Report**: Saved to `copilot/session-YYYY-MM-DD/fontawesome-audit-report.json`

## Icon Mappings

The tool includes mappings for common Font Awesome icons:

### Brand Icons

- `fab fa-linkedin-in` → `bi bi-linkedin`
- `fab fa-github` → `bi bi-github`
- `fab fa-youtube` → `bi bi-youtube`
- `fab fa-microsoft` → `bi bi-microsoft`
- `fab fa-react` → `bi bi-react`

### Solid Icons

- `fas fa-search` → `bi bi-search`
- `fas fa-rss` → `bi bi-rss`
- `fas fa-info-circle` → `bi bi-info-circle`
- `fas fa-external-link-alt` → `bi bi-box-arrow-up-right`
- `fas fa-home` → `bi bi-house`

### Regular Icons

- `far fa-star` → `bi bi-star`
- `far fa-heart` → `bi bi-heart`
- `far fa-envelope` → `bi bi-envelope`

*Full mapping available in the tool source code*

## Sample Output

```
🔍 Font Awesome Audit Tool
===========================
📁 Scanning: C:\GitHub\MarkHazleton\markhazleton-blog\src/**/*.pug

📄 Found 131 PUG files to scan

📊 AUDIT REPORT
================
📄 Total files scanned: 131
⚠️  Files with issues: 0
🚨 Total Font Awesome references: 0
✅ Automatically fixable: 0
❓ Requires manual review: 0

🎉 No Font Awesome references found! All clean!
```

## When Issues Are Found

If Font Awesome references are detected, the tool will show:

```
📋 DETAILED FINDINGS
=====================

📄 src/pug/layouts/header.pug
─────────────────────────────────
  ✅ Line 25: fab fa-linkedin-in → bi bi-linkedin
     Context: i.fab.fa-linkedin-in
  ❓ Line 30: fab fa-custom-icon → NEEDS MANUAL REVIEW
     Context: i.fab.fa-custom-icon

💡 SUGGESTIONS
===============

✅ Ready to fix (automatic replacements available):
   fab fa-linkedin-in → bi bi-linkedin

❓ Requires manual review:
   fab fa-custom-icon → Find equivalent Bootstrap Icon

   Visit https://icons.getbootstrap.com/ to find alternatives
```

## Technical Details

### Detection Patterns

The tool uses regex patterns to find Font Awesome classes:

- `fab fa-*` (Brand icons)
- `fas fa-*` (Solid icons)  
- `far fa-*` (Regular icons)
- `fa fa-*` (Legacy format, converted to `fas`)

### File Structure

```
tools/audit/
├── fontawesome-audit-standalone.js  # Main audit tool
└── fontawesome-audit.js            # Extended version with glob support
```

### Generated Reports

JSON reports are saved to:

```
copilot/session-YYYY-MM-DD/fontawesome-audit-report.json
```

Report structure:

```json
{
  "timestamp": "2025-10-12T...",
  "summary": {
    "totalFiles": 131,
    "filesWithIssues": 0,
    "totalIssues": 0,
    "fixableIssues": 0
  },
  "issues": [],
  "iconMappings": { ... }
}
```

## Integration

The audit tool is integrated into the build pipeline via npm scripts:

```json
{
  "scripts": {
    "audit:fontawesome": "node tools/audit/fontawesome-audit-standalone.js"
  }
}
```

## Maintenance

To add new icon mappings, edit the `iconMappings` object in the tool:

```javascript
const iconMappings = {
    // Add new mappings here
    'fas fa-new-icon': 'bi bi-equivalent-icon',
    // ...
};
```

## Related Tools

- **Build System**: `npm run build` - Compiles PUG templates
- **SEO Audit**: `npm run audit:seo` - SEO and accessibility checks
- **Performance Audit**: `npm run audit:perf` - Lighthouse performance analysis

## Dependencies

**None** - The standalone version uses only Node.js built-in modules:

- `fs` - File system operations
- `path` - Path utilities

## License

This tool is part of the Mark Hazleton Blog project and follows the same license terms.
