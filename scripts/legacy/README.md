# Legacy Build Scripts

This folder contains build scripts that have been consolidated into the unified `build.js` script.

## Moved Scripts

### Individual Build Scripts (Replaced by `build.js`)

- **`build-assets.js`** - Simple wrapper that called `render-assets.js`
- **`build-modern-scss.js`** - Simple wrapper that called `render-modern-scss.js`
- **`build-pug.js`** - Simple wrapper that called `render-pug.js`
- **`build-scripts.js`** - Simple wrapper that called `render-scripts.js`
- **`build-scss.js`** - Simple wrapper that called `render-scss.js`

### SCSS Renderers (Replaced by `scss-renderer.js`)

- **`render-scss.js`** - Main SCSS compiler (consolidated)
- **`render-modern-scss.js`** - Modern SCSS compiler (consolidated)

## Why These Were Consolidated

1. **Eliminated Redundancy**: Each `build-*.js` was just a 3-line wrapper calling the corresponding `render-*.js`
2. **Unified Interface**: Single entry point for all build tasks
3. **Better Performance**: Reduced process spawning overhead
4. **Easier Maintenance**: One script to maintain instead of 10+
5. **Consistent Logging**: Unified progress reporting and error handling

## New Structure

### Unified Build System

```bash
# Build everything
npm run build

# Build specific components
npm run build:pug
npm run build:scss
npm run build:scripts
npm run build:assets
npm run build:sitemap
npm run build:rss
```

### Direct Script Usage

```bash
# Build everything
node scripts/build.js

# Build specific tasks
node scripts/build.js --pug --scss
node scripts/build.js --assets --sitemap
```

## Current Active Scripts

The following scripts remain active in the main `/scripts` folder:

- **`build.js`** - Unified build system
- **`clean.js`** - Clean output directory
- **`start.js`** - Development server
- **`scss-renderer.js`** - Consolidated SCSS compilation
- **`render-pug.js`** - PUG template rendering
- **`render-scripts.js`** - JavaScript bundling
- **`render-assets.js`** - Asset copying
- **`update-rss.js`** - RSS feed generation
- **`update-sitemap.js`** - Sitemap generation
- **`seo-helper.js`** - SEO utilities
- **`seo-validation-report.js`** - SEO validation

---
*Consolidated on: July 13, 2025*
*Reason: Reduce complexity and eliminate redundant wrapper scripts*
