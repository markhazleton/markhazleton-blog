# GitHub Copilot Repository Custom Instructions

## Mark Hazleton Personal Article Site - PUG & Bootstrap 5

Static site generator using PUG templates, Bootstrap 5, and a Node.js build system. Content is managed through JSON files (`articles.json`, `projects.json`, `sections.json`) with automatic SEO, RSS, and sitemap generation.

## 🏗️ Build System Architecture

### Unified Build Process

All builds run through `tools/build/build.js` - a sophisticated build orchestrator with:
- **Intelligent caching**: Tracks file dependencies and only rebuilds changed files (`.build-cache/`)
- **Parallel execution**: Runs independent tasks concurrently for faster builds
- **Modular renderers**: Each build step is a separate module (render-pug.js, scss-renderer.js, etc.)
- **Performance tracking**: Records build times and generates performance reports

**Key Commands:**
```bash
npm run build              # Full production build (all steps)
npm run build:pug          # PUG templates only
npm run build:scss         # SCSS compilation only
npm start                  # Build + dev server (requires manual rebuild on changes)
npm run clean              # Remove docs/ directory
npm run clean:cache        # Clear build cache
```

### Build Pipeline Phases

**Phase 1 - Prerequisites (Sequential):**
1. `build:sections` - Updates sections.json with article counts from articles.json
2. `fonts` - Downloads Inter font locally
3. `placeholders` - Generates placeholder images

**Phase 2 - Parallel Execution:**
- `build:pug` - Compiles PUG templates to HTML (skips layouts/modules)
- `build:projectPages` - Generates individual project pages from projects.json
- `build:scss` - Compiles styles.scss and modern-styles.scss to CSS
- `build:scripts` - Processes JavaScript files

**Phase 3 - Final Steps (Parallel):**
- `build:assets` - Copies static files and JSON data to docs/
- `build:sitemap` - Generates sitemap.xml from sections + articles
- `build:rss` - Creates RSS feed from articles.json
- `build:projectsRss` - Creates project RSS feed

### Data Flow Architecture

```
src/articles.json ──┬──> sections.json (article counts)
                    ├──> PUG templates (modern-layout.pug reads article data)
                    ├──> sitemap.xml (SEO)
                    └──> rss.xml (RSS feed)

src/projects.json ──┬──> PUG templates (project pages)
                    └──> projects-rss.xml

src/pug/ ──────────┬──> docs/*.html (compiled output)
  ├─ layouts/      │    └─ modern-layout.pug: Auto-generates ALL SEO meta tags
  ├─ modules/      │        from JSON data (title, description, og:*, twitter:*)
  └─ articles/     └──> Individual article HTML files
```

## ⚠️ CRITICAL RULE: ARTICLE META TAGS

**🚨 NEVER ADD META TAGS TO ARTICLE .PUG FILES! 🚨**

**WHY**: `modern-layout.pug` automatically generates ALL SEO meta tags from `articles.json` data during build. The layout reads article metadata and creates:
- `<title>`, `<meta description>`, `<meta keywords>`
- Canonical URLs
- Open Graph tags (og:title, og:description, og:image, og:video)
- Twitter Card tags (twitter:title, twitter:description, twitter:image)
- Robots directives and structured data

**Article PUG Structure (Content Only):**
```pug
extends ../layouts/modern-layout

block layout-content
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
          // All article content goes here - NO meta tags!
          section#introduction.mb-5
            h2.h3.mb-4 Section Title
```

**Metadata Location:** All article metadata (title, description, keywords, SEO tags) is defined in `src/articles.json`:
```json
{
  "id": 99,
  "Section": "Development",
  "slug": "my-article.html",
  "name": "Article Title",
  "description": "120-160 character description for SEO",
  "keywords": "keyword1, keyword2, keyword3",
  "seo": {
    "title": "SEO-optimized title (30-60 chars)",
    "description": "Meta description",
    "canonical": "https://markhazleton.com/my-article.html"
  },
  "og": { "title": "...", "description": "..." },
  "twitter": { "title": "...", "description": "..." }
}
```

## Technology Stack Preferences

**PUG Templates** (v3.0.3): Use for all HTML with 2-space indentation, extends/block inheritance, and semantic HTML5 elements

**Bootstrap 5** (v5.3.8): Use utility classes exclusively - avoid custom CSS unless critical

**SCSS Compilation**: Dual-pipeline system (main + modern styles) with PostCSS, autoprefixer, and cssnano

**Content Management**: JSON-based system (`articles.json`, `projects.json`, `sections.json`) drives all dynamic content

**Bootstrap Icons**: ONLY icon library allowed - use `bi bi-*` classes (NO Font Awesome, Material Icons, etc.)

## Code Standards

### PUG Template Conventions

- Use 2-space indentation consistently
- Use semantic HTML5 elements: `article`, `section`, `header`, `footer`, `nav`
- Implement proper template inheritance with `extends` and `block` patterns
- Use interpolation syntax `#{variable}` for dynamic content
- Use buffered code `=` for escaped output, unbuffered `!=` for HTML content

### Critical PUG Formatting Rules (Error Prevention)

- **NEVER concatenate elements on the same line** - Each PUG element must be on its own line
- **Maintain consistent indentation** - PUG is extremely sensitive to indentation inconsistencies
- **Always include proper spacing between sections** - Add blank lines between major sections
- **Check for missing newlines** - Especially after text content or pipe (`|`) operators
- **Validate nested structures** - Ensure proper parent-child relationships with correct indentation
- **Test build after major edits** - Run `npm run build:pug` frequently to catch errors early
- **NEVER mix PUG syntax with period continuation** - When using `.` for multi-line text blocks, use pure HTML for links and markup

### Period Continuation Syntax Rules

**CRITICAL**: The period (`.`) continuation character creates a literal text block where PUG syntax is NOT parsed. Any markup within must be pure HTML.

```pug
// ❌ WRONG: PUG link syntax in period continuation block
p.mb-3.
  Visit the 
  a(href='https://example.com' target='_blank') website
  |  for more information.

// ✅ CORRECT: Use pure HTML for links in period continuation
p.mb-3.
  Visit the <a href="https://example.com" target="_blank">website</a> for more information.

// ✅ ALTERNATIVE: Split content to use PUG syntax
p.mb-3
  | Visit the 
  a(href='https://example.com' target='_blank') website
  |  for more information.
```

**Key Rules for Period Continuation:**
- After `p.mb-3.` or similar, everything is treated as literal text
- Use HTML entities: `<a href="...">`, `<strong>`, `<em>`, etc.
- Or split the paragraph to avoid period continuation
- NEVER try to use `a(href='...')` inside a period continuation block
- Same applies to any PUG syntax: `strong`, `em`, `code`, etc.

### Common PUG Formatting Errors to Avoid

```pug
// ❌ WRONG: Missing newline after ul element
ul.list-group.list-group-flush                    li.list-group-item Content

// ✅ CORRECT: Proper newline and indentation
ul.list-group.list-group-flush
  li.list-group-item Content

// ❌ WRONG: Missing indentation for nested elements
.card-body                          h6.card-title
  i.bi.bi-icon.me-2
  | Title Text

// ✅ CORRECT: Proper indentation structure
.card-body
  h6.card-title
    i.bi.bi-icon.me-2
    | Title Text

// ❌ WRONG: Text content merged with next element
p.
  Some content text            section#next-section.mb-5

// ✅ CORRECT: Proper spacing between elements
p.
  Some content text

section#next-section.mb-5
```

### PUG Build Validation Process

1. After significant changes, immediately run `npm run build:pug`
2. If build fails, check for:
    - Inconsistent indentation (usually 2-space increments)
    - Missing newlines between elements
    - Incorrectly nested structures
    - Mixed tabs and spaces
    - Missing `.` (dot) syntax after code elements containing HTML/text content
3. Use consistent 2-space indentation throughout
4. Validate that all opening elements have proper closing structure

### Critical Code Block Formatting Rules

When including code examples in articles, follow these strict formatting rules:

```pug
// ❌ WRONG: Missing dot syntax causes build errors
pre.language-html.bg-dark.text-light.p-3.rounded
  code.language-html.text-light
    <button class="btn btn-primary">
      Button Text
    </button>

// ✅ CORRECT: Dot syntax prevents PUG from parsing HTML as markup
pre.language-html.bg-dark.text-light.p-3.rounded
  code.language-html.text-light.
    <button class="btn btn-primary">
      Button Text
    </button>
```

**CRITICAL**: Always add `.` (dot) after `code` elements when they contain HTML, JavaScript, or other markup that could be misinterpreted by PUG as actual template code.

### Bootstrap 5 Implementation

- Prioritize utility classes over custom CSS: `mb-4`, `py-3`, `text-primary`
- Use responsive utilities: `d-none d-md-block`, `col-12 col-lg-8`
- Apply consistent spacing using Bootstrap's spacing scale
- Use semantic color classes: `bg-primary`, `text-muted`, `border-secondary`
- Implement mobile-first responsive design approach

### File Structure

```
views/
├── layout/base.pug
├── pages/
├── components/
└── mixins/
```

## Content Architecture

Create templates for professional article/blog content with proper SEO structure.

Implement article metadata structure with title, publishDate, category, tags, and author fields.

Design responsive layouts suitable for technical articles and project management content.

Include professional bio sections highlighting Solutions Architect expertise.

## Professional Branding Requirements

**Clean, professional design** suitable for Solutions Architect / Project Management consultant portfolio

**Content tone**: Conversational and practical - avoid marketing hyperbole and excessive superlatives

**Visual design**: Minimize color chaos - use Bootstrap semantic colors sparingly, favor muted/neutral tones

**Typography hierarchy**: Enhance readability for technical content with consistent spacing (mb-4, py-3, etc.)

## Content Style and Visual Design Guidelines

### Writing Standards
- **Casual and approachable**: Write as if explaining to a colleague
- **Practical focus**: Real-world applications over theory
- **Authentic voice**: Include challenges and limitations, not just successes

### Visual Constraints
- **NO custom CSS**: Only Bootstrap classes from existing build system
- **NO inline styles**: Never use `style="..."` attributes
- **NO new icon libraries**: Bootstrap Icons only (bi bi-*)
- **Simple layouts**: Clean, organized layouts over complex multi-colored sections

## SEO and Performance

**🚨 CRITICAL: Article meta tags are NEVER added to .pug files! They are automatically generated from `articles.json` during the build process.**

**SEO Guidelines Reference**: See `SEO.md` for complete validation rules, character limits (title: 30-60, description: 120-160, og:description: 120-160), and optimization requirements

**Accessibility**: Follow WCAG guidelines - proper heading hierarchy (h1-h6), alt text for images, keyboard navigation

## Article Management

**Content System**: JSON-based with 101+ articles in `articles.json` organized by sections (Development, Case Studies, Project Management, AI & Machine Learning, etc.)

**Article Creation Process**:
1. Add entry to `src/articles.json` with complete metadata (see `Authoring.md`)
2. Create `.pug` file in `src/pug/articles/` extending `modern-layout`
3. Use article mixins from `src/pug/modules/article-mixins.pug` for consistent components
4. Run `npm run build` to compile and generate SEO tags automatically

**Available Mixins**:
- `+articleHeader(article)` - Hero section with title/subtitle
- `+tableOfContents(items)` - Navigation for long articles
- `+articleSection(id, title, iconClass)` - Consistent section headers
- `+codeBlock(code, language)` - Syntax-highlighted code blocks
- `+alertBox(type, title, content)` - Info/warning/success alerts

## Generated Documentation Management

**IMPORTANT**: All Copilot-generated .md files (reports, documentation, summaries, etc.) must be placed in `/copilot/session-{date}/` directory structure.

- Use format: `/copilot/session-YYYY-MM-DD/filename.md`
- Example: `/copilot/session-2025-08-31/BUILD_OPTIMIZATION_REPORT.md`
- This keeps generated documentation organized and separate from core project files
- Date should reflect when the session/analysis was performed

## Deployment & CI/CD

**GitHub Actions**: Automated deployment on push to `main` branch (`.github/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml`)
- Sets up Node.js 20 with npm caching
- Runs `npm ci --include=dev` for all dependencies
- Executes `npm run build` to generate production files
- Deploys `docs/` directory to Azure Static Web Apps
- Triggers IndexNow API for search engine indexing

**Local Development**: `npm start` runs build + BrowserSync server on port 3000 (requires manual rebuild on changes)

**Site Maintenance**: Automated audits via GitHub Actions
- Monthly (1st @ 09:00 UTC): Full audit with Lighthouse, accessibility, SEO, SSL checks → `reports/YYYY-MM.md`
- Nightly (daily @ 09:00 UTC): Quick checks including homepage audit and SSL status

**Audit Commands**:
```bash
npm run audit:all    # Complete audit suite
npm run audit:perf   # Lighthouse performance
npm run audit:seo    # SEO and accessibility
npm run audit:a11y   # pa11y-ci accessibility
npm run audit:ssl    # SSL certificate monitoring
```

## WebAdmin - AI Content Generation

**Location**: `WebAdmin/mwhWebAdmin/` - .NET Core web application for content management

**AI Features**: Single-click generation of all SEO metadata using OpenAI GPT-4
- Generates: keywords, description, summary, SEO tags, Open Graph, Twitter Card metadata
- Provides: Real-time validation with A-F grading, character counting, live feedback
- Integration: Direct npm build system integration for seamless content updates

**Technical Stack**: ASP.NET Core with structured logging (ILogger), OpenAI structured output API

## Example Code Patterns

When generating PUG templates, follow this structure:

```pug
extends layout/base

block content
  main.container.py-4
    article.row
      .col-12.col-lg-8
        header.mb-4
          h1.display-4= title
          .text-muted= publishDate
        .article-content!= content
      aside.col-12.col-lg-4
        // Sidebar content
```

When creating Bootstrap components, use utility classes:

```pug
.card.shadow-sm
  .card-body
    h5.card-title= article.title
    p.card-text.text-muted= article.excerpt
    a.btn.btn-primary(href=article.url) Read More
```

### Good vs. Bad Content Styling Examples

#### ❌ BAD: Excessive colors and marketing style

```pug
// DON'T DO THIS - too many competing colors and marketing language
.row.g-3.mb-4
  .col-md-6
    .card.h-100.border-danger
      .card-header.bg-danger.text-white
        h5.card-title.mb-0
          i.fa.fa-exclamation-triangle.me-2  // Wrong icons!
          | 🚨 REVOLUTIONARY Bootstrap Challenges! 🚨
      .card-body
        p.fw-bold.text-danger GAME-CHANGING PROBLEMS:
  .col-md-6
    .card.h-100.border-success
      .card-header.bg-success.text-white
        h5.card-title.mb-0
          i.fa.fa-check.me-2  // Wrong icons!
          | ✨ AMAZING Tailwind Benefits! ✨
```

#### ✅ GOOD: Clean, casual, and readable

```pug
// DO THIS - simple, conversational, consistent styling
.row.g-3.mb-4
  .col-lg-6
    .card.border.h-100
      .card-header.bg-light
        h5.card-title.mb-0
          i.bi.bi-exclamation-triangle.me-2.text-muted
          | Bootstrap Challenges
      .card-body
        p.mb-3 Here's what I ran into while using Bootstrap:
        ul.mb-0
          li Design consistency issues across projects
          li Override complexity when customizing
  .col-lg-6
    .card.border.h-100
      .card-header.bg-light
        h5.card-title.mb-0
          i.bi.bi-check-circle.me-2.text-muted
          | Tailwind Benefits
      .card-body
        p.mb-3 What I found helpful about Tailwind:
        ul.mb-0
          li More control over design decisions
          li Smaller bundle sizes with purging
```

### Code Block Best Practices

Always use dark backgrounds for code blocks to improve readability:

```pug
// Correct code block formatting
pre.language-html.bg-dark.text-light.p-3.rounded
  code.language-html.text-light.
    <button class="btn btn-primary">
      Click Me
    </button>
```

## YouTube Embed Best Practices

### ⚠️ Critical Configuration Requirements

**X-Frame-Options Header**: MUST be `SAMEORIGIN` (NOT `DENY`)
- `X-Frame-Options: DENY` blocks ALL iframes including YouTube embeds
- Changed in `staticwebapp.config.json` from DENY to SAMEORIGIN
- This is the #1 cause of "Video player configuration error" (Error 153)

### Required CSP Directives for YouTube

All three configuration files must have identical CSP directives:
- `src/staticwebapp.config.json` (deployed to Azure)
- `src/pug/layouts/modern-layout.pug` (local development)
- `src/pug/layouts/performance-optimized-layout.pug` (optimized pages)

**Complete YouTube CSP Requirements:**
```http
default-src 'self' data:;
script-src 'self' 'unsafe-inline' 'unsafe-eval' data: https://www.youtube.com https://s.ytimg.com https://*.youtube.com;
connect-src 'self' data: https://www.youtube.com https://www.youtube.com/youtubei/v1/ https://*.youtube.com https://*.googlevideo.com https://*.ytimg.com;
img-src 'self' data: https://i.ytimg.com https://*.ytimg.com https://*.youtube.com https://*.googlevideo.com https://*.ggpht.com;
media-src 'self' data: https://*.googlevideo.com https://*.youtube.com blob:;
frame-src 'self' data: https://www.youtube.com https://www.youtube-nocookie.com;
worker-src 'self' blob:;
child-src 'self' data: https://www.youtube.com https://www.youtube-nocookie.com;
```

**Key CSP Elements:**
- `https://*.ggpht.com` in img-src - Required for YouTube thumbnails (Google Photos CDN)
- `https://www.youtube.com/youtubei/v1/` in connect-src - YouTube API endpoint
- `blob:` in media-src and worker-src - Required for video playback
- `data:` scheme across multiple directives - Required for YouTube player internal data URLs
- `'unsafe-eval'` in script-src - Required for YouTube player JavaScript

### Standard YouTube iframe Pattern

**ALWAYS use this exact pattern for ALL YouTube embeds:**

```pug
.ratio.ratio-16x9
  iframe(
    src="https://www.youtube.com/embed/VIDEO_ID"
    title="Descriptive Video Title"
    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
    referrerpolicy="strict-origin-when-cross-origin"
    allowfullscreen
  )
```

**Critical Requirements:**
- ✅ `referrerpolicy="strict-origin-when-cross-origin"` - Modern security best practice
- ✅ `allow` attribute with full permissions list
- ✅ Descriptive `title` attribute (not "YouTube video player")
- ✅ `allowfullscreen` boolean attribute (no value needed)
- ❌ NO `frameborder` attribute (deprecated in HTML5)
- ❌ NO `sandbox` attribute (breaks YouTube functionality)

### YouTube Domain Options

**youtube.com vs youtube-nocookie.com:**
- `youtube.com` - Full tracking, immediate cookies, all features
- `youtube-nocookie.com` - GDPR-friendly, delayed cookies, reduced tracking

**Use youtube-nocookie.com for:**
- European audience (GDPR compliance)
- Privacy-conscious implementations
- Cookie consent requirements

### Common YouTube Embed Issues

**Error 153 - Video player configuration error:**
- Caused by: X-Frame-Options: DENY blocking iframe
- Caused by: Missing CSP directives
- Caused by: Incorrect referrer policy
- Fixed by: Setting X-Frame-Options: SAMEORIGIN + complete CSP

**Tracking/API failures (ERR_BLOCKED_BY_CLIENT):**
- Often caused by browser ad blockers (not CSP)
- May show as failed www.youtube.com/youtubei/v1/log_event requests
- Does not affect video playback if CSP is correct
- Users with ad blockers may see these errors (expected behavior)

### iframe Attributes Reference

**Required Attributes:**
```pug
src="https://www.youtube.com/embed/VIDEO_ID"              // Video URL
title="Descriptive title for accessibility"               // Screen reader support
allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
referrerpolicy="strict-origin-when-cross-origin"          // Security best practice
allowfullscreen                                            // Enable fullscreen mode
```

**Deprecated/Forbidden:**
- ❌ `frameborder="0"` - Use CSS border instead
- ❌ `sandbox` - Breaks YouTube player unless carefully configured
- ❌ Generic titles like "YouTube video player"

### Bootstrap Responsive Wrapper

**Always wrap YouTube iframes in Bootstrap ratio containers:**

```pug
.ratio.ratio-16x9    // 16:9 aspect ratio (standard)
  iframe(...)
  
.ratio.ratio-4x3     // 4:3 aspect ratio (legacy)
  iframe(...)
  
.ratio.ratio-1x1     // Square video
  iframe(...)
```

This maintains aspect ratio across all screen sizes and prevents layout shift.

## Content Context

This is a professional website for Mark Hazleton, a Solutions Architect and Project Management consultant.

Target audience includes technical professionals, potential clients, and industry peers.

Content focuses on project management insights, web development, and IT solutions.

All code and design decisions should reflect technical competence and professional credibility.

## Debugging and Error Prevention

### PUG Error Patterns and Solutions

#### Error: "Inconsistent indentation. Expecting either X or Y spaces/tabs"

**Cause**: Mixed indentation levels or improper nesting structure
**Solution**:

- Use exactly 2 spaces per indentation level
- Never mix tabs and spaces
- Ensure parent-child relationships are properly indented
- Use a code editor with visible whitespace to detect issues

#### Error: Elements concatenated on same line

**Cause**: Missing newlines between PUG elements
**Common locations**:

- After `ul.list-group` before first `li` item
- After text content (using `|` operator) before next element
- After `block` declarations before content
- Between `section` elements

**Prevention checklist**:

- [ ] Each PUG element starts on its own line
- [ ] Proper blank lines between major sections
- [ ] No text content merged with element declarations
- [ ] Table of contents lists have proper newlines
- [ ] All accordion items are properly separated

### Pre-commit Validation Steps

1. **Visual inspection**: Check for proper indentation alignment
2. **Build test**: Run `npm run build:pug` before committing
3. **Content review**: Ensure no missing content in sections
4. **Structure validation**: Verify all sections have complete content

### Content Completeness Checklist

When creating comprehensive articles, ensure all sections contain:

- [ ] Proper heading structure with icons
- [ ] Complete paragraph content (no empty sections)
- [ ] All accordion items have full button and body content
- [ ] FAQ sections have questions AND answers
- [ ] Code examples are complete and properly formatted
- [ ] All card components have complete titles and content

### Emergency Debug Commands

```bash
# Quick PUG syntax check
npm run build:pug

# Full build to catch all errors
npm run build

# Clean and rebuild if caching issues
npm run clean && npm run build
```

### Container Structure Consistency

**CRITICAL**: Maintain consistent container structure throughout articles to prevent width jumping:

```pug
// ✅ CORRECT: All sections inside consistent container
article#main-article
  .container
    .row
      .col-lg-8.mx-auto
        // ALL article sections go here
        section#introduction.mb-5
          h2.h3.mb-4 Section Title

        section#main-content.mb-5
          h2.h3.mb-4 Another Section

        section#conclusion.mb-5
          h2.h3.mb-4 Final Section

// ❌ WRONG: Sections breaking out of container cause width jumping
article#main-article
  .container
    .row
      .col-lg-8.mx-auto
        section#introduction.mb-5
          h2.h3.mb-4 Section Title

// This section breaks out and causes width issues!
section#main-content.mb-5
  h2.h3.mb-4 Another Section

  .container
    .row
      .col-lg-8.mx-auto
        section#conclusion.mb-5
          h2.h3.mb-4 Final Section
```

**Key Rules**:

- Keep ALL article sections within the same `.col-lg-8.mx-auto` container
- Never break sections out to full width unless specifically intended
- Test on different screen sizes to ensure consistent layout

### Advanced PUG Patterns for Complex Articles

#### Table of Contents Structure

```pug
// ALWAYS use this exact pattern for TOC
nav#table-of-contents.mb-5(aria-label='Table of Contents')
  .card.bg-light
    .card-header
      h3.card-title.mb-0.fw-bold
        i.bi.bi-list-ul.me-2
        | Table of Contents
    .card-body
      ul.list-group.list-group-flush
        li.list-group-item: a.text-decoration-none(href='#section1') Section 1
        li.list-group-item: a.text-decoration-none(href='#section2') Section 2
```

#### Section Transitions

```pug
// ALWAYS include blank line before new sections
            p.
              Final paragraph of previous section content.

            section#new-section.mb-5
              h2.h3.mb-4
                i.bi.bi-icon.me-2
                | Section Title
```

#### Card Component Patterns

```pug
// Ensure proper indentation for nested card content
.card.mb-4
  .card-header.bg-primary.text-white
    h5.card-title.mb-0
      i.bi.bi-icon.me-2
      | Card Title
  .card-body
    p.
      Card content paragraph.

    .row.g-3
      .col-md-6
        .card.border-secondary
          .card-body
            h6.card-title
              i.bi.bi-icon.me-2
              | Nested Card Title
            p.card-text Content text here
```

#### Accordion Structures

```pug
// Complete accordion item pattern - never truncate
.accordion-item
  h3.accordion-header#headingExample
    button.accordion-button.collapsed(
      type='button'
      data-bs-toggle='collapse'
      data-bs-target='#collapseExample'
      aria-expanded='false'
      aria-controls='collapseExample'
    )
      i.bi.bi-icon.me-2
      | Accordion Title
  .accordion-collapse.collapse#collapseExample(
    aria-labelledby='headingExample'
    data-bs-parent='#accordionParent'
  )
    .accordion-body
      p.
        Complete accordion content here.
```
