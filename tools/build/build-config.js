const path = require('path');
const os = require('os');

/**
 * Build Configuration
 * Centralized configuration for the build system
 */
module.exports = {
    // Cache configuration
    cache: {
        enabled: process.env.NODE_ENV !== 'production' || process.env.BUILD_CACHE === 'true',
        directory: '.build-cache',
        ttl: 24 * 60 * 60 * 1000, // 24 hours
        cleanOnStart: false
    },

    // Performance tracking
    performance: {
        tracking: true,
        reportPath: '.build-cache/performance.json',
        showTrends: true
    },

    // Build optimization
    optimization: {
        parallel: true,
        maxConcurrency: Math.min(os.cpus().length, 4), // Limit to 4 max
        minifyHtml: process.env.NODE_ENV === 'production',
        generateSourceMaps: process.env.NODE_ENV === 'development',
        compressionLevel: process.env.NODE_ENV === 'production' ? 9 : 1
    },

    // Path configuration
    paths: {
        src: './src',
        build: './build',
        output: './docs',
        cache: './.build-cache',
        scripts: './scripts',
        assets: './src/assets',
        temp: './.build-cache/temp'
    },

    // File patterns
    patterns: {
        pug: {
            include: ['**/*.pug'],
            exclude: [
                '**/include/**',
                '**/mixin/**',
                '**/layouts/**',
                '**/modules/**'
            ]
        },
        scss: {
            main: 'src/scss/styles.scss',
            modern: 'src/scss/modern-styles.scss',
            watch: ['src/scss/**/*.scss']
        },
        scripts: {
            include: ['src/js/**/*.js'],
            exclude: ['src/js/vendor/**']
        },
        assets: {
            include: ['**/*'],
            exclude: ['**/*.pug', '**/*.scss', '**/*.js']
        }
    },

    // Task dependencies
    dependencies: {
        sitemap: ['sections', 'pug'],
        rss: ['sections'],
        projectsRss: ['sections'],
        pug: ['sections', 'placeholders', 'fonts'],
        projectPages: ['sections', 'placeholders', 'fonts'],
        scripts: [],
        scss: ['fonts'],
        assets: ['placeholders', 'fonts'],
        sections: [],
        placeholders: [],
        fonts: [],
        searchIndex: ['sections']
    },

    // Parallel execution groups
    parallelGroups: {
        phase1: ['fonts', 'placeholders', 'sections'], // Must run first
        phase2: ['pug', 'projectPages', 'scss', 'scripts', 'assets'], // Can run in parallel
        phase3: ['sitemap', 'rss', 'projectsRss', 'searchIndex'] // Depend on phase2 completion
    },

    // Error handling
    errorHandling: {
        retryCount: 3,
        retryDelay: 1000, // 1 second
        continueOnError: false,
        logErrors: true
    },

    // Development server
    devServer: {
        port: 3000,
        open: true,
        notify: false,
        cors: true,
        watch: {
            enabled: true,
            files: [
                'docs/**/*.html',
                'docs/css/*.css',
                'docs/js/*.js'
            ]
        }
    },

    // Logging
    logging: {
        level: process.env.LOG_LEVEL || 'info', // debug, info, warn, error
        timestamps: true,
        colors: true,
        progress: true
    },

    // File size limits (in bytes)
    limits: {
        maxFileSize: 10 * 1024 * 1024, // 10MB
        maxBundleSize: 5 * 1024 * 1024, // 5MB
        warnThreshold: 1 * 1024 * 1024 // 1MB
    },

    // Environment-specific overrides
    environments: {
        development: {
            cache: { enabled: true },
            optimization: {
                parallel: true,
                minifyHtml: false,
                generateSourceMaps: true
            },
            logging: { level: 'debug' }
        },
        production: {
            cache: { enabled: false },
            optimization: {
                parallel: true,
                minifyHtml: true,
                generateSourceMaps: false,
                compressionLevel: 9
            },
            logging: { level: 'info' }
        },
        ci: {
            cache: { enabled: true, ttl: 60 * 60 * 1000 }, // 1 hour in CI
            optimization: { parallel: true },
            logging: { level: 'warn' }
        }
    }
};

/**
 * Get environment-specific configuration
 */
function getConfig() {
    const baseConfig = module.exports;
    const env = process.env.NODE_ENV || 'development';
    const isCI = process.env.CI === 'true';

    // Determine environment
    let envConfig = {};
    if (isCI) {
        envConfig = baseConfig.environments.ci || {};
    } else if (baseConfig.environments[env]) {
        envConfig = baseConfig.environments[env];
    }

    // Deep merge configuration
    return mergeDeep(baseConfig, envConfig);
}

/**
 * Deep merge utility
 */
function mergeDeep(target, source) {
    const result = { ...target };

    for (const key in source) {
        if (source[key] && typeof source[key] === 'object' && !Array.isArray(source[key])) {
            result[key] = mergeDeep(target[key] || {}, source[key]);
        } else {
            result[key] = source[key];
        }
    }

    return result;
}

// Export both the base config and the environment-aware getter
module.exports.getConfig = getConfig;
module.exports.mergeDeep = mergeDeep;
