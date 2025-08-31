const chokidar = require('chokidar');
const path = require('path');
const upath = require('upath');

/**
 * Development File Watcher
 * Provides intelligent file watching for development builds
 */
class DevelopmentWatcher {
    constructor(builder, options = {}) {
        this.builder = builder;
        this.options = {
            debounceMs: options.debounceMs || 300,
            usePolling: options.usePolling || true,  // Enable polling by default for Windows
            interval: options.interval || 1000,     // Poll every second
            ...options
        };
        this.watchers = new Map();
        this.debounceTimers = new Map();
        this.isRunning = false;
    }

    /**
     * Start watching for file changes
     */
    start() {
        if (this.isRunning) {
            console.log('👀 Watcher already running');
            return;
        }

        console.log('👀 Starting development file watcher...');
        this.isRunning = true;

        // Watch PUG files
        this.addWatcher(
            'src/**/*.pug',
            this.handlePugChange.bind(this),
            'PUG templates'
        );

        // Watch SCSS files
        this.addWatcher(
            'src/scss/**/*.scss',
            this.handleScssChange.bind(this),
            'SCSS stylesheets'
        );

        // Watch JS files
        this.addWatcher(
            'src/js/**/*.js',
            this.handleJsChange.bind(this),
            'JavaScript files'
        );

        // Watch data files
        this.addWatcher(
            'src/**/*.json',
            this.handleDataChange.bind(this),
            'Data files'
        );

        // Watch asset files (images, fonts, etc.)
        this.addWatcher(
            ['src/assets/**/*', 'src/*.{txt,ico,png,jpg,jpeg,gif,svg,webp}'],
            this.handleAssetChange.bind(this),
            'Asset files'
        );

        console.log('✅ File watcher started successfully');
        console.log('📝 Watching for changes... (Ctrl+C to stop)');
    }

    /**
     * Stop watching
     */
    stop() {
        if (!this.isRunning) return;

        console.log('🛑 Stopping file watcher...');

        this.watchers.forEach(watcher => watcher.close());
        this.watchers.clear();

        this.debounceTimers.forEach(timer => clearTimeout(timer));
        this.debounceTimers.clear();

        this.isRunning = false;
        console.log('✅ File watcher stopped');
    }

    /**
     * Add a file watcher
     */
    addWatcher(pattern, callback, description) {
        const watchOptions = {
            ignored: [
                '**/node_modules/**',
                '**/docs/**',
                '**/.build-cache/**',
                '**/.git/**'
            ],
            persistent: true,
            ignoreInitial: true,
            usePolling: this.options.usePolling,
            interval: this.options.interval || 1000,
            binaryInterval: this.options.interval || 1000,
            atomic: true
        };

        const watcher = chokidar.watch(pattern, watchOptions);

        watcher.on('change', (filePath) => {
            console.log(`🔍 Raw change detected: ${filePath}`);
            this.debounceCallback(`change:${pattern}`, () => {
                console.log(`\n📝 ${description} changed: ${path.relative(process.cwd(), filePath)}`);
                callback(filePath, 'change');
            });
        });

        watcher.on('add', (filePath) => {
            this.debounceCallback(`add:${pattern}`, () => {
                console.log(`\n➕ ${description} added: ${path.relative(process.cwd(), filePath)}`);
                callback(filePath, 'add');
            });
        });

        watcher.on('unlink', (filePath) => {
            console.log(`\n➖ ${description} removed: ${path.relative(process.cwd(), filePath)}`);
            callback(filePath, 'unlink');
        });

        watcher.on('error', (error) => {
            console.error(`❌ Watcher error for ${description}:`, error.message);
        });

        this.watchers.set(pattern, watcher);
        console.log(`👀 Watching ${description}: ${pattern}`);
    }

    /**
     * Debounce callback execution
     */
    debounceCallback(key, callback) {
        if (this.debounceTimers.has(key)) {
            clearTimeout(this.debounceTimers.get(key));
        }

        const timer = setTimeout(() => {
            this.debounceTimers.delete(key);
            callback();
        }, this.options.debounceMs);

        this.debounceTimers.set(key, timer);
    }

    /**
     * Handle PUG file changes
     */
    async handlePugChange(filePath, eventType) {
        try {
            // If it's a layout/module file, rebuild all PUG
            const isLayoutOrModule = filePath.includes('/layouts/') ||
                                   filePath.includes('/modules/') ||
                                   filePath.includes('/include/');

            if (isLayoutOrModule) {
                console.log('🔄 Layout/module changed, rebuilding all PUG templates...');
                await this.builder.buildPug();
            } else {
                // Just rebuild the specific file
                console.log('🔄 Rebuilding PUG template...');
                await this.builder.buildPug();
            }

            console.log('✅ PUG rebuild complete');
        } catch (error) {
            console.error('❌ PUG rebuild failed:', error.message);
        }
    }

    /**
     * Handle SCSS file changes
     */
    async handleScssChange(filePath, eventType) {
        try {
            console.log('🔄 Rebuilding SCSS stylesheets...');
            await this.builder.buildSCSS();
            console.log('✅ SCSS rebuild complete');
        } catch (error) {
            console.error('❌ SCSS rebuild failed:', error.message);
        }
    }

    /**
     * Handle JavaScript file changes
     */
    async handleJsChange(filePath, eventType) {
        try {
            console.log('🔄 Rebuilding JavaScript files...');
            await this.builder.buildScripts();
            console.log('✅ JavaScript rebuild complete');
        } catch (error) {
            console.error('❌ JavaScript rebuild failed:', error.message);
        }
    }

    /**
     * Handle data file changes (JSON files)
     */
    async handleDataChange(filePath, eventType) {
        try {
            const fileName = path.basename(filePath);
            console.log(`🔄 Data file changed: ${fileName}`);

            // Always rebuild sections first when data changes
            await this.builder.buildSections();

            // If articles.json changed, rebuild sitemap and RSS
            if (fileName === 'articles.json') {
                await this.builder.buildSitemap();
                await this.builder.buildRSS();
            }

            console.log('✅ Data rebuild complete');
        } catch (error) {
            console.error('❌ Data rebuild failed:', error.message);
        }
    }

    /**
     * Handle asset file changes
     */
    async handleAssetChange(filePath, eventType) {
        try {
            console.log('🔄 Rebuilding assets...');
            await this.builder.buildAssets();
            console.log('✅ Asset rebuild complete');
        } catch (error) {
            console.error('❌ Asset rebuild failed:', error.message);
        }
    }

    /**
     * Get watcher status
     */
    getStatus() {
        return {
            isRunning: this.isRunning,
            watcherCount: this.watchers.size,
            pendingTimers: this.debounceTimers.size
        };
    }
}

module.exports = DevelopmentWatcher;
