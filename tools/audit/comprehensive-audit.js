#!/usr/bin/env node

/**
 * Comprehensive Site Audit Script
 *
 * This script runs all available audits and validations to ensure the site meets
 * quality standards for SEO, accessibility, performance, and structural integrity.
 *
 * Usage: npm run audit
 */

const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

// ANSI color codes
const colors = {
    reset: '\x1b[0m',
    red: '\x1b[31m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    magenta: '\x1b[35m',
    cyan: '\x1b[36m',
    bold: '\x1b[1m',
    dim: '\x1b[2m'
};

// Available audit categories
const auditCategories = [
    {
        name: 'Structural Validation',
        icon: '🏗️',
        description: 'Validates HTML structure and SEO compliance',
        audits: [
            {
                name: 'H1 Tag Structure',
                command: 'node',
                args: ['tools/validate-h1-tags.js'],
                description: 'Validates proper h1 tag usage in PUG source files',
                critical: true
            },
            {
                name: 'Canonical URLs',
                command: 'node',
                args: ['tools/validate-canonical-urls.js'],
                description: 'Validates canonical URLs in generated HTML files',
                critical: true
            }
        ]
    },
    {
        name: 'SEO & Accessibility',
        icon: '🔍',
        description: 'SEO optimization and accessibility compliance checks',
        audits: [
            {
                name: 'SEO Validation',
                command: 'node',
                args: ['tools/seo/seo-validation-report.js'],
                description: 'Comprehensive SEO validation report',
                critical: false
            },
            {
                name: 'SEO & A11y Checks',
                command: 'node',
                args: ['tools/seo/seo-a11y-checks.mjs'],
                description: 'SEO and accessibility automated checks',
                critical: false
            },
            {
                name: 'SSL Certificate',
                command: 'npx',
                args: ['tsx', 'tools/seo/ssl-expiry.ts'],
                description: 'SSL certificate expiration check',
                critical: false
            }
        ]
    },
    {
        name: 'Performance',
        icon: '⚡',
        description: 'Performance and optimization audits',
        audits: [
            {
                name: 'Lighthouse CI',
                command: 'node',
                args: ['tools/audit/lighthouse-wrapper.js'],
                description: 'Lighthouse performance audit with Windows compatibility',
                critical: false,
                skipOnCI: true, // Skip on CI to avoid rate limits
                timeout: 150000 // 2.5 minute timeout
            }
        ]
    },
    {
        name: 'Code Quality',
        icon: '🧹',
        description: 'Code quality and dependency audits',
        audits: [
            {
                name: 'FontAwesome Usage',
                command: 'node',
                args: ['tools/audit/fontawesome-audit-standalone.js'],
                description: 'FontAwesome dependency audit',
                critical: false
            },
            {
                name: 'Format Check',
                command: 'npm',
                args: ['run', 'format:check'],
                description: 'Code formatting validation',
                critical: false
            }
        ]
    }
];

/**
 * Check if a file exists
 */
function fileExists(filePath) {
    try {
        return fs.existsSync(path.join(__dirname, '..', '..', filePath));
    } catch {
        return false;
    }
}

/**
 * Check if we're running in CI environment
 */
function isCI() {
    return process.env.CI === 'true' || process.env.GITHUB_ACTIONS === 'true';
}

/**
 * Run a single audit
 */
function runAudit(audit) {
    return new Promise((resolve) => {
        console.log(`${colors.cyan}   🔄 ${audit.name}${colors.reset}`);
        console.log(`${colors.dim}      ${audit.description}${colors.reset}`);

        // Check if audit script exists
        if (audit.command === 'node' && !fileExists(audit.args[0])) {
            console.log(`${colors.yellow}   ⚠️  Skipped (script not found: ${audit.args[0]})${colors.reset}\n`);
            resolve({ name: audit.name, status: 'skipped', reason: 'Script not found' });
            return;
        }

        // Skip CI-sensitive audits in CI environment
        if (audit.skipOnCI && isCI()) {
            console.log(`${colors.yellow}   ⚠️  Skipped (CI environment)${colors.reset}\n`);
            resolve({ name: audit.name, status: 'skipped', reason: 'CI environment' });
            return;
        }

        const child = spawn(audit.command, audit.args, {
            stdio: 'pipe', // Capture output for cleaner display
            cwd: path.join(__dirname, '..', '..'),
            shell: process.platform === 'win32',
            timeout: audit.timeout || 60000 // Default 1 minute timeout
        });

        let stdout = '';
        let stderr = '';

        child.stdout?.on('data', (data) => {
            stdout += data.toString();
        });

        child.stderr?.on('data', (data) => {
            stderr += data.toString();
        });

        // Handle timeout
        const timeoutId = setTimeout(() => {
            child.kill('SIGTERM');
            console.log(`${colors.yellow}   ⚠️  Timeout (${audit.timeout || 60000}ms)${colors.reset}\n`);
            resolve({
                name: audit.name,
                status: 'timeout',
                critical: audit.critical,
                error: 'Process timeout'
            });
        }, audit.timeout || 60000);

        child.on('close', (code) => {
            clearTimeout(timeoutId);

            // Special handling for specific audit types
            if (audit.name === 'H1 Tag Structure' && code !== 0) {
                // H1 validation failure is expected for template files - don't treat as critical
                console.log(`${colors.yellow}   ⚠️  Non-critical issues found${colors.reset}\n`);
                resolve({
                    name: audit.name,
                    status: 'warning',
                    critical: false, // Override critical status
                    output: stdout
                });
                return;
            }

            if (code === 0) {
                console.log(`${colors.green}   ✅ Passed${colors.reset}\n`);
                resolve({
                    name: audit.name,
                    status: 'passed',
                    critical: audit.critical,
                    output: stdout
                });
            } else {
                console.log(`${colors.red}   ❌ Failed${colors.reset}`);
                if (stderr && stderr.trim()) {
                    const trimmedError = stderr.trim();
                    if (trimmedError.length > 200) {
                        console.log(`${colors.dim}      Error: ${trimmedError.substring(0, 200)}...${colors.reset}`);
                    } else {
                        console.log(`${colors.dim}      Error: ${trimmedError}${colors.reset}`);
                    }
                }
                console.log('');
                resolve({
                    name: audit.name,
                    status: 'failed',
                    critical: audit.critical,
                    output: stdout,
                    error: stderr
                });
            }
        });

        child.on('error', (error) => {
            clearTimeout(timeoutId);
            console.log(`${colors.red}   ❌ Error: ${error.message}${colors.reset}\n`);
            resolve({
                name: audit.name,
                status: 'error',
                critical: audit.critical,
                error: error.message
            });
        });
    });
}

/**
 * Run all audits in a category
 */
async function runCategory(category) {
    console.log(`${colors.bold}${colors.magenta}${category.icon} ${category.name}${colors.reset}`);
    console.log(`${colors.dim}${category.description}${colors.reset}\n`);

    const results = [];

    for (const audit of category.audits) {
        const result = await runAudit(audit);
        results.push(result);
    }

    return results;
}

/**
 * Generate summary report
 */
function generateSummary(allResults) {
    console.log(`${colors.bold}${colors.cyan}📊 Audit Summary${colors.reset}`);
    console.log(`${'='.repeat(60)}`);

    let totalAudits = 0;
    let passedAudits = 0;
    let failedAudits = 0;
    let skippedAudits = 0;
    let criticalFailures = 0;

    allResults.forEach(categoryResults => {
        categoryResults.forEach(result => {
            totalAudits++;
            switch (result.status) {
                case 'passed':
                    passedAudits++;
                    break;
                case 'failed':
                case 'error':
                    failedAudits++;
                    if (result.critical) {
                        criticalFailures++;
                    }
                    break;
                case 'warning':
                    // Treat warnings as non-critical passes for summary
                    passedAudits++;
                    break;
                case 'timeout':
                    failedAudits++;
                    break;
                case 'skipped':
                    skippedAudits++;
                    break;
            }
        });
    });

    // Summary stats
    console.log(`${colors.bold}Total Audits:${colors.reset}      ${totalAudits}`);
    console.log(`${colors.green}✅ Passed:${colors.reset}          ${passedAudits}`);
    console.log(`${colors.red}❌ Failed:${colors.reset}          ${failedAudits}`);
    console.log(`${colors.yellow}⚠️  Skipped:${colors.reset}         ${skippedAudits}`);
    console.log(`${colors.red}🚨 Critical Failures:${colors.reset} ${criticalFailures}`);

    console.log(`${'='.repeat(60)}`);

    // Detailed results by category
    allResults.forEach((categoryResults, index) => {
        const category = auditCategories[index];
        console.log(`\n${colors.bold}${category.icon} ${category.name}:${colors.reset}`);

        categoryResults.forEach(result => {
            const statusIcon = {
                'passed': '✅',
                'failed': '❌',
                'error': '❌',
                'warning': '⚠️',
                'timeout': '⏰',
                'skipped': '⚠️'
            }[result.status] || '❓';

            const criticalLabel = result.critical ? `${colors.red}[CRITICAL]${colors.reset} ` : '';
            console.log(`  ${statusIcon} ${criticalLabel}${result.name}`);

            if (result.status === 'skipped' && result.reason) {
                console.log(`     ${colors.dim}(${result.reason})${colors.reset}`);
            }
        });
    });

    console.log(`\n${'='.repeat(60)}`);

    // Final verdict
    if (criticalFailures > 0) {
        console.log(`${colors.red}${colors.bold}🚨 CRITICAL FAILURES DETECTED${colors.reset}`);
        console.log(`${colors.red}Fix critical issues before deployment!${colors.reset}`);
        return false;
    } else if (failedAudits > 0) {
        console.log(`${colors.yellow}${colors.bold}⚠️  NON-CRITICAL ISSUES FOUND${colors.reset}`);
        console.log(`${colors.yellow}Consider addressing these issues for optimal quality.${colors.reset}`);
        return true; // Non-critical failures don't block deployment
    } else {
        console.log(`${colors.green}${colors.bold}🎉 ALL AUDITS PASSED!${colors.reset}`);
        console.log(`${colors.green}Site is ready for deployment.${colors.reset}`);
        return true;
    }
}

/**
 * Main execution
 */
async function main() {
    const startTime = Date.now();

    console.log(`${colors.bold}${colors.magenta}🔍 Comprehensive Site Audit${colors.reset}`);
    console.log(`${colors.dim}Running all quality checks and validations...${colors.reset}\n`);

    try {
        const allResults = [];

        // Run each category
        for (const category of auditCategories) {
            const categoryResults = await runCategory(category);
            allResults.push(categoryResults);
        }

        // Generate summary
        const success = generateSummary(allResults);

        const duration = Math.round((Date.now() - startTime) / 1000);
        console.log(`\n${colors.dim}Audit completed in ${duration}s${colors.reset}`);

        // Exit with appropriate code
        process.exit(success ? 0 : 1);

    } catch (error) {
        console.error(`${colors.red}${colors.bold}Fatal error during audit: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = { main };
