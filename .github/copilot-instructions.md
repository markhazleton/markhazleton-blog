# GitHub Copilot Repository Custom Instructions
## Mark Hazleton Personal Article Site - PUG & Bootstrap 5

This file provides custom instructions for GitHub Copilot when working on Mark Hazleton's personal article site project.

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

## SEO and Performance

Generate proper meta tag structure in PUG templates using mixins.

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

## Content Context

This is a professional website for Mark Hazleton, a Solutions Architect and Project Management consultant.

Target audience includes technical professionals, potential clients, and industry peers.

Content focuses on project management insights, web development, and IT solutions.

All code and design decisions should reflect technical competence and professional credibility.
