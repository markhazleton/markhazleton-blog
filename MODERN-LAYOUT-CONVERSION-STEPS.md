# Modern Layout Conversion Steps

This document outlines the step-by-step process for converting articles from the legacy `articles` layout to the modern `modern-layout` following best practices.

## Conversion Steps

### 1. Update Layout Extension

**From:**

```pug
extends ../layouts/articles
```

**To:**

```pug
extends ../layouts/modern-layout
```

### 2. Restructure Meta Tags

**Before:** Meta tags were mixed in the pagehead block
**After:** Organize meta tags into proper blocks:

```pug
block pagehead
  title [Article Title]
  meta(name='description', content='[SEO Description]')
  meta(name='keywords', content='[Keywords]')
  meta(name='author', content='Mark Hazleton')

block canonical
  link(rel='canonical', href='https://markhazleton.com/[article-path]')

block og_overrides
  meta(property='og:title', content='[Article Title]')
  meta(property='og:description', content='[SEO Description]')
  meta(property='og:url', content='https://markhazleton.com/[article-path]')
  meta(property='og:type', content='article')

block twitter_overrides
  meta(name='twitter:title', content='[Article Title]')
  meta(name='twitter:description', content='[SEO Description]')
```

### 3. Validated layout-content Block Structure

```pug
block layout-content
  br
  // Hero Section
  section.bg-gradient-primary.py-5
    .container
      .row.align-items-center
        .col-lg-10.mx-auto.text-center
          h1.display-4.fw-bold.mb-3
            i.bi.bi-[icon].me-3
            | [Article Title]
          h2.h3.mb-4 [Subtitle]
          p.lead.mb-5
            | [Introduction/Summary]

  // Main Article Content
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
```

### 4. Add Table of Contents (for longer articles)

```pug
// Table of Contents
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

### 5. Convert Sections to Modern Structure

**From:** Simple headers with paragraphs

```pug
h2 Section Title
p Content here
```

**To:** Bootstrap card-based sections with icons

```pug
section#section-id.mb-5
  h2.h3.text-primary.mb-4
    i.bi.bi-[icon].me-2
    | Section Title
  .card.shadow-sm.border-0
    .card-body
      p.mb-0
        | Content here
```

### 6. Use Cards for Enhanced Visual Appeal

For important content, use colored card headers:

```pug
.card.shadow-sm.border-0
  .card-header.bg-success.text-white
    h3.h5.mb-0
      i.bi.bi-[icon].me-2
      | Card Title
  .card-body
    p.mb-0
      | Card content
```

### 7. Convert Lists to Accordions (for complex comparisons)

**From:** Definition lists

```pug
dl
  dt Term
  dd Definition
```

**To:** Bootstrap accordions

```pug
.accordion#accordion-id.mb-4
  .accordion-item
    h4.accordion-header#headingOne
      button.accordion-button.collapsed(
        type='button'
        data-bs-toggle='collapse'
        data-bs-target='#collapseOne'
        aria-expanded='false'
        aria-controls='collapseOne'
      )
        i.bi.bi-[icon].me-2
        | Term/Question
    .accordion-collapse.collapse#collapseOne(
      aria-labelledby='headingOne'
      data-bs-parent='#accordion-id'
    )
      .accordion-body
        p.mb-0
          | Definition/Answer
```

### 8. Add Conclusion Section

```pug
// Conclusion Section
section#conclusion.mb-5
  .container
    .row
      .col-lg-8.mx-auto
        .card.border-primary.shadow-sm
          .card-header.bg-primary.text-white
            h2.h3.mb-0
              i.bi.bi-trophy.me-2
              | Conclusion
          .card-body
            p.lead.mb-4
              | Summary statement
            .alert.alert-success.border-0.shadow-sm.mb-4
              .d-flex.align-items-start
                i.bi.bi-star-fill.text-success.me-3.fs-4
                div
                  h4.alert-heading.h6.mb-2 Key Takeaway
                  p.mb-0
                    | Main insight
            p.mb-0
              | Final thoughts
```

### 9. Use Modern Grid Layouts for Use Cases

**From:** Simple definition lists
**To:** Bootstrap grid with cards

```pug
.row.g-4
  .col-md-6
    .card.h-100.shadow-sm
      .card-header.bg-primary.text-white
        h4.card-title.mb-0
          i.bi.bi-[icon].me-2
          | Project Name
      .card-body
        p.card-text
          | Project description
        a.btn.btn-outline-primary(href='#', target='_blank') Visit Site
```

### 10. Add Alert Boxes for Key Information

```pug
.alert.alert-info.border-0.shadow-sm
  .d-flex.align-items-start
    i.bi.bi-info-circle-fill.text-info.me-3.fs-4
    div
      h4.alert-heading.h6.mb-2 Important Note
      p.mb-0
        | Important information
```

## Critical PUG Formatting Requirements

1. **Consistent 2-space indentation**
2. **Each element on its own line**
3. **Proper blank lines between major sections**
4. **No concatenated elements**
5. **Validate with `node scripts/build-pug.js` after changes**

## Bootstrap 5 Components Used

- **Cards** with colored headers for visual hierarchy
- **Accordions** for FAQ-style content
- **Alerts** for highlighting key information
- **Grid system** for responsive layouts
- **Bootstrap Icons** for visual enhancement
- **Utility classes** for spacing and colors

## Icon Recommendations

- `bi-cloud` - Cloud/hosting topics
- `bi-question-circle` - Why/questions
- `bi-play-circle` - Getting started
- `bi-lightning` - Performance/serverless
- `bi-arrow-repeat` - Deployment/CI/CD
- `bi-shield-check` - Security
- `bi-globe` - Real-world examples
- `bi-currency-dollar` - Cost/pricing
- `bi-bar-chart` - Comparisons
- `bi-cpu` - Technical features
- `bi-people` - Community
- `bi-rocket` - Future/innovation
- `bi-trophy` - Conclusion/success

## Testing Checklist

- [ ] PUG builds without errors (`node scripts/build-pug.js`)
- [ ] All sections have proper IDs for table of contents links
- [ ] Bootstrap components are properly structured
- [ ] Icons are appropriate and consistent
- [ ] Responsive layout works on mobile
- [ ] Meta tags are properly formatted
- [ ] All accordion items have unique IDs
- [ ] External links have proper attributes (`target='_blank'`)
