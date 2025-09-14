# Tools Directory

This directory contains all the tools and utilities for the Mark Hazleton Blog project, organized by purpose.

## Directory Structure

```text
tools/
├── build/          # Build system tools
├── seo/            # SEO validation and audit tools  
├── git/            # Git analysis and reporting tools
├── maintenance/    # Maintenance and monitoring tools
└── README.md       # This file
```

## Build Tools (`/build/`)

Core build system components:

- **build.js** - Unified build script for all site components
- **clean.js** - Cleans build artifacts and output directories
- **start.js** - Development server with live reload

### Build Usage

```bash
npm run build      # Full site build
npm run clean      # Clean build artifacts
npm start          # Start development server
```

## SEO Tools (`/seo/`)

SEO validation and optimization tools:

- **seo-validation-report.js** - Comprehensive SEO validation with PUG file mapping
- **seo-a11y-checks.mjs** - Accessibility and SEO automated checks
- **ssl-expiry.ts** - SSL certificate expiration monitoring

### SEO Usage

```bash
npm run seo:audit    # Run SEO validation report
npm run audit:seo    # Run SEO and accessibility checks
npm run audit:ssl    # Check SSL certificate status
npm run audit:all    # Run comprehensive audit suite
```

## Git Analysis Tools (`/git/`)

Git repository analysis and reporting:

- **Generate-GitActivityReport.ps1** - Comprehensive Git activity report with GitHub integration
- **Quick-GitReport.ps1** - Quick Git activity overview

### Git Analysis Usage

```powershell
# Generate HTML report for last 10 days (outputs to temp/reports)
.\tools\git\Generate-GitActivityReport.ps1

# Generate JSON report for last 14 days  
.\tools\git\Generate-GitActivityReport.ps1 -Days 14 -Format JSON

# Console output for quick overview
.\tools\git\Generate-GitActivityReport.ps1 -Days 7 -Format Console
```

## Maintenance Tools (`/maintenance/`)

Site maintenance and monitoring utilities:

- **report-monthly.mjs** - Generate monthly maintenance reports  
- **apply-autofixes.mjs** - Automated code fixes and optimizations
- **Analyze-CodeQuality.ps1** - Code quality analysis and metrics
- **Analyze-Dependencies.ps1** - Dependency analysis and security checks

### Maintenance Usage

```bash
npm run report:monthly  # Generate monthly report (outputs to temp/reports)
```

```powershell
# Run code quality analysis
.\tools\maintenance\Analyze-CodeQuality.ps1

# Check dependencies for vulnerabilities  
.\tools\maintenance\Analyze-Dependencies.ps1
```

## Output Location

**All tool-generated reports are saved to `temp/reports/`** which is excluded from git via `.gitignore`. This centralizes all temporary outputs and keeps the repository clean.

The `copilot/` directory remains separate for Copilot-generated documentation and analysis files.

## Configuration

Tools are integrated with the project's npm scripts in `package.json`. Update script paths there if tool locations change.

GitHub Actions workflows reference these tools for automated testing and reporting in CI/CD pipelines.
