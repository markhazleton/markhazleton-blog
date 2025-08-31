# Mark Hazleton Blog

[Mark Hazleton Blog](https://markhazleton.com/) is Mark Hazleton's professional blog and portfolio site featuring articles on project management, web development, and technology solutions. Built with a modern static site generation system using PUG templates, Bootstrap 5, and a unified Node.js build system.

[![Azure Static Web Apps CI/CD](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml)
 [![Monthly Maintenance](https://github.com/markhazleton/markhazleton-blog/actions/workflows/monthly-maintenance.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/monthly-maintenance.yml)
 [![Nightly Quick Checks](https://github.com/markhazleton/markhazleton-blog/actions/workflows/nightly-quickchecks.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/nightly-quickchecks.yml)

## Quick Links

- **[SEO Guidelines](SEO.md)** - Complete SEO validation system and optimization rules
- **[Article Authoring Guide](Authoring.md)** - Step-by-step content creation guide

## Technology Stack

- **Template Engine**: PUG 3.0.3 for semantic HTML generation
- **CSS Framework**: Bootstrap 5.3.8 with custom SCSS
- **Build System**: Unified Node.js build script with modular renderers
- **Development Server**: BrowserSync 3.0.4 for live development
- **Content Management**: JSON-based article system with RSS/Sitemap generation
- **Deployment**: Azure Static Web Apps with GitHub Actions CI/CD

## Project Structure

```text
markhazleton-blog/
├── src/                          # Source files
│   ├── pug/                      # PUG templates
│   │   ├── layouts/              # Base layouts
│   │   ├── modules/              # Reusable components and mixins
│   │   ├── articles/             # Article-specific templates
│   │   ├── projectmechanics/     # Project mechanics section
│   │   └── *.pug                 # Page templates
│   ├── scss/                     # SCSS stylesheets
│   │   ├── styles.scss           # Main stylesheet
│   │   ├── modern-styles.scss    # Modern CSS features
│   │   ├── components/           # Component styles
│   │   ├── sections/             # Section-specific styles
│   │   └── variables/            # SCSS variables
│   ├── js/                       # JavaScript source files
│   ├── assets/                   # Static assets (images, fonts)
│   ├── articles.json             # Article metadata
│   ├── projects.json             # Project data
│   ├── sections.json             # Content sections data
│   ├── rss.xml                   # Generated RSS feed
│   └── sitemap.xml               # Generated XML sitemap
├── scripts/                      # Build system scripts
│   ├── build.js                  # Unified build system
│   ├── render-*.js               # Component renderers
│   ├── scss-renderer.js          # SCSS compilation
│   ├── start.js                  # Development server
│   ├── update-rss.js             # RSS feed generator
│   ├── update-sitemap.js         # Sitemap generator
│   ├── update-sections.js        # Sections data processor
│   └── seo-*.js                  # SEO utilities
├── docs/                         # Generated static site
├── tools/                        # Maintenance and audit tools
├── WebAdmin/                     # AI-powered content management
│   └── mwhWebAdmin/              # .NET Core web admin application
└── .github/workflows/            # CI/CD configuration
```

## Build System Architecture

The build system uses a unified approach with a single `build.js` script that handles all compilation tasks through modular renderers.

### Unified Build Script (`build.js`)

The main build script provides a single entry point for all build operations:

- **Modular Design**: Each build task (PUG, SCSS, JavaScript, etc.) is handled by specialized renderer modules
- **Selective Building**: Support for building specific components via command-line flags
- **Progress Reporting**: Detailed timing and status information for each build step
- **Error Handling**: Comprehensive error reporting and graceful failure handling

### Core Renderers

1. **render-pug.js**: PUG template compilation
   - Processes all PUG files in `src/pug/` (excluding layouts and modules)
   - Supports template inheritance with `extends` and `block` patterns
   - Generates semantic HTML5 output in `docs/`

2. **scss-renderer.js**: SCSS compilation system
   - Compiles `src/scss/styles.scss` to `docs/css/styles.css` (main styles)
   - Compiles `src/scss/modern-styles.scss` to `docs/css/modern-styles.css` (modern features)
   - Integrates Bootstrap 5.3.8 and custom styles
   - Uses Dart Sass 1.91.0 with deprecation warning suppression
   - PostCSS processing with autoprefixer and cssnano minification

3. **render-scripts.js**: JavaScript processing
   - Processes JavaScript files from `src/js/`
   - Outputs optimized JS to `docs/js/`
   - Handles bundling and minification

4. **render-assets.js**: Static asset management
   - Copies images, fonts, and other resources from `src/assets/` to `docs/assets/`
   - Copies JSON configuration files (staticwebapp.config.json, sections.json, etc.)
   - Preserves directory structure and file permissions
   - Handles favicons, manifests, and configuration files

### Content Generation

- **update-sections.js**: Processes sections.json with article data
  - Updates content sections with article counts and categorization
  - Provides data for navigation and content organization

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

### Development Tools

- **start.js**: Development server
  - Starts BrowserSync server on port 3000
  - Serves files from the `docs/` directory
  - Watches for file changes and auto-refreshes browser
  - Requires running `npm run build` first

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

1. Ensures the project is built (requires `npm run build` to be run first)
2. Starts BrowserSync on port 3000
3. Serves files from the `docs/` directory
4. Watches for file changes and auto-refreshes browser

### Build Process Flow

#### Full Build (`npm run build`)

1. `build:sections` - Updates sections with article data
2. `build:pug` - Compiles all PUG templates to HTML
3. `build:scss` - Compiles both main and modern SCSS to CSS
4. `build:scripts` - Processes JavaScript files
5. `build:assets` - Copies static assets and configuration files
6. `build:sitemap` - Creates XML sitemap
7. `build:rss` - Generates RSS feed

#### Individual Build Steps

You can build specific components using the `--flag` approach:

```bash
# Build specific components
node scripts/build.js --pug          # Only PUG templates
node scripts/build.js --scss         # Only SCSS compilation
node scripts/build.js --assets       # Only static assets
node scripts/build.js --pug --scss   # Multiple components
```

## Content Management

### Article System

Articles are managed through a JSON-based system:

- **articles.json**: Central article metadata registry with 101+ articles
- Individual article PUG templates in `src/pug/articles/` and root `src/pug/`
- Automatic RSS and sitemap generation from article data
- Support for categories, tags, publication dates, and descriptions
- Section-based organization (Development, Case Studies, Project Management, AI & Machine Learning, etc.)

### Project Portfolio

Projects are managed through:

- **projects.json**: Project metadata and descriptions
- Integration with portfolio display templates
- Support for technology tags and project links

### Sections System

Content is organized into thematic sections via `sections.json`:

- **Dynamic Content**: Automatically updated with article counts
- **Categories**: Development (23), Case Studies (17), Project Management (14), AI & Machine Learning (24), and more
- **Integration**: Used for navigation and content organization

## NPM Scripts Reference

### Production Build

- `npm run build` - Complete production build sequence
  - Runs all build steps: sections → pug → scss → scripts → assets → sitemap → rss

### Targeted Build Commands

- `npm run build:sections` - Updates sections.json with article data
- `npm run build:pug` - Compiles PUG templates to HTML files
- `npm run build:scss` - Compiles SCSS stylesheets (both main and modern)
- `npm run build:scripts` - Processes JavaScript files from `src/js/`
- `npm run build:assets` - Copies static files and configuration
- `npm run build:sitemap` - Creates XML sitemap for SEO
- `npm run build:rss` - Generates RSS 2.0 feed from articles.json

### Development Scripts

- `npm start` - Builds project and starts BrowserSync development server
  - Serves from `docs/` directory on port 3000
  - Auto-refreshes browser on file changes
  - Requires manual rebuild when source files change

### Audit and Maintenance Scripts

- `npm run seo:audit` - Runs SEO validation reports
- `npm run audit:perf` - Lighthouse CI performance audits
- `npm run audit:seo` - SEO and accessibility checks
- `npm run audit:a11y` - Accessibility audits using pa11y-ci
- `npm run audit:ssl` - SSL certificate expiry checks
- `npm run audit:all` - Comprehensive audit suite
- `npm run report:monthly` - Generates monthly maintenance reports

### Build Management

- `npm run clean` - Removes `docs/` directory for clean builds
- `npm run update-rss` - Manually updates RSS feed
- `npm run update-sitemap` - Manually updates XML sitemap

## Deployment

The site uses automated deployment to Azure Static Web Apps through GitHub Actions.

### Deployment Configuration

- **Workflow**: `.github/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml`
- **Trigger**: Push to `main` branch, pull requests, or manual workflow dispatch
- **Build Environment**: Ubuntu latest with Node.js 20
- **Dependencies**: Full development dependencies installed via `npm ci`

### Deployment Process

1. GitHub Actions triggers on push to main branch or PR
2. Sets up Node.js 20 environment with npm caching
3. Installs all dependencies including development tools
4. Runs `npm run build` to generate production files in `docs/`
5. Deploys `docs/` directory to Azure Static Web Apps
6. Triggers IndexNow API for search engine indexing
7. Provides failure notifications via GitHub comments

### Static Web App Configuration

- **Source Directory**: `docs/` (pre-built static files)
- **Configuration**: `staticwebapp.config.json` for routing and headers
- **Custom Domain**: markhazleton.com configured through Azure
- **SSL**: Automatically provided by Azure Static Web Apps
- **IndexNow**: Automatic search engine notification on successful deployment

## Site Maintenance

Automated monitoring and maintenance run via GitHub Actions:

- **Monthly Maintenance** (1st @ 09:00 UTC): Comprehensive audits including Lighthouse performance, link checking, accessibility (a11y), SEO validation, SSL certificate monitoring, and report generation to `reports/YYYY-MM.md`
- **Nightly Quick Checks** (daily @ 09:00 UTC): Focused checks including Lighthouse homepage audit, sample link validation, and SSL certificate status

### Local Audit Commands

Run the full audit suite locally:

```bash
npm ci
npm run audit:all
```

Individual audit components:

```bash
npm run audit:perf    # Lighthouse performance audit
npm run audit:seo     # SEO and accessibility checks  
npm run audit:ssl     # SSL certificate expiry check
npm run audit:a11y    # Accessibility audit with pa11y-ci
```

### Maintenance Artifacts

- **Performance Reports**: Lighthouse CI results
- **Accessibility Reports**: Generated in `artifacts/a11y.json`
- **Link Check Results**: Comprehensive site link validation
- **SSL Monitoring**: Certificate expiry tracking
- **Monthly Reports**: Consolidated reports in `reports/` directory

## Dependencies

### Core Runtime Dependencies

- **cheerio 1.1.2**: HTML/XML parsing and manipulation
- **fs-extra 11.3.1**: Enhanced file system operations with promises
- **glob 11.0.3**: File pattern matching and discovery
- **prismjs 1.30.0**: Syntax highlighting for code blocks
- **terser 5.43.1**: JavaScript minification and optimization

### Development Dependencies

#### Frameworks and UI

- **Bootstrap 5.3.8**: CSS framework and responsive components
- **Bootstrap Icons 1.13.1**: Comprehensive icon library
- **bootswatch 5.3.7**: Bootstrap theme collection

#### Build System

- **Pug 3.0.3**: Clean, whitespace-sensitive template engine
- **Sass 1.91.0**: SCSS compilation with modern features
- **PostCSS 8.5.6**: CSS post-processing pipeline
- **autoprefixer 10.4.21**: Automatic vendor prefix management
- **cssnano 7.1.1**: CSS optimization and minification

#### Development Server Tools

- **BrowserSync 3.0.4**: Development server with live reload
- **chokidar 4.0.3**: Cross-platform file watching (not currently used)
- **concurrently 9.2.1**: Multi-process task runner

#### Quality Assurance

- **@lhci/cli 0.15.1**: Lighthouse CI for performance auditing
- **pa11y-ci 4.0.1**: Accessibility testing automation
- **prettier 3.6.2**: Code formatting and style consistency

#### Utilities

- **dayjs 1.11.18**: Lightweight date manipulation
- **commander 14.0.0**: Command-line interface framework
- **npm-check-updates 18.0.3**: Dependency update management
- **tsx 4.20.5**: TypeScript execution engine
- **typescript 5.9.2**: TypeScript compiler

## SCSS Modernization Status

### Current State

The project currently uses legacy Sass `@import` syntax, which is being deprecated in Dart Sass 3.0.0.

### Deprecation Handling

- `quietDeps: true` option suppresses dependency warnings from external libraries
- Custom logger filters deprecation messages during development
- Hardcoded Bootstrap variables in some files to avoid module system issues

### Future Migration Plan

1. Update custom SCSS functions to use modern Sass modules (`@use`/`@forward`)
2. Migrate component files to use namespaced variables
3. Reorganize variable dependencies for module compatibility
4. Complete migration when Bootstrap updates for Sass 3.0 compatibility

## Browser Support

- **Modern Browsers**: Chrome, Firefox, Safari, Edge (latest versions)
- **Mobile**: iOS Safari, Chrome Mobile, Samsung Internet
- **Progressive Enhancement**: Modern CSS features loaded via separate stylesheet
- **Development**: Local HTTPS support for testing
- **Production**: HTTPS via Azure Static Web Apps SSL

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

The site includes a comprehensive Web Admin application (`WebAdmin/mwhWebAdmin/`) with advanced AI-powered content generation capabilities:

- **AI Content Generation**: Single-click generation of all SEO metadata fields using OpenAI GPT-4
- **SEO Validation Dashboard**: Real-time validation with A-F grading system for content quality
- **Interactive Forms**: Live character counting and validation feedback for optimal content
- **Site Integration**: Direct integration with npm build system for seamless content updates

#### AI Content Generation Features

When clicking "Generate AI Content", the system automatically creates:

- **Core Fields**: Keywords, description, summary optimized for search engines
- **SEO Metadata**: Title tags, meta descriptions, canonical URLs
- **Social Media**: Open Graph and Twitter Card metadata for social sharing
- **Conclusion Sections**: Title, summary, key takeaways, and final thoughts

#### Technical Implementation

- **Structured Output**: Uses OpenAI's structured output API for consistent field generation
- **Real-time Validation**: Immediate feedback on character limits and SEO requirements
- **Visual Feedback**: Field highlighting and loading states for enhanced user experience
- **Comprehensive Logging**: Full debugging support with structured logging using ILogger

### Recent Improvements

#### Cleanup Activities

Extensive cleanup of development files to maintain a lean, maintainable codebase:

- **Removed 35+ obsolete files**: Legacy SSL scripts, migration utilities, and documentation fragments
- **Consolidated Documentation**: All SEO rules now centralized in single `SEO.md` file
- **Streamlined Scripts**: Removed redundant server files and certificate utilities
- **Eliminated Debug Code**: Cleaned up console logging and simplified user interfaces

#### Console Logging Enhancement

- **Structured Logging**: Migrated from `Console.WriteLine` to proper `ILogger` instances
- **Debugging Support**: Comprehensive logging for AI content generation flow and error tracking
- **Performance Improvement**: More efficient logging with structured parameters and context

#### UI/UX Improvements

- **Simplified Interfaces**: Reduced confusing multiple buttons to single, clear action points
- **Enhanced Visual Feedback**: 8-second field highlighting with smooth CSS transitions
- **Better Error Handling**: User-friendly error messages with detailed server feedback
- **Loading States**: Professional loading indicators with timeout handling for better UX
