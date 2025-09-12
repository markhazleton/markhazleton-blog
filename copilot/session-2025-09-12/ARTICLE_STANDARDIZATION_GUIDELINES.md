# Article Standardization Guidelines

## Modern-Layout PUG Article Best Practices

**Based on:** `the-balanced-equation-crafting-the-perfect-project-team-mix.pug` Transformation  
**Date:** September 12, 2025  
**Status:** Reference Implementation Complete  

---

## 🎯 Overview

This document provides detailed guidelines for standardizing all PUG articles to follow the modern-layout best practices. The transformation of `the-balanced-equation-crafting-the-perfect-project-team-mix.pug` serves as our reference implementation.

---

## ✅ Reference Implementation Structure

### 1. **Standard Header Pattern**

```pug
extends ../layouts/modern-layout

block layout-content
  br
```

**✅ CORRECT** - Always use this exact pattern  
**❌ AVOID** - Custom layout extensions or missing `br` tag

### 2. **Modern Hero Section Pattern**

```pug
  // Hero Section
  section.bg-gradient-primary.py-5
    .container
      .row.align-items-center
        .col-lg-10.mx-auto.text-center
          h1.display-4.fw-bold.mb-3
            i.bi.bi-[icon].me-3
            | Article Title from articles.json
          h2.h3.mb-4 Subtitle from articles.json
          p.lead.mb-5
            | Introduction paragraph from articles.json summary
```

**Key Requirements:**

- ✅ Use `section.bg-gradient-primary.py-5` wrapper
- ✅ Bootstrap grid structure: `.container` → `.row` → `.col-lg-10.mx-auto.text-center`
- ✅ `h1.display-4.fw-bold.mb-3` for main title
- ✅ `h2.h3.mb-4` for subtitle
- ✅ `p.lead.mb-5` for introduction
- ✅ Bootstrap Icons with `.me-3` spacing
- ✅ Title and subtitle should match `articles.json` data

**❌ LEGACY PATTERNS TO REPLACE:**

```pug
// ❌ DON'T USE - Legacy projectmechanics patterns
article#post.projectmechanics-section.projectmechanics-section-background
  .projectmechanics-section-content
    h1
      i.bi.bi-people-fill.me-2.text-primary
      | The Balanced Team
    .subheading Building the Best Team for Your Project
```

### 3. **Main Article Structure**

```pug
  // Main Article Content
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
```

**Requirements:**

- ✅ Use `article#main-article` wrapper
- ✅ Bootstrap grid: `.container` → `.row` → `.col-lg-8.mx-auto`
- ✅ Center content in 8-column layout for optimal readability

### 4. **Article Metadata Display**

```pug
          // Article metadata and sharing
          .article-meta.text-muted.mb-4.text-center
            time(datetime=publishedDate) #{new Date(publishedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}
            span.mx-2 •
            span by Mark Hazleton
            span.mx-2 •
            span #{estimatedReadTime} min read
```

**Requirements:**

- ✅ Use `.article-meta.text-muted.mb-4.text-center` classes
- ✅ Include publish date, author, and estimated read time
- ✅ Use semantic `<time>` element with `datetime` attribute
- ✅ Proper spacing with `span.mx-2 •` separators

### 5. **Table of Contents Pattern**

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
                  li.list-group-item: a.text-decoration-none(href='#section-id') Section Title
```

**Requirements:**

- ✅ Use semantic `<nav>` with `aria-label`
- ✅ Bootstrap card structure with `.bg-light`
- ✅ List group for clean presentation
- ✅ Anchor links match section IDs
- ✅ Include for articles with 4+ major sections

---

## 🎨 Content Section Patterns

### A. **Standard Section Header**

```pug
          section#section-id.mb-5
            h2.h3.text-primary.mb-4
              i.bi.bi-[icon].me-2
              | Section Title
```

### B. **Card-Based Content Presentation**

```pug
            .row.g-4.mb-4
              .col-md-6
                .card.h-100.border-primary
                  .card-header.bg-primary.text-white
                    h4.card-title.mb-0
                      i.bi.bi-[icon].me-2
                      | Card Title
                  .card-body
                    p.card-text Content here
```

### C. **Accordion for Complex Information**

```pug
            .accordion#uniqueId.mb-4
              .accordion-item
                h4.accordion-header#headingOne
                  button.accordion-button(
                    type='button'
                    data-bs-toggle='collapse'
                    data-bs-target='#collapseOne'
                    aria-expanded='true'
                    aria-controls='collapseOne'
                  )
                    i.bi.bi-[icon].me-2
                    | Accordion Title
                .accordion-collapse.collapse.show#collapseOne(
                  aria-labelledby='headingOne'
                  data-bs-parent='#uniqueId'
                )
                  .accordion-body
                    p. Content here
```

### D. **Alert Boxes for Important Information**

```pug
            .alert.alert-info.border-0.shadow-sm.mb-4
              .d-flex.align-items-start
                i.bi.bi-info-circle.text-info.me-3.fs-4
                div
                  h5.alert-heading.mb-2 Heading
                  p.mb-0 Content here
```

### E. **Back to Top Navigation**

```pug
          // Back to Top link
          .text-center.mb-4
            a(href='#table-of-contents' class='btn btn-outline-primary')
              i.bi.bi-arrow-up-circle.me-2
              | Back to Top
```

---

## 🚫 Legacy Patterns to Eliminate

### 1. **ProjectMechanics CSS Classes**

```pug
// ❌ REMOVE ALL OF THESE:
.projectmechanics-section
.projectmechanics-section-background
.projectmechanics-section-content
.subheading
```

### 2. **Non-Bootstrap Structure**

```pug
// ❌ REPLACE WITH BOOTSTRAP GRID:
article#post.custom-class
  .custom-content-wrapper
```

### 3. **Basic Definition Lists**

```pug
// ❌ REPLACE WITH CARDS OR ACCORDIONS:
dl
  dt Term
  dd Definition
```

### 4. **Plain Unordered Lists**

```pug
// ❌ ENHANCE WITH BOOTSTRAP COMPONENTS:
ul
  li Plain list item
```

---

## 📋 Article Evaluation Checklist

### **Phase 1: Structure Assessment**

- [ ] Uses `extends ../layouts/modern-layout`
- [ ] Has proper hero section with `section.bg-gradient-primary.py-5`
- [ ] Uses Bootstrap grid throughout (`container` → `row` → `col-*`)
- [ ] Main content wrapped in `article#main-article`
- [ ] No legacy `.projectmechanics-*` classes

### **Phase 2: Content Organization**

- [ ] Article metadata display included
- [ ] Table of contents for 4+ sections
- [ ] Section IDs match TOC anchor links
- [ ] Proper heading hierarchy (h2 for sections, h3 for subsections)
- [ ] "Back to Top" links for long articles

### **Phase 3: Bootstrap Compliance**

- [ ] Uses Bootstrap 5 utility classes exclusively
- [ ] Cards for grouped content
- [ ] Accordions for complex information
- [ ] Alert boxes for important callouts
- [ ] Icons from Bootstrap Icons library
- [ ] Responsive design with proper breakpoints

### **Phase 4: Accessibility & SEO**

- [ ] Semantic HTML elements (`nav`, `article`, `section`, `time`)
- [ ] Proper `aria-label` attributes
- [ ] Heading hierarchy follows logical order
- [ ] Images have alt text (when added)
- [ ] Links are descriptive

---

## 🔄 Transformation Process

### **Step 1: Pre-Assessment**

1. Identify current article structure
2. Check articles.json for title, subtitle, summary data
3. Note any custom CSS dependencies
4. List sections for TOC planning

### **Step 2: Hero Section Conversion**

1. Replace legacy wrapper with modern hero section
2. Update title to match articles.json `name`
3. Add subtitle from articles.json `subtitle`
4. Use summary for introduction paragraph
5. Choose appropriate Bootstrap Icon

### **Step 3: Content Restructuring**

1. Convert to Bootstrap grid layout
2. Transform definition lists to cards/accordions
3. Add section IDs for navigation
4. Enhance lists with Bootstrap styling
5. Add appropriate icons and colors

### **Step 4: Navigation Enhancement**

1. Add article metadata display
2. Create table of contents
3. Add "Back to Top" links
4. Test all anchor links

### **Step 5: Validation**

1. Run build test: `npm run build:pug`
2. Check responsive design
3. Validate HTML accessibility
4. Test navigation functionality

---

## 📊 Priority Matrix for Article Updates

### **Priority 1: Immediate Updates Needed**

Articles using `.projectmechanics-*` classes or non-Bootstrap structure

### **Priority 2: Enhancement Candidates**

Articles missing table of contents, metadata display, or navigation

### **Priority 3: Optimization Targets**

Articles with basic content that could benefit from cards, accordions, or alerts

### **Priority 4: Maintenance**

Articles already following modern-layout but missing minor enhancements

---

## 🎯 Success Metrics

### **Before/After Comparison:**

- **Structure Score:** Bootstrap compliance percentage
- **Navigation Score:** TOC and "Back to Top" implementation
- **Accessibility Score:** Semantic HTML and ARIA attributes
- **User Experience Score:** Visual hierarchy and content organization

### **Build Performance:**

- No compilation errors
- Consistent build times
- Proper caching effectiveness

---

## 📝 Next Steps for Complete Standardization

1. **Audit Remaining Articles** using the evaluation checklist
2. **Batch Process Similar Articles** by section or complexity level
3. **Create Article Templates** for common patterns (interview, tutorial, case study)
4. **Update Documentation** to reflect these guidelines
5. **Implement Quality Gates** in the build process to enforce standards

---

## 💡 Key Takeaways

1. **Consistency is King** - Every article should follow the same structural patterns
2. **Bootstrap First** - Always use Bootstrap 5 utilities over custom CSS
3. **Accessibility Matters** - Semantic HTML and proper ARIA attributes are required
4. **Navigation is Essential** - TOC and metadata improve user experience
5. **Test Everything** - Build validation ensures no regressions

---

**Reference Implementation:** `the-balanced-equation-crafting-the-perfect-project-team-mix.pug`  
**Guidelines Version:** 1.0  
**Last Updated:** September 12, 2025
