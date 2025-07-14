#!/usr/bin/env node

/**
 * Comprehensive SEO Validation Report
 * Provides detailed location-specific feedback for all SEO issues
 * Maps HTML files back to their PUG source files for easier fixing
 */

const fs = require('fs');
const path = require('path');
const cheerio = require('cheerio');

// Load articles data
const articlesPath = path.join(__dirname, '../src/articles.json');
const articles = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

// Create articles lookup by slug
const articleLookup = {};
articles.forEach(article => {
    articleLookup[article.slug] = article;
});

/**
 * Map HTML file path to corresponding PUG source file
 */
function mapHtmlToPugFile(htmlFilePath) {
    const relativePath = path.relative(path.join(__dirname, '../docs'), htmlFilePath);
    const htmlFileName = path.basename(htmlFilePath, '.html');

    // Handle root level files
    if (!relativePath.includes('/')) {
        const pugPath = path.join(__dirname, '../src/pug', htmlFileName + '.pug');
        if (fs.existsSync(pugPath)) {
            return path.relative(process.cwd(), pugPath);
        }
    }

    // Handle files in subdirectories (like articles/)
    const pugPath = path.join(__dirname, '../src/pug', relativePath.replace('.html', '.pug'));
    if (fs.existsSync(pugPath)) {
        return path.relative(process.cwd(), pugPath);
    }

    // Fallback: return original HTML path if PUG not found
    return path.relative(process.cwd(), htmlFilePath) + ' (PUG source not found)';
}

/**
 * Validate HTML file and provide detailed feedback
 */
function validateHtmlFile(filePath) {
    const issues = [];
    const pugFilePath = mapHtmlToPugFile(filePath);
    const htmlFileName = path.basename(filePath, '.html');
    const article = articleLookup[htmlFileName];

    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const $ = cheerio.load(content);

        // Article context for reporting - now includes PUG source
        const context = article ?
            `[Article: ${htmlFileName}, ID: ${article.id}, PUG Source: ${pugFilePath}]` :
            `[File: ${htmlFileName}, PUG Source: ${pugFilePath}]`;

        // Validate title
        const title = $('title').text().trim();
        if (!title) {
            issues.push({
                severity: 'ERROR',
                location: '<head><title>',
                issue: 'Missing title tag',
                context: context,
                suggestion: 'Add a <title> tag in the <head> section'
            });
        } else {
            if (title.length < 30) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><title>',
                    issue: `Title too short (${title.length} chars, should be ≥30)`,
                    context: context,
                    content: title.substring(0, 100),
                    suggestion: 'Expand title with relevant keywords while staying under 60 characters'
                });
            } else if (title.length > 60) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><title>',
                    issue: `Title too long (${title.length} chars, should be ≤60)`,
                    context: context,
                    content: title.substring(0, 100),
                    suggestion: 'Shorten title while keeping important keywords'
                });
            }
        }

        // Validate meta description
        const description = $('meta[name="description"]').attr('content');
        if (!description) {
            issues.push({
                severity: 'ERROR',
                location: '<head><meta name="description">',
                issue: 'Missing meta description',
                context: context,
                suggestion: 'Add <meta name="description" content="..."> in the <head> section'
            });
        } else {
            if (description.length < 120) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><meta name="description">',
                    issue: `Description too short (${description.length} chars, should be ≥120)`,
                    context: context,
                    content: description.substring(0, 100),
                    suggestion: 'Expand description with more relevant details'
                });
            } else if (description.length > 160) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><meta name="description">',
                    issue: `Description too long (${description.length} chars, should be ≤160)`,
                    context: context,
                    content: description.substring(0, 100),
                    suggestion: 'Shorten description while keeping key information'
                });
            }
        }

        // Validate meta keywords
        const keywords = $('meta[name="keywords"]').attr('content');
        if (!keywords) {
            issues.push({
                severity: 'WARNING',
                location: '<head><meta name="keywords">',
                issue: 'Missing meta keywords',
                context: context,
                suggestion: 'Add <meta name="keywords" content="..."> with relevant keywords'
            });
        }

        // Validate canonical URL
        const canonical = $('link[rel="canonical"]').attr('href');
        if (!canonical) {
            issues.push({
                severity: 'WARNING',
                location: '<head><link rel="canonical">',
                issue: 'Missing canonical URL',
                context: context,
                suggestion: 'Add <link rel="canonical" href="..."> to prevent duplicate content issues'
            });
        }

        // Validate H1 tags
        const h1Tags = $('h1');
        if (h1Tags.length === 0) {
            issues.push({
                severity: 'WARNING',
                location: '<body> content',
                issue: 'Missing H1 tag',
                context: context,
                suggestion: 'Add exactly one <h1> tag for the main page heading'
            });
        } else if (h1Tags.length > 1) {
            issues.push({
                severity: 'WARNING',
                location: '<body> content',
                issue: `Multiple H1 tags (${h1Tags.length} found, should be 1)`,
                context: context,
                suggestion: 'Use only one <h1> tag per page, use <h2>-<h6> for subheadings'
            });
        }

        // Validate images without alt text
        const imagesWithoutAlt = $('img:not([alt])');
        if (imagesWithoutAlt.length > 0) {
            const examples = [];
            imagesWithoutAlt.slice(0, 3).each((i, img) => {
                const src = $(img).attr('src') || 'unknown source';
                examples.push(src);
            });

            issues.push({
                severity: 'WARNING',
                location: '<img> tags',
                issue: `Images without alt text (${imagesWithoutAlt.length} found)`,
                context: context,
                content: `Examples: ${examples.join(', ')}${imagesWithoutAlt.length > 3 ? '...' : ''}`,
                suggestion: 'Add alt attributes to all images for accessibility and SEO'
            });
        }

        // Validate Open Graph tags
        const ogTitle = $('meta[property="og:title"]').attr('content');
        const ogDescription = $('meta[property="og:description"]').attr('content');
        const ogImage = $('meta[property="og:image"]').attr('content');

        if (!ogTitle || !ogDescription || !ogImage) {
            const missing = [];
            if (!ogTitle) missing.push('og:title');
            if (!ogDescription) missing.push('og:description');
            if (!ogImage) missing.push('og:image');

            issues.push({
                severity: 'INFO',
                location: '<head> Open Graph meta tags',
                issue: `Missing Open Graph tags: ${missing.join(', ')}`,
                context: context,
                suggestion: 'Add Open Graph meta tags for better social media sharing'
            });
        }

        // Validate Twitter Card tags
        const twitterCard = $('meta[name="twitter:card"]').attr('content');
        const twitterTitle = $('meta[name="twitter:title"]').attr('content');

        if (!twitterCard || !twitterTitle) {
            issues.push({
                severity: 'INFO',
                location: '<head> Twitter Card meta tags',
                issue: 'Missing Twitter Card tags',
                context: context,
                suggestion: 'Add Twitter Card meta tags for better Twitter sharing'
            });
        }

    } catch (error) {
        issues.push({
            severity: 'ERROR',
            location: 'File processing',
            issue: `Error reading file: ${error.message}`,
            context: context,
            suggestion: 'Check file permissions and format'
        });
    }

    return issues;
}

/**
 * Generate comprehensive report grouped by PUG source files
 */
function generateReport() {
    console.log('🔍 COMPREHENSIVE SEO VALIDATION REPORT');
    console.log('=====================================\n');

    const docsPath = path.join(__dirname, '../docs');
    const pugFileIssues = new Map(); // Group issues by PUG file
    const summary = {
        htmlFiles: 0,
        pugFiles: 0,
        errors: 0,
        warnings: 0,
        info: 0,
        pugFilesWithIssues: 0
    };

    // Find all HTML files
    function findHtmlFiles(dir) {
        const files = [];
        const entries = fs.readdirSync(dir);

        for (const entry of entries) {
            const fullPath = path.join(dir, entry);
            const stat = fs.statSync(fullPath);

            if (stat.isDirectory()) {
                files.push(...findHtmlFiles(fullPath));
            } else if (entry.endsWith('.html')) {
                files.push(fullPath);
            }
        }

        return files;
    }

    const htmlFiles = findHtmlFiles(docsPath);
    summary.htmlFiles = htmlFiles.length;

    console.log(`📊 Analyzing ${htmlFiles.length} HTML files, mapping to PUG sources...\n`);

    // Process each file and group by PUG source
    for (const filePath of htmlFiles) {
        const issues = validateHtmlFile(filePath);
        const pugFilePath = mapHtmlToPugFile(filePath);

        if (issues.length > 0) {
            if (!pugFileIssues.has(pugFilePath)) {
                pugFileIssues.set(pugFilePath, []);
            }

            // Add all issues for this HTML file to the PUG file's issue list
            pugFileIssues.get(pugFilePath).push(...issues);

            // Count by severity
            issues.forEach(issue => {
                switch (issue.severity) {
                    case 'ERROR': summary.errors++; break;
                    case 'WARNING': summary.warnings++; break;
                    case 'INFO': summary.info++; break;
                }
            });
        }
    }

    summary.pugFiles = pugFileIssues.size;
    summary.pugFilesWithIssues = pugFileIssues.size;

    // Display results
    if (pugFileIssues.size === 0) {
        console.log('🎉 EXCELLENT! No SEO issues found!');
        return;
    }

    console.log('📋 SUMMARY:');
    console.log(`• HTML files analyzed: ${summary.htmlFiles}`);
    console.log(`• PUG source files with issues: ${summary.pugFilesWithIssues}`);
    console.log(`• Total errors: ${summary.errors}`);
    console.log(`• Total warnings: ${summary.warnings}`);
    console.log(`• Total info items: ${summary.info}\n`);

    // Display detailed issues grouped by PUG file
    console.log('🔍 DETAILED ISSUES BY PUG SOURCE FILE:\n');

    // Sort PUG files for consistent output
    const sortedPugFiles = Array.from(pugFileIssues.keys()).sort();

    sortedPugFiles.forEach(pugFile => {
        const issues = pugFileIssues.get(pugFile);
        const errorCount = issues.filter(i => i.severity === 'ERROR').length;
        const warningCount = issues.filter(i => i.severity === 'WARNING').length;
        const infoCount = issues.filter(i => i.severity === 'INFO').length;

        console.log(`📄 ${pugFile}:`);
        console.log(`   Issues: ${errorCount} errors, ${warningCount} warnings, ${infoCount} info`);
        console.log('   ═══════════════════════════════════════════════════════════════\n');

        // Group issues by severity for better readability
        const errorIssues = issues.filter(i => i.severity === 'ERROR');
        const warningIssues = issues.filter(i => i.severity === 'WARNING');
        const infoIssues = issues.filter(i => i.severity === 'INFO');

        // Display errors first
        if (errorIssues.length > 0) {
            console.log('   ❌ ERRORS (fix these first):');
            errorIssues.forEach(issue => {
                console.log(`      • ${issue.issue}`);
                console.log(`        Location: ${issue.location}`);
                if (issue.content) {
                    console.log(`        Content: ${issue.content}`);
                }
                console.log(`        Solution: ${issue.suggestion}\n`);
            });
        }

        // Display warnings
        if (warningIssues.length > 0) {
            console.log('   ⚠️  WARNINGS (important for SEO):');
            warningIssues.forEach(issue => {
                console.log(`      • ${issue.issue}`);
                console.log(`        Location: ${issue.location}`);
                if (issue.content) {
                    console.log(`        Content: ${issue.content}`);
                }
                console.log(`        Solution: ${issue.suggestion}\n`);
            });
        }

        // Display info items
        if (infoIssues.length > 0) {
            console.log('   ℹ️  INFO (nice to have):');
            infoIssues.forEach(issue => {
                console.log(`      • ${issue.issue}`);
                console.log(`        Location: ${issue.location}`);
                if (issue.content) {
                    console.log(`        Content: ${issue.content}`);
                }
                console.log(`        Solution: ${issue.suggestion}\n`);
            });
        }

        console.log('   ───────────────────────────────────────────────────────────────\n');
    });

    // Provide actionable recommendations
    console.log('💡 NEXT STEPS:');
    console.log('1. Open the PUG source files listed above to make fixes');
    if (summary.errors > 0) {
        console.log('2. Fix ERROR items first - these are missing essential SEO elements');
    }
    if (summary.warnings > 0) {
        console.log('3. Address WARNING items - these affect SEO performance');
    }
    if (summary.info > 0) {
        console.log('4. Consider INFO items - these improve social media sharing');
    }
    console.log('5. Run `npm run build:pug` to regenerate HTML after PUG changes');
    console.log('6. Run this report again after fixes to verify improvements\n');
}

// Run the report
generateReport();
