#!/usr/bin/env node

/**
 * Comprehensive SEO Validation Report
 * Provides detailed location-specific feedback for all SEO issues
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
 * Validate HTML file and provide detailed feedback
 */
function validateHtmlFile(filePath) {
    const issues = [];
    const relativePath = path.relative(process.cwd(), filePath);
    const slug = path.basename(filePath);
    const article = articleLookup[slug];

    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const $ = cheerio.load(content);

        // Article context for reporting
        const context = article ?
            `[Article: ${slug}, ID: ${article.id}, Source: ${article.slug}]` :
            `[File: ${slug}, No article data]`;

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
 * Generate comprehensive report
 */
function generateReport() {
    console.log('🔍 COMPREHENSIVE SEO VALIDATION REPORT');
    console.log('=====================================\n');

    const docsPath = path.join(__dirname, '../docs');
    const allIssues = [];
    const summary = {
        files: 0,
        errors: 0,
        warnings: 0,
        info: 0,
        filesWithIssues: 0
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
    summary.files = htmlFiles.length;

    console.log(`📊 Analyzing ${htmlFiles.length} HTML files...\n`);

    // Process each file
    for (const filePath of htmlFiles) {
        const issues = validateHtmlFile(filePath);

        if (issues.length > 0) {
            summary.filesWithIssues++;
            allIssues.push({
                file: path.relative(process.cwd(), filePath),
                issues: issues
            });

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

    // Display results
    if (allIssues.length === 0) {
        console.log('🎉 EXCELLENT! No SEO issues found!');
        return;
    }

    console.log('📋 SUMMARY:');
    console.log(`• Total files: ${summary.files}`);
    console.log(`• Files with issues: ${summary.filesWithIssues}`);
    console.log(`• Files without issues: ${summary.files - summary.filesWithIssues}`);
    console.log(`• Total errors: ${summary.errors}`);
    console.log(`• Total warnings: ${summary.warnings}`);
    console.log(`• Total info items: ${summary.info}\n`);

    // Display detailed issues
    console.log('🔍 DETAILED ISSUES:\n');

    allIssues.forEach(fileResult => {
        console.log(`📄 ${fileResult.file}:`);

        fileResult.issues.forEach(issue => {
            const icon = issue.severity === 'ERROR' ? '❌' :
                        issue.severity === 'WARNING' ? '⚠️' : 'ℹ️';

            console.log(`  ${icon} ${issue.severity}: ${issue.issue}`);
            console.log(`     Location: ${issue.location}`);
            console.log(`     Context: ${issue.context}`);
            if (issue.content) {
                console.log(`     Content: ${issue.content}`);
            }
            console.log(`     Solution: ${issue.suggestion}\n`);
        });
    });

    // Provide actionable recommendations
    console.log('💡 NEXT STEPS:');
    if (summary.errors > 0) {
        console.log('1. Fix ERROR items first - these are missing essential SEO elements');
    }
    if (summary.warnings > 0) {
        console.log('2. Address WARNING items - these affect SEO performance');
    }
    if (summary.info > 0) {
        console.log('3. Consider INFO items - these improve social media sharing');
    }
    console.log('4. Run this report again after fixes to verify improvements\n');
}

// Run the report
generateReport();
