# Mark Hazleton Resume

[Mark Hazleton Resume](https://markhazleton.com/) is the Mark Hazleton online resume/CV based on the theme for [Bootstrap](https://getbootstrap.com/) created by [Start Bootstrap](https://startbootstrap.com/) with the following template [Start Bootstrap - Source Template](https://startbootstrap.com/theme/resume/)

[![Azure Static Web Apps CI/CD](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml/badge.svg)](https://github.com/markhazleton/markhazleton-blog/actions/workflows/azure-static-web-apps-white-stone-0f5cd1910.yml)

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

## About

Start Bootstrap is based on the [Bootstrap](https://getbootstrap.com/) framework created by [Mark Otto](https://twitter.com/mdo) and [Jacob Thorton](https://twitter.com/fat).

## Copyright and License

Copyright 2013-2025 d.b.a. Control Origins. Code released under the [MIT](https://github.com/markhazleton/markhazleton-blog/blob/main/LICENSE) license.
