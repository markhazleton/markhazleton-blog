# Quickstart: Mobile-First Website Optimization

**Feature**: 001-mobile-first  
**Date**: January 5, 2026  
**Branch**: `001-mobile-first`

## Overview

This guide provides step-by-step instructions to implement mobile-first responsive optimization for the Mark Hazleton blog. The implementation enhances mobile user experience through responsive design, readable typography, touch-friendly navigation, and performance optimization.

**Estimated Time**: 8-12 hours (implementation + testing)

---

## Prerequisites

✅ **Required**:
- Node.js 20+ installed
- Git checkout of `001-mobile-first` branch
- Familiarity with PUG templates, SCSS, Bootstrap 5
- Local development environment set up (`npm install` completed)

✅ **Recommended**:
- Mobile device or browser devtools for testing
- Lighthouse CLI or Chrome DevTools for performance testing
- Understanding of mobile-first responsive design principles

---

## Quick Start (TL;DR)

```bash
# 1. Ensure you're on the feature branch
git checkout 001-mobile-first

# 2. Create new SCSS files for mobile-first styles
# - src/scss/_variables.scss (typography scale)
# - src/scss/_typography.scss (font system)
# - src/scss/_navigation.scss (bottom nav)
# - src/scss/_responsive.scss (breakpoint styles)

# 3. Update PUG templates
# - src/pug/layouts/modern-layout.pug (viewport meta)
# - src/pug/modules/navigation.pug (bottom nav component)
# - src/pug/index.pug (above-fold content hierarchy)

# 4. Build and test
npm run build
npm start  # Opens localhost:3000

# 5. Validate
npm run audit:perf  # Lighthouse performance
npm run audit:a11y  # Accessibility check
```

---

## Step-by-Step Implementation

### Phase 1: SCSS Foundation (2-3 hours)

#### 1.1 Create Typography Scale Variables

Create `src/scss/_variables.scss`:

```scss
// Typography Scale - 1.200 ratio (Minor Third)
// Mobile base: 16px, Tablet: 17px, Desktop: 18px

:root {
  // Base and scale
  --font-size-base: 16px;
  --scale-ratio: 1.200;
  
  // Calculated scale
  --font-size-body: var(--font-size-base);
  --font-size-h6: calc(var(--font-size-base) * var(--scale-ratio));
  --font-size-h5: calc(var(--font-size-h6) * var(--scale-ratio));
  --font-size-h4: calc(var(--font-size-h5) * var(--scale-ratio));
  --font-size-h3: calc(var(--font-size-h4) * var(--scale-ratio));
  --font-size-h2: calc(var(--font-size-h3) * var(--scale-ratio));
  --font-size-h1: calc(var(--font-size-h2) * var(--scale-ratio));
  
  // Line heights
  --line-height-body: 1.6;
  --line-height-heading: 1.3;
  
  // Spacing
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;
  
  // Touch targets
  --touch-target-min: 44px;
  --touch-target-spacing: 0.25rem;
  
  // Breakpoint adjustments
  @media (min-width: 768px) {
    --font-size-base: 17px;
  }
  
  @media (min-width: 1200px) {
    --font-size-base: 18px;
  }
}
```

#### 1.2 Create Typography System

Create `src/scss/_typography.scss`:

```scss
// Mobile-First Typography System

body {
  font-size: var(--font-size-body);
  line-height: var(--line-height-body);
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
}

h1, h2, h3, h4, h5, h6 {
  line-height: var(--line-height-heading);
  font-weight: 600;
  margin-top: var(--spacing-xl);
  margin-bottom: var(--spacing-md);
}

h1 { font-size: var(--font-size-h1); }
h2 { font-size: var(--font-size-h2); }
h3 { font-size: var(--font-size-h3); }
h4 { font-size: var(--font-size-h4); }
h5 { font-size: var(--font-size-h5); }
h6 { font-size: var(--font-size-h6); }

// Line length control
.article-content,
.content-container {
  max-width: 60ch;
  margin-left: auto;
  margin-right: auto;
  padding-left: 1.25rem;
  padding-right: 1.25rem;
  
  @media (min-width: 768px) {
    max-width: 75ch;
    padding-left: 2rem;
    padding-right: 2rem;
  }
  
  @media (min-width: 1200px) {
    padding-left: 3rem;
    padding-right: 3rem;
  }
}
```

#### 1.3 Create Bottom Navigation Styles

Create `src/scss/_navigation.scss`:

```scss
// Bottom Sticky Navigation (Mobile)

.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1030;
  background: #fff;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
  padding-bottom: env(safe-area-inset-bottom);
  
  @media (min-width: 768px) {
    position: static;
    border-top: none;
    box-shadow: none;
    padding-bottom: 0;
  }
}

.bottom-nav__list {
  display: flex;
  justify-content: space-around;
  align-items: center;
  list-style: none;
  margin: 0;
  padding: 0;
  height: 60px;
  
  @media (min-width: 768px) {
    height: auto;
    justify-content: flex-start;
    gap: 1rem;
  }
}

.bottom-nav__item {
  flex: 1;
  max-width: 80px;
  
  @media (min-width: 768px) {
    flex: 0 0 auto;
    max-width: none;
  }
}

.bottom-nav__link {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: var(--touch-target-min);
  min-width: var(--touch-target-min);
  padding: var(--spacing-sm);
  text-decoration: none;
  color: #6c757d;
  transition: all 0.2s ease;
  
  &:hover,
  &:focus {
    background: rgba(0, 0, 0, 0.05);
    color: #212529;
  }
  
  &.active {
    color: var(--bs-primary, #0d6efd);
  }
  
  @media (min-width: 768px) {
    flex-direction: row;
    gap: 0.5rem;
  }
}

.bottom-nav__icon {
  font-size: 1.25rem;
  margin-bottom: 0.25rem;
  
  @media (min-width: 768px) {
    margin-bottom: 0;
  }
}

.bottom-nav__label {
  font-size: 0.75rem;
  
  @media (min-width: 768px) {
    font-size: 1rem;
  }
}

// Add bottom padding to body to prevent content from being hidden
body {
  padding-bottom: 60px;
  
  @media (min-width: 768px) {
    padding-bottom: 0;
  }
}
```

#### 1.4 Create Responsive Utilities

Create `src/scss/_responsive.scss`:

```scss
// Mobile-First Responsive Utilities

// Touch targets
@mixin touch-target {
  @media (max-width: 991px) {
    min-height: var(--touch-target-min);
    min-width: var(--touch-target-min);
    padding: var(--spacing-sm);
    margin: var(--touch-target-spacing);
  }
}

.btn,
.form-control,
.form-select {
  @include touch-target;
}

// Responsive images
img {
  max-width: 100%;
  height: auto;
}

.img-fluid {
  max-width: 100%;
  height: auto;
}

// Mobile content spacing
.mb-mobile {
  margin-bottom: var(--spacing-md);
  
  @media (min-width: 768px) {
    margin-bottom: var(--spacing-lg);
  }
  
  @media (min-width: 1200px) {
    margin-bottom: var(--spacing-xl);
  }
}

// Hide on mobile
.hide-mobile {
  @media (max-width: 767px) {
    display: none !important;
  }
}

// Show only on mobile
.show-mobile {
  @media (min-width: 768px) {
    display: none !important;
  }
}
```

#### 1.5 Import New SCSS Files

Update `src/scss/styles.scss` and `src/scss/modern-styles.scss`:

```scss
// Add at top of both files
@import 'variables';
@import 'typography';
@import 'navigation';
@import 'responsive';

// ... existing imports ...
```

---

### Phase 2: PUG Template Updates (3-4 hours)

#### 2.1 Verify Viewport Meta Tag

Update `src/pug/layouts/modern-layout.pug` (should already exist, verify):

```pug
doctype html
html(lang='en')
  head
    meta(charset='UTF-8')
    meta(name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no')
    //- ... rest of head
```

#### 2.2 Create Bottom Navigation Component

Create `src/pug/modules/navigation.pug`:

```pug
mixin bottomNav(items, currentPath)
  nav.bottom-nav(role='navigation' aria-label='Primary Navigation')
    ul.bottom-nav__list
      each item in items
        li.bottom-nav__item
          a.bottom-nav__link(
            href=item.url
            class=currentPath === item.url ? 'active' : ''
            aria-current=currentPath === item.url ? 'page' : undefined
          )
            i.bi(class=`bi-${item.icon}`).bottom-nav__icon
            span.bottom-nav__label= item.label
```

#### 2.3 Update Modern Layout to Include Navigation

Update `src/pug/layouts/modern-layout.pug`:

```pug
include ../modules/navigation

//- After main content, before closing body
+bottomNav([
  {url: '/', icon: 'house', label: 'Home'},
  {url: '/articles.html', icon: 'file-text', label: 'Articles'},
  {url: '/projects.html', icon: 'briefcase', label: 'Projects'},
  {url: '/about.html', icon: 'person', label: 'About'},
  {url: '/contact.html', icon: 'envelope', label: 'Contact'}
], currentPath)
```

#### 2.4 Update Homepage Content Hierarchy

Update `src/pug/index.pug`:

```pug
extends layouts/modern-layout

block layout-content
  .homepage-hero
    //- Identity Block (Order 1)
    .homepage-hero__identity.mb-mobile
      .text-center
        img.rounded-circle(
          src='/assets/images/profile.jpg'
          alt='Mark Hazleton'
          width='120'
          height='120'
          loading='eager'
        )
        h1.h2.mt-3 Mark Hazleton
        p.lead Solutions Architect & Project Management Consultant
        p Delivering scalable web solutions and technical leadership
    
    //- Featured Article Block (Order 2)
    .homepage-hero__featured.mb-mobile
      .card
        img.card-img-top(
          src='/assets/images/featured-article.jpg'
          alt='Featured Article'
          loading='eager'
        )
        .card-body
          h2.card-title.h4 Latest Featured Article
          p.card-text Explore modern web development patterns and best practices...
          a.btn.btn-primary(href='/articles.html') Read Article
    
    //- CTA Block (Order 3)
    .homepage-hero__cta.mb-mobile
      .text-center
        a.btn.btn-lg.btn-primary.mb-3(href='/articles.html') View All Articles
        br
        a.text-muted(href='/projects.html') Explore My Projects →
```

#### 2.5 Add Responsive Image Mixin

Add to `src/pug/modules/article-mixins.pug`:

```pug
mixin responsiveImage(src, alt, loading, sizes)
  - var width = sizes && sizes.width || ''
  - var height = sizes && sizes.height || ''
  - var loadingAttr = loading || 'lazy'
  img.img-fluid(
    src=src
    alt=alt
    loading=loadingAttr
    width=width
    height=height
  )
```

#### 2.6 Update Article Templates

Update article templates (e.g., `src/pug/articles/*.pug`) to use new patterns:

```pug
extends ../layouts/modern-layout

block layout-content
  article.article-content
    header.mb-5
      h1 Article Title
      p.text-muted Published: January 5, 2026
    
    section.mb-5
      +responsiveImage('/assets/images/article-hero.jpg', 'Hero image', 'eager', {width: 1200, height: 630})
      
      p Article content with optimized line length...
      
      h2 Section Heading
      p More content...
      
      +responsiveImage('/assets/images/content-image.jpg', 'Content image', 'lazy', {width: 800, height: 600})
```

---

### Phase 3: Build and Test (2-3 hours)

#### 3.1 Build the Site

```bash
# Clean build
npm run clean
npm run build

# Or incremental build
npm run build:scss
npm run build:pug
```

**Verify**: No build errors, check console output.

#### 3.2 Start Development Server

```bash
npm start
```

**Expected**: Opens `http://localhost:3000` with BrowserSync.

#### 3.3 Manual Testing Checklist

Use browser DevTools device emulation:

**Mobile (375px - iPhone SE)**:
- [ ] No horizontal scrolling
- [ ] Bottom navigation visible and functional
- [ ] Text readable without zooming (16px body text)
- [ ] All buttons/links at least 44x44px
- [ ] Homepage shows identity + featured article + CTA
- [ ] Images load (hero eager, below-fold lazy)
- [ ] Line length ~35-60 characters

**Tablet (768px - iPad)**:
- [ ] Multi-column layouts where appropriate
- [ ] Bottom nav transitions to header/static nav
- [ ] Touch targets still adequate
- [ ] Line length ~45-75 characters
- [ ] Content uses screen space effectively

**Desktop (1200px+)**:
- [ ] Content doesn't stretch excessively
- [ ] Hover states work on navigation
- [ ] Line length remains optimal
- [ ] Multi-column layouts (3-4 columns)

**Orientation Testing**:
- [ ] Portrait mode works on mobile/tablet
- [ ] Landscape mode works on mobile/tablet
- [ ] Content adjusts appropriately

#### 3.4 Automated Testing

```bash
# Lighthouse performance audit
npm run audit:perf

# Target scores:
# Performance: 90+
# Accessibility: 95+
# Best Practices: 90+
# SEO: 95+

# Accessibility audit
npm run audit:a11y

# Should pass all WCAG 2.1 AA tests
# Zero touch target failures
```

#### 3.5 Real Device Testing

Test on actual devices if available:
- iPhone SE / iPhone 12/13 (iOS Safari)
- Android phone (Chrome)
- iPad (Safari)
- Various desktop browsers (Chrome, Firefox, Edge, Safari)

---

## Troubleshooting

### Issue: Bottom navigation not showing

**Solution**: Check z-index conflicts, verify SCSS is compiled, check browser console for errors.

```scss
// Ensure z-index is appropriate
.bottom-nav {
  z-index: 1030; // Below Bootstrap modals (1055)
}
```

### Issue: Horizontal scrolling on mobile

**Solution**: Check for fixed-width elements, verify responsive images, add `overflow-x: hidden` as last resort.

```scss
// Debug helper
* {
  max-width: 100%;
}
```

### Issue: Touch targets too small

**Solution**: Verify touch-target mixin is applied, check button/link padding.

```bash
# Use pa11y to find touch target issues
npm run audit:a11y
```

### Issue: Text too small on mobile

**Solution**: Verify CSS custom properties are loading, check base font size.

```css
/* Debug in browser console */
getComputedStyle(document.documentElement).getPropertyValue('--font-size-base')
```

### Issue: Build fails

**Solution**: Check SCSS syntax, verify PUG indentation (2 spaces), run `npm run clean` and rebuild.

```bash
npm run clean
npm run build:scss  # Test SCSS separately
npm run build:pug   # Test PUG separately
```

---

## Performance Optimization Checklist

- [ ] All below-fold images use `loading="lazy"`
- [ ] Hero/featured images use `loading="eager"`
- [ ] Images have width/height attributes (prevent CLS)
- [ ] Font loading optimized (WOFF2 format)
- [ ] CSS minified in production
- [ ] No unused CSS (verify in Lighthouse)
- [ ] Touch targets meet 44x44px minimum
- [ ] Text contrast meets 4.5:1 ratio
- [ ] Viewport meta tag configured correctly
- [ ] No layout shift (CLS < 0.1)

---

## Validation Commands

```bash
# Full audit suite
npm run audit:all

# Individual audits
npm run audit:perf   # Lighthouse performance
npm run audit:seo    # SEO validation
npm run audit:a11y   # Accessibility (pa11y)
npm run audit:ssl    # SSL certificate check

# Build validation
npm run build        # Should complete without errors
npm run clean:cache  # Clear cache if issues
```

---

## Next Steps

After completing implementation:

1. **Run Full Test Suite**: Execute all automated audits
2. **Document Results**: Record Lighthouse scores, accessibility findings
3. **Create PR**: Commit changes with descriptive messages
4. **Request Review**: Tag reviewers for code review
5. **Deploy to Staging**: Test on staging environment
6. **Monitor Metrics**: Track mobile bounce rate, session duration
7. **Iterate**: Address any issues found in testing

---

## Resources

- **Spec**: [spec.md](./spec.md)
- **Research**: [research.md](./research.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Contracts**: [contracts/frontend-contracts.md](./contracts/frontend-contracts.md)
- **Bootstrap 5 Docs**: https://getbootstrap.com/docs/5.3/
- **WCAG 2.1**: https://www.w3.org/WAI/WCAG21/quickref/
- **Lighthouse**: https://developers.google.com/web/tools/lighthouse

---

## Support

If you encounter issues:
1. Check [troubleshooting section](#troubleshooting) above
2. Review [research.md](./research.md) for implementation patterns
3. Consult [contracts/frontend-contracts.md](./contracts/frontend-contracts.md) for interface specifications
4. Review `.github/copilot-instructions.md` for PUG/Bootstrap patterns

**Estimated completion time with testing**: 8-12 hours
