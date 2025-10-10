/**
 * JavaScript and Tracking Optimization
 * Addresses unused tracking code and JavaScript optimization
 */

const fs = require('fs-extra');
const path = require('path');
const { glob } = require('glob');

class JSTrackingOptimizer {
    constructor(options = {}) {
        this.srcPath = options.srcPath || path.resolve(__dirname, '../../src');
        this.docsPath = options.docsPath || path.resolve(__dirname, '../../docs');
    }

    /**
     * Analyze tracking scripts and their impact
     */
    async analyzeTrackingScripts() {
        console.log('🔍 Analyzing tracking scripts and JavaScript usage...');

        const analysis = {
            trackingScripts: [],
            renderBlockingScripts: [],
            optimizationOpportunities: [],
            estimatedSavings: 0
        };

        // Common tracking patterns to look for
        const trackingPatterns = [
            {
                name: 'Google Analytics',
                patterns: [/google-analytics\.com/, /gtag\(/g, /GoogleAnalyticsObject/],
                estimatedSize: 45000, // ~45KB
                blockingTime: 200 // ~200ms
            },
            {
                name: 'Google Tag Manager',
                patterns: [/googletagmanager\.com/, /gtm\.js/, /dataLayer/],
                estimatedSize: 35000, // ~35KB
                blockingTime: 150 // ~150ms
            },
            {
                name: 'Facebook Pixel',
                patterns: [/facebook\.net/, /fbq\(/g, /connect\.facebook\.net/],
                estimatedSize: 25000, // ~25KB
                blockingTime: 100 // ~100ms
            },
            {
                name: 'LinkedIn Insight',
                patterns: [/linkedin\.com.*insight/, /_linkedin_partner_id/],
                estimatedSize: 20000, // ~20KB
                blockingTime: 80 // ~80ms
            }
        ];

        // Scan all PUG files for tracking scripts
        const pugPattern = path.join(this.srcPath, '**/*.pug');
        const pugFiles = await glob(pugPattern);

        for (const file of pugFiles) {
            const content = await fs.readFile(file, 'utf8');
            const relativePath = path.relative(this.srcPath, file);

            // Check for render-blocking scripts
            const scriptMatches = content.match(/script\([^)]*src\s*=\s*['"][^'"]*['"][^)]*\)/g);
            if (scriptMatches) {
                scriptMatches.forEach(match => {
                    if (!match.includes('defer') && !match.includes('async')) {
                        analysis.renderBlockingScripts.push({
                            file: relativePath,
                            script: match,
                            issue: 'Render-blocking script without defer/async'
                        });
                    }
                });
            }

            // Check for tracking scripts
            trackingPatterns.forEach(pattern => {
                pattern.patterns.forEach(regex => {
                    if (regex.test(content)) {
                        analysis.trackingScripts.push({
                            file: relativePath,
                            name: pattern.name,
                            estimatedSize: pattern.estimatedSize,
                            blockingTime: pattern.blockingTime
                        });
                        analysis.estimatedSavings += pattern.estimatedSize;
                    }
                });
            });
        }

        return analysis;
    }

    /**
     * Check for GDPR compliance issues
     */
    async checkGDPRCompliance() {
        console.log('🔒 Checking GDPR compliance for tracking scripts...');

        const issues = [];
        const pugPattern = path.join(this.srcPath, '**/*.pug');
        const pugFiles = await glob(pugPattern);

        for (const file of pugFiles) {
            const content = await fs.readFile(file, 'utf8');
            const relativePath = path.relative(this.srcPath, file);

            // Check for tracking without consent
            if (content.includes('gtag(') || content.includes('ga(')) {
                if (!content.includes('consent') && !content.includes('cookieconsent')) {
                    issues.push({
                        file: relativePath,
                        issue: 'Google Analytics loading without consent check',
                        severity: 'high',
                        fix: 'Move analytics behind consent wall'
                    });
                }
            }

            if (content.includes('facebook.net') || content.includes('fbq(')) {
                if (!content.includes('consent')) {
                    issues.push({
                        file: relativePath,
                        issue: 'Facebook Pixel loading without consent check',
                        severity: 'high',
                        fix: 'Move Facebook tracking behind consent wall'
                    });
                }
            }
        }

        return issues;
    }

    /**
     * Analyze JavaScript bundle sizes and opportunities
     */
    async analyzeJavaScriptBundles() {
        console.log('📦 Analyzing JavaScript bundles...');

        const jsPath = path.join(this.docsPath, 'js');
        const analysis = {
            bundles: [],
            totalSize: 0,
            opportunities: []
        };

        if (await fs.pathExists(jsPath)) {
            const jsFiles = await glob(path.join(jsPath, '*.js'));

            for (const file of jsFiles) {
                const stats = await fs.stat(file);
                const fileName = path.basename(file);

                analysis.bundles.push({
                    name: fileName,
                    size: stats.size,
                    sizeKB: (stats.size / 1024).toFixed(1)
                });

                analysis.totalSize += stats.size;

                // Check for optimization opportunities
                if (fileName.includes('bootstrap') && stats.size > 100000) {
                    analysis.opportunities.push({
                        file: fileName,
                        issue: 'Large Bootstrap bundle',
                        fix: 'Consider tree-shaking or using only needed components',
                        potentialSavings: Math.round(stats.size * 0.4) // Estimate 40% savings
                    });
                }

                if (fileName.includes('jquery') && stats.size > 80000) {
                    analysis.opportunities.push({
                        file: fileName,
                        issue: 'Large jQuery bundle',
                        fix: 'Consider replacing with vanilla JS or lighter alternatives',
                        potentialSavings: stats.size
                    });
                }
            }
        }

        return analysis;
    }

    /**
     * Generate optimization recommendations
     */
    generateOptimizationPlan(trackingAnalysis, gdprIssues, jsAnalysis) {
        console.log('\n📋 JavaScript & Tracking Optimization Report:');
        console.log('==============================================');

        // Tracking Scripts Analysis
        if (trackingAnalysis.trackingScripts.length > 0) {
            console.log('\n📊 Tracking Scripts Found:');
            const trackingByType = {};
            trackingAnalysis.trackingScripts.forEach(script => {
                if (!trackingByType[script.name]) {
                    trackingByType[script.name] = { count: 0, size: 0, time: 0 };
                }
                trackingByType[script.name].count++;
                trackingByType[script.name].size += script.estimatedSize;
                trackingByType[script.name].time += script.blockingTime;
            });

            Object.entries(trackingByType).forEach(([name, data]) => {
                console.log(`   • ${name}: ${data.count} instances (~${(data.size/1024).toFixed(1)}KB, ~${data.time}ms impact)`);
            });
        }

        // GDPR Issues
        if (gdprIssues.length > 0) {
            console.log('\n🔒 GDPR Compliance Issues:');
            gdprIssues.forEach(issue => {
                console.log(`   • ${issue.file}: ${issue.issue}`);
                console.log(`     Fix: ${issue.fix}`);
            });
        }

        // JavaScript Bundles
        if (jsAnalysis.bundles.length > 0) {
            console.log('\n📦 JavaScript Bundles:');
            jsAnalysis.bundles
                .sort((a, b) => b.size - a.size)
                .forEach(bundle => {
                    console.log(`   • ${bundle.name}: ${bundle.sizeKB}KB`);
                });
        }

        // Render-blocking Scripts
        if (trackingAnalysis.renderBlockingScripts.length > 0) {
            console.log('\n🚫 Render-blocking Scripts:');
            trackingAnalysis.renderBlockingScripts.forEach(script => {
                console.log(`   • ${script.file}: ${script.issue}`);
            });
        }

        // Optimization Opportunities
        console.log('\n💡 Optimization Opportunities:');

        // 1. Tracking optimization
        const totalTrackingSize = trackingAnalysis.trackingScripts.reduce((sum, script) => sum + script.estimatedSize, 0);
        const totalTrackingTime = trackingAnalysis.trackingScripts.reduce((sum, script) => sum + script.blockingTime, 0);

        if (totalTrackingSize > 0) {
            console.log(`\n1. 🎯 Defer Tracking Scripts:`);
            console.log(`   • Current impact: ~${(totalTrackingSize/1024).toFixed(1)}KB, ~${totalTrackingTime}ms blocking`);
            console.log(`   • Move all tracking behind consent and after page load`);
            console.log(`   • Estimated savings: ${totalTrackingTime}ms faster First Contentful Paint`);
        }

        // 2. JavaScript optimization
        if (jsAnalysis.opportunities.length > 0) {
            console.log(`\n2. 📦 JavaScript Bundle Optimization:`);
            jsAnalysis.opportunities.forEach((opp, index) => {
                console.log(`   ${index + 1}. ${opp.file}: ${opp.issue}`);
                console.log(`      Fix: ${opp.fix}`);
                console.log(`      Potential savings: ~${(opp.potentialSavings/1024).toFixed(1)}KB`);
            });
        }

        // 3. GDPR fixes
        if (gdprIssues.length > 0) {
            console.log(`\n3. 🔒 GDPR Compliance:`);
            console.log(`   • Implement consent management before loading tracking`);
            console.log(`   • Use privacy-first analytics alternatives`);
            console.log(`   • Consider server-side tracking for critical metrics`);
        }

        // Calculate total estimated improvements
        const totalJSSavings = jsAnalysis.opportunities.reduce((sum, opp) => sum + opp.potentialSavings, 0);
        console.log(`\n🎯 Total Estimated Improvements:`);
        console.log(`   • Tracking delay savings: ${totalTrackingTime}ms faster FCP`);
        console.log(`   • JavaScript size reduction: ~${(totalJSSavings/1024).toFixed(1)}KB`);
        console.log(`   • Combined performance impact: ~${Math.round(totalTrackingTime + (totalJSSavings/1024)*5)}ms faster page load`);

        return {
            trackingOptimizations: totalTrackingTime,
            jsOptimizations: totalJSSavings,
            gdprFixes: gdprIssues.length,
            totalImpact: totalTrackingTime + Math.round((totalJSSavings/1024)*5)
        };
    }

    /**
     * Create consent-aware tracking implementation
     */
    async createConsentAwareTracking() {
        console.log('\n🔧 Creating consent-aware tracking implementation...');

        const consentTrackingScript = `
// Consent-Aware Tracking Implementation
class ConsentAwareTracking {
    constructor() {
        this.consentGiven = false;
        this.queuedEvents = [];
        this.checkConsent();
    }

    checkConsent() {
        // Check for existing consent (cookies, localStorage, etc.)
        const consent = localStorage.getItem('tracking-consent');
        if (consent === 'granted') {
            this.initializeTracking();
        }
    }

    grantConsent() {
        this.consentGiven = true;
        localStorage.setItem('tracking-consent', 'granted');
        this.initializeTracking();
        this.processQueuedEvents();
    }

    revokeConsent() {
        this.consentGiven = false;
        localStorage.setItem('tracking-consent', 'denied');
        // Clean up tracking scripts and cookies
        this.cleanupTracking();
    }

    initializeTracking() {
        if (!this.consentGiven) return;

        // Load Google Analytics only after consent
        window.addEventListener('load', () => {
            const script = document.createElement('script');
            script.async = true;
            script.src = 'https://www.google-analytics.com/analytics.js';
            document.head.appendChild(script);

            script.onload = () => {
                ga('create', 'UA-146971717-1', 'auto');
                ga('send', 'pageview');
            };
        });
    }

    track(event, data) {
        if (this.consentGiven) {
            // Send tracking data
            if (typeof ga !== 'undefined') {
                ga('send', 'event', event, data);
            }
        } else {
            // Queue for later if user gives consent
            this.queuedEvents.push({ event, data, timestamp: Date.now() });
        }
    }

    processQueuedEvents() {
        this.queuedEvents.forEach(({ event, data }) => {
            this.track(event, data);
        });
        this.queuedEvents = [];
    }

    cleanupTracking() {
        // Remove tracking cookies and scripts
        document.cookie.split(";").forEach(cookie => {
            const eqPos = cookie.indexOf("=");
            const name = eqPos > -1 ? cookie.substr(0, eqPos).trim() : cookie.trim();
            if (name.startsWith('_ga') || name.startsWith('_gid')) {
                document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/";
            }
        });
    }
}

// Initialize tracking system
window.trackingSystem = new ConsentAwareTracking();
`.trim();

        const outputPath = path.join(this.docsPath, 'js', 'consent-tracking.js');
        await fs.writeFile(outputPath, consentTrackingScript);

        console.log(`✅ Created consent-aware tracking script: ${outputPath}`);

        return consentTrackingScript;
    }

    /**
     * Run complete analysis
     */
    async run() {
        console.log('🚀 Starting JavaScript and Tracking optimization analysis...\n');

        try {
            const trackingAnalysis = await this.analyzeTrackingScripts();
            const gdprIssues = await this.checkGDPRCompliance();
            const jsAnalysis = await this.analyzeJavaScriptBundles();

            const optimizationPlan = this.generateOptimizationPlan(trackingAnalysis, gdprIssues, jsAnalysis);

            // Create consent-aware implementation
            await this.createConsentAwareTracking();

            console.log('\n📋 Next Steps:');
            console.log('1. Implement the performance-optimized-layout.pug for articles');
            console.log('2. Replace current tracking with consent-aware implementation');
            console.log('3. Add defer/async attributes to remaining scripts');
            console.log('4. Consider removing unused JavaScript libraries');
            console.log('5. Test the implementation with Lighthouse');

            return optimizationPlan;

        } catch (error) {
            console.error('❌ Analysis failed:', error.message);
            throw error;
        }
    }
}

module.exports = JSTrackingOptimizer;

// CLI usage
if (require.main === module) {
    const optimizer = new JSTrackingOptimizer();
    optimizer.run().catch(console.error);
}
