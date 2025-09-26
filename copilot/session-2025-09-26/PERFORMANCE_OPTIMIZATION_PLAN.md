# Performance Optimization Plan - Mark Hazleton Blog

*Generated: September 26, 2025*

## Current Performance Issues (From Lighthouse Audit)

### 🚨 Critical Issues

1. **Render-blocking CSS**: 48.6 KiB, 340ms potential savings
2. **Image delivery**: 15,686 KiB potential savings
3. **Font display optimization**: Not specified but impacting LCP
4. **Cache lifetimes**: 11 KiB savings available

## Analysis of Current Implementation

### Current CSS Setup

- **modern-styles.css**: 315.24 KB (render-blocking)
- **styles.css**: 311.49 KB (potentially unused)
- Both files are loaded synchronously, blocking initial render

### Current Layout Structure

- Bootstrap 5.3.8 + Bootstrap Icons + Font Awesome + PrismJS all bundled
- Google Analytics loaded synchronously
- Large CSS bundle blocking critical rendering path

## Optimization Strategy

### Phase 1: Critical CSS Implementation ⚡

#### 1.1 Extract Critical CSS

Create critical CSS containing only above-the-fold styles:

- Navigation styles
- Hero section styles  
- Basic typography and layout
- Essential Bootstrap utilities

#### 1.2 Implement CSS Loading Strategy

```html
<!-- Critical CSS inline -->
<style>
  /* Critical above-the-fold styles */
</style>

<!-- Non-critical CSS deferred -->
<link rel="preload" href="/css/modern-styles.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
<noscript><link rel="stylesheet" href="/css/modern-styles.css"></noscript>
```

### Phase 2: CSS Bundle Optimization 📦

#### 2.1 Split CSS Bundles

- **critical.css** (~10-15KB): Above-the-fold styles
- **main.css** (~200KB): Core Bootstrap + site styles  
- **components.css** (~50KB): Interactive components
- **icons.css** (~60KB): Bootstrap Icons + Font Awesome

#### 2.2 Conditional Loading

```javascript
// Load component CSS only when needed
if (document.querySelector('.accordion')) {
  loadCSS('/css/components.css');
}
```

### Phase 3: Image Optimization 🖼️

#### 3.1 Implement Modern Image Formats

```html
<picture>
  <source srcset="/assets/img/hero.avif" type="image/avif">
  <source srcset="/assets/img/hero.webp" type="image/webp">
  <img src="/assets/img/hero.jpg" alt="Mark Hazleton" loading="lazy">
</picture>
```

#### 3.2 Responsive Images

```html
<img srcset="/assets/img/hero-400.webp 400w,
             /assets/img/hero-800.webp 800w,
             /assets/img/hero-1200.webp 1200w"
     sizes="(max-width: 768px) 100vw, 50vw"
     src="/assets/img/hero-800.webp"
     alt="Mark Hazleton">
```

#### 3.3 Lazy Loading Strategy

- Implement intersection observer for images below the fold
- Use `loading="lazy"` attribute
- Provide low-quality image placeholders (LQIP)

### Phase 4: Font Optimization 📝

#### 4.1 Font Display Strategy

```css
@font-face {
  font-family: 'CustomFont';
  src: url('/fonts/font.woff2') format('woff2');
  font-display: swap; /* Prevent invisible text during font load */
}
```

#### 4.2 Font Preloading

```html
<link rel="preload" href="/fonts/main.woff2" as="font" type="font/woff2" crossorigin>
```

### Phase 5: Caching Strategy 🗄️

#### 5.1 HTTP Headers Implementation

```
# Static assets
Cache-Control: public, max-age=31536000, immutable

# HTML files  
Cache-Control: public, max-age=3600, stale-while-revalidate=86400

# CSS/JS with versioning
Cache-Control: public, max-age=31536000, immutable
```

#### 5.2 Service Worker Implementation

- Cache critical resources
- Implement stale-while-revalidate strategy
- Background sync for updates

## Implementation Roadmap

### Week 1: Critical CSS Foundation

- [ ] Extract critical CSS (navigation, hero, typography)
- [ ] Implement inline critical CSS in layout
- [ ] Add preload hints for non-critical CSS
- [ ] Test render-blocking improvements

### Week 2: CSS Bundle Optimization  

- [ ] Split CSS into logical bundles
- [ ] Implement conditional loading
- [ ] Remove unused CSS (potentially eliminate styles.css)
- [ ] Add CSS minification improvements

### Week 3: Image Optimization

- [ ] Convert images to WebP/AVIF formats
- [ ] Implement responsive image srcsets
- [ ] Add lazy loading with intersection observer
- [ ] Create image optimization build step

### Week 4: Font & Caching

- [ ] Optimize font loading with font-display: swap
- [ ] Add font preloading for critical fonts  
- [ ] Implement HTTP caching headers
- [ ] Add service worker for advanced caching

## Expected Performance Gains

### Lighthouse Score Improvements

- **First Contentful Paint (FCP)**: -340ms (CSS optimization)
- **Largest Contentful Paint (LCP)**: -500-800ms (image + font optimization)
- **Total Blocking Time (TBT)**: -200ms (deferred loading)
- **Cumulative Layout Shift (CLS)**: Maintained or improved

### File Size Reductions

- **Critical path**: ~48KB → ~15KB (69% reduction)
- **Total payload**: Potential 15MB reduction with image optimization
- **Cache efficiency**: 11KB+ additional savings

## Technical Implementation Details

### Build Process Changes

1. Add critical CSS extraction to build pipeline
2. Implement CSS splitting and minification
3. Add image optimization build step
4. Create service worker generation

### PUG Template Updates

1. Inline critical CSS in `<head>`
2. Add preload hints for non-critical resources
3. Implement conditional CSS loading
4. Add service worker registration

### SCSS Architecture Changes

1. Reorganize for critical/non-critical split
2. Create component-specific partials
3. Implement utility-first critical styles
4. Optimize Bootstrap customization

## Monitoring & Validation

### Performance Metrics Tracking

- Lighthouse CI integration for ongoing monitoring
- Core Web Vitals tracking in Google Analytics
- Real User Monitoring (RUM) implementation
- Performance budget enforcement

### Success Criteria

- **FCP**: < 1.5s (currently ~2.0s)
- **LCP**: < 2.5s (currently ~3.0s+)
- **FID**: < 100ms (maintain)
- **CLS**: < 0.1 (maintain)
- **Overall Lighthouse Performance**: > 95 (currently ~80-85)

## Risk Mitigation

### Potential Issues

1. **Flash of Unstyled Content (FOUC)**: Mitigated by comprehensive critical CSS
2. **JavaScript dependencies**: Ensure polyfills for older browsers
3. **Complex image processing**: Implement fallbacks for unsupported formats
4. **Build complexity**: Maintain clear documentation and error handling

### Rollback Strategy

- Keep current implementation available
- Feature flags for gradual rollout
- A/B testing capability
- Performance monitoring alerts

## Next Steps

1. **Immediate (Today)**: Begin critical CSS extraction
2. **Week 1**: Implement basic render-blocking CSS fix
3. **Week 2**: Full CSS optimization deployment
4. **Week 3**: Image optimization rollout
5. **Week 4**: Complete performance optimization package

This optimization plan should address all identified Lighthouse issues and significantly improve your Core Web Vitals scores, leading to better SEO rankings and user experience.
