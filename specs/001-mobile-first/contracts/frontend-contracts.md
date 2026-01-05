# Contracts: SCSS/CSS Patterns & PUG Template Interfaces

**Feature**: 001-mobile-first  
**Date**: January 5, 2026  
**Type**: Front-end Contracts (CSS Patterns, PUG Mixins, HTML Structure)

## Overview

This document defines the "contracts" for mobile-first implementation - the interfaces, patterns, and structures that components must follow for consistent responsive behavior. For a static site generator, these contracts are CSS class patterns, SCSS mixins, and PUG template structures rather than HTTP APIs.

---

## SCSS Mixins & Functions

### Mobile-First Media Query Mixin

**Contract**: `responsive-breakpoint($breakpoint)`

Generates mobile-first `min-width` media queries using Bootstrap 5 breakpoint variables.

```scss
/// @param {string} $breakpoint - Breakpoint name (sm, md, lg, xl, xxl)
/// @returns Media query block with min-width
/// @example
///   .element {
///     font-size: 16px;
///     @include responsive-breakpoint(md) {
///       font-size: 18px;
///     }
///   }
@mixin responsive-breakpoint($breakpoint) {
  @if map-has-key($grid-breakpoints, $breakpoint) {
    $min-width: map-get($grid-breakpoints, $breakpoint);
    @media (min-width: $min-width) {
      @content;
    }
  } @else {
    @error "Unknown breakpoint: #{$breakpoint}. Use: sm, md, lg, xl, xxl";
  }
}
```

**Usage Requirements**:
- Must use standard Bootstrap breakpoint names
- Mobile styles (base) written outside mixin
- Larger screens progressively enhanced inside mixin

---

### Typography Scale Function

**Contract**: `font-size($level)`

Calculates font size for given typography level using CSS custom properties.

```scss
/// @param {string} $level - Typography level (h1-h6, body)
/// @returns CSS custom property reference
/// @example
///   h2 {
///     font-size: font-size(h2);
///   }
@function font-size($level) {
  @return var(--font-size-#{$level});
}
```

**Required CSS Custom Properties** (defined in :root):
```css
--font-size-base: 16px;
--scale-ratio: 1.200;
--font-size-body: var(--font-size-base);
--font-size-h6: calc(var(--font-size-base) * var(--scale-ratio));
--font-size-h5: calc(var(--font-size-h6) * var(--scale-ratio));
--font-size-h4: calc(var(--font-size-h5) * var(--scale-ratio));
--font-size-h3: calc(var(--font-size-h4) * var(--scale-ratio));
--font-size-h2: calc(var(--font-size-h3) * var(--scale-ratio));
--font-size-h1: calc(var(--font-size-h2) * var(--scale-ratio));
```

**Breakpoint Overrides** (adjust base, cascade maintains ratio):
```css
@media (min-width: 768px) {
  :root { --font-size-base: 17px; }
}

@media (min-width: 1200px) {
  :root { --font-size-base: 18px; }
}
```

---

### Touch Target Mixin

**Contract**: `touch-target($mobile-only: true)`

Ensures interactive elements meet WCAG 2.1 AA minimum touch target requirements.

```scss
/// @param {boolean} $mobile-only - Apply only on mobile/tablet (default: true)
/// @example
///   .custom-button {
///     @include touch-target;
///   }
@mixin touch-target($mobile-only: true) {
  @if $mobile-only {
    @media (max-width: 991px) {
      min-height: 44px;
      min-width: 44px;
      padding: 0.5rem;
      margin: 0.25rem;
    }
  } @else {
    min-height: 44px;
    min-width: 44px;
    padding: 0.5rem;
    margin: 0.25rem;
  }
}
```

**Validation**:
- Must result in minimum 44x44px clickable/tappable area
- Must have at least 8px spacing between adjacent touch targets
- Desktop (992px+) may override with smaller targets if hover-based

---

### Line Length Control Mixin

**Contract**: `line-length($breakpoint)`

Controls character count per line for optimal readability.

```scss
/// @param {string} $breakpoint - Target breakpoint (mobile, tablet, desktop)
/// @example
///   .article-content {
///     @include line-length(mobile);
///     @include responsive-breakpoint(md) {
///       @include line-length(tablet);
///     }
///   }
@mixin line-length($breakpoint) {
  @if $breakpoint == 'mobile' {
    max-width: 60ch; // ~35-60 characters
    padding-left: 1.25rem;
    padding-right: 1.25rem;
  } @else if $breakpoint == 'tablet' {
    max-width: 75ch; // ~45-75 characters
    padding-left: 2rem;
    padding-right: 2rem;
  } @else if $breakpoint == 'desktop' {
    max-width: 75ch; // ~45-75 characters
    padding-left: 3rem;
    padding-right: 3rem;
  }
  margin-left: auto;
  margin-right: auto;
}
```

---

## CSS Class Contracts

### Bottom Sticky Navigation

**Contract**: `.bottom-nav` component

Required structure and behavior for mobile bottom navigation bar.

```html
<nav class="bottom-nav" role="navigation" aria-label="Primary">
  <ul class="bottom-nav__list">
    <li class="bottom-nav__item">
      <a href="/" class="bottom-nav__link">
        <i class="bi bi-house bottom-nav__icon"></i>
        <span class="bottom-nav__label">Home</span>
      </a>
    </li>
    <!-- 4-5 items total -->
  </ul>
</nav>
```

**Required CSS**:
```scss
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1030;
  padding-bottom: env(safe-area-inset-bottom);
  background: #fff;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
  
  @include responsive-breakpoint(md) {
    position: static;
    box-shadow: none;
  }
}

.bottom-nav__list {
  display: flex;
  justify-content: space-around;
  list-style: none;
  margin: 0;
  padding: 0;
}

.bottom-nav__item {
  flex: 1;
  max-width: 80px;
}

.bottom-nav__link {
  @include touch-target;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-decoration: none;
  color: inherit;
  
  &:hover, &:focus {
    background: rgba(0, 0, 0, 0.05);
  }
  
  &.active {
    color: var(--bs-primary);
  }
}

.bottom-nav__icon {
  font-size: 1.25rem;
  margin-bottom: 0.25rem;
}

.bottom-nav__label {
  font-size: 0.75rem;
}
```

**Validation**:
- Must be fixed on mobile (< 768px)
- Must be static on tablet+ (>= 768px)
- Must handle iPhone notch with safe-area-inset-bottom
- Must have z-index below modals but above content

---

### Responsive Image Pattern

**Contract**: Image elements must follow lazy loading pattern

```html
<!-- Above-fold / hero images -->
<img 
  class="img-fluid" 
  src="/assets/images/hero.jpg" 
  alt="Descriptive text"
  loading="eager"
  width="1200"
  height="630"
>

<!-- Below-fold / article images -->
<img 
  class="img-fluid" 
  src="/assets/images/article.jpg" 
  alt="Descriptive text"
  loading="lazy"
  width="800"
  height="600"
>
```

**Requirements**:
- All images must have `.img-fluid` class (Bootstrap responsive)
- Above-fold: `loading="eager"`
- Below-fold: `loading="lazy"`
- All images must have `alt` attribute
- Width/height attributes prevent layout shift

---

### Article Content Container

**Contract**: `.article-content` with proper line length control

```html
<article class="article-content">
  <h1>Article Title</h1>
  <p>Body content with controlled line length...</p>
</article>
```

**Required CSS**:
```scss
.article-content {
  @include line-length(mobile);
  
  @include responsive-breakpoint(md) {
    @include line-length(tablet);
  }
  
  @include responsive-breakpoint(xl) {
    @include line-length(desktop);
  }
  
  // Typography
  font-size: font-size(body);
  line-height: 1.6;
  
  h1 { font-size: font-size(h1); }
  h2 { font-size: font-size(h2); }
  h3 { font-size: font-size(h3); }
  h4 { font-size: font-size(h4); }
  h5 { font-size: font-size(h5); }
  h6 { font-size: font-size(h6); }
  
  // Spacing
  p, ul, ol {
    margin-bottom: 1.5rem;
  }
  
  h2, h3, h4, h5, h6 {
    margin-top: 2rem;
    margin-bottom: 1rem;
  }
}
```

---

## PUG Template Contracts

### Mobile-Optimized Layout Extension

**Contract**: All page templates must extend `modern-layout` with proper viewport configuration

```pug
extends ../layouts/modern-layout

block layout-content
  article.article-content
    section.mb-5
      h1 Page Title
      p Content with proper line length control
```

**Required in modern-layout.pug head**:
```pug
meta(name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no')
```

---

### Responsive Image Mixin

**Contract**: `+responsiveImage(src, alt, loading, sizes)`

PUG mixin for consistent responsive image implementation.

```pug
//- @param {string} src - Image source path
//- @param {string} alt - Alternative text
//- @param {string} loading - 'eager' or 'lazy'
//- @param {object} sizes - Optional width/height
//- @example
//-   +responsiveImage('/assets/hero.jpg', 'Hero image', 'eager', {width: 1200, height: 630})
mixin responsiveImage(src, alt, loading, sizes)
  - var width = sizes && sizes.width || ''
  - var height = sizes && sizes.height || ''
  img.img-fluid(
    src=src
    alt=alt
    loading=loading
    width=width
    height=height
  )
```

---

### Bottom Navigation Mixin

**Contract**: `+bottomNav(items, currentPath)`

PUG mixin for consistent bottom navigation implementation.

```pug
//- @param {array} items - Navigation items [{url, icon, label}]
//- @param {string} currentPath - Current page path for active state
//- @example
//-   +bottomNav([
//-     {url: '/', icon: 'house', label: 'Home'},
//-     {url: '/articles.html', icon: 'file-text', label: 'Articles'}
//-   ], currentPath)
mixin bottomNav(items, currentPath)
  nav.bottom-nav(role='navigation' aria-label='Primary')
    ul.bottom-nav__list
      each item in items
        li.bottom-nav__item
          a.bottom-nav__link(
            href=item.url
            class=currentPath === item.url ? 'active' : ''
            aria-current=currentPath === item.url ? 'page' : undefined
          )
            i.bi(class=`bi-${item.icon}` class='bottom-nav__icon')
            span.bottom-nav__label= item.label
```

---

### Content Hierarchy Mixin

**Contract**: `+homepageHero(identity, featured, cta)`

PUG mixin for above-the-fold mobile content hierarchy.

```pug
//- @param {object} identity - {name, title, description, image}
//- @param {object} featured - Article object from articles.json
//- @param {object} cta - {text, url}
//- @example
//-   +homepageHero(identity, featuredArticle, {text: 'View All Articles', url: '/articles.html'})
mixin homepageHero(identity, featured, cta)
  .homepage-hero
    .homepage-hero__identity
      if identity.image
        img.rounded-circle(src=identity.image alt=identity.name width='120' height='120')
      h1.h2= identity.name
      p.lead= identity.title
      p= identity.description
    
    .homepage-hero__featured
      .card
        if featured.image
          img.card-img-top(src=featured.image alt=featured.title loading='eager')
        .card-body
          h2.card-title.h4= featured.title
          p.card-text= featured.excerpt
          a.btn.btn-primary(href=featured.url) Read Article
    
    .homepage-hero__cta
      a.btn.btn-lg.btn-primary(href=cta.url)= cta.text
```

---

## Validation Contracts

### Lighthouse CI Assertions

**Contract**: Mobile performance thresholds

```json
{
  "ci": {
    "assert": {
      "preset": "lighthouse:recommended",
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.95}],
        "viewport": ["error"],
        "uses-responsive-images": ["error"],
        "font-size": ["error"],
        "tap-targets": ["error"],
        "first-contentful-paint": ["error", {"maxNumericValue": 1500}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 2500}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}]
      }
    }
  }
}
```

---

### pa11y-ci Configuration

**Contract**: Accessibility testing configuration

```json
{
  "defaults": {
    "standard": "WCAG2AA",
    "runners": ["axe"],
    "viewport": {
      "width": 375,
      "height": 667
    }
  },
  "urls": [
    "http://localhost:3000/",
    "http://localhost:3000/articles.html",
    "http://localhost:3000/projects.html"
  ]
}
```

---

## Breaking Change Policy

Any changes to these contracts that affect existing templates or styles must:

1. **Document the change**: Update this contracts document with migration guide
2. **Version the contract**: Add date and version number to changed sections
3. **Provide deprecation notice**: Mark old patterns as deprecated with timeline
4. **Test comprehensively**: Run full build + Lighthouse + pa11y validation
5. **Update templates gradually**: Phase migration across templates if needed

---

## Summary

These contracts define the interfaces for mobile-first implementation:

- **SCSS Mixins**: Reusable responsive patterns (breakpoints, typography, touch targets)
- **CSS Classes**: Component structures (bottom-nav, article-content, responsive images)
- **PUG Mixins**: Template helpers (responsive images, navigation, content hierarchy)
- **Validation**: Automated testing contracts (Lighthouse thresholds, pa11y config)

All contracts enforce mobile-first methodology, WCAG 2.1 AA compliance, and performance optimization within the existing PUG/Bootstrap 5/SCSS architecture.
