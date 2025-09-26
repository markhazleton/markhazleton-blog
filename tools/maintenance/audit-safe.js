#!/usr/bin/env node

/**
 * Safe Audit Runner for Windows
 * Handles Lighthouse permission issues and provides fallback options
 */

const { execSync, spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

class SafeAuditor {
    constructor() {
        this.results = {
            performance: { status: 'pending', error: null },
            seo: { status: 'pending', error: null },
            ssl: { status: 'pending', error: null },
            a11y: { status: 'pending', error: null }
        };
    }

    async runPerformanceAudit() {
        console.log('🚀 Running Performance Audit...');
        try {
            // Clear any lingering Chrome processes
            try {
                execSync('taskkill /f /im chrome.exe /t', { stdio: 'ignore' });
            } catch (e) {
                // Ignore if no Chrome processes
            }

            // Clear temp lighthouse files
            try {
                const tempDir = process.env.TEMP || process.env.TMP || '/tmp';
                execSync(`powershell -Command "Get-ChildItem '${tempDir}\\lighthouse*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue"`, { stdio: 'ignore' });
            } catch (e) {
                // Ignore cleanup errors
            }

            // Run lighthouse with timeout and error handling
            const result = execSync('npx lhci autorun --config=lighthouserc.json', {
                encoding: 'utf8',
                timeout: 180000, // 3 minute timeout
                stdio: ['pipe', 'pipe', 'pipe']
            });

            this.results.performance.status = 'success';
            console.log('✅ Performance audit completed');
            return true;

        } catch (error) {
            this.results.performance.status = 'failed';
            this.results.performance.error = error.message;
            console.log('⚠️  Performance audit failed:', error.message);

            // Try simplified single-URL audit as fallback
            return await this.runSimplifiedPerformanceAudit();
        }
    }

    async runSimplifiedPerformanceAudit() {
        console.log('🔄 Trying simplified performance audit...');
        try {
            // Test just the homepage
            const result = execSync('npx lighthouse https://markhazleton.com/ --output=json --output-path=./artifacts/lighthouse-simple.json --chrome-flags="--headless --no-sandbox --disable-dev-shm-usage"', {
                encoding: 'utf8',
                timeout: 120000 // 2 minute timeout
            });

            console.log('✅ Simplified performance audit completed');
            this.results.performance.status = 'partial';
            return true;
        } catch (error) {
            console.log('❌ Simplified performance audit also failed');
            return false;
        }
    }

    async runSeoAudit() {
        console.log('🔍 Running SEO Audit...');
        try {
            execSync('node tools/seo/seo-a11y-checks.mjs', {
                encoding: 'utf8',
                timeout: 60000
            });

            this.results.seo.status = 'success';
            console.log('✅ SEO audit completed');
            return true;
        } catch (error) {
            this.results.seo.status = 'failed';
            this.results.seo.error = error.message;
            console.log('❌ SEO audit failed:', error.message);
            return false;
        }
    }

    async runSslAudit() {
        console.log('🔒 Running SSL Certificate Audit...');
        try {
            execSync('npx tsx tools/seo/ssl-expiry.ts', {
                encoding: 'utf8',
                timeout: 30000
            });

            this.results.ssl.status = 'success';
            console.log('✅ SSL audit completed');
            return true;
        } catch (error) {
            this.results.ssl.status = 'failed';
            this.results.ssl.error = error.message;
            console.log('❌ SSL audit failed:', error.message);
            return false;
        }
    }

    async runA11yAudit() {
        console.log('♿ Running Accessibility Audit...');
        try {
            // Ensure artifacts directory exists
            if (!fs.existsSync('artifacts')) {
                fs.mkdirSync('artifacts', { recursive: true });
            }

            execSync('npx pa11y-ci --config pa11yci.json --reporter json > artifacts/a11y.json 2> artifacts/a11y.stderr.log', {
                encoding: 'utf8',
                timeout: 120000,
                shell: true
            });

            this.results.a11y.status = 'success';
            console.log('✅ Accessibility audit completed');
            return true;
        } catch (error) {
            this.results.a11y.status = 'failed';
            this.results.a11y.error = error.message;
            console.log('❌ Accessibility audit failed:', error.message);
            return false;
        }
    }

    generateReport() {
        console.log('\n📊 Audit Results Summary');
        console.log('========================');

        const statuses = Object.entries(this.results);
        let successCount = 0;
        let totalCount = statuses.length;

        statuses.forEach(([audit, result]) => {
            const icon = result.status === 'success' ? '✅' :
                        result.status === 'partial' ? '⚠️' : '❌';

            console.log(`${icon} ${audit.toUpperCase()}: ${result.status}`);

            if (result.error) {
                console.log(`   Error: ${result.error.substring(0, 100)}...`);
            }

            if (result.status === 'success' || result.status === 'partial') {
                successCount++;
            }
        });

        console.log(`\n📈 Overall: ${successCount}/${totalCount} audits completed`);

        if (successCount === totalCount) {
            console.log('🎉 All audits completed successfully!');
        } else if (successCount > 0) {
            console.log('⚠️  Some audits completed with issues');
        } else {
            console.log('❌ All audits failed - check configuration');
        }

        return successCount / totalCount;
    }

    async runAll() {
        console.log('🚀 Starting Safe Audit Process');
        console.log('===============================\n');

        // Ensure artifacts directory exists
        if (!fs.existsSync('artifacts')) {
            fs.mkdirSync('artifacts', { recursive: true });
        }

        // Run all audits
        await this.runPerformanceAudit();
        await this.runSeoAudit();
        await this.runSslAudit();
        await this.runA11yAudit();

        // Generate and return report
        return this.generateReport();
    }
}

// CLI execution
if (require.main === module) {
    const auditor = new SafeAuditor();

    auditor.runAll()
        .then(successRate => {
            if (successRate >= 0.75) {
                process.exit(0);
            } else {
                process.exit(1);
            }
        })
        .catch(error => {
            console.error('💥 Audit process crashed:', error);
            process.exit(1);
        });
}

module.exports = SafeAuditor;
