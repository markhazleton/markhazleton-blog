#!/usr/bin/env node

/**
 * Canonical URL Validation Script
 *
 * This script validates canonical URLs in generated HTML files against the articles.json metadata.
 * It helps identify mismatches, broken links, and missing canonical tags after the build process.
 *
 * Usage: node tools/validate-canonical-urls.js
 */

const fs = require('fs');
const path = require('path');
const { JSDOM } = require('jsdom');

// Configuration
const config = {
    articlesJsonPath: path.join(__dirname, '..', 'docs', 'articles.json'),
    docsDir: path.join(__dirname, '..', 'docs'),
    baseUrl: 'https://markhazleton.com',
    excludePatterns: [
        'articles.html', // Main articles index
        'projects.html', // Main projects index
        'index.html',    // Homepage
        'search.html',   // Search page
        '400.html',      // Error pages
        '404.html'
    ]
};

// ANSI color codes for console output
const colors = {
    reset: '\x1b[0m',
    red: '\x1b[31m',
    green: '\x1b[32m',
    yellow: '\x1b[33m',
    blue: '\x1b[34m',
    magenta: '\x1b[35m',
    cyan: '\x1b[36m',
    white: '\x1b[37m',
    bold: '\x1b[1m'
};

// Results tracking
const results = {
    totalArticles: 0,
    validatedFiles: 0,
    errors: [],
    warnings: [],
    mismatches: [],
    missingFiles: [],
    missingCanonical: [],
    validCanonicals: 0
};

/**
 * Load and parse articles.json
 */
function loadArticlesData() {
    try {
        const articlesContent = fs.readFileSync(config.articlesJsonPath, 'utf8');
        return JSON.parse(articlesContent);
    } catch (error) {
        console.error(`${colors.red}Error loading articles.json: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

/**
 * Extract canonical URL from HTML content
 */
function extractCanonicalUrl(htmlContent) {
    try {
        const dom = new JSDOM(htmlContent);
        const document = dom.window.document;

        // Look for canonical link tag
        const canonicalLink = document.querySelector('link[rel="canonical"]');
        if (canonicalLink) {
            return canonicalLink.getAttribute('href');
        }

        return null;
    } catch (error) {
        return null;
    }
}

/**
 * Extract page title from HTML content
 */
function extractPageTitle(htmlContent) {
    try {
        const dom = new JSDOM(htmlContent);
        const document = dom.window.document;

        const titleElement = document.querySelector('title');
        if (titleElement) {
            return titleElement.textContent.trim();
        }

        return null;
    } catch (error) {
        return null;
    }
}

/**
 * Extract meta description from HTML content
 */
function extractMetaDescription(htmlContent) {
    try {
        const dom = new JSDOM(htmlContent);
        const document = dom.window.document;

        const metaDesc = document.querySelector('meta[name="description"]');
        if (metaDesc) {
            return metaDesc.getAttribute('content');
        }

        return null;
    } catch (error) {
        return null;
    }
}

/**
 * Count h1 tags in HTML content
 */
function countH1Tags(htmlContent) {
    try {
        const dom = new JSDOM(htmlContent);
        const document = dom.window.document;

        const h1Tags = document.querySelectorAll('h1');
        return {
            count: h1Tags.length,
            tags: Array.from(h1Tags).map(tag => ({
                text: tag.textContent.trim(),
                classes: tag.className,
                id: tag.id
            }))
        };
    } catch (error) {
        return { count: 0, tags: [] };
    }
}

/**
 * Validate a single article
 */
function validateArticle(article) {
    const validation = {
        article: article,
        filePath: null,
        exists: false,
        canonicalInHtml: null,
        canonicalExpected: null,
        canonicalMatch: false,
        titleInHtml: null,
        metaDescInHtml: null,
        h1Analysis: null,
        issues: [],
        warnings: []
    };

    // Determine file path
    if (article.slug) {
        // Handle directory-style slugs ending with '/'
        if (article.slug.endsWith('/')) {
            validation.filePath = path.join(config.docsDir, article.slug, 'index.html');
        } else {
            validation.filePath = path.join(config.docsDir, article.slug);
        }
        validation.canonicalExpected = article.seo?.canonical || `${config.baseUrl}/${article.slug}`;
    } else {
        validation.issues.push('No slug defined in articles.json');
        return validation;
    }

    // Check if file exists
    if (!fs.existsSync(validation.filePath)) {
        validation.issues.push(`HTML file not found: ${validation.filePath}`);
        results.missingFiles.push(validation);
        return validation;
    }

    validation.exists = true;

    try {
        // Read HTML content
        const htmlContent = fs.readFileSync(validation.filePath, 'utf8');

        // Extract canonical URL
        validation.canonicalInHtml = extractCanonicalUrl(htmlContent);

        // Extract other metadata
        validation.titleInHtml = extractPageTitle(htmlContent);
        validation.metaDescInHtml = extractMetaDescription(htmlContent);
        validation.h1Analysis = countH1Tags(htmlContent);

        // Validate canonical URL
        if (!validation.canonicalInHtml) {
            validation.issues.push('No canonical URL found in HTML');
            results.missingCanonical.push(validation);
        } else if (validation.canonicalInHtml !== validation.canonicalExpected) {
            validation.issues.push(`Canonical URL mismatch: Expected "${validation.canonicalExpected}", Found "${validation.canonicalInHtml}"`);
            results.mismatches.push(validation);
        } else {
            validation.canonicalMatch = true;
            results.validCanonicals++;
        }

        // Additional validations
        if (!validation.titleInHtml) {
            validation.warnings.push('No title tag found');
        } else if (validation.titleInHtml.length > 60) {
            validation.warnings.push(`Title too long (${validation.titleInHtml.length} chars): "${validation.titleInHtml.substring(0, 50)}..."`);
        }

        if (!validation.metaDescInHtml) {
            validation.warnings.push('No meta description found');
        } else if (validation.metaDescInHtml.length > 160) {
            validation.warnings.push(`Meta description too long (${validation.metaDescInHtml.length} chars)`);
        }

        // H1 validation
        if (validation.h1Analysis.count === 0) {
            validation.warnings.push('No h1 tag found');
        } else if (validation.h1Analysis.count > 1) {
            validation.issues.push(`Multiple h1 tags found (${validation.h1Analysis.count})`);
        }

        results.validatedFiles++;

    } catch (error) {
        validation.issues.push(`Error reading/parsing HTML: ${error.message}`);
    }

    return validation;
}

/**
 * Generate detailed report
 */
function generateReport(validations) {
    console.log(`${colors.bold}${colors.cyan}=== Canonical URL Validation Report ===${colors.reset}\n`);

    // Summary statistics
    console.log(`${colors.bold}Summary:${colors.reset}`);
    console.log(`  Total articles in articles.json: ${results.totalArticles}`);
    console.log(`  HTML files validated: ${results.validatedFiles}`);
    console.log(`  Valid canonical URLs: ${colors.green}${results.validCanonicals}${colors.reset}`);
    console.log(`  Missing HTML files: ${colors.red}${results.missingFiles.length}${colors.reset}`);
    console.log(`  Missing canonical tags: ${colors.red}${results.missingCanonical.length}${colors.reset}`);
    console.log(`  Canonical URL mismatches: ${colors.red}${results.mismatches.length}${colors.reset}\n`);

    // Missing files
    if (results.missingFiles.length > 0) {
        console.log(`${colors.bold}${colors.red}Missing HTML Files (${results.missingFiles.length}):${colors.reset}`);
        results.missingFiles.forEach(validation => {
            console.log(`  ${colors.red}❌${colors.reset} ${validation.article.slug} - ${validation.article.name || 'Unknown'}`);
        });
        console.log('');
    }

    // Missing canonical tags
    if (results.missingCanonical.length > 0) {
        console.log(`${colors.bold}${colors.red}Missing Canonical Tags (${results.missingCanonical.length}):${colors.reset}`);
        results.missingCanonical.forEach(validation => {
            console.log(`  ${colors.red}❌${colors.reset} ${validation.article.slug}`);
        });
        console.log('');
    }

    // Canonical URL mismatches
    if (results.mismatches.length > 0) {
        console.log(`${colors.bold}${colors.red}Canonical URL Mismatches (${results.mismatches.length}):${colors.reset}`);
        results.mismatches.forEach(validation => {
            console.log(`\n${colors.yellow}${validation.article.slug}${colors.reset}`);
            console.log(`  Expected: ${colors.green}${validation.canonicalExpected}${colors.reset}`);
            console.log(`  Found:    ${colors.red}${validation.canonicalInHtml}${colors.reset}`);
        });
        console.log('');
    }

    // H1 tag issues
    const h1Issues = validations.filter(v => v.h1Analysis && v.h1Analysis.count !== 1);
    if (h1Issues.length > 0) {
        console.log(`${colors.bold}${colors.yellow}H1 Tag Issues (${h1Issues.length}):${colors.reset}`);
        h1Issues.forEach(validation => {
            const count = validation.h1Analysis.count;
            const status = count === 0 ? 'No h1 tags' : `${count} h1 tags`;
            console.log(`  ${colors.yellow}⚠️${colors.reset} ${validation.article.slug} - ${status}`);
            if (count > 1) {
                validation.h1Analysis.tags.forEach((tag, index) => {
                    console.log(`    ${index + 1}. "${tag.text.substring(0, 50)}${tag.text.length > 50 ? '...' : ''}"`);
                });
            }
        });
        console.log('');
    }

    // SEO warnings
    const seoWarnings = validations.filter(v => v.warnings.length > 0);
    if (seoWarnings.length > 0) {
        console.log(`${colors.bold}${colors.blue}SEO Warnings (${seoWarnings.length}):${colors.reset}`);
        seoWarnings.forEach(validation => {
            if (validation.warnings.length > 0) {
                console.log(`\n${colors.cyan}${validation.article.slug}${colors.reset}`);
                validation.warnings.forEach(warning => {
                    console.log(`  ${colors.blue}ℹ️${colors.reset} ${warning}`);
                });
            }
        });
        console.log('');
    }

    // Success summary
    const successfulValidations = validations.filter(v =>
        v.canonicalMatch && v.h1Analysis && v.h1Analysis.count === 1 && v.warnings.length === 0
    );

    console.log(`${colors.bold}${colors.green}Perfect Articles (${successfulValidations.length}):${colors.reset}`);
    console.log(`  Articles with correct canonical URLs, single h1 tag, and no SEO warnings\n`);

    // Recommendations
    console.log(`${colors.bold}${colors.magenta}Recommendations:${colors.reset}`);

    if (results.mismatches.length > 0) {
        console.log(`  ${colors.yellow}•${colors.reset} Fix canonical URL mismatches in the build process or articles.json`);
    }

    if (results.missingCanonical.length > 0) {
        console.log(`  ${colors.yellow}•${colors.reset} Add canonical link tags to templates/layouts`);
    }

    if (h1Issues.length > 0) {
        console.log(`  ${colors.yellow}•${colors.reset} Fix h1 tag issues - ensure exactly one h1 per page`);
    }

    console.log(`  ${colors.yellow}•${colors.reset} Run this validation after each build to catch issues early`);
}

/**
 * Main execution
 */
function main() {
    console.log(`${colors.bold}Validating canonical URLs in generated HTML files...${colors.reset}\n`);

    try {
        // Check if jsdom is available
        try {
            require.resolve('jsdom');
        } catch (error) {
            console.error(`${colors.red}Error: jsdom is required but not installed.${colors.reset}`);
            console.log(`Please install it by running: ${colors.cyan}npm install --save-dev jsdom${colors.reset}`);
            process.exit(1);
        }

        // Load articles data
        const articlesData = loadArticlesData();

        // Filter out non-article entries and excluded patterns
        const articles = articlesData.filter(article => {
            if (!article.slug) return false;

            // Skip excluded patterns
            const isExcluded = config.excludePatterns.some(pattern =>
                article.slug.includes(pattern)
            );

            return !isExcluded;
        });

        results.totalArticles = articles.length;

        console.log(`Found ${articles.length} articles to validate...\n`);

        // Validate each article
        const validations = articles.map(article => validateArticle(article));

        // Generate report
        generateReport(validations);

        // Exit with error code if issues found
        const hasErrors = results.missingFiles.length > 0 ||
                         results.missingCanonical.length > 0 ||
                         results.mismatches.length > 0;

        if (hasErrors) {
            console.log(`${colors.red}\n❌ Validation failed with ${results.missingFiles.length + results.missingCanonical.length + results.mismatches.length} errors${colors.reset}`);
            process.exit(1);
        } else {
            console.log(`${colors.green}\n✅ All canonical URLs validated successfully!${colors.reset}`);
            process.exit(0);
        }

    } catch (error) {
        console.error(`${colors.red}Error: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = { main, validateArticle, extractCanonicalUrl };
