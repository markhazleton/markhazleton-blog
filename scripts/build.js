#!/usr/bin/env node

/**
 * Unified Build Script for Mark Hazleton Blog
 * Consolidates all build functionality into a single script
 * Usage: node scripts/build.js [--pug] [--scss] [--scripts] [--assets] [--sitemap] [--rss]
 */

const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Import all render functions
const renderPug = require('./render-pug');
const { renderSCSS, renderModernSCSS } = require('./scss-renderer');
const renderScripts = require('./render-scripts');
const renderAssets = require('./render-assets');

// Import utility functions
const updateRSS = require('./update-rss');
const updateSitemap = require('./update-sitemap');

const sh = require('shelljs');
const upath = require('upath');

class BlogBuilder {
    constructor() {
        this.srcPath = upath.resolve(upath.dirname(__filename), '../src');
        this.tasks = {
            pug: this.buildPug.bind(this),
            scss: this.buildSCSS.bind(this),
            scripts: this.buildScripts.bind(this),
            assets: this.buildAssets.bind(this),
            sitemap: this.buildSitemap.bind(this),
            rss: this.buildRSS.bind(this)
        };
    }

    /**
     * Build PUG templates
     */
    async buildPug() {
        console.log('🎨 Building PUG templates...');
        const startTime = Date.now();

        // Find all PUG files that should be rendered
        const pugFiles = sh.find(this.srcPath)
            .filter(filePath =>
                filePath.match(/\.pug$/) &&
                !filePath.match(/include/) &&
                !filePath.match(/mixin/) &&
                !filePath.match(/\/pug\/layouts\//) &&
                !filePath.match(/\/pug\/modules\//)
            );

        // Process each file
        for (const filePath of pugFiles) {
            await renderPug(filePath);
        }

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ PUG templates built (${pugFiles.length} files, ${duration}s)`);
    }

    /**
     * Build SCSS stylesheets
     */
    async buildSCSS() {
        console.log('🎨 Building SCSS stylesheets...');
        const startTime = Date.now();

        // Build main styles
        renderSCSS();

        // Build modern styles
        renderModernSCSS();

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ SCSS stylesheets built (${duration}s)`);
    }

    /**
     * Build JavaScript files
     */
    async buildScripts() {
        console.log('📜 Building JavaScript files...');
        const startTime = Date.now();

        await renderScripts();

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ JavaScript files built (${duration}s)`);
    }

    /**
     * Build assets
     */
    async buildAssets() {
        console.log('📦 Building assets...');
        const startTime = Date.now();

        renderAssets();

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ Assets built (${duration}s)`);
    }

    /**
     * Build sitemap
     */
    async buildSitemap() {
        console.log('🗺️ Building sitemap...');
        const startTime = Date.now();

        updateSitemap();

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ Sitemap built (${duration}s)`);
    }

    /**
     * Build RSS feed
     */
    async buildRSS() {
        console.log('📡 Building RSS feed...');
        const startTime = Date.now();

        updateRSS();

        const duration = ((Date.now() - startTime) / 1000).toFixed(2);
        console.log(`✅ RSS feed built (${duration}s)`);
    }

    /**
     * Run specific build tasks
     */
    async run(taskNames = []) {
        const totalStartTime = Date.now();

        console.log('🚀 Mark Hazleton Blog - Build Process');
        console.log('=====================================');

        // If no specific tasks, run all
        if (taskNames.length === 0) {
            taskNames = Object.keys(this.tasks);
        }

        // Validate task names
        const invalidTasks = taskNames.filter(task => !this.tasks[task]);
        if (invalidTasks.length > 0) {
            console.error(`❌ Invalid tasks: ${invalidTasks.join(', ')}`);
            console.error(`Available tasks: ${Object.keys(this.tasks).join(', ')}`);
            process.exit(1);
        }

        // Run tasks sequentially
        for (const taskName of taskNames) {
            try {
                await this.tasks[taskName]();
            } catch (error) {
                console.error(`❌ Error in ${taskName}:`, error.message);
                process.exit(1);
            }
        }

        const totalDuration = ((Date.now() - totalStartTime) / 1000).toFixed(2);
        console.log('');
        console.log(`🎉 Build completed successfully in ${totalDuration}s`);
        console.log(`📁 Output: docs/`);
    }
}

// CLI handling
if (require.main === module) {
    const args = process.argv.slice(2);
    const taskNames = [];

    // Check for help
    if (args.includes('--help') || args.includes('-h')) {
        console.log('🚀 Mark Hazleton Blog - Build System');
        console.log('===================================');
        console.log('');
        console.log('Usage: node scripts/build.js [options]');
        console.log('');
        console.log('Options:');
        console.log('  --pug       Build PUG templates');
        console.log('  --scss      Build SCSS stylesheets');
        console.log('  --scripts   Build JavaScript files');
        console.log('  --assets    Build assets');
        console.log('  --sitemap   Build sitemap');
        console.log('  --rss       Build RSS feed');
        console.log('  --help, -h  Show this help message');
        console.log('');
        console.log('Examples:');
        console.log('  node scripts/build.js           # Build everything');
        console.log('  node scripts/build.js --pug     # Build only PUG templates');
        console.log('  node scripts/build.js --scss --scripts  # Build SCSS and scripts');
        console.log('');
        process.exit(0);
    }

    // Parse arguments
    for (const arg of args) {
        if (arg.startsWith('--')) {
            const task = arg.substring(2);
            taskNames.push(task);
        }
    }

    const builder = new BlogBuilder();
    builder.run(taskNames).catch(error => {
        console.error('❌ Build failed:', error);
        process.exit(1);
    });
}

module.exports = BlogBuilder;
