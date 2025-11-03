<!--
Sync Impact Report - Constitution Update
Version Change: 1.0.0 → 1.0.1 (Alignment with copilot-instructions.md)
Modified Principles: None (clarifications only)
Added Sections: None
Removed Sections: None
Changes Made:
  - Updated build system paths to tools/build/build.js (was scripts/build.js)
  - Clarified modern-layout auto-generates meta tags from articles.json
  - Added reference to article-mixins.pug for consistent components
  - Specified copilot/session-{date}/ directory for generated docs
  - Updated build command references to match actual npm scripts
Templates Status:
  ✅ plan-template.md - Compatible with constitution gates
  ✅ spec-template.md - Aligns with content-first principles
  ✅ tasks-template.md - Aligns with build system approach
Follow-up TODOs: None
-->

# Mark Hazleton Blog Constitution

## Core Principles

### I. Content-First Architecture

All features MUST serve content delivery and presentation as the primary goal. The static site generator exists to transform content (articles, projects) into optimized HTML with proper SEO, accessibility, and performance.

**Rationale**: This is a content-focused professional blog and portfolio. Every technical decision should enhance content discoverability, readability, and impact. Features that don't directly support content delivery require explicit justification.

**Rules**:
- Content data (articles.json, projects.json, sections.json) is the single source of truth
- Templates (PUG) MUST extend modern-layout.pug and use semantic HTML5 elements
- Meta tags are NEVER manually added to article .pug files - modern-layout.pug auto-generates ALL SEO meta tags from articles.json data during build
- All content changes trigger automatic RSS, sitemap, and SEO artifact regeneration
- Article templates use only `block layout-content` for article content - NO meta tag blocks

### II. Build System Modularity

Build operations MUST be modular, cacheable, and independently executable. All builds run through `tools/build/build.js` - a sophisticated build orchestrator with intelligent caching, parallel execution, and modular renderers.

**Rationale**: Enables selective rebuilds for faster development cycles, parallel execution for performance, and clear separation of concerns. Supports incremental builds and reduces unnecessary work. Build cache (`.build-cache/`) tracks file dependencies to skip unchanged artifacts.

**Rules**:
- Each renderer module handles exactly one build concern (render-pug.js, scss-renderer.js, etc.) in tools/build/
- Build cache tracks file dependencies to skip unchanged artifacts
- All renderers accept command-line flags for targeted execution via npm scripts
- Phase dependencies are explicit: Phase 1 (prerequisites) → Phase 2 (parallel execution) → Phase 3 (final steps)
- Performance tracking records build times and generates reports

### III. Template Consistency & Semantic HTML

PUG templates MUST follow strict formatting rules with 2-space indentation, semantic HTML5 structure, and Bootstrap 5 utility classes. Custom CSS is avoided unless critical.

**Rationale**: Prevents build errors from indentation issues, ensures accessibility through semantic markup, and maintains visual consistency across all pages. Reduces maintenance burden and improves developer experience.

**Rules**:
- 2-space indentation with NO tabs or mixed spacing
- Each PUG element on its own line with proper blank line separation
- Period continuation (`.`) requires pure HTML for links/markup
- Bootstrap Icons ONLY (bi bi-*) - no other icon libraries
- Test builds frequently with `npm run build:pug` to catch errors early

### IV. Professional Content Standards

Content tone MUST be conversational and practical. Visual design MUST be clean with minimal color usage, favoring muted tones. Marketing hyperbole and excessive styling are prohibited.

**Rationale**: Target audience is technical professionals seeking practical insights, not marketing content. Credibility comes from authentic voice and clear presentation, not flashy design or superlatives.

**Rules**:
- Write as if explaining to a colleague - authentic voice including challenges
- NO inline styles (`style="..."`) or custom CSS outside build system
- Use Bootstrap semantic colors sparingly - default to neutral/muted tones
- Simple card/section layouts over complex multi-colored designs
- Code blocks MUST use dark backgrounds for readability

### V. SEO & Accessibility Standards

SEO metadata MUST meet validation requirements: title (30-60 chars), description (120-160 chars), Open Graph fields, Twitter Cards. WCAG accessibility guidelines MUST be followed.

**Rationale**: Professional visibility depends on search engine discoverability and inclusive design. Automated validation ensures consistency and prevents regression.

**Rules**:
- All SEO metadata defined in articles.json/projects.json
- Automatic validation via SEO.md guidelines and validation scripts
- Proper heading hierarchy (h1-h6), alt text, keyboard navigation
- Sitemap and RSS auto-generated from content data
- IndexNow API triggered on deployment for immediate indexing

### VI. Automated Quality Gates

Deployment MUST pass automated audits: Lighthouse performance, pa11y accessibility, SEO validation, SSL certificate checks. Monthly comprehensive audits generate reports.

**Rationale**: Quality is non-negotiable for professional credibility. Automated gates prevent degradation and provide continuous monitoring of site health.

**Rules**:
- GitHub Actions run audits on: monthly schedule, nightly quick checks, PR validation
- Lighthouse CI tracks performance metrics with baseline thresholds
- pa11y-ci validates WCAG compliance on all pages
- SSL certificate expiry monitored with advance warnings
- Reports stored in `reports/YYYY-MM.md` for historical tracking

## Technical Stack Requirements

**Mandatory Technologies**:
- **PUG 3.0.3**: Template engine with extends/block inheritance, 2-space indentation
- **Bootstrap 5.3.8**: CSS framework - utility classes only, NO custom CSS unless critical
- **Bootstrap Icons**: ONLY icon library allowed (bi bi-*) - NO Font Awesome, Material Icons, etc.
- **Node.js**: Build system runtime with modular renderers in tools/build/
- **SCSS**: Dual compilation (styles.scss + modern-styles.scss) with PostCSS/autoprefixer/cssnano
- **BrowserSync 3.0.4**: Development server with live reload on port 3000

**Deployment Stack**:
- **Azure Static Web Apps**: Hosting with automatic SSL
- **GitHub Actions**: CI/CD pipeline with deployment automation
- **docs/ directory**: Pre-built static files deployed directly

**Prohibited**:
- Custom CSS frameworks or preprocessors beyond SCSS
- Icon libraries other than Bootstrap Icons
- Client-side JavaScript frameworks (React, Vue, Angular) for static content
- Any build tools outside the unified build.js system

## Development Workflow

### Content Creation Process

1. Add metadata entry to `src/articles.json` or `src/projects.json` with complete SEO metadata
2. Create PUG template in `src/pug/articles/` extending `modern-layout`
3. Use article mixins from `src/pug/modules/article-mixins.pug` for consistent components:
   - `+articleHeader(article)` - Hero section with title/subtitle
   - `+tableOfContents(items)` - Navigation for long articles
   - `+articleSection(id, title, iconClass)` - Consistent section headers
   - `+codeBlock(code, language)` - Syntax-highlighted code blocks
   - `+alertBox(type, title, content)` - Info/warning/success alerts
4. Run `npm run build` to compile and validate (or `npm run build:pug` for templates only)
5. Verify SEO tags auto-generated correctly in output HTML in docs/
6. Test locally with `npm start` (BrowserSync on port 3000) before committing

### Build Validation Gates

Before committing:
- Run `npm run build:pug` to catch template syntax errors
- Verify all sections have complete content (no empty placeholders)
- Check container structure consistency (keep ALL sections within `.col-lg-8.mx-auto` container to prevent width jumping)
- Validate proper 2-space indentation with NO tabs or mixed spacing
- Ensure proper blank line spacing between major sections
- Ensure code blocks use dot syntax (`.`) after `code` elements when containing HTML/JS content
- Verify each PUG element is on its own line with proper newlines
- Check period continuation blocks (`.`) use pure HTML for links/markup, NOT PUG syntax

### Deployment Pipeline

1. Push to `main` branch triggers GitHub Actions workflow
2. Node.js 20 environment setup with npm caching
3. `npm ci --include=dev` installs all dependencies
4. `npm run build` generates production files in `docs/`
5. Azure Static Web Apps deployment from `docs/` directory
6. IndexNow API notification for search engine indexing
7. Automated audits run on schedule (monthly comprehensive, nightly quick checks)

## Governance

This constitution supersedes all other development practices and guidelines. All code reviews, feature specifications, and implementation plans MUST verify compliance with core principles.

**Amendment Process**:
- Amendments require documented rationale and impact analysis
- Version follows semantic versioning: MAJOR (breaking principle changes), MINOR (new principles/sections), PATCH (clarifications)
- Sync Impact Report MUST be generated documenting affected templates and dependencies
- All dependent artifacts (plan-template.md, spec-template.md, tasks-template.md) MUST be reviewed for consistency

**Complexity Justification**:
When violating simplicity principles (e.g., adding new dependencies, custom build steps, non-standard patterns), provide explicit documentation:
- What simpler alternative was considered
- Why the simpler approach is insufficient
- Specific problem the complexity solves
- Impact on maintainability and future development

**Runtime Development Guidance**:
For detailed implementation patterns, code standards, and troubleshooting, consult `.github/copilot-instructions.md` which provides comprehensive guidance on:
- PUG formatting rules and error prevention (2-space indentation, period continuation syntax, common errors)
- Bootstrap 5 implementation patterns and utility class usage
- SEO implementation and validation (see also `SEO.md` for complete validation rules)
- Debugging workflows and emergency commands
- Container structure consistency and advanced PUG patterns
- Article authoring best practices (see also `Authoring.md` for step-by-step guide)

**Generated Documentation Management**:
All AI/Copilot-generated .md files (reports, documentation, summaries) MUST be placed in `/copilot/session-YYYY-MM-DD/` directory structure to keep generated documentation organized and separate from core project files.

**Compliance Enforcement**:
- All PRs MUST pass automated build validation
- Template changes MUST pass `npm run build:pug` before merge
- Content additions MUST include complete SEO metadata in JSON files
- Deployment MUST pass Lighthouse and accessibility audits
- Constitution violations in complexity require documented justification

**Version**: 1.0.1 | **Ratified**: 2025-11-02 | **Last Amended**: 2025-11-02
