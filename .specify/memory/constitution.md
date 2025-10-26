<!--
Sync Impact Report:
- Version change: Initial → 1.0.0 (new constitution)
- Added sections: Technology Standards, Quality Assurance, Development Workflow
- Principles created: 5 new principles covering build system integrity, SEO compliance, template standards, professional content quality, and automated quality assurance
- Templates requiring updates: ✅ All templates align with constitution principles
- No deferred TODOs - all placeholders filled with concrete project requirements
-->

# Mark Hazleton Blog Constitution

## Core Principles

### I. Unified Build System Integrity
Every build operation MUST use the centralized build.js orchestrator with modular renderers. Build components (PUG, SCSS, assets, content generation) MUST be independently buildable via command flags. All build processes MUST provide detailed timing and error reporting. Build artifacts MUST be reproducible across environments.

**Rationale**: Ensures consistent, maintainable, and debuggable build processes while enabling selective rebuilds for efficient development workflows.

### II. SEO-First Content Standards (NON-NEGOTIABLE)
All article content MUST validate against comprehensive SEO rules before publication. Title tags (30-60 chars), meta descriptions (120-160 chars), and keywords (3-8 count) are mandatory. Open Graph and Twitter Card metadata MUST be complete and character-compliant. All images MUST have descriptive alt text. Article meta tags are NEVER manually added to .pug files - they are automatically generated from articles.json metadata.

**Rationale**: Professional blog content requires consistent search engine optimization to maintain visibility and credibility in the Solutions Architect domain.

### III. Bootstrap 5 + PUG Template Standards
All HTML generation MUST use PUG 3.0.3 templates with proper inheritance (extends/block patterns). Bootstrap 5.3.8 utility classes are the ONLY styling mechanism - no custom CSS additions. PUG formatting MUST use 2-space indentation with proper element separation. Code blocks containing HTML MUST use dot syntax to prevent parsing conflicts. All icons MUST use Bootstrap Icons (bi bi-*) exclusively.

**Rationale**: Maintains visual consistency, reduces maintenance overhead, and prevents build failures from template formatting errors.

### IV. Professional Content Quality
Article content MUST maintain conversational but professional tone suitable for Solutions Architect audience. Avoid marketing hyperbole and excessive color usage. Content MUST be practical, authentic, and include real-world applications. Visual elements MUST enhance readability without creating distraction. All content MUST pass accessibility standards (WCAG guidelines).

**Rationale**: Establishes credibility and professionalism required for business development and technical leadership positioning.

### V. Automated Quality Assurance
All deployments MUST pass automated quality gates including Lighthouse performance audits, accessibility testing (pa11y-ci), SSL certificate validation, and link checking. Monthly maintenance reports MUST be generated with comprehensive site health metrics. SEO validation MUST be enforced at multiple checkpoints (PowerShell scripts, .NET services, edit forms, AI generation). Build failures MUST block deployment.

**Rationale**: Ensures consistent professional quality and prevents degradation of site performance and user experience.

## Technology Standards

### Required Technology Stack
- **Template Engine**: PUG 3.0.3 with semantic HTML5 output
- **CSS Framework**: Bootstrap 5.3.8 with Dart Sass compilation
- **Content Management**: JSON-based system (articles.json, projects.json, sections.json)
- **Development Server**: BrowserSync with live reload
- **Deployment**: Azure Static Web Apps with GitHub Actions CI/CD
- **Quality Assurance**: Lighthouse CI, pa11y-ci, monthly maintenance automation

### Forbidden Practices
- Manual CSS additions or overrides of Bootstrap classes
- Custom icon libraries beyond Bootstrap Icons
- Direct HTML generation bypassing PUG templates
- Manual meta tag insertion in article PUG files
- Build processes outside the unified build.js system

## Quality Assurance

### Content Validation Gates
1. **SEO Validation**: All content MUST pass PowerShell audit scripts and .NET validation services
2. **Accessibility**: All pages MUST achieve WCAG compliance via pa11y-ci testing
3. **Performance**: All pages MUST maintain Lighthouse scores above baseline thresholds
4. **Link Integrity**: All internal and external links MUST be validated monthly
5. **SSL Security**: Certificate expiry MUST be monitored with 30-day advance warnings

### AI-Powered Quality Enhancement
- OpenAI GPT-4 integration for automated SEO metadata generation
- Real-time validation with A-F grading system for content quality
- Character count validation with visual feedback
- Structured output API for consistent field generation

## Development Workflow

### Build Process Requirements
1. **Development**: `npm start` launches BrowserSync after complete build
2. **Production**: `npm run build` executes full build sequence with validation
3. **Selective Building**: Individual components buildable via `--pug`, `--scss`, `--assets` flags
4. **Content Updates**: RSS and sitemap generation MUST be automated on article changes
5. **Quality Gates**: All builds MUST pass before deployment to Azure Static Web Apps

### Code Review Standards
- All PUG templates MUST pass build validation before merge
- SEO metadata changes MUST be validated against character limits
- Bootstrap class usage MUST be reviewed for consistency
- Content quality MUST be assessed for professional tone and accuracy
- Build performance MUST be monitored and optimized

### Deployment Validation
- GitHub Actions MUST complete successful build and quality checks
- Azure Static Web Apps deployment MUST complete without errors
- IndexNow API MUST be triggered for search engine notification
- Post-deployment smoke testing MUST verify core functionality

## Governance

This constitution supersedes all other development practices and standards. All pull requests and code reviews MUST verify compliance with these principles. Any complexity or exceptions MUST be explicitly justified and documented.

For runtime development guidance, refer to `.github/copilot-instructions.md` which provides detailed implementation guidance aligned with these constitutional principles.

Amendment procedures require documentation of rationale, backward compatibility assessment, and migration plan for existing content. All amendments MUST maintain the professional quality and technical excellence standards established herein.

**Version**: 1.0.0 | **Ratified**: 2025-10-26 | **Last Amended**: 2025-10-26
