/**
 * Critical CSS Extractor for Mark Hazleton Blog
 * Extracts above-the-fold styles to inline in HTML head
 */

const fs = require('fs-extra');
const path = require('path');
const postcss = require('postcss');
const upath = require('upath');

class CriticalCSSExtractor {
    constructor(options = {}) {
        this.srcPath = options.srcPath || path.resolve(__dirname, '../../src');
        this.docsPath = options.docsPath || path.resolve(__dirname, '../../docs');
        this.criticalSelectors = [
            // Essential CSS variables and root
            ':root', 'data-bs-theme',

            // Navigation (above-the-fold)
            '.navbar', '.navbar-brand', '.navbar-nav', '.nav-link',
            '.navbar-toggler', '.navbar-collapse', '.navbar-expand-lg',
            '.navbar-dark', '.bg-dark',

            // Essential layout (above-the-fold only)
            '.container', '.container-fluid', '.row',
            '.col-12', '.col-md-6', '.col-lg-8', '.col-lg-4',

            // Critical buttons (visible immediately)
            '.btn', '.btn-primary', '.btn-outline-light',

            // Essential utilities (above-the-fold)
            '.fixed-top', '.d-flex', '.text-white', '.text-center',
            '.mb-4', '.py-5', '.me-auto', '.ms-auto',

            // Essential typography (hero section)
            'h1', 'h2', '.display-4', '.lead',

            // Critical responsive (mobile-first)
            'min-width: 992px', 'min-width: 768px'
        ];

        this.deferredSelectors = [
            // Component-specific styles
            '.accordion', '.card', '.modal', '.carousel', '.dropdown',
            '.toast', '.popover', '.tooltip', '.collapse',

            // Icons (load after critical path)
            '.bi-', '.fa-', '.fab-', '.fas-', '.far-',

            // Code highlighting
            '.highlight', '.language-', 'pre\\[class.*="language-"\\]',
            '.token', 'code\\[class.*="language-"\\]',

            // Less critical utilities
            '.border-', '.rounded-', '.shadow-', '.bg-light', '.bg-secondary'
        ];
    }

    /**
     * Extract critical CSS from the main stylesheet
     */
    async extractCriticalCSS() {
        console.log('🎯 Extracting critical CSS...');

        const modernStylesPath = path.join(this.docsPath, 'css', 'modern-styles.css');
        if (!await fs.pathExists(modernStylesPath)) {
            throw new Error('modern-styles.css not found. Run SCSS build first.');
        }

        const fullCSS = await fs.readFile(modernStylesPath, 'utf8');

        // Parse CSS with PostCSS
        const root = postcss.parse(fullCSS);
        const criticalRules = [];
        const deferredRules = [];

        root.walkRules(rule => {
            const selector = rule.selector;
            const isCritical = this.criticalSelectors.some(crit => selector.includes(crit));
            const isDeferred = this.deferredSelectors.some(def => selector.includes(def));

            if (isCritical && !isDeferred) {
                criticalRules.push(rule.toString());
            }
        });

        // Also include essential at-rules (media queries for mobile-first)
        root.walkAtRules(rule => {
            if (rule.name === 'media' && (
                rule.params.includes('min-width: 768px') ||
                rule.params.includes('min-width: 992px') ||
                rule.params.includes('prefers-color-scheme')
            )) {
                // Check if the media query contains critical selectors
                const ruleString = rule.toString();
                const hasCriticalContent = this.criticalSelectors.some(crit =>
                    ruleString.includes(crit)
                );

                if (hasCriticalContent) {
                    criticalRules.push(ruleString);
                }
            }
        });

        const criticalCSS = criticalRules.join('\n');

        // Minify critical CSS
        const minifiedCritical = await this.minifyCSS(criticalCSS);

        // Save critical CSS
        const criticalPath = path.join(this.docsPath, 'css', 'critical.css');
        await fs.writeFile(criticalPath, minifiedCritical);

        console.log(`✅ Critical CSS extracted: ${(minifiedCritical.length / 1024).toFixed(1)}KB`);
        return minifiedCritical;
    }

    /**
     * Minify CSS using basic optimization
     */
    async minifyCSS(css) {
        return css
            .replace(/\/\*[\s\S]*?\*\//g, '') // Remove comments
            .replace(/\s+/g, ' ') // Collapse whitespace
            .replace(/;\s*}/g, '}') // Remove unnecessary semicolons
            .replace(/{\s*/g, '{') // Remove space after opening brace
            .replace(/}\s*/g, '}') // Remove space after closing brace
            .replace(/,\s*/g, ',') // Remove space after commas
            .replace(/:\s*/g, ':') // Remove space after colons
            .trim();
    }

    /**
     * Generate non-critical CSS by removing critical styles
     */
    async generateNonCriticalCSS() {
        console.log('📦 Generating non-critical CSS...');

        const modernStylesPath = path.join(this.docsPath, 'css', 'modern-styles.css');
        const fullCSS = await fs.readFile(modernStylesPath, 'utf8');

        const root = postcss.parse(fullCSS);
        const nonCriticalRules = [];

        root.walkRules(rule => {
            const selector = rule.selector;
            const isCritical = this.criticalSelectors.some(crit =>
                selector.includes(crit) || selector.match(new RegExp(crit.replace('*', '.*')))
            );

            if (!isCritical) {
                nonCriticalRules.push(rule.toString());
            }
        });

        // Include non-critical at-rules
        // Include non-critical at-rules
        root.walkAtRules(rule => {
            const ruleString = rule.toString();
            const isCriticalMedia = rule.name === 'media' && (
                rule.params.includes('min-width: 768px') ||
                rule.params.includes('min-width: 992px') ||
                rule.params.includes('prefers-color-scheme')
            );

            if (!isCriticalMedia) {
                nonCriticalRules.push(ruleString);
            }
        });

        const nonCriticalCSS = nonCriticalRules.join('\n');

        // Save non-critical CSS
        const nonCriticalPath = path.join(this.docsPath, 'css', 'non-critical.css');
        await fs.writeFile(nonCriticalPath, nonCriticalCSS);

        console.log(`✅ Non-critical CSS generated: ${(nonCriticalCSS.length / 1024).toFixed(1)}KB`);
        return nonCriticalCSS;
    }

    /**
     * Create the CSS loading snippet for deferred loading
     */
    generateLoadingSnippet() {
        return `
<!-- Critical CSS (inline) -->
<style id="critical-css">
/* Critical CSS will be inserted here */
</style>

<!-- Non-critical CSS (deferred) -->
<link rel="preload" href="/css/non-critical.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
<noscript><link rel="stylesheet" href="/css/non-critical.css"></noscript>

<!-- Fallback for older browsers -->
<script>
(function() {
    var link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = '/css/non-critical.css';
    var supportsPreload = link.relList && link.relList.supports && link.relList.supports('preload');
    if (!supportsPreload) {
        document.head.appendChild(link);
    }
})();
</script>`.trim();
    }

    /**
     * Run the complete critical CSS extraction process
     */
    async run() {
        try {
            const criticalCSS = await this.extractCriticalCSS();
            await this.generateNonCriticalCSS();

            const loadingSnippet = this.generateLoadingSnippet();

            console.log('\n📋 Implementation Instructions:');
            console.log('1. Replace the current CSS link in your PUG layout with the loading snippet');
            console.log('2. Insert the critical CSS inline in the <head> section');
            console.log('3. Test the implementation with Lighthouse');

            return {
                criticalCSS,
                loadingSnippet,
                size: {
                    critical: (criticalCSS.length / 1024).toFixed(1) + 'KB',
                    estimated_savings: '340ms (render-blocking elimination)'
                }
            };

        } catch (error) {
            console.error('❌ Critical CSS extraction failed:', error.message);
            throw error;
        }
    }
}

module.exports = CriticalCSSExtractor;

// CLI usage
if (require.main === module) {
    const extractor = new CriticalCSSExtractor();
    extractor.run().catch(console.error);
}
