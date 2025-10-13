#!/usr/bin/env node

/**
 * Comprehensive Post-Build Validation Script
 *
 * This script runs all post-build validations to ensure the generated site meets
 * SEO standards and doesn't have structural issues.
 *
 * Usage: npm run validate:all
 */

const { spawn } = require('child_process');
const path = require('path');

// ANSI color codes
const colors = {
    reset: '\x1b[0m',
    red: '\x1b[31m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    magenta: '\x1b[35m',
    cyan: '\x1b[36m',
    bold: '\x1b[1m'
};

// Validation scripts to run
const validations = [
    {
        name: 'H1 Tag Structure',
        script: 'tools/validate-h1-tags.js',
        description: 'Validates proper h1 tag usage in PUG source files'
    },
    {
        name: 'Canonical URLs',
        script: 'tools/validate-canonical-urls.js',
        description: 'Validates canonical URLs in generated HTML files'
    }
];

/**
 * Run a validation script
 */
function runValidation(validation) {
    return new Promise((resolve, reject) => {
        console.log(`${colors.cyan}🔍 Running ${validation.name} validation...${colors.reset}`);
        console.log(`   ${validation.description}\n`);

        const child = spawn('node', [validation.script], {
            stdio: 'inherit',
            cwd: path.join(__dirname, '..')
        });

        child.on('close', (code) => {
            if (code === 0) {
                console.log(`${colors.green}✅ ${validation.name} validation passed${colors.reset}\n`);
                resolve({ name: validation.name, passed: true });
            } else {
                console.log(`${colors.red}❌ ${validation.name} validation failed${colors.reset}\n`);
                resolve({ name: validation.name, passed: false });
            }
        });

        child.on('error', (error) => {
            console.error(`${colors.red}Error running ${validation.name}: ${error.message}${colors.reset}`);
            reject(error);
        });
    });
}

/**
 * Main execution
 */
async function main() {
    console.log(`${colors.bold}${colors.magenta}🚀 Running Post-Build Validations${colors.reset}\n`);
    console.log(`This script validates the generated site for SEO and structural issues.\n`);

    const results = [];
    let allPassed = true;

    try {
        // Run each validation
        for (const validation of validations) {
            const result = await runValidation(validation);
            results.push(result);

            if (!result.passed) {
                allPassed = false;
            }
        }

        // Summary report
        console.log(`${colors.bold}${colors.cyan}📊 Validation Summary${colors.reset}`);
        console.log(`${'='.repeat(50)}`);

        results.forEach(result => {
            const status = result.passed ?
                `${colors.green}✅ PASSED${colors.reset}` :
                `${colors.red}❌ FAILED${colors.reset}`;
            console.log(`${result.name.padEnd(25)} ${status}`);
        });

        console.log(`${'='.repeat(50)}`);

        if (allPassed) {
            console.log(`${colors.green}${colors.bold}🎉 All validations passed! Site is ready for deployment.${colors.reset}`);
            process.exit(0);
        } else {
            const failedCount = results.filter(r => !r.passed).length;
            console.log(`${colors.red}${colors.bold}💥 ${failedCount} validation(s) failed. Please fix issues before deployment.${colors.reset}`);
            process.exit(1);
        }

    } catch (error) {
        console.error(`${colors.red}Fatal error during validation: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = { main };
