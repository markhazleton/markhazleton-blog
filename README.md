# Mark Hazleton Blog

[Mark Hazleton Blog](https://markhazleton.com/) is Mark Hazleton's professional blog and portfolio site featuring articles on project management, web development, and technology solutions. Built with a modern static site generation system using PUG templates, Bootstrap 5, and custom build tools.

[![Azure Static Web Apps CI/CD](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml)
 [![Monthly Maintenance](https://github.com/markhazleton/markhazleton-blog/actions/workflows/monthly-maintenance.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/monthly-maintenance.yml)
 [![Nightly Quick Checks](https://github.com/markhazleton/markhazleton-blog/actions/workflows/nightly-quickchecks.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/nightly-quickchecks.yml)

## Quick Links

- **[SEO Guidelines](SEO.md)** - Complete SEO validation system and optimization rules
- **[Article Authoring Guide](Authoring.md)** - Step-by-step content creation guide

## Technology Stack

- **Template Engine**: PUG (formerly Jade) for semantic HTML generation
- **CSS Framework**: Bootstrap 5 with custom SCSS
- **Build System**: Custom Node.js scripts with file watching
- **Development Server**: BrowserSync with SSL support
- **Content Management**: JSON-based article system with RSS/Sitemap generation
- **Deployment**: Azure Static Web Apps with GitHub Actions

## Project Structure

```
markhazleton-blog/
├── src/                          # Source files
│   ├── pug/                      # PUG templates
│   │   ├── layouts/              # Base layouts
│   │   ├── includes/             # Reusable components
│   │   ├── mixins/               # PUG mixins
│   │   └── pages/                # Page templates
│   ├── scss/                     # SCSS stylesheets
│   │   ├── styles.scss           # Main stylesheet
│   │   └── modern-styles.scss    # Modern CSS features
│   ├── js/                       # JavaScript source files
│   ├── assets/                   # Static assets (images, fonts)
│   ├── articles.json             # Article metadata
│   ├── projects.json             # Project data
│   ├── rss.xml                   # Generated RSS feed
│   └── sitemap.xml               # Generated XML sitemap
├── scripts/                      # Build system scripts
│   ├── build-*.js                # Core build scripts
│   ├── render-*.js               # Processing renderers
│   ├── sb-watch.js               # File watcher
│   ├── start.js                  # Development server
│   ├── update-rss.js             # RSS feed generator
│   ├── update-sitemap.js         # Sitemap generator
│   └── generate-article-json.js  # Article processing
├── docs/                         # Generated static site
└── .github/workflows/            # CI/CD configuration
```

## Build System Architecture

The build system is a sophisticated static site generator with the following components:

### Core Build Scripts

1. **build-assets.js**: Asset pipeline for static files
   - Copies images, fonts, and other resources from `src/assets/` to `docs/assets/`
   - Preserves directory structure and file permissions
   - Handles favicons, manifests, and configuration files

2. **build-pug.js**: PUG template compilation
   - Processes all PUG files in `src/pug/` (excluding includes, mixins, and layouts)
   - Supports template inheritance with `extends` and `block` patterns
   - Generates semantic HTML5 output in `docs/`

3. **build-scss.js**: Primary SCSS compilation
   - Compiles `src/scss/styles.scss` to `docs/css/styles.css`
   - Integrates Bootstrap 5 and custom styles
   - Uses Dart Sass with deprecation warning suppression

4. **build-modern-scss.js**: Modern CSS features
   - Compiles `src/scss/modern-styles.scss` to `docs/css/modern-styles.css`
   - Handles advanced CSS features and browser compatibility

5. **build-scripts.js**: JavaScript processing
   - Processes JavaScript files from `src/js/`
   - Outputs optimized JS to `docs/js/`

### Renderer Scripts

Each build script uses a corresponding renderer for actual file processing:

- **render-assets.js**: File system operations for static asset copying
- **render-pug.js**: PUG template compilation with layout inheritance
- **render-scss.js**: SCSS compilation with PostCSS processing (autoprefixer, cssnano)
- **render-modern-scss.js**: Modern SCSS compilation with advanced features
- **render-scripts.js**: JavaScript bundling and optimization

### Development Tools

- **sb-watch.js**: Intelligent file watcher using Chokidar
  - Monitors `src/` directory for changes
  - Routes file changes to appropriate renderers
  - Handles partial rebuilds for includes/mixins (rebuilds dependent files)
  - Supports hot reloading during development

- **start.js**: Development server orchestration
  - Runs file watcher and BrowserSync concurrently using `concurrently`
  - Configures SSL certificates for local HTTPS development
  - Cross-platform support (Windows/macOS/Linux)
  - Auto-refreshes browser on file changes with debouncing (2000ms)

- **start-debug.js**: Enhanced development mode with additional logging

### Content Generation Scripts

- **generate-article-json.js**: Article metadata processor
  - Extracts metadata from PUG templates
  - Generates individual JSON files for articles
  - Creates combined article collections
  - Supports command-line options for flexible processing

- **update-rss.js**: RSS 2.0 feed generator
  - Reads from `articles.json` for article data
  - Generates standards-compliant RSS feed with up to 500 items
  - Includes proper XML escaping and RSS date formatting
  - Updates `src/rss.xml` automatically

- **update-sitemap.js**: XML sitemap generator
  - Creates search engine optimized sitemaps
  - Dynamic priority assignment based on content age
  - Proper ISO 8601 date formatting
  - Follows sitemap.org protocol specifications
  - Updates `src/sitemap.xml` automatically

### Utility Scripts

- **clean.js**: Build directory cleanup utility
  - Removes `docs/` directory for fresh builds
  - Prevents build artifacts from accumulating

## SCSS Compilation System

The project uses a dual SCSS compilation approach to handle both legacy and modern CSS features:

### Primary Stylesheet (`styles.scss`)

- Integrates Bootstrap 5 components and utilities
- Custom variables and theme customizations
- Compiled with Dart Sass using `@import` syntax (legacy)
- PostCSS processing with autoprefixer and cssnano minification

### Modern Stylesheet (`modern-styles.scss`)

- Advanced CSS features and experimental properties
- Future-compatible CSS syntax
- Separate compilation pipeline for progressive enhancement

### Sass Deprecation Handling

Both compilation processes include sophisticated deprecation warning management:

- `quietDeps: true` suppresses dependency warnings
- Custom logger filters out deprecation messages
- Maintains compatibility with Bootstrap 5 while preparing for Sass 3.0

## Development Workflow

### Initial Setup

```bash
npm install
npm run build
```

### Development Server

```bash
npm start
```

This command:

1. Builds all source files
2. Starts the file watcher (`sb-watch.js`)
3. Launches BrowserSync with SSL support
4. Opens the site in your default browser
5. Watches for changes and auto-refreshes

### File Change Detection

The watcher intelligently handles different file types:

- **PUG files**: Compiles changed files, or all files if includes/layouts change
- **SCSS files**: Recompiles all stylesheets
- **JS files**: Processes JavaScript files
- **Assets**: Copies changed static files

### Build Process Flow

#### Full Build (`npm run build`)

1. `clean` - Removes existing `docs/` directory
2. `build:pug` - Compiles all PUG templates to HTML
3. `build:scss` - Compiles primary SCSS to CSS
4. `build:modern-scss` - Compiles modern SCSS features
5. `build:scripts` - Processes JavaScript files
6. `build:assets` - Copies static assets
7. `update-rss` - Generates RSS feed
8. `update-sitemap` - Creates XML sitemap

#### Incremental Development Builds

- File watcher detects specific changes
- Only affected files are reprocessed
- Dependent files are rebuilt when includes/layouts change
- Browser refreshes automatically after processing

## Content Management

### Article System

Articles are managed through a JSON-based system:

- **articles.json**: Central article metadata registry
- Individual article PUG templates in `src/pug/articles/`
- Automatic RSS and sitemap generation from article data
- Support for categories, tags, publication dates, and descriptions

### Project Portfolio

Projects are managed through:

- **projects.json**: Project metadata and descriptions
- Integration with portfolio display templates
- Support for technology tags and project links

## NPM Scripts Reference

### Production Build

- `npm run build` - Complete production build sequence
  - Runs all build steps in order: clean → pug → scss → modern-scss → scripts → assets → rss → sitemap

### Individual Build Steps

- `npm run build:assets` - Copies static files from `src/assets/` to `docs/assets/`
- `npm run build:pug` - Compiles PUG templates to HTML files
- `npm run build:scss` - Compiles primary SCSS stylesheet (`styles.scss`)
- `npm run build:modern-scss` - Compiles modern CSS features (`modern-styles.scss`)
- `npm run build:scripts` - Processes JavaScript files from `src/js/`

### Development Scripts

- `npm start` or `npm run start` - Runs full development environment
  - Builds the project
  - Starts file watcher
  - Launches BrowserSync with SSL
  - Opens browser and watches for changes
- `npm run start:debug` - Development mode with enhanced logging

### Content Management

- `npm run update-rss` - Generates RSS 2.0 feed from articles.json
- `npm run update-sitemap` - Creates XML sitemap for SEO

### Utility Scripts

- `npm run clean` - Removes `docs/` directory for clean builds
- `npm run update-deps` - Updates all dependencies using npm-check-updates

### Alternative Development Servers

If you encounter issues with the main development setup, these simplified scripts are available:

- `node simple-serve.js` - Basic BrowserSync server with auto-reload
- `node http-server.js` - Lightweight HTTP server for testing

These provide simpler alternatives to the full development environment and can help with troubleshooting.

## Deployment

The site uses automated deployment to Azure Static Web Apps through GitHub Actions.

### Deployment Configuration

- **Workflow**: `.github/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml`
- **Trigger**: Push to `main` branch or pull requests
- **Build Environment**: Ubuntu latest with Node.js 20
- **Dependencies**: Full development dependencies installed via `npm ci`

### Deployment Process

1. GitHub Actions triggers on push to main branch
2. Sets up Node.js 20 environment with npm caching
3. Installs all dependencies including development tools
4. Runs `npm run build` to generate production files
5. Deploys `docs/` directory to Azure Static Web Apps
6. No additional build steps required during deployment (pre-built approach)

### Static Web App Configuration

- **Source Directory**: `docs/` (pre-built static files)
- **Configuration**: `staticwebapp.config.json` for routing and headers
- **Custom Domain**: Configured through Azure Static Web Apps
- **SSL**: Automatically provided by Azure

## Site Maintenance

Automated monitoring and maintenance run via GitHub Actions:

- Monthly Maintenance (1st @ 09:00 UTC): Lighthouse, links, a11y, SEO, SSL, and report.
- Nightly Quick Checks (daily @ 09:00 UTC): Lighthouse homepage, links (sample), SSL.

Run locally:

```bash
npm ci
npm run audit:all
```

Artifacts are uploaded per run, and monthly reports are written to `maintenance/reports/YYYY-MM.md`.

## Dependencies

### Core Dependencies

- **cheerio**: HTML/XML parsing and manipulation
- **fs-extra**: Enhanced file system operations
- **glob**: File pattern matching
- **prismjs**: Syntax highlighting for code blocks
- **terser**: JavaScript minification

### Development Dependencies

- **Bootstrap 5.3.6**: CSS framework and components
- **Bootstrap Icons 1.13.1**: Icon library
- **Pug 3.0.3**: Template engine
- **Sass 1.89.1**: SCSS compilation
- **BrowserSync 3.0.4**: Development server with live reload
- **Chokidar 4.0.3**: Cross-platform file watching
- **PostCSS/Autoprefixer/CSSnano**: CSS processing pipeline
- **Concurrently**: Multi-process task runner

## SCSS Modernization Status

### Current State

The project currently uses legacy Sass `@import` syntax, which is being deprecated in Dart Sass 3.0.0.

### Deprecation Handling

- `quietDeps: true` option suppresses dependency warnings
- Custom logger filters deprecation messages
- Hardcoded Bootstrap variables in some files to avoid module system issues

### Future Migration Plan

1. Update custom functions to use modern Sass modules (`@use`/`@forward`)
2. Migrate component files to namespaced variables
3. Reorganize variable dependencies for module compatibility
4. Complete migration when Bootstrap updates for Sass 3.0 compatibility

## Browser Support

- **Modern Browsers**: Chrome, Firefox, Safari, Edge (latest versions)
- **Mobile**: iOS Safari, Chrome Mobile, Samsung Internet
- **Progressive Enhancement**: Modern CSS features in separate stylesheet
- **SSL**: Local development and production both use HTTPS

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes in the `src/` directory
4. Test locally with `npm start`
5. Run `npm run build` to ensure clean production build
6. Submit a pull request

## License and Copyright

Copyright 2013-2025 Mark Hazleton. Code released under the [MIT](https://github.com/markhazleton/markhazleton-blog/blob/main/LICENSE) license.

Original Bootstrap theme by [Start Bootstrap](https://startbootstrap.com/), based on the [Bootstrap](https://getbootstrap.com/) framework created by [Mark Otto](https://twitter.com/mdo) and [Jacob Thorton](https://twitter.com/fat).

## Web Admin Features

### AI Content Generation

The site includes a comprehensive Web Admin application (`WebAdmin/mwhWebAdmin/`) with advanced AI-powered content generation:

- **AI Content Generation**: Single-click generation of all SEO metadata fields using OpenAI GPT-4
- **SEO Validation Dashboard**: Real-time validation with A-F grading system
- **Interactive Forms**: Live character counting and validation feedback
- **Site Refresh**: Direct integration with npm build system for content updates

#### AI Content Generation Features

When clicking "Generate AI Content", the system automatically creates:

- **Core Fields**: Keywords, description, summary
- **SEO Metadata**: Title, meta description, canonical URLs
- **Social Media**: Open Graph and Twitter Card metadata
- **Conclusion Sections**: Title, summary, key takeaways, final thoughts

#### Technical Implementation

- **Structured Output**: Uses OpenAI's structured output for consistent field generation
- **Real-time Validation**: Immediate feedback on character limits and SEO requirements
- **Visual Feedback**: Field highlighting and loading states for user experience
- **Comprehensive Logging**: Full debugging support with structured logging

### Recent Improvements

#### Cleanup Activities

Extensive cleanup of development files to maintain a lean codebase:

- **Removed 35+ obsolete files**: SSL scripts, migration utilities, documentation fragments
- **Consolidated Documentation**: All SEO rules now in single `SEO.md` file
- **Streamlined Scripts**: Removed redundant server files and certificate utilities
- **Eliminated Debug Code**: Cleaned up console logging and simplified interfaces

#### Console Logging Enhancement

- **Structured Logging**: Migrated from `Console.WriteLine` to proper `ILogger` instances
- **Debugging Support**: Comprehensive logging for AI content generation flow
- **Performance Improvement**: More efficient logging with structured parameters

#### UI/UX Improvements

- **Simplified Interfaces**: Reduced confusing multiple buttons to single, clear actions
- **Enhanced Visual Feedback**: 8-second field highlighting with smooth transitions
- **Better Error Handling**: User-friendly messages with detailed server feedback
- **Loading States**: Professional loading indicators with timeout handling
