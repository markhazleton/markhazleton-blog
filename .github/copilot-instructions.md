# GitHub Copilot Repository Custom Instructions

## Mark Hazleton Personal Article Site - PUG & Bootstrap 5

This file provides custom instructions for GitHub Copilot when working on Mark Hazleton's personal article site project.

## ⚠️ CRITICAL RULE: ARTICLE META TAGS

**🚨 NEVER ADD META TAGS TO ARTICLE .PUG FILES! 🚨**

**META TAGS ARE AUTOMATICALLY GENERATED FROM THE BUILD PROCESS USING `articles.json` DATA**

### ❌ DO NOT DO THIS in article .pug files:

```pug
block pagehead
  title Article Title
  meta(name='description', content='...')
  meta(name='keywords', content='...')

block canonical
  link(rel='canonical', href='...')

block og_overrides
  meta(property='og:title', content='...')

block twitter_overrides
  meta(name='twitter:title', content='...')
```

### ✅ CORRECT ARTICLE .PUG STRUCTURE:

```pug
extends ../layouts/modern-layout

block layout-content
  br
  // Article content starts here
  section.bg-gradient-primary.py-5
    // Content only - NO meta tags!
```

**WHY**: The build system automatically generates all SEO meta tags, canonical links, Open Graph tags, and Twitter Card tags from the metadata in `src/articles.json`. Adding them to .pug files creates duplicates and conflicts.

**REMEMBER**: Article metadata (title, description, keywords, etc.) belongs in `src/articles.json`, NOT in .pug files!

## Technology Stack Preferences

Use PUG template engine for all HTML templating, with proper indentation (2 spaces) and semantic structure.

Use Bootstrap 5 utility classes and components exclusively for styling, avoiding custom CSS unless absolutely necessary.

All PUG templates should use extends/block inheritance pattern with a base layout template.

Implement mixins for reusable components like article cards, navigation elements, and form components.

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

Maintain clean, professional design aesthetic suitable for business development.

Use typography hierarchy that enhances readability for technical content.

Include integration points for LinkedIn and professional social media.

Design layouts that showcase technical expertise and project management credentials.

## Content Style and Visual Design Guidelines

### Writing Tone and Style

- **Conversational and casual**: Write in a friendly, approachable tone as if explaining to a colleague
- **Avoid hyperbole and marketing speak**: No "revolutionary," "game-changing," or excessive superlatives
- **Keep it practical**: Focus on real-world applications and honest assessments
- **Be authentic**: Share genuine experiences, including challenges and limitations

### Visual Design Principles

- **Minimize color chaos**: Avoid using multiple bright colors that compete for attention
- **Consistent color palette**: Stick to Bootstrap's semantic colors (primary, secondary, success, info, warning, danger) sparingly
- **Reduce visual noise**: Limit the variety of styling elements on a single page
- **Prioritize readability**: Content should be easy to scan and read without visual distractions
- **Simple layouts**: Favor clean, organized layouts over complex multi-colored sections

### Icon Usage Standards

- **Bootstrap Icons ONLY**: Use only Bootstrap Icons (bi bi-\*) throughout the site
- **NO other icon libraries**: Do not use Font Awesome, Material Icons, or any other icon sets
- **Consistent icon styling**: Use `.text-primary`, `.text-muted`, or contextual colors sparingly
- **Semantic icon selection**: Choose icons that clearly represent the content or action

### Styling Constraints

- **NO custom CSS**: Only use existing Bootstrap classes available through the build system
- **NO inline styles**: Never add `style="..."` attributes to PUG elements
- **NO new style creation**: Do not create new CSS classes or modify existing ones
- **Build system dependency**: All styling must work with `npm run build` without additions

### Content Structure Guidelines

- **Avoid marketing card overload**: Limit colorful promotional cards and call-out boxes
- **Simple comparisons**: Use basic tables or simple lists instead of complex colored comparison cards
- **Minimal badges and labels**: Use badges sparingly and only when functionally necessary
- **Consistent spacing**: Rely on Bootstrap's spacing utilities (mb-4, py-3, etc.) for layout

## SEO and Performance

**🚨 CRITICAL: Article meta tags are NEVER added to .pug files! They are automatically generated from `articles.json` during the build process.**

Generate proper meta tag structure in PUG templates using mixins **ONLY for non-article pages**.

Use semantic HTML5 elements for better search engine understanding.

Implement lazy loading for images and optimize Bootstrap bundle size.

Create clean, descriptive URL structures for articles and categories.

## Accessibility Standards

Follow WCAG guidelines for color contrast and semantic markup.

Implement proper heading hierarchy (h1-h6) for screen readers.

Include alt text for images and proper form labels.

Ensure keyboard navigation support for all interactive elements.

## Article Management

Design templates that support markdown content conversion to HTML.

Create consistent article card components for listing pages.

Implement category and tag filtering functionality.

Support various content lengths and media types (text, images, code snippets).

## Generated Documentation Management

**IMPORTANT**: All Copilot-generated .md files (reports, documentation, summaries, etc.) must be placed in `/copilot/session-{date}/` directory structure.

- Use format: `/copilot/session-YYYY-MM-DD/filename.md`
- Example: `/copilot/session-2025-08-31/BUILD_OPTIMIZATION_REPORT.md`
- This keeps generated documentation organized and separate from core project files
- Date should reflect when the session/analysis was performed

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
