# Mark Hazleton Resume

[Mark Hazleton Resume](https://markhazleton.com/) is the Mark Hazleton online resume/CV based on the theme for [Bootstrap](https://getbootstrap.com/) created by [Start Bootstrap](https://startbootstrap.com/) with the following template [Start Bootstrap - Source Template](https://startbootstrap.com/theme/resume/)

[![Azure Static Web Apps CI/CD](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml)

## Site Build Process

This website is built using a customized static site generation system that transforms source files (Pug, SCSS, JavaScript) into a complete static website. Here's a detailed explanation of how the build process works:

### Project Structure

- **src/**: Contains all source files
  - **pug/**: Pug templates that compile to HTML
  - **scss/**: SCSS files that compile to CSS
  - **js/**: JavaScript source files
  - **assets/**: Static assets (images, fonts, etc.)
- **scripts/**: Node.js scripts that handle the build process
- **docs/**: Output directory containing the generated static site

### Build System Components

The build system consists of several specialized scripts that handle different aspects of the build process:

#### Core Build Scripts

1. **build-assets.js**: Copies static files from `src/assets/` to `docs/assets/`
   - Handles images, fonts, and other static resources
   - Preserves directory structure

2. **build-pug.js**: Compiles Pug templates to HTML
   - Processes templates in `src/pug/`
   - Supports layouts, includes, and mixins
   - Outputs HTML files to the `docs/` directory

3. **build-scss.js**: Compiles SCSS to CSS
   - Processes SCSS files in `src/scss/`
   - Includes Bootstrap components
   - Handles variables, mixins, and functions
   - Outputs compiled CSS to `docs/css/`

4. **build-scripts.js**: Processes JavaScript files
   - Handles `src/js/scripts.js` and other JS files
   - Outputs processed JS to `docs/js/`

#### Renderer Scripts

Each build script has a corresponding renderer script that handles the actual transformation:

- **render-assets.js**: File copying logic for static assets
- **render-pug.js**: Template compilation logic
- **render-scss.js**: SCSS compilation with source maps
- **render-scripts.js**: JavaScript processing

#### Utility Scripts

- **clean.js**: Removes the `docs/` directory for clean builds
- **sb-watch.js**: Watches for file changes and triggers appropriate renderers
  - Uses chokidar for file watching
  - Detects file types and routes to appropriate renderer
  - Handles partial rebuilds for efficiency
- **start.js**: Main development script
  - Runs the watcher and browser-sync concurrently
  - Configures SSL for local development
  - Auto-refreshes the browser when files change

### Development Workflow

1. Source files are edited in the `src/` directory
2. The file watcher (`sb-watch.js`) detects changes
3. Appropriate renderer scripts process the changed files
4. Output is generated in the `docs/` directory
5. Browser-sync refreshes the browser to show changes

### Build Process Flow

When running a full build:

1. `clean.js` removes existing output files
2. `build-assets.js` copies static assets
3. `build-pug.js` compiles Pug templates to HTML
4. `build-scss.js` compiles SCSS to CSS
5. `build-scripts.js` processes JavaScript files

For incremental builds during development:

1. File watcher detects changes to specific files
2. Only affected files are reprocessed
3. For template includes or layouts, all dependent files are rebuilt

### RSS and Sitemap Generation

The build process automatically generates RSS feed and XML sitemap files to improve site visibility and syndication capabilities:

#### RSS Feed Generation

The `update-rss.js` script generates a standards-compliant RSS 2.0 feed:

- Reads article data from `articles.json`
- Sorts articles by publication date (newest first)
- Generates an RSS feed with the 20 most recent articles
- Includes essential RSS elements like title, link, description, and publication date
- Supports optional elements like category and content:encoded
- Follows RSS 2.0 specifications with proper XML namespaces
- Updates `rss.xml` in the source directory

The RSS feed helps readers subscribe to site updates and stay informed about new content through RSS readers.

#### XML Sitemap Generation

The `update-sitemap.js` script creates a comprehensive XML sitemap for search engines:

- Automatically includes all articles from `articles.json`
- Adds the homepage with highest priority (1.0)
- Dynamically assigns priority values based on content age:
  - Recent articles (< 30 days): 0.8
  - Moderately recent articles (30-90 days): 0.6
  - Older articles: 0.5
- Sets change frequency based on content age (daily, weekly, monthly)
- Formats dates according to the ISO 8601 standard required by sitemap protocol
- Follows the sitemap.org protocol with proper schema references
- Updates `sitemap.xml` in the source directory

The sitemap helps search engines discover and index all pages on the site, potentially improving SEO performance.

Both RSS and sitemap files are automatically updated as part of the regular build process, ensuring they always contain the latest content information.

## Usage

### npm Scripts

-   `npm run build` builds the project - this builds assets, HTML, JS, and CSS into `docs`
-   `npm run build:assets` copies the files in the `src/assets/` directory into `docs`
-   `npm run build:pug` compiles the Pug located in the `src/pug/` directory into `docs`
-   `npm run build:scripts` brings the `src/js/scripts.js` file into `docs`
-   `npm run build:scss` compiles the SCSS files located in the `src/scss/` directory into `docs`
-   `npm run clean` deletes the `docs` directory to prepare for rebuilding the project
-   `npm run start:debug` runs the project in debug mode
-   `npm start` or `npm run start` runs the project, launches a live preview in your default browser, and watches for changes made to files in `src`

### Simplified Development Scripts

Two simplified scripts are available for local development if you encounter issues with the standard npm scripts:

-   `node simple-serve.js` - Uses browser-sync to serve the site with automatic reloading
-   `node http-server.js` - Uses http-server for a lightweight development server

These scripts provide a simpler alternative to the standard build process and can be helpful for troubleshooting.

### SCSS Modernization

This project currently uses Sass `@import` syntax which is being deprecated in Dart Sass 3.0.0. A future update will need to migrate to the new `@use` and `@forward` module system. Current workarounds include:

1. Using the quietDeps option in `render-scss.js` to suppress deprecation warnings
2. Hardcoding some color values and variables in SCSS files to avoid module system issues
3. Keeping the existing `@import` syntax for Bootstrap compatibility

#### Migration Plan

1. Update custom functions to use modern Sass modules
2. Gradually migrate component files to use namespaced variables
3. Reorganize variable dependencies for proper module compatibility
4. Fully migrate when Bootstrap updates for Sass 3.0 compatibility

You must have npm installed in order to use this build environment.

## Deployment

The site is automatically deployed to Azure Static Web Apps using GitHub Actions. The workflow is configured in `.github/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml`.

The deployment process:
1. Triggers on push to the main branch
2. Uses the pre-built static files in the `docs/` directory
3. Deploys them to Azure Static Web Apps
4. No additional build steps are required during deployment since the site is pre-built

## About

Start Bootstrap is based on the [Bootstrap](https://getbootstrap.com/) framework created by [Mark Otto](https://twitter.com/mdo) and [Jacob Thorton](https://twitter.com/fat).

## Copyright and License

Copyright 2013-2025 d.b.a. Control Origins. Code released under the [MIT](https://github.com/markhazleton/markhazleton-blog/blob/main/LICENSE) license.
