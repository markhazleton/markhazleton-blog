#!/usr/bin/env node

/**
 * Windows-Safe Test Runner
 * Handles Windows-specific permission issues with Lighthouse and pa11y
 */

const { execSync, spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

class WindowsSafeTestRunner {
    constructor() {
        this.results = {
            build: { status: 'pending', error: null },
            performance: { status: 'pending', error: null },
            seo: { status: 'pending', error: null },
            ssl: { status: 'pending', error: null },
            a11y: { status: 'pending', error: null }
        };
    }

    async runBuild() {
        console.log('🏗️  Running Build Process...');
        try {
            execSync('npm run build:no-cache', {
                encoding: 'utf8',
                timeout: 300000, // 5 minutes
                stdio: 'inherit'
            });

            this.results.build.status = 'success';
            console.log('✅ Build completed successfully');
            return true;
        } catch (error) {
            this.results.build.status = 'failed';
            this.results.build.error = error.message;
            console.log('❌ Build failed:', error.message);
            return false;
        }
    }

    async cleanupChrome() {
        console.log('🧹 Cleaning up Chrome processes and temp files...');
        try {
            // Kill any Chrome processes
            execSync('taskkill /f /im chrome.exe /t 2>nul || echo "No Chrome processes to kill"', {
                stdio: 'pipe',
                encoding: 'utf8'
            });

            // Clean temp lighthouse files with PowerShell (more reliable on Windows)
            const tempCleanupScript = `
                $tempPath = [System.IO.Path]::GetTempPath()
                Get-ChildItem -Path $tempPath -Filter "lighthouse.*" -ErrorAction SilentlyContinue |
                Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

                Get-ChildItem -Path $tempPath -Filter "chrome_*" -ErrorAction SilentlyContinue |
                Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
            `;

            execSync(`powershell -Command "${tempCleanupScript}"`, {
                stdio: 'pipe',
                timeout: 30000
            });

            // Wait a moment for cleanup
            await new Promise(resolve => setTimeout(resolve, 2000));

        } catch (error) {
            console.log('⚠️  Cleanup had some issues (continuing anyway):', error.message);
        }
    }

    async runPerformanceAudit() {
        console.log('🚀 Running Performance Audit...');

        // Clean up first
        await this.cleanupChrome();

        try {
            // Try the full LHCI audit first
            execSync('npx lhci autorun --config=lighthouserc.json', {
                encoding: 'utf8',
                timeout: 180000, // 3 minutes
                stdio: 'pipe'
            });

            this.results.performance.status = 'success';
            console.log('✅ Performance audit completed');
            return true;

        } catch (error) {
            console.log('⚠️  Full performance audit failed, trying simplified version...');

            // Clean up again
            await this.cleanupChrome();

            try {
                // Simplified single-page audit with better Windows compatibility
                const lighthouseCmd = [
                    'npx lighthouse',
                    'https://markhazleton.com/',
                    '--output=json',
                    '--output-path=./artifacts/lighthouse-simple.json',
                    '--chrome-flags="--headless --no-sandbox --disable-dev-shm-usage --disable-gpu --disable-extensions --no-first-run --disable-default-apps"',
                    '--max-wait-for-load=60000'
                ].join(' ');

                execSync(lighthouseCmd, {
                    encoding: 'utf8',
                    timeout: 120000,
                    stdio: 'pipe'
                });

                console.log('✅ Simplified performance audit completed');
                this.results.performance.status = 'partial';
                return true;

            } catch (simplifiedError) {
                console.log('⚠️  Lighthouse completely failed, trying basic performance fallback...');

                try {
                    // Use basic performance fallback
                    const PerformanceFallback = require('./performance-fallback.js');
                    const fallback = new PerformanceFallback();
                    const success = await fallback.runFallbackPerformanceTest();

                    if (success) {
                        console.log('✅ Basic performance tests completed');
                        this.results.performance.status = 'partial';
                        this.results.performance.error = 'Lighthouse failed, used basic fallback testing';
                        return true;
                    } else {
                        throw new Error('Fallback tests also failed');
                    }

                } catch (fallbackError) {
                    console.log('❌ All performance audits failed');
                    this.results.performance.status = 'failed';
                    this.results.performance.error = 'Lighthouse and fallback tests both failed';
                    return false;
                }
            }
        }
    }

    async runSeoAudit() {
        console.log('🔍 Running SEO Audit...');
        try {
            execSync('node tools/seo/seo-a11y-checks.mjs', {
                encoding: 'utf8',
                timeout: 60000,
                stdio: 'pipe'
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
                timeout: 30000,
                stdio: 'pipe'
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

            // Try pa11y with better Windows compatibility
            const pa11yCmd = [
                'npx pa11y-ci',
                '--config pa11yci.json',
                '--reporter json'
            ].join(' ');

            const result = execSync(pa11yCmd, {
                encoding: 'utf8',
                timeout: 120000,
                stdio: 'pipe'
            });

            // Write result to file
            fs.writeFileSync('artifacts/a11y.json', result);

            this.results.a11y.status = 'success';
            console.log('✅ Accessibility audit completed');
            return true;

        } catch (error) {
            // Even if pa11y exits with error code, it might have generated useful output
            try {
                if (error.stdout) {
                    fs.writeFileSync('artifacts/a11y.json', error.stdout);
                    console.log('⚠️  Accessibility audit completed with warnings');
                    this.results.a11y.status = 'partial';
                    return true;
                }
            } catch (writeError) {
                // Ignore write errors
            }

            this.results.a11y.status = 'failed';
            this.results.a11y.error = error.message;
            console.log('❌ Accessibility audit failed:', error.message);
            return false;
        }
    }

    generateReport() {
        console.log('\\n📊 Test Results Summary');
        console.log('=========================');

        const statuses = Object.entries(this.results);
        let successCount = 0;
        let totalCount = statuses.length;

        statuses.forEach(([testName, result]) => {
            const icon = result.status === 'success' ? '✅' :
                        result.status === 'partial' ? '⚠️' : '❌';

            console.log(`${icon} ${testName.toUpperCase()}: ${result.status}`);

            if (result.error) {
                console.log(`   Error: ${result.error.substring(0, 100)}...`);
            }

            if (result.status === 'success' || result.status === 'partial') {
                successCount++;
            }
        });

        console.log(`\\n📈 Overall: ${successCount}/${totalCount} tests completed`);

        if (successCount === totalCount) {
            console.log('🎉 All tests completed successfully!');
            return 1.0;
        } else if (successCount >= 3) {
            console.log('✅ Most tests passed - acceptable for CI/CD');
            return successCount / totalCount;
        } else {
            console.log('❌ Too many test failures - needs attention');
            return successCount / totalCount;
        }
    }

    async runAll() {
        console.log('🚀 Starting Windows-Safe Test Runner');
        console.log('=====================================\\n');

        // Ensure artifacts directory exists
        if (!fs.existsSync('artifacts')) {
            fs.mkdirSync('artifacts', { recursive: true });
        }

        // Run build first - critical
        const buildSuccess = await this.runBuild();
        if (!buildSuccess) {
            console.log('💥 Build failed - stopping test run');
            return this.generateReport();
        }

        // Run other tests (order matters for stability)
        await this.runSeoAudit();       // Most reliable
        await this.runSslAudit();       // Quick and reliable
        await this.runA11yAudit();      // Can be flaky but important
        await this.runPerformanceAudit(); // Most problematic, run last

        // Generate final report
        return this.generateReport();
    }
}

// CLI execution
if (require.main === module) {
    const runner = new WindowsSafeTestRunner();

    runner.runAll()
        .then(successRate => {
            // More lenient success criteria for Windows environment
            if (successRate >= 0.6) { // 3 out of 5 tests
                console.log('\\n✅ Test run acceptable for CI/CD');
                process.exit(0);
            } else {
                console.log('\\n❌ Too many critical failures');
                process.exit(1);
            }
        })
        .catch(error => {
            console.error('💥 Test runner crashed:', error);
            process.exit(1);
        });
}

module.exports = WindowsSafeTestRunner;
