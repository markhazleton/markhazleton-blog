/**
 * CSS Optimization Tool for Mark Hazleton Blog
 * Helps identify and remove unused CSS rules
 */

const fs = require('fs-extra');
const path = require('path');
const { glob } = require('glob');

class CSSOptimizer {
    constructor(options = {}) {
        this.srcPath = options.srcPath || path.resolve(__dirname, '../../src');
        this.docsPath = options.docsPath || path.resolve(__dirname, '../../docs');
        this.pugFiles = [];
        this.cssFiles = [];
        this.unusedRules = [];
    }

    /**
     * Scan all PUG files to find used CSS classes
     */
    async scanPugFiles() {
        console.log('🔍 Scanning PUG files for CSS usage...');

        const pugPattern = path.join(this.srcPath, '**/*.pug');
        this.pugFiles = await glob(pugPattern);

        const usedClasses = new Set();
        const usedIds = new Set();

        for (const file of this.pugFiles) {
            const content = await fs.readFile(file, 'utf8');

            // Extract classes from class attributes and utility classes
            const classMatches = content.match(/class\s*[=:]\s*['"`]([^'"`]*?)['"`]/g);
            if (classMatches) {
                classMatches.forEach(match => {
                    const classes = match.replace(/class\s*[=:]\s*['"`]/, '').replace(/['"`]$/, '');
                    classes.split(/\s+/).forEach(cls => {
                        if (cls.trim()) usedClasses.add(cls.trim());
                    });
                });
            }

            // Extract classes from shorthand syntax like .class-name
            const shorthandMatches = content.match(/\.\w+[\w-]*/g);
            if (shorthandMatches) {
                shorthandMatches.forEach(match => {
                    usedClasses.add(match.substring(1)); // Remove the leading dot
                });
            }

            // Extract IDs
            const idMatches = content.match(/id\s*[=:]\s*['"`]([^'"`]*?)['"`]|#\w+[\w-]*/g);
            if (idMatches) {
                idMatches.forEach(match => {
                    if (match.startsWith('#')) {
                        usedIds.add(match.substring(1));
                    } else {
                        const id = match.replace(/id\s*[=:]\s*['"`]/, '').replace(/['"`]$/, '');
                        usedIds.add(id);
                    }
                });
            }
        }

        console.log(`✅ Found ${usedClasses.size} CSS classes and ${usedIds.size} IDs in use`);
        return { usedClasses, usedIds };
    }

    /**
     * Analyze CSS files to find unused rules
     */
    async analyzeCSSUsage() {
        console.log('📊 Analyzing CSS usage...');

        const { usedClasses, usedIds } = await this.scanPugFiles();

        const modernStylesPath = path.join(this.docsPath, 'css', 'modern-styles.css');
        const stylesPath = path.join(this.docsPath, 'css', 'styles.css');

        const reports = {};

        for (const cssFile of [modernStylesPath, stylesPath]) {
            if (await fs.pathExists(cssFile)) {
                const report = await this.analyzeSingleCSSFile(cssFile, usedClasses, usedIds);
                reports[path.basename(cssFile)] = report;
            }
        }

        return reports;
    }

    /**
     * Analyze a single CSS file
     */
    async analyzeSingleCSSFile(filePath, usedClasses, usedIds) {
        const content = await fs.readFile(filePath, 'utf8');
        const fileName = path.basename(filePath);

        // Simple CSS rule extraction (not perfect but good enough for analysis)
        const cssRules = content.match(/[^{}]+\{[^{}]*\}/g) || [];

        const report = {
            totalRules: cssRules.length,
            unusedRules: [],
            unusedSelectors: [],
            potentialSavings: 0
        };

        cssRules.forEach((rule, index) => {
            const selectorPart = rule.split('{')[0].trim();
            const declarationPart = rule.split('{')[1]?.split('}')[0] || '';

            // Skip certain rules that are always needed
            if (this.shouldKeepRule(selectorPart)) {
                return;
            }

            let isUsed = false;

            // Check if any selectors in this rule are used
            const selectors = selectorPart.split(',').map(s => s.trim());

            for (const selector of selectors) {
                if (this.isSelectorUsed(selector, usedClasses, usedIds)) {
                    isUsed = true;
                    break;
                }
            }

            if (!isUsed) {
                report.unusedRules.push({
                    index,
                    selector: selectorPart,
                    declarations: declarationPart,
                    size: rule.length
                });
                report.potentialSavings += rule.length;
            }
        });

        console.log(`📄 ${fileName}: ${report.unusedRules.length}/${report.totalRules} unused rules (${(report.potentialSavings / 1024).toFixed(1)}KB potential savings)`);

        return report;
    }

    /**
     * Check if a selector is used
     */
    isSelectorUsed(selector, usedClasses, usedIds) {
        // Clean the selector
        const cleanSelector = selector.replace(/::?[a-zA-Z-]+/g, '') // Remove pseudo-elements/classes
                                    .replace(/\[.*?\]/g, '') // Remove attribute selectors
                                    .replace(/\s*[>~+]\s*/g, ' ') // Simplify combinators
                                    .trim();

        // Check for class selectors
        const classMatches = cleanSelector.match(/\.[a-zA-Z][\w-]*/g);
        if (classMatches) {
            for (const match of classMatches) {
                if (usedClasses.has(match.substring(1))) {
                    return true;
                }
            }
        }

        // Check for ID selectors
        const idMatches = cleanSelector.match(/#[a-zA-Z][\w-]*/g);
        if (idMatches) {
            for (const match of idMatches) {
                if (usedIds.has(match.substring(1))) {
                    return true;
                }
            }
        }

        // Check for element selectors that should be kept
        if (/^[a-zA-Z][a-zA-Z0-9]*$/.test(cleanSelector) &&
            ['html', 'body', 'main', 'header', 'footer', 'nav', 'section', 'article', 'aside', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'a', 'ul', 'ol', 'li'].includes(cleanSelector)) {
            return true;
        }

        return false;
    }

    /**
     * Rules that should always be kept
     */
    shouldKeepRule(selector) {
        const keepPatterns = [
            /^:root/, // CSS variables
            /^@/, // At-rules (media queries, keyframes, etc.)
            /^\*/, // Universal selector
            /^html/, // HTML element
            /^body/, // Body element
            /data-bs-theme/, // Bootstrap theme
            /\[data-/, // Data attributes
            /forced-colors/, // Accessibility
            /:focus/, // Focus states
            /:hover/, // Hover states
            /:active/, // Active states
            /:disabled/, // Disabled states
            /@media/, // Media queries
            /@supports/, // Feature queries
        ];

        return keepPatterns.some(pattern => pattern.test(selector));
    }

    /**
     * Generate optimization report
     */
    async generateReport() {
        const reports = await this.analyzeCSSUsage();

        let totalUnused = 0;
        let totalSavings = 0;

        console.log('\n📋 CSS Optimization Report:');
        console.log('==========================================');

        Object.entries(reports).forEach(([fileName, report]) => {
            totalUnused += report.unusedRules.length;
            totalSavings += report.potentialSavings;

            console.log(`\n📄 ${fileName}:`);
            console.log(`   • Total rules: ${report.totalRules}`);
            console.log(`   • Unused rules: ${report.unusedRules.length}`);
            console.log(`   • Potential savings: ${(report.potentialSavings / 1024).toFixed(1)}KB`);
            console.log(`   • Usage: ${((report.totalRules - report.unusedRules.length) / report.totalRules * 100).toFixed(1)}%`);

            if (report.unusedRules.length > 0) {
                console.log('\n   Top unused selectors:');
                report.unusedRules
                    .sort((a, b) => b.size - a.size)
                    .slice(0, 5)
                    .forEach(rule => {
                        console.log(`   • ${rule.selector.substring(0, 60)}... (${rule.size}B)`);
                    });
            }
        });

        console.log('\n🎯 Summary:');
        console.log(`   • Total unused rules: ${totalUnused}`);
        console.log(`   • Total potential savings: ${(totalSavings / 1024).toFixed(1)}KB`);
        console.log(`   • Estimated performance impact: ${Math.round(totalSavings / 1024 * 10)}ms faster CSS parsing`);

        console.log('\n💡 Recommendations:');
        console.log('1. Consider removing unused Bootstrap components');
        console.log('2. Use PurgeCSS or similar tool for production builds');
        console.log('3. Split CSS into critical and non-critical parts');
        console.log('4. Consider using CSS-in-JS for component-specific styles');

        return reports;
    }

    /**
     * Create an optimized CSS file with unused rules removed
     */
    async createOptimizedCSS(inputFile, outputFile) {
        console.log(`🎯 Creating optimized version of ${path.basename(inputFile)}...`);

        const { usedClasses, usedIds } = await this.scanPugFiles();
        const content = await fs.readFile(inputFile, 'utf8');

        // This is a simplified optimization - in production you'd want a proper CSS parser
        const cssRules = content.match(/[^{}]+\{[^{}]*\}/g) || [];
        const optimizedRules = [];

        cssRules.forEach(rule => {
            const selectorPart = rule.split('{')[0].trim();

            if (this.shouldKeepRule(selectorPart) ||
                this.isSelectorUsed(selectorPart, usedClasses, usedIds)) {
                optimizedRules.push(rule);
            }
        });

        const optimizedCSS = optimizedRules.join('\n');
        await fs.writeFile(outputFile, optimizedCSS);

        const originalSize = content.length;
        const optimizedSize = optimizedCSS.length;
        const savings = originalSize - optimizedSize;

        console.log(`✅ Optimization complete:`);
        console.log(`   • Original: ${(originalSize / 1024).toFixed(1)}KB`);
        console.log(`   • Optimized: ${(optimizedSize / 1024).toFixed(1)}KB`);
        console.log(`   • Savings: ${(savings / 1024).toFixed(1)}KB (${((savings / originalSize) * 100).toFixed(1)}%)`);
    }

    /**
     * Run the complete optimization analysis
     */
    async run() {
        console.log('🚀 Starting CSS optimization analysis...\n');

        try {
            const reports = await this.generateReport();

            // Optionally create optimized versions
            const modernStylesPath = path.join(this.docsPath, 'css', 'modern-styles.css');
            if (await fs.pathExists(modernStylesPath)) {
                const optimizedPath = path.join(this.docsPath, 'css', 'modern-styles.optimized.css');
                await this.createOptimizedCSS(modernStylesPath, optimizedPath);
            }

            return reports;

        } catch (error) {
            console.error('❌ CSS optimization failed:', error.message);
            throw error;
        }
    }
}

module.exports = CSSOptimizer;

// CLI usage
if (require.main === module) {
    const optimizer = new CSSOptimizer();
    optimizer.run().catch(console.error);
}
