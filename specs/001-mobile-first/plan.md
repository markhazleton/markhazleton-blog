# Implementation Plan: Mobile-First Website Optimization

**Branch**: `001-mobile-first` | **Date**: January 5, 2026 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-mobile-first/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Optimize the Mark Hazleton blog for mobile-first experience by implementing responsive design methodology where base styles target mobile devices (375px+) and progressively enhance for tablet (768-1024px) and desktop (1200px+). Focus on readable typography (16px base, 1.5-1.6 line height, 1.200 modular scale), touch-friendly navigation (bottom sticky bar with 4-5 key links, 44x44px touch targets), optimized content hierarchy (identity + featured article + CTA above fold), and performance optimization (lazy loading below-fold images, FCP under 1.5s). Implementation works within existing PUG/Bootstrap 5/SCSS build system.

## Technical Context

**Language/Version**: PUG 3.0.3, SCSS (SASS), HTML5, CSS3  
**Primary Dependencies**: Bootstrap 5.3.8, Bootstrap Icons, Node.js build system (tools/build/build.js)  
**Storage**: Static content (articles.json, projects.json, sections.json), pre-built HTML in docs/  
**Testing**: Lighthouse CI (performance, accessibility), pa11y-ci (WCAG validation), responsive design testing tools, real device testing  
**Target Platform**: Modern web browsers (last 2 versions + Safari iOS 12+), responsive from 320px to 2560px+ width  
**Project Type**: Static site generator - PUG templates compile to HTML with SCSS compilation to CSS  
**Performance Goals**: Mobile Lighthouse Performance 90+, Accessibility 95+, First Contentful Paint <1.5s on 4G, Google Mobile-Friendly Test 100%  
**Constraints**: Must work within existing PUG/Bootstrap 5 framework, NO custom CSS frameworks, Bootstrap Icons only, maintain semantic HTML5 structure, WCAG 2.1 AA compliance (4.5:1 contrast, 44x44px touch targets)  
**Scale/Scope**: 100+ articles, multiple project pages, homepage, navigation system - all responsive across mobile/tablet/desktop breakpoints

## Error Handling & Rollback Strategy

**Build Failures**:
- SCSS compilation errors: Check syntax, validate variable references, clear build cache with `npm run clean:cache`
- PUG template errors: Verify 2-space indentation, check for missing newlines, validate proper nesting
- Recovery steps: `npm run clean:cache && npm ci --include=dev && npm run build`

**Development Issues**:
- Responsive layout breaks: Test at all breakpoints (375px, 768px, 1024px, 1200px) using browser devtools
- Typography scale issues: Verify CSS custom properties in src/scss/_variables.scss
- Touch target failures: Audit with pa11y-ci, adjust mixin in src/scss/_responsive.scss

**Deployment Rollback**:
- GitHub: Revert commit with `git revert <commit-hash>` and push to trigger redeployment
- Azure Static Web Apps: Previous deployment remains available in Azure portal for immediate rollback
- Emergency rollback: Use Azure portal to swap to previous production deployment slot

**Testing Failures**:
- Lighthouse audit failures: Review specific metrics, optimize images, adjust critical CSS
- Accessibility failures: Run pa11y-ci with `npm run audit:a11y`, fix specific WCAG violations
- Cross-browser issues: Test locally with BrowserSync, validate on real devices before deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**I. Content-First Architecture**: ✅ PASS
- Mobile-first optimization directly enhances content delivery and readability
- Works within existing content structure (articles.json, projects.json)
- All changes serve to improve content presentation on mobile devices
- No changes to content data structure - only presentation layer optimization

**II. Build System Modularity**: ✅ PASS
- Implementation uses existing build system (tools/build/build.js)
- SCSS changes integrated into existing compilation pipeline (styles.scss, modern-styles.scss)
- PUG template changes compiled through existing render-pug.js renderer
- No new build dependencies or custom build steps required
- Leverages build cache for incremental compilation

**III. Template Consistency & Semantic HTML**: ✅ PASS
- All PUG templates follow 2-space indentation standard
- Semantic HTML5 structure maintained (article, section, nav elements)
- Bootstrap 5 utility classes used exclusively for responsive design
- Bootstrap Icons only for any iconography needs
- NO custom CSS - all styling through Bootstrap utilities and SCSS variables

**IV. Professional Content Standards**: ✅ PASS
- Focus on readability and user experience, not flashy design
- Clean, accessible mobile navigation (bottom sticky bar)
- Muted color palette with Bootstrap semantic colors used sparingly
- Simple, functional layouts optimized for content consumption
- Dark code blocks for readability maintained

**V. SEO & Accessibility Standards**: ✅ PASS
- Maintains existing SEO metadata system (articles.json auto-generates tags)
- Enhances accessibility with 44x44px touch targets and WCAG 2.1 AA compliance
- Proper heading hierarchy and keyboard navigation support
- Improves mobile SEO through Google Mobile-Friendly optimization
- RSS and sitemap generation unaffected

**VI. Automated Quality Gates**: ✅ PASS
- Lighthouse CI will validate mobile performance improvements (target 90+)
- pa11y-ci will verify WCAG compliance and touch target requirements
- Existing audit workflows continue (monthly comprehensive, nightly checks)
- Success criteria aligned with automated testing metrics
- No changes to deployment pipeline or audit configuration

**Summary**: All constitutional principles satisfied. This is a presentation-layer optimization that enhances content delivery, works within existing build system, maintains template standards, improves accessibility, and aligns with automated quality gates. No violations or complexity justifications required.

## Project Structure

### Documentation (this feature)

```text
specs/001-mobile-first/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
├── checklists/          # Quality validation checklists
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── pug/
│   ├── layouts/
│   │   └── modern-layout.pug          # [MODIFY] Add mobile-first viewport meta, responsive utilities
│   ├── modules/
│   │   ├── article-mixins.pug         # [MODIFY] Enhance responsive patterns
│   │   └── navigation.pug             # [NEW/MODIFY] Bottom sticky navigation component
│   ├── articles/                      # [MODIFY] Update article templates for mobile optimization
│   ├── projects/                      # [MODIFY] Update project templates for mobile optimization
│   └── index.pug                      # [MODIFY] Implement above-fold content hierarchy
├── scss/
│   ├── styles.scss                    # [MODIFY] Add mobile-first responsive styles
│   ├── modern-styles.scss             # [MODIFY] Add mobile-first responsive styles
│   ├── _variables.scss                # [NEW/MODIFY] Typography scale variables (1.200 ratio)
│   ├── _typography.scss               # [NEW/MODIFY] Mobile-first typography system
│   ├── _navigation.scss               # [NEW/MODIFY] Bottom sticky navigation styles
│   └── _responsive.scss               # [NEW/MODIFY] Breakpoint-specific optimizations
├── articles.json                      # [NO CHANGE] Content metadata unchanged
├── projects.json                      # [NO CHANGE] Content metadata unchanged
└── sections.json                      # [NO CHANGE] Section structure unchanged

tools/build/
└── build.js                           # [NO CHANGE] Existing build orchestrator

docs/                                  # Generated output directory
├── *.html                             # [OUTPUT] Mobile-optimized HTML files
└── css/                               # [OUTPUT] Compiled responsive CSS

.build-cache/                          # Build cache for incremental compilation
```

**Structure Decision**: This is a static site generator project (Option 1: Single project). The implementation modifies existing PUG templates and SCSS styles within the established src/ directory structure. No new build tools or dependencies required - all changes integrate with existing tools/build/build.js orchestrator and modular renderers. Focus is on presentation layer (templates + styles) without altering content data or build system architecture.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations identified.** All implementation changes work within established constitutional principles and existing technical architecture.
