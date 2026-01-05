# Research: Mobile-First Website Optimization

**Feature**: 001-mobile-first  
**Date**: January 5, 2026  
**Status**: Phase 0 Complete

## Research Questions & Findings

### R1: Mobile-First SCSS Architecture

**Decision**: Implement mobile-first media queries using `min-width` approach with Bootstrap 5 breakpoint variables

**Rationale**: 
- Mobile-first methodology writes base styles for smallest screens, then uses `min-width` media queries to enhance for larger screens
- Bootstrap 5 provides standard breakpoint variables: `$grid-breakpoints: (xs: 0, sm: 576px, md: 768px, lg: 992px, xl: 1200px, xxl: 1400px)`
- This approach reduces CSS specificity conflicts and ensures mobile performance is optimized
- Aligns with existing Bootstrap 5 framework already in use

**Alternatives Considered**:
- Desktop-first with `max-width` queries - Rejected: Requires overriding desktop styles for mobile, increases CSS size and complexity
- Custom breakpoint system - Rejected: Unnecessary when Bootstrap 5 breakpoints already established and working

**Implementation Pattern**:
```scss
// Base styles (mobile-first, applies to all)
.element {
  font-size: 16px;
  padding: 1rem;
}

// Tablet enhancement
@media (min-width: 768px) {
  .element {
    padding: 1.5rem;
  }
}

// Desktop enhancement
@media (min-width: 1200px) {
  .element {
    padding: 2rem;
  }
}
```

---

### R2: Typography Scale Implementation

**Decision**: Use CSS custom properties (CSS variables) for modular typography scale with 1.200 ratio

**Rationale**:
- CSS custom properties allow responsive typography adjustments at breakpoints without duplicating rules
- 1.200 ratio (Minor Third) provides 16px base → 19.2px → 23.04px → 27.65px → 33.18px → 39.81px
- Can adjust base font size at different breakpoints while maintaining ratio consistency
- Modern browser support (all evergreen browsers + IE11 with PostCSS fallbacks if needed)

**Alternatives Considered**:
- SCSS variables only - Rejected: Cannot adjust dynamically at runtime, requires duplicating rules for each breakpoint
- Fixed px values - Rejected: Less flexible for responsive adjustments and user zoom preferences
- `clamp()` for fluid typography - Considered for future enhancement, but starting with stepped approach for predictability

**Implementation Pattern**:
```scss
:root {
  --font-size-base: 16px;
  --scale-ratio: 1.200;
  --font-size-h6: calc(var(--font-size-base) * var(--scale-ratio));
  --font-size-h5: calc(var(--font-size-h6) * var(--scale-ratio));
  --font-size-h4: calc(var(--font-size-h5) * var(--scale-ratio));
  --font-size-h3: calc(var(--font-size-h4) * var(--scale-ratio));
  --font-size-h2: calc(var(--font-size-h3) * var(--scale-ratio));
  --font-size-h1: calc(var(--font-size-h2) * var(--scale-ratio));
}

@media (min-width: 768px) {
  :root {
    --font-size-base: 17px; // Slightly larger on tablet
  }
}

@media (min-width: 1200px) {
  :root {
    --font-size-base: 18px; // Slightly larger on desktop
  }
}
```

---

### R3: Bottom Sticky Navigation Pattern

**Decision**: Implement bottom sticky navigation using `position: fixed` with `bottom: 0` and appropriate z-index management

**Rationale**:
- Bottom positioning keeps navigation within thumb reach on mobile devices (optimal UX for one-handed use)
- Fixed positioning ensures navigation remains accessible while scrolling through long articles
- Z-index management prevents conflicts with modals, dropdowns, or other overlays
- Safe area insets (`env(safe-area-inset-bottom)`) handle notched devices like iPhone X+

**Alternatives Considered**:
- CSS `position: sticky` - Rejected: Requires parent container configuration and doesn't work reliably across all scroll contexts
- JavaScript scroll detection - Rejected: Adds unnecessary complexity and potential performance issues
- Top sticky navigation - Rejected: Clarification session determined bottom navigation for better mobile UX

**Implementation Pattern**:
```scss
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1030; // Below Bootstrap modals (1055) but above content
  padding-bottom: env(safe-area-inset-bottom); // iPhone X+ notch handling
  background: #fff;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
  
  @media (min-width: 768px) {
    position: static; // Remove fixed positioning on tablet+
    box-shadow: none;
  }
}

.bottom-nav__item {
  min-height: 48px; // Touch target size
  min-width: 48px;
  padding: 0.5rem;
}
```

---

### R4: Image Lazy Loading Strategy

**Decision**: Use native browser `loading="lazy"` attribute for below-the-fold images

**Rationale**:
- Native lazy loading supported in all modern browsers (Chrome 77+, Firefox 75+, Safari 15.4+, Edge 79+)
- No JavaScript required, reducing complexity and improving reliability
- Browser handles intersection observer logic automatically with optimal performance
- Fallback to eager loading in unsupported browsers is acceptable (progressive enhancement)
- Works seamlessly with existing PUG template system

**Alternatives Considered**:
- IntersectionObserver JavaScript - Rejected: Unnecessary complexity when native support is excellent
- Third-party lazy loading libraries (lazysizes, lozad) - Rejected: Additional dependencies, native solution is simpler
- Blur-up placeholders - Considered for future enhancement, but adds complexity to image pipeline

**Implementation Pattern**:
```pug
// Hero/above-fold image (eager load)
img.img-fluid(
  src="/assets/images/hero.jpg"
  alt="Hero image description"
  loading="eager"
)

// Below-fold image (lazy load)
img.img-fluid(
  src="/assets/images/article-image.jpg"
  alt="Article image description"
  loading="lazy"
)
```

---

### R5: Line Length Control (Characters Per Line)

**Decision**: Use `max-width` with `ch` units combined with responsive padding to control line length

**Rationale**:
- `ch` unit represents the width of the "0" character in the current font, providing semantic measure for character count
- Mobile: `max-width: 60ch` achieves ~35-60 character range with appropriate padding
- Tablet/Desktop: `max-width: 75ch` achieves ~45-75 character range
- Combined with fluid padding ensures content doesn't touch screen edges
- Works naturally with Bootstrap's container system

**Alternatives Considered**:
- Fixed pixel widths - Rejected: Doesn't adapt to different font sizes or zoom levels
- Viewport units (vw) - Rejected: Doesn't account for font size changes, harder to predict character count
- JavaScript measurement - Rejected: Unnecessary complexity, CSS solution is more performant

**Implementation Pattern**:
```scss
.article-content {
  max-width: 60ch; // Mobile: ~35-60 characters
  margin-left: auto;
  margin-right: auto;
  padding-left: 1.25rem;
  padding-right: 1.25rem;
  
  @media (min-width: 768px) {
    max-width: 75ch; // Tablet+: ~45-75 characters
    padding-left: 2rem;
    padding-right: 2rem;
  }
  
  @media (min-width: 1200px) {
    padding-left: 3rem;
    padding-right: 3rem;
  }
}
```

---

### R6: Touch Target Size Implementation

**Decision**: Use Bootstrap utility classes with custom SCSS mixins for consistent 44x44px minimum touch targets

**Rationale**:
- WCAG 2.1 Level AA requires 44x44px minimum touch target size (Success Criterion 2.5.5)
- Bootstrap 5 buttons and form controls meet this by default with proper padding
- Custom mixin ensures consistency across custom interactive elements
- Adequate spacing (8px minimum) between touch targets prevents accidental taps

**Alternatives Considered**:
- Manually setting padding on each element - Rejected: Error-prone and inconsistent
- JavaScript measurement - Rejected: Unnecessary, CSS solution is cleaner and more performant
- Larger touch targets (48x48px) - Considered but 44x44px meets standard and provides better content density

**Implementation Pattern**:
```scss
@mixin touch-target {
  min-height: 44px;
  min-width: 44px;
  padding: 0.5rem;
  margin: 0.25rem; // 4px spacing
  
  @media (min-width: 1200px) {
    min-height: auto; // Desktop can use smaller targets with hover
    min-width: auto;
  }
}

.custom-button {
  @include touch-target;
}

// Bootstrap overrides if needed
.btn {
  @media (max-width: 767px) {
    min-height: 44px;
    min-width: 44px;
  }
}
```

---

### R7: Content Hierarchy Above the Fold

**Decision**: Use Flexbox column layout with `order` property for optimal mobile content stacking

**Rationale**:
- Flexbox provides reliable vertical stacking on mobile with easy reordering
- `order` property allows visual reordering without changing HTML structure (preserves semantic order for accessibility)
- Identity section first (who you are), featured article second (credibility), CTA third (action)
- Approximately 600px viewport height on mobile accommodates all three elements above fold

**Alternatives Considered**:
- CSS Grid - Rejected: Overkill for single-column mobile layout, Flexbox is simpler
- Multiple HTML orders with display toggling - Rejected: Creates duplicate content, bad for SEO and maintenance
- JavaScript viewport detection - Rejected: Not needed, CSS handles responsively

**Implementation Pattern**:
```scss
.homepage-hero {
  display: flex;
  flex-direction: column;
  
  &__identity {
    order: 1;
    margin-bottom: 1.5rem;
  }
  
  &__featured {
    order: 2;
    margin-bottom: 1.5rem;
  }
  
  &__cta {
    order: 3;
    margin-bottom: 2rem;
  }
  
  @media (min-width: 768px) {
    flex-direction: row;
    flex-wrap: wrap;
    
    &__identity {
      flex: 0 0 100%;
      order: 1;
    }
    
    &__featured {
      flex: 0 0 60%;
      order: 2;
    }
    
    &__cta {
      flex: 0 0 40%;
      order: 3;
    }
  }
}
```

---

### R8: Viewport Meta Tag Configuration

**Decision**: Use standard mobile-optimized viewport meta tag with `width=device-width, initial-scale=1`

**Rationale**:
- `width=device-width` ensures layout viewport matches device screen width
- `initial-scale=1` sets default zoom level to 1:1 (no zoom)
- Allows user zoom (no `user-scalable=no`) per WCAG requirements
- Standard configuration works across all mobile browsers
- Already in modern-layout.pug, verify it's correct

**Alternatives Considered**:
- `maximum-scale=1, user-scalable=no` - Rejected: Violates WCAG 2.1 Success Criterion 1.4.4 (Resize text)
- `viewport-fit=cover` - Considered for iPhone X+ notch handling but handle via CSS safe-area-insets instead

**Implementation Pattern**:
```pug
meta(name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no')
```

---

## Research Summary

All technical decisions support mobile-first responsive implementation within existing PUG/Bootstrap 5/SCSS architecture:

- **Architecture**: Mobile-first SCSS with `min-width` media queries using Bootstrap breakpoints
- **Typography**: CSS custom properties for 1.200 modular scale, responsive base size adjustments
- **Navigation**: Bottom fixed navigation for mobile with static positioning on larger screens
- **Performance**: Native lazy loading for below-fold images, no additional dependencies
- **Readability**: `ch` units for line length control, proper touch targets, optimal content hierarchy
- **Accessibility**: WCAG 2.1 AA compliance throughout (touch targets, contrast, zoom support)

Ready to proceed to Phase 1: Design & Contracts.
