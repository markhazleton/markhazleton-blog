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
const articlesPath = path.join(__dirname, '../../src/articles.json');
const articles = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

// Load projects data
const projectsPath = path.join(__dirname, '../../src/projects.json');
const projects = fs.existsSync(projectsPath) ? JSON.parse(fs.readFileSync(projectsPath, 'utf8')) : [];

// Create articles lookup by slug
const articleLookup = {};
articles.forEach(article => {
    articleLookup[article.slug] = article;
});

// Create projects lookup by slug
const projectLookup = {};
projects.forEach(project => {
    if (project.slug) {
        projectLookup[project.slug] = project;
    }
});

/**
 * Map HTML file path to corresponding PUG source file with enhanced project detection
 */
function mapHtmlToPugFile(htmlFilePath) {
    const relativePath = path.relative(path.join(__dirname, '../../docs'), htmlFilePath);
    const htmlFileName = path.basename(htmlFilePath, '.html');

    // Normalize path separators for cross-platform compatibility
    const normalizedPath = relativePath.replace(/\\/g, '/');

    // Handle project detail pages - check if this is a project file
    if (normalizedPath.startsWith('projects/') && normalizedPath.endsWith('/index.html')) {
        const projectSlug = normalizedPath.split('/')[1];
        return {
            pugFile: 'src/pug/projects/templates/project-detail.pug',
            type: 'project',
            slug: projectSlug,
            displayPath: `src/pug/projects/templates/project-detail.pug (project: ${projectSlug})`
        };
    }

    // Handle article pages
    if (normalizedPath.startsWith('articles/') && normalizedPath.endsWith('.html')) {
        const articleSlug = path.basename(normalizedPath, '.html');
        const pugPath = path.join(__dirname, '../../src/pug/articles', articleSlug + '.pug');
        if (fs.existsSync(pugPath)) {
            return {
                pugFile: path.relative(process.cwd(), pugPath),
                type: 'article',
                slug: articleSlug,
                displayPath: path.relative(process.cwd(), pugPath)
            };
        }
    }

    // Handle root level files
    if (!normalizedPath.includes('/')) {
        const pugPath = path.join(__dirname, '../../src/pug', htmlFileName + '.pug');
        if (fs.existsSync(pugPath)) {
            return {
                pugFile: path.relative(process.cwd(), pugPath),
                type: 'page',
                slug: htmlFileName,
                displayPath: path.relative(process.cwd(), pugPath)
            };
        }
    }

    // Handle files in subdirectories (generic)
    const pugPath = path.join(__dirname, '../../src/pug', normalizedPath.replace('.html', '.pug'));
    if (fs.existsSync(pugPath)) {
        return {
            pugFile: path.relative(process.cwd(), pugPath),
            type: 'page',
            slug: htmlFileName,
            displayPath: path.relative(process.cwd(), pugPath)
        };
    }

    // Fallback: return original HTML path if PUG not found
    return {
        pugFile: path.relative(process.cwd(), htmlFilePath) + ' (PUG source not found)',
        type: 'unknown',
        slug: htmlFileName,
        displayPath: path.relative(process.cwd(), htmlFilePath) + ' (PUG source not found)'
    };
}

/**
 * Validate HTML file and provide detailed feedback with enhanced page identification
 */
function validateHtmlFile(filePath) {
    const issues = [];
    const pugMapping = mapHtmlToPugFile(filePath);
    const htmlFileName = path.basename(filePath, '.html');

    // Get context based on page type
    let context = '';
    let pageTitle = '';
    let pageType = pugMapping.type;

    if (pugMapping.type === 'project') {
        const project = projectLookup[pugMapping.slug];
        if (project) {
            pageTitle = project.p || project.name || 'Unknown Project';
            context = `[Project: "${pageTitle}", Slug: ${pugMapping.slug}, Template: ${pugMapping.pugFile}]`;
        } else {
            context = `[Project: ${pugMapping.slug} (not found in projects.json), Template: ${pugMapping.pugFile}]`;
        }
    } else if (pugMapping.type === 'article') {
        const article = articleLookup[pugMapping.slug];
        if (article) {
            pageTitle = article.title || 'Unknown Article';
            context = `[Article: "${pageTitle}", ID: ${article.id}, PUG: ${pugMapping.pugFile}]`;
        } else {
            context = `[Article: ${pugMapping.slug} (not found in articles.json), PUG: ${pugMapping.pugFile}]`;
        }
    } else {
        context = `[Page: ${htmlFileName}, Type: ${pugMapping.type}, PUG: ${pugMapping.pugFile}]`;
    }

    try {
        const content = fs.readFileSync(filePath, 'utf8');
        const $ = cheerio.load(content);

        // Validate title
        const title = $('title').text().trim();
        if (!title) {
            issues.push({
                severity: 'ERROR',
                location: '<head><title>',
                issue: 'Missing title tag',
                context: context,
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
                suggestion: 'Add a <title> tag in the <head> section'
            });
        } else {
            if (title.length < 30) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><title>',
                    issue: `Title too short (${title.length} chars, should be ≥30)`,
                    context: context,
                    pageTitle: pageTitle,
                    pageType: pageType,
                    pugFile: pugMapping.pugFile,
                    content: title.substring(0, 100),
                    actualTitle: title,
                    suggestion: 'Expand title with relevant keywords while staying under 60 characters'
                });
            } else if (title.length > 60) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><title>',
                    issue: `Title too long (${title.length} chars, should be ≤60)`,
                    context: context,
                    pageTitle: pageTitle,
                    pageType: pageType,
                    pugFile: pugMapping.pugFile,
                    content: title.substring(0, 100),
                    actualTitle: title,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
                suggestion: 'Add <meta name="description" content="..."> in the <head> section'
            });
        } else {
            if (description.length < 120) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><meta name="description">',
                    issue: `Description too short (${description.length} chars, should be ≥120)`,
                    context: context,
                    pageTitle: pageTitle,
                    pageType: pageType,
                    pugFile: pugMapping.pugFile,
                    content: description.substring(0, 100),
                    suggestion: 'Expand description with more relevant details'
                });
            } else if (description.length > 160) {
                issues.push({
                    severity: 'WARNING',
                    location: '<head><meta name="description">',
                    issue: `Description too long (${description.length} chars, should be ≤160)`,
                    context: context,
                    pageTitle: pageTitle,
                    pageType: pageType,
                    pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
                suggestion: 'Add exactly one <h1> tag for the main page heading'
            });
        } else if (h1Tags.length > 1) {
            issues.push({
                severity: 'WARNING',
                location: '<body> content',
                issue: `Multiple H1 tags (${h1Tags.length} found, should be 1)`,
                context: context,
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
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
                pageTitle: pageTitle,
                pageType: pageType,
                pugFile: pugMapping.pugFile,
                suggestion: 'Add Twitter Card meta tags for better Twitter sharing'
            });
        }

    } catch (error) {
        issues.push({
            severity: 'ERROR',
            location: 'File processing',
            issue: `Error reading file: ${error.message}`,
            context: context,
            pageTitle: pageTitle,
            pageType: pageType,
            pugFile: pugMapping.pugFile,
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

    const docsPath = path.join(__dirname, '../../docs');
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
        const pugMapping = mapHtmlToPugFile(filePath);

        if (issues.length > 0) {
            // Use the normalized PUG file path as the key
            const pugKey = pugMapping.displayPath;

            if (!pugFileIssues.has(pugKey)) {
                pugFileIssues.set(pugKey, {
                    pugFile: pugMapping.pugFile,
                    type: pugMapping.type,
                    issues: []
                });
            }

            // Add all issues for this HTML file to the PUG file's issue list
            pugFileIssues.get(pugKey).issues.push(...issues);

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

    // Sort PUG files for consistent output, with projects grouped together
    const sortedPugFiles = Array.from(pugFileIssues.keys()).sort((a, b) => {
        const aData = pugFileIssues.get(a);
        const bData = pugFileIssues.get(b);

        // Sort by type first (projects together), then by name
        if (aData.type !== bData.type) {
            const order = { 'project': 1, 'article': 2, 'page': 3, 'unknown': 4 };
            return (order[aData.type] || 5) - (order[bData.type] || 5);
        }
        return a.localeCompare(b);
    });

    sortedPugFiles.forEach(pugFileKey => {
        const pugData = pugFileIssues.get(pugFileKey);
        const issues = pugData.issues;
        const errorCount = issues.filter(i => i.severity === 'ERROR').length;
        const warningCount = issues.filter(i => i.severity === 'WARNING').length;
        const infoCount = issues.filter(i => i.severity === 'INFO').length;

        // Group issues by unique combinations to avoid repetition
        const uniqueIssues = new Map();
        issues.forEach(issue => {
            const key = `${issue.severity}:${issue.location}:${issue.issue}`;
            if (!uniqueIssues.has(key)) {
                uniqueIssues.set(key, {
                    ...issue,
                    pages: [{ pageTitle: issue.pageTitle, actualTitle: issue.actualTitle }]
                });
            } else {
                const existing = uniqueIssues.get(key);
                existing.pages.push({ pageTitle: issue.pageTitle, actualTitle: issue.actualTitle });
            }
        });

        console.log(`📄 ${pugFileKey}:`);
        if (pugData.type === 'project') {
            console.log(`   Type: Project Template (generates multiple project pages)`);
        } else if (pugData.type === 'article') {
            console.log(`   Type: Article`);
        } else {
            console.log(`   Type: ${pugData.type}`);
        }
        console.log(`   Issues: ${errorCount} errors, ${warningCount} warnings, ${infoCount} info`);
        console.log('   ═══════════════════════════════════════════════════════════════\n');

        // Group issues by severity for better readability
        const errorIssues = Array.from(uniqueIssues.values()).filter(i => i.severity === 'ERROR');
        const warningIssues = Array.from(uniqueIssues.values()).filter(i => i.severity === 'WARNING');
        const infoIssues = Array.from(uniqueIssues.values()).filter(i => i.severity === 'INFO');

        // Display errors first
        if (errorIssues.length > 0) {
            console.log('   ❌ ERRORS (fix these first):');
            errorIssues.forEach(issue => {
                console.log(`      • ${issue.issue}`);
                console.log(`        Location: ${issue.location}`);
                if (issue.content) {
                    console.log(`        Content: ${issue.content}`);
                }
                if (issue.pages.length > 1) {
                    console.log(`        Affects ${issue.pages.length} pages: ${issue.pages.map(p => p.pageTitle || 'Unknown').slice(0, 3).join(', ')}${issue.pages.length > 3 ? '...' : ''}`);
                } else if (issue.pages[0].pageTitle) {
                    console.log(`        Page: ${issue.pages[0].pageTitle}`);
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
                if (issue.pages.length > 1) {
                    console.log(`        Affects ${issue.pages.length} pages:`);
                    issue.pages.slice(0, 5).forEach(page => {
                        if (page.actualTitle) {
                            console.log(`          - "${page.pageTitle}": "${page.actualTitle}"`);
                        } else {
                            console.log(`          - ${page.pageTitle || 'Unknown'}`);
                        }
                    });
                    if (issue.pages.length > 5) {
                        console.log(`          ... and ${issue.pages.length - 5} more`);
                    }
                } else if (issue.pages[0].pageTitle) {
                    console.log(`        Page: ${issue.pages[0].pageTitle}`);
                    if (issue.pages[0].actualTitle) {
                        console.log(`        Actual Title: "${issue.pages[0].actualTitle}"`);
                    }
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
                if (issue.pages.length > 1) {
                    console.log(`        Affects ${issue.pages.length} pages: ${issue.pages.map(p => p.pageTitle || 'Unknown').slice(0, 3).join(', ')}${issue.pages.length > 3 ? '...' : ''}`);
                } else if (issue.pages[0].pageTitle) {
                    console.log(`        Page: ${issue.pages[0].pageTitle}`);
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

        // Check if project template issues exist
        const hasProjectIssues = Array.from(pugFileIssues.values()).some(data => data.type === 'project');
        if (hasProjectIssues) {
            console.log('   📝 For project template issues:');
            console.log('      - Edit projects.json to update project titles and descriptions');
            console.log('      - Modify src/pug/projects/templates/project-detail.pug template if needed');
            console.log('      - Run `npm run build:pug` to regenerate all project pages');
        }
    }

    if (summary.info > 0) {
        console.log('4. Consider INFO items - these improve social media sharing');
    }

    console.log('5. Run `npm run build:pug` to regenerate HTML after PUG changes');
    console.log('6. Run this report again after fixes to verify improvements');
    console.log('\n💡 TIP: For project pages, focus on optimizing the template and projects.json data\n');
}

// Run the report
generateReport();
