#!/usr/bin/env node

/**
 * Unified Build Script for Mark Hazleton Blog
 * Enhanced with caching, performance tracking, and parallel execution
 * Usage: node scripts/build.js [--pug] [--scss] [--scripts] [--assets] [--sitemap] [--rss] [--projectsRss] [--no-cache] [--no-parallel]
 */

const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Import all render functions
const renderPug = require('./render-pug');
const { renderSCSS, renderModernSCSS } = require('./scss-renderer');
const renderScripts = require('./render-scripts');
const renderAssets = require('./render-assets');
const renderProjectPage = require('./render-project-pages');

// Import utility functions
const updateRSS = require('./update-rss');
const updateProjectsRSS = require('./update-projects-rss');
const updateSitemap = require('./update-sitemap');
const updateSectionsWithArticles = require('./update-sections');
const PlaceholderGenerator = require('./generate-placeholders');
const FontDownloader = require('./download-fonts');

// Import optimization utilities
const BuildCache = require('./cache-manager');
const PerformanceTracker = require('./performance-tracker');
const ErrorRecovery = require('./error-recovery');
const buildConfig = require('./build-config');

const sh = require('shelljs');
const upath = require('upath');

class BlogBuilder {
    constructor(options = {}) {
        this.config = buildConfig.getConfig();
        this.srcPath = upath.resolve(upath.dirname(__filename), '../../src');

        // Initialize optimization components
        this.cache = new BuildCache({
            cacheDir: this.config.cache.directory,
            enabled: this.config.cache.enabled && !options.noCache,
            ttl: this.config.cache.ttl
        });

        this.performance = new PerformanceTracker({
            enabled: this.config.performance.tracking,
            reportPath: this.config.performance.reportPath
        });

        this.errorRecovery = new ErrorRecovery({
            retryCount: this.config.errorHandling.retryCount,
            retryDelay: this.config.errorHandling.retryDelay,
            continueOnError: this.config.errorHandling.continueOnError
        });

        this.parallel = this.config.optimization.parallel && !options.noParallel;

        this.tasks = {
            fonts: this.buildFonts.bind(this),
            placeholders: this.buildPlaceholders.bind(this),
            sections: this.buildSections.bind(this),
            pug: this.buildPug.bind(this),
            projectPages: this.buildProjectPages.bind(this),
            scss: this.buildSCSS.bind(this),
            scripts: this.buildScripts.bind(this),
            assets: this.buildAssets.bind(this),
            sitemap: this.buildSitemap.bind(this),
            rss: this.buildRSS.bind(this),
            projectsRss: this.buildProjectsRSS.bind(this)
        };

        // Clean expired cache on startup if enabled
        if (this.config.cache.cleanOnStart) {
            const cleaned = this.cache.cleanExpired();
            if (cleaned > 0) {
                console.log(`🧹 Cleaned ${cleaned} expired cache entries`);
            }
        }
    }

    /**
     * Build local fonts
     */
    async buildFonts() {
        const taskName = 'fonts';
        this.performance.start(taskName);

        const cacheKey = 'fonts';
        const targetPath = 'docs/assets/fonts';

        if (!this.cache.shouldRebuild(cacheKey, targetPath)) {
            this.performance.markCached(taskName);
            return;
        }

        console.log('🔤 Building local fonts...');

        await this.errorRecovery.retryTask(taskName, async () => {
            const downloader = new FontDownloader();
            await downloader.downloadInterFont();
            this.cache.markBuilt(cacheKey, targetPath);
        });

        this.performance.end(taskName);
    }

    /**
     * Build placeholder images
     */
    async buildPlaceholders() {
        const taskName = 'placeholders';
        this.performance.start(taskName);

        const cacheKey = 'placeholders';
        const targetPath = 'docs/assets/img/placeholders';

        if (!this.cache.shouldRebuild(cacheKey, targetPath)) {
            this.performance.markCached(taskName);
            return;
        }

        console.log('📷 Building placeholder images...');

        await this.errorRecovery.retryTask(taskName, async () => {
            const generator = new PlaceholderGenerator();
            await generator.generatePlaceholders();
            this.cache.markBuilt(cacheKey, targetPath);
        });

        this.performance.end(taskName);
    }

    /**
     * Build sections with articles data
     */
    async buildSections() {
        const taskName = 'sections';
        this.performance.start(taskName);

        // Check cache
        const sectionsFile = upath.join(this.srcPath, 'sections.json');
        const articlesFile = upath.join(this.srcPath, 'articles.json');
        const targetFile = upath.join('docs', 'sections.json');

        if (!this.cache.shouldRebuild(sectionsFile, targetFile, [articlesFile])) {
            this.performance.markCached(taskName);
            return;
        }

        console.log('📚 Building sections with articles...');

        await this.errorRecovery.retryTask(taskName, async () => {
            updateSectionsWithArticles();
            this.cache.markBuilt(sectionsFile, targetFile, [articlesFile]);
        });

        this.performance.end(taskName);
    }

    /**
     * Build PUG templates with intelligent caching
     */
    async buildPug() {
        const taskName = 'pug';
        this.performance.start(taskName);

        console.log('🎨 Building PUG templates...');

        // Find all PUG files that should be rendered
        const pugFiles = sh.find(this.srcPath)
            .filter(filePath =>
                filePath.match(/\.pug$/) &&
                !filePath.match(/include/) &&
                !filePath.match(/mixin/) &&
                !filePath.match(/\/pug\/layouts\//) &&
                !filePath.match(/\/pug\/modules\//) &&
                !filePath.match(/\/pug\/projects\/templates\//)
            );

        let processedCount = 0;
        let cachedCount = 0;

        // Process each file with caching
        for (const filePath of pugFiles) {
            const relativePath = path.relative(this.srcPath, filePath);
            const targetFile = filePath.replace(/\.pug$/, '.html').replace(/src[\\\/]pug[\\\/]/, 'docs/');

            // Get dependencies (layouts, includes, data files)
            const dependencies = [
                ...sh.find(upath.join(this.srcPath, 'pug', 'layouts')).filter(f => f.endsWith('.pug')),
                ...sh.find(upath.join(this.srcPath, 'pug', 'modules')).filter(f => f.endsWith('.pug')),
                upath.join(this.srcPath, 'articles.json'),
                upath.join(this.srcPath, 'projects.json'),
                upath.join(this.srcPath, 'sections.json')
            ].filter(f => fs.existsSync(f));

            if (!this.cache.shouldRebuild(filePath, targetFile, dependencies)) {
                cachedCount++;
                continue;
            }

            await this.errorRecovery.retryTask(`pug:${relativePath}`, async () => {
                await renderPug(filePath);
                this.cache.markBuilt(filePath, targetFile, dependencies);
                processedCount++;
            });
        }

        if (cachedCount > 0) {
            console.log(`💾 ${cachedCount} PUG files used from cache`);
        }

        console.log(`✅ PUG templates built (${processedCount} processed, ${cachedCount} cached)`);
        this.performance.end(taskName);
    }

    /**
     * Build project detail pages from projects.json data
     */
    async buildProjectPages() {
        const taskName = 'projectPages';
        this.performance.start(taskName);

        const templatePath = upath.join(this.srcPath, 'pug', 'projects', 'templates', 'project-detail.pug');

        if (!fs.existsSync(templatePath)) {
            this.performance.markCached(taskName);
            console.warn('⚠️ Project detail template not found. Skipping project page generation.');
            return;
        }

        const projectsFile = upath.join(this.srcPath, 'projects.json');
        if (!fs.existsSync(projectsFile)) {
            this.performance.markCached(taskName);
            console.warn('⚠️ projects.json not found. Skipping project page generation.');
            return;
        }

        let projectsData;
        try {
            projectsData = JSON.parse(fs.readFileSync(projectsFile, 'utf8'));
        } catch (error) {
            this.performance.markCached(taskName);
            console.error('? Failed to read projects.json:', error.message);
            return;
        }

        if (!Array.isArray(projectsData) || projectsData.length === 0) {
            this.performance.markCached(taskName);
            console.warn('⚠️ No project entries found in projects.json.');
            return;
        }

        console.log('?? Building project detail pages...');

        const dependencies = [
            templatePath,
            ...sh.find(upath.join(this.srcPath, 'pug', 'layouts')).filter(f => f.endsWith('.pug')),
            ...sh.find(upath.join(this.srcPath, 'pug', 'modules')).filter(f => f.endsWith('.pug')),
            projectsFile
        ].filter(f => fs.existsSync(f));

        let processedCount = 0;
        let cachedCount = 0;

        for (const project of projectsData) {
            if (!project || !project.slug) {
                console.warn('⚠️ Skipping project entry without a slug.');
                continue;
            }

            const destPath = upath.join('docs', 'projects', project.slug, 'index.html');
            const currentSlug = `projects/${project.slug}/index.html`;

            if (!this.cache.shouldRebuild(templatePath, destPath, dependencies)) {
                cachedCount++;
                continue;
            }

            await this.errorRecovery.retryTask(`projectPages:${project.slug}`, async () => {
                await renderProjectPage({
                    templatePath,
                    project,
                    destPath,
                    currentSlug
                });
                this.cache.markBuilt(templatePath, destPath, dependencies);
                processedCount++;
            });
        }

        if (cachedCount > 0) {
            console.log(`?? ${cachedCount} project pages used from cache`);
        }

        console.log(`? Project detail pages built (${processedCount} processed, ${cachedCount} cached)`);

        this.performance.end(taskName);
    }

    /**
     * Build SCSS stylesheets with caching
     */
    async buildSCSS() {
        const taskName = 'scss';
        this.performance.start(taskName);

        console.log('🎨 Building SCSS stylesheets...');

        const mainScss = upath.join(this.srcPath, 'scss', 'styles.scss');
        const modernScss = upath.join(this.srcPath, 'scss', 'modern-styles.scss');
        const mainTarget = upath.join('docs', 'css', 'styles.css');
        const modernTarget = upath.join('docs', 'css', 'modern-styles.css');

        // Get SCSS dependencies
        const scssDependencies = sh.find(upath.join(this.srcPath, 'scss'))
            .filter(f => f.endsWith('.scss'));

        let rebuiltCount = 0;

        // Build main styles
        if (this.cache.shouldRebuild(mainScss, mainTarget, scssDependencies)) {
            await this.errorRecovery.retryTask('scss:main', async () => {
                renderSCSS();
                this.cache.markBuilt(mainScss, mainTarget, scssDependencies);
                rebuiltCount++;
            });
        } else {
            console.log('💾 Main styles (cached)');
        }

        // Build modern styles
        if (this.cache.shouldRebuild(modernScss, modernTarget, scssDependencies)) {
            await this.errorRecovery.retryTask('scss:modern', async () => {
                renderModernSCSS();
                this.cache.markBuilt(modernScss, modernTarget, scssDependencies);
                rebuiltCount++;
            });
        } else {
            console.log('💾 Modern styles (cached)');
        }

        if (rebuiltCount === 0) {
            this.performance.markCached(taskName);
        } else {
            console.log(`✅ SCSS stylesheets built (${rebuiltCount} files processed)`);
            this.performance.end(taskName);
        }
    }

    /**
     * Build JavaScript files with caching
     */
    async buildScripts() {
        const taskName = 'scripts';
        this.performance.start(taskName);

        console.log('📜 Building JavaScript files...');

        const jsFiles = sh.find(upath.join(this.srcPath, 'js'))
            .filter(f => f.endsWith('.js') && !f.includes('vendor'));

        if (jsFiles.length === 0) {
            this.performance.markCached(taskName);
            return;
        }

        // For now, check if any JS file needs rebuilding
        const targetDir = upath.join('docs', 'js');
        const needsRebuild = jsFiles.some(jsFile => {
            const targetFile = upath.join(targetDir, path.basename(jsFile));
            return this.cache.shouldRebuild(jsFile, targetFile);
        });

        if (!needsRebuild) {
            this.performance.markCached(taskName);
            console.log('💾 JavaScript files (cached)');
            return;
        }

        await this.errorRecovery.retryTask(taskName, async () => {
            await renderScripts();

            // Mark all JS files as built
            jsFiles.forEach(jsFile => {
                const targetFile = upath.join(targetDir, path.basename(jsFile));
                this.cache.markBuilt(jsFile, targetFile);
            });
        });

        this.performance.end(taskName);
    }

    /**
     * Build assets with caching
     */
    async buildAssets() {
        const taskName = 'assets';
        this.performance.start(taskName);

        console.log('📦 Building assets...');

        // Key asset files to check
        const assetFiles = [
            upath.join(this.srcPath, 'staticwebapp.config.json'),
            upath.join(this.srcPath, 'sections.json'),
            upath.join(this.srcPath, 'projects.json'),
            upath.join(this.srcPath, 'articles.json')
        ].filter(f => fs.existsSync(f));

        const targetDir = 'docs';
        const needsRebuild = assetFiles.some(assetFile => {
            const targetFile = upath.join(targetDir, path.basename(assetFile));
            return this.cache.shouldRebuild(assetFile, targetFile);
        });

        if (!needsRebuild) {
            this.performance.markCached(taskName);
            console.log('💾 Assets (cached)');
            return;
        }

        await this.errorRecovery.retryTask(taskName, async () => {
            renderAssets();

            // Mark assets as built
            assetFiles.forEach(assetFile => {
                const targetFile = upath.join(targetDir, path.basename(assetFile));
                this.cache.markBuilt(assetFile, targetFile);
            });
        });

        this.performance.end(taskName);
    }

    /**
     * Build sitemap with caching
     */
    async buildSitemap() {
        const taskName = 'sitemap';
        this.performance.start(taskName);

        console.log('🗺️ Building sitemap...');

        const sectionsFile = upath.join('docs', 'sections.json');
        const articlesFile = upath.join('docs', 'articles.json');
        const targetFile = upath.join('docs', 'sitemap.xml');

        if (!this.cache.shouldRebuild(sectionsFile, targetFile, [articlesFile])) {
            this.performance.markCached(taskName);
            console.log('💾 Sitemap (cached)');
            return;
        }

        await this.errorRecovery.retryTask(taskName, async () => {
            updateSitemap();
            this.cache.markBuilt(sectionsFile, targetFile, [articlesFile]);
        });

        this.performance.end(taskName);
    }

    /**
     * Build RSS feed with caching
     */
    async buildRSS() {
        const taskName = 'rss';
        this.performance.start(taskName);

        console.log('📡 Building RSS feed...');

        const articlesFile = upath.join('docs', 'articles.json');
        const targetFile = upath.join('docs', 'rss.xml');

        if (!this.cache.shouldRebuild(articlesFile, targetFile)) {
            this.performance.markCached(taskName);
            console.log('💾 RSS feed (cached)');
            return;
        }

        await this.errorRecovery.retryTask(taskName, async () => {
            updateRSS();
            this.cache.markBuilt(articlesFile, targetFile);
        });

        this.performance.end(taskName);
    }

    /**
     * Build Projects RSS feed with caching
     */
    async buildProjectsRSS() {
        const taskName = 'projectsRss';
        this.performance.start(taskName);

        console.log('📡 Building Projects RSS feed...');

        const projectsFile = upath.join('docs', 'projects.json');
        const targetFile = upath.join('docs', 'projects-rss.xml');

        if (!this.cache.shouldRebuild(projectsFile, targetFile)) {
            this.performance.markCached(taskName);
            console.log('💾 Projects RSS feed (cached)');
            return;
        }

        await this.errorRecovery.retryTask(taskName, async () => {
            updateProjectsRSS();
            this.cache.markBuilt(projectsFile, targetFile);
        });

        this.performance.end(taskName);
    }

    /**
     * Run specific build tasks with parallel execution support
     */
    async run(taskNames = []) {
        console.log('🚀 Mark Hazleton Blog - Enhanced Build Process');
        console.log('===============================================');

        // Show cache status
        const cacheStats = this.cache.getStats();
        if (cacheStats.enabled) {
            console.log(`💾 Cache: ${cacheStats.totalEntries} entries, ${cacheStats.cacheSize}`);
        }

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

        try {
            if (this.parallel && taskNames.length > 1) {
                await this.runParallel(taskNames);
            } else {
                await this.runSequential(taskNames);
            }

            // Show performance report
            this.performance.report();
            this.performance.saveToFile();

            // Show trends if available
            if (this.config.performance.showTrends) {
                this.performance.showTrends();
            }

            // Report any errors
            this.errorRecovery.reportErrors();

            console.log('🎉 Build completed successfully');
            console.log(`📁 Output: docs/`);

        } catch (error) {
            console.error(`❌ Build failed: ${error.message}`);
            this.errorRecovery.reportErrors();

            // Save error log
            const errorLogPath = upath.join(this.config.cache.directory, 'error.log');
            await this.errorRecovery.saveErrorLog(errorLogPath);

            process.exit(1);
        }
    }

    /**
     * Run tasks in parallel using dependency groups
     */
    async runParallel(taskNames) {
        const groups = this.config.parallelGroups;

        // Phase 1: Prerequisites (sequential)
        const phase1Tasks = taskNames.filter(t => groups.phase1.includes(t));
        if (phase1Tasks.length > 0) {
            console.log('🔄 Phase 1: Prerequisites');
            for (const taskName of phase1Tasks) {
                await this.tasks[taskName]();
            }
        }

        // Phase 2: Independent tasks (parallel)
        const phase2Tasks = taskNames.filter(t => groups.phase2.includes(t));
        if (phase2Tasks.length > 0) {
            console.log('🔄 Phase 2: Parallel execution');
            await Promise.all(phase2Tasks.map(taskName => this.tasks[taskName]()));
        }

        // Phase 3: Dependent tasks (parallel within group)
        const phase3Tasks = taskNames.filter(t => groups.phase3.includes(t));
        if (phase3Tasks.length > 0) {
            console.log('🔄 Phase 3: Final tasks');
            await Promise.all(phase3Tasks.map(taskName => this.tasks[taskName]()));
        }
    }

    /**
     * Run tasks sequentially
     */
    async runSequential(taskNames) {
        console.log('� Sequential execution');
        for (const taskName of taskNames) {
            await this.tasks[taskName]();
        }
    }
}

// CLI handling
if (require.main === module) {
    const args = process.argv.slice(2);
    const taskNames = [];
    const options = {
        noCache: false,
        noParallel: false
    };

    // Check for help
    if (args.includes('--help') || args.includes('-h')) {
        console.log('🚀 Mark Hazleton Blog - Enhanced Build System');
        console.log('=============================================');
        console.log('');
        console.log('Usage: node scripts/build.js [options] [tasks]');
        console.log('');
        console.log('Tasks:');
        console.log('  --sections  Build sections with articles');
        console.log('  --pug       Build PUG templates');
        console.log('  --scss      Build SCSS stylesheets');
        console.log('  --scripts   Build JavaScript files');
        console.log('  --assets    Build assets');
        console.log('  --sitemap   Build sitemap');
        console.log('  --rss       Build articles RSS feed');
        console.log('  --projectsRss Build projects RSS feed');
        console.log('  --projectPages Build project detail pages');
        console.log('');
        console.log('Options:');
        console.log('  --no-cache    Disable build caching');
        console.log('  --no-parallel Disable parallel execution');
        console.log('  --help, -h    Show this help message');
        console.log('');
        console.log('Examples:');
        console.log('  node scripts/build.js                    # Build everything');
        console.log('  node scripts/build.js --pug              # Build only PUG templates');
        console.log('  node scripts/build.js --sections --assets # Build sections and assets');
        console.log('  node scripts/build.js --no-cache         # Build without caching');
        console.log('  node scripts/build.js --no-parallel      # Build sequentially');
        console.log('');
        process.exit(0);
    }

    // Parse arguments
    for (const arg of args) {
        if (arg === '--no-cache') {
            options.noCache = true;
        } else if (arg === '--no-parallel') {
            options.noParallel = true;
        } else if (arg.startsWith('--')) {
            const task = arg.substring(2);
            taskNames.push(task);
        }
    }

    const builder = new BlogBuilder(options);
    builder.run(taskNames).catch(error => {
        console.error('❌ Build failed:', error);
        process.exit(1);
    });
}

module.exports = BlogBuilder;
