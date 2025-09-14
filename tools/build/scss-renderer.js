'use strict';

/**
 * Unified SCSS Renderer
 * Handles both main styles and modern styles compilation
 */

const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');
const fs = require('fs');
const upath = require('upath');
const postcss = require('postcss');
const sass = require('sass');
const sh = require('shelljs');

class SCSSRenderer {
    constructor() {
        this.baseOptions = {
            loadPaths: [
                upath.resolve(upath.dirname(__filename), '../../node_modules')
            ],
            quietDeps: true,
            logger: {
                warn: function(message, options) {
                    // Only show warnings that don't contain specific deprecation messages
                    if (!message.includes('deprecated') && !message.includes('Deprecation')) {
                        console.warn(message);
                    }
                }
            }
        };

        this.postCSSPlugins = [
            autoprefixer,
            cssnano({
                preset: 'default'
            })
        ];
    }

    /**
     * Compile a single SCSS file
     */
    async compileFile(inputPath, outputPath, description = '') {
        console.log(`Building ${description || path.basename(outputPath)}...`);

        try {
            // Compile SCSS
            const result = sass.compile(inputPath, this.baseOptions);

            // Ensure output directory exists
            const destPathDirname = upath.dirname(outputPath);
            if (!sh.test('-e', destPathDirname)) {
                sh.mkdir('-p', destPathDirname);
            }

            // Process with PostCSS
            const processed = await postcss(this.postCSSPlugins)
                .process(result.css, { from: undefined });

            // Handle warnings
            processed.warnings().forEach(warn => {
                console.warn(warn.toString());
            });

            // Write output
            fs.writeFileSync(outputPath, processed.css);
            console.log(`✅ ${description || 'SCSS'} compiled successfully to ${outputPath}`);

        } catch (error) {
            console.error(`❌ Error compiling ${description || 'SCSS'}:`, error.message);
            throw error;
        }
    }

    /**
     * Render main styles
     */
    async renderMainStyles() {
        const inputPath = upath.resolve(upath.dirname(__filename), '../../src/scss/styles.scss');
        const outputPath = upath.resolve(upath.dirname(__filename), '../../docs/css/styles.css');

        await this.compileFile(inputPath, outputPath, 'main styles');
    }

    /**
     * Render modern styles
     */
    async renderModernStyles() {
        const inputPath = upath.resolve(upath.dirname(__filename), '../../src/scss/modern-styles.scss');
        const outputPath = upath.resolve(upath.dirname(__filename), '../../docs/css/modern-styles.css');

        await this.compileFile(inputPath, outputPath, 'modern styles');
    }

    /**
     * Render all SCSS files
     */
    async renderAll() {
        await this.renderMainStyles();
        await this.renderModernStyles();
    }
}

// For backward compatibility, export both individual functions and the class
const renderer = new SCSSRenderer();

module.exports = renderer.renderAll.bind(renderer);
module.exports.renderSCSS = renderer.renderMainStyles.bind(renderer);
module.exports.renderModernSCSS = renderer.renderModernStyles.bind(renderer);
module.exports.SCSSRenderer = SCSSRenderer;
