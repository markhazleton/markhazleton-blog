# Data Model: Mobile-First Website Optimization

**Feature**: 001-mobile-first  
**Date**: January 5, 2026  
**Phase**: 1 - Design & Contracts

## Overview

This feature is presentation-layer optimization and does not introduce new data entities. The data model documents the configuration entities and state that drive responsive behavior across breakpoints.

## Configuration Entities

### Breakpoint System

Defines responsive breakpoints for layout adaptations. These align with Bootstrap 5 standard breakpoints.

**Entity**: `Breakpoint`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `name` | string | Breakpoint identifier | Required. One of: xs, sm, md, lg, xl, xxl |
| `minWidth` | number | Minimum width in pixels | Required. Integer >= 0 |
| `maxWidth` | number \| null | Maximum width in pixels | Optional. Integer > minWidth or null for largest |
| `containerMaxWidth` | number \| null | Content container max-width | Optional. Integer <= maxWidth |

**Values** (Bootstrap 5 Standard):
```
xs:  0px - 575px   (container: fluid, no max-width)
sm:  576px - 767px (container: 540px)
md:  768px - 991px (container: 720px)
lg:  992px - 1199px (container: 960px)
xl:  1200px - 1399px (container: 1140px)
xxl: 1400px+ (container: 1320px)
```

**Relationships**: Referenced by typography scale, navigation configuration, and layout rules

---

### Typography Scale

Defines the modular typographic hierarchy using 1.200 ratio (Minor Third).

**Entity**: `TypographyScale`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `level` | string | Heading level | Required. One of: h1, h2, h3, h4, h5, h6, body |
| `baseFontSize` | number | Font size in pixels | Required. Integer >= 12 |
| `scaleRatio` | number | Multiplier for level progression | Required. 1.200 (fixed) |
| `lineHeight` | number | Line height ratio | Required. 1.5-1.6 for body, 1.2-1.4 for headings |
| `marginBottom` | string | Bottom spacing | Required. CSS value (rem or em) |
| `fontWeight` | number | Font weight | Required. 400-700 |

**Calculated Values** (16px base, 1.200 ratio):
```
body: 16px (base)
h6:   19.2px  (16 * 1.200)
h5:   23.04px  (19.2 * 1.200)
h4:   27.65px  (23.04 * 1.200)
h3:   33.18px  (27.65 * 1.200)
h2:   39.81px  (33.18 * 1.200)
h1:   47.78px  (39.81 * 1.200)
```

**Breakpoint Adjustments**:
- Mobile (< 768px): Base 16px
- Tablet (768-1199px): Base 17px
- Desktop (1200px+): Base 18px

**Relationships**: Applied to all text content, validated against WCAG size requirements

---

### Navigation Configuration

Defines mobile navigation behavior and layout.

**Entity**: `NavigationConfig`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `pattern` | string | Navigation pattern type | Required. "bottom-sticky" for mobile |
| `position` | string | CSS position value | Required. "fixed" for mobile, "static" for desktop |
| `itemCount` | number | Number of primary nav items | Required. 4-5 items for bottom bar |
| `minTouchTarget` | number | Minimum touch target size (px) | Required. 44 (WCAG 2.1 AA) |
| `spacing` | number | Spacing between items (px) | Required. >= 8 |
| `zIndex` | number | Stacking order | Required. 1030 (below modals, above content) |

**Navigation Items**:
```
1. Home (bi-house icon)
2. Articles (bi-file-text icon)
3. Projects (bi-briefcase icon)
4. About (bi-person icon)
5. Contact (bi-envelope icon)
```

**Breakpoint Behavior**:
- Mobile (< 768px): Fixed bottom bar, icon + label
- Tablet (768-1199px): Static header navigation, horizontal layout
- Desktop (1200px+): Static header navigation with hover states

**Relationships**: References touch target standards, integrates with Bootstrap navigation components

---

### Touch Target Standards

Defines minimum interactive element sizes for touch devices.

**Entity**: `TouchTarget`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `minHeight` | number | Minimum height in pixels | Required. 44 (WCAG 2.1 AA) |
| `minWidth` | number | Minimum width in pixels | Required. 44 (WCAG 2.1 AA) |
| `minSpacing` | number | Minimum space between targets (px) | Required. >= 8 |
| `appliesTo` | string[] | Element types | Required. ['button', 'a', 'input', etc.] |
| `breakpointExceptions` | object | Desktop exceptions | Optional. {desktop: {minHeight: 'auto'}} |

**WCAG 2.1 Reference**: Success Criterion 2.5.5 Target Size (Level AAA is 44x44px, we're using this as baseline)

**Relationships**: Applied to all interactive elements on mobile/tablet breakpoints

---

### Content Hierarchy Configuration

Defines above-the-fold content ordering for mobile homepage.

**Entity**: `ContentBlock`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `id` | string | Block identifier | Required. Unique string |
| `type` | string | Content type | Required. One of: identity, featured, cta, list |
| `order` | number | Mobile display order | Required. Integer >= 1 |
| `maxHeight` | number | Maximum height in viewport units | Optional. Integer 10-50 |
| `breakpointLayout` | object | Layout per breakpoint | Required. {xs, sm, md, lg, xl} |

**Mobile Homepage Blocks** (Target: ~600px viewport height):
```
1. Identity Block (order: 1)
   - Name/title
   - Brief description
   - Profile image
   - Height: ~200px

2. Featured Article Block (order: 2)
   - Article card
   - Image thumbnail
   - Title + excerpt
   - Height: ~250px

3. Primary CTA Block (order: 3)
   - Call-to-action button
   - Secondary link
   - Height: ~150px
```

**Relationships**: Uses flexbox `order` property for visual reordering while maintaining semantic HTML

---

### Image Loading Strategy

Defines lazy loading behavior for images.

**Entity**: `ImageLoadConfig`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `src` | string | Image source URL | Required. Valid URL or path |
| `loading` | string | Loading strategy | Required. "eager" or "lazy" |
| `position` | string | Page position | Required. "above-fold" or "below-fold" |
| `alt` | string | Alternative text | Required. Non-empty string |
| `responsive` | boolean | Use responsive images | Required. true |

**Loading Rules**:
- Above-fold (hero, featured): `loading="eager"`
- Below-fold (article body, grid items): `loading="lazy"`
- All images: `responsive=true` (Bootstrap `.img-fluid` class)

**Relationships**: Integrates with PUG image mixins and Bootstrap responsive utilities

---

### Line Length Configuration

Defines optimal character count per line for readability.

**Entity**: `LineLength`

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `breakpoint` | string | Breakpoint name | Required. One of: xs, sm, md, lg, xl |
| `minChars` | number | Minimum characters | Required. Integer 30-50 |
| `maxChars` | number | Maximum characters | Required. Integer 50-80 |
| `maxWidth` | string | CSS max-width | Required. `{maxChars}ch` unit |
| `padding` | string | Horizontal padding | Required. CSS value (rem) |

**Breakpoint Values**:
```
Mobile (xs, sm): 35-60 characters, max-width: 60ch, padding: 1.25rem
Tablet (md, lg): 45-75 characters, max-width: 75ch, padding: 2rem
Desktop (xl, xxl): 45-75 characters, max-width: 75ch, padding: 3rem
```

**Relationships**: Applied to article content containers, validated through readability testing

---

## State Transitions

### Viewport Resize Behavior

The responsive system transitions between states based on viewport width changes.

**States**:
1. **Mobile State** (< 768px)
   - Single column layout
   - Bottom sticky navigation
   - 35-60 character line length
   - Touch-optimized spacing

2. **Tablet State** (768-1199px)
   - Multi-column layouts (2-3 columns)
   - Static header navigation
   - 45-75 character line length
   - Touch-optimized spacing

3. **Desktop State** (1200px+)
   - Multi-column layouts (3-4 columns)
   - Static header navigation with hover
   - 45-75 character line length (max-width constrained)
   - Mouse-optimized spacing

**Transitions**:
- Smooth CSS transitions (0.3s ease-in-out) for layout changes
- No JavaScript required for responsive behavior
- Progressive enhancement approach

**Validation Rules**:
- Viewport meta tag must be properly configured
- No horizontal scrolling at any width
- Touch targets maintain minimum size through tablet breakpoint
- Content remains readable at all zoom levels up to 200%

---

## Validation & Constraints

### WCAG 2.1 Level AA Requirements

| Requirement | Success Criterion | Implementation |
|-------------|-------------------|----------------|
| Touch target size | 2.5.5 (AAA) | Min 44x44px on mobile/tablet |
| Text contrast | 1.4.3 | 4.5:1 for body, 3:1 for large text |
| Text resize | 1.4.4 | Support 200% zoom without horizontal scroll |
| Orientation | 1.3.4 | Support portrait and landscape |

### Performance Constraints

| Metric | Target | Validation Method |
|--------|--------|-------------------|
| First Contentful Paint | < 1.5s on 4G | Lighthouse CI |
| Largest Contentful Paint | < 2.5s | Lighthouse CI |
| Cumulative Layout Shift | < 0.1 | Lighthouse CI |
| Touch target failures | 0 | pa11y-ci |

### Cross-Browser Support

Minimum browser versions:
- Chrome/Edge: Last 2 versions
- Firefox: Last 2 versions
- Safari: iOS 12+, macOS last 2 versions
- Samsung Internet: Last 2 versions

---

## Summary

This data model defines the configuration entities and behavioral rules for mobile-first responsive implementation. All entities work within existing content structure (articles.json, projects.json) without requiring new data storage or API endpoints. Validation occurs through automated testing (Lighthouse CI, pa11y-ci) and real device testing across defined breakpoints.
