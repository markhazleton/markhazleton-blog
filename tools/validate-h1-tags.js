#!/usr/bin/env node

/**
 * H1 Tag Validation Script
 *
 * This script analyzes all PUG files to check for h1 tag usage and validates
 * that articles follow proper heading structure:
 * - Only one h1 per page (preferably from layout/metadata)
 * - Article content should start with h2 tags
 * - Proper heading hierarchy
 */

const fs = require('fs');
const path = require('path');

// Configuration
const config = {
    srcDir: path.join(__dirname, '..', 'src', 'pug'),
    excludeDirs: ['mixins', 'includes'],
    fileExtension: '.pug'
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
    totalFiles: 0,
    filesWithH1: 0,
    filesWithMultipleH1: 0,
    articleFiles: 0,
    articleFilesWithH1: 0,
    layoutFiles: 0,
    issues: []
};

/**
 * Recursively find all PUG files
 */
function findPugFiles(dir, fileList = []) {
    const files = fs.readdirSync(dir);

    files.forEach(file => {
        const filePath = path.join(dir, file);
        const stat = fs.statSync(filePath);

        if (stat.isDirectory() && !config.excludeDirs.includes(file)) {
            findPugFiles(filePath, fileList);
        } else if (stat.isFile() && path.extname(file) === config.fileExtension) {
            fileList.push(filePath);
        }
    });

    return fileList;
}

/**
 * Analyze a single PUG file for h1 tags
 */
function analyzeFile(filePath) {
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    const relativePath = path.relative(config.srcDir, filePath);

    const analysis = {
        file: relativePath,
        isArticle: relativePath.includes('articles/'),
        isLayout: relativePath.includes('layouts/'),
        h1Tags: [],
        h2Tags: [],
        h3Tags: [],
        codeBlocks: [],
        issues: []
    };

    let inCodeBlock = false;
    let codeBlockType = null;

    lines.forEach((line, index) => {
        const lineNum = index + 1;
        const trimmedLine = line.trim();

        // Track code blocks to ignore h1 tags within them
        if (trimmedLine.startsWith('pre.') || trimmedLine.startsWith('code.')) {
            inCodeBlock = true;
            codeBlockType = trimmedLine;
            analysis.codeBlocks.push({ line: lineNum, type: codeBlockType });
        } else if (inCodeBlock && trimmedLine === '' && lines[index + 1] && !lines[index + 1].startsWith(' ')) {
            inCodeBlock = false;
            codeBlockType = null;
        }

        // Look for heading tags (not in code blocks)
        if (!inCodeBlock) {
            // Match h1 tags
            const h1Match = trimmedLine.match(/^h1[\s\.#]|^h1$/);
            if (h1Match) {
                analysis.h1Tags.push({
                    line: lineNum,
                    content: trimmedLine,
                    fullLine: line
                });
            }

            // Match h2 tags
            const h2Match = trimmedLine.match(/^h2[\s\.#]|^h2$/);
            if (h2Match) {
                analysis.h2Tags.push({
                    line: lineNum,
                    content: trimmedLine
                });
            }

            // Match h3 tags
            const h3Match = trimmedLine.match(/^h3[\s\.#]|^h3$/);
            if (h3Match) {
                analysis.h3Tags.push({
                    line: lineNum,
                    content: trimmedLine
                });
            }
        }

        // Check for potential h1 content in code blocks or comments
        if (trimmedLine.includes('<h1') || trimmedLine.includes('h1>')) {
            analysis.codeBlocks.push({
                line: lineNum,
                type: 'html-content',
                content: trimmedLine
            });
        }
    });

    // Analyze issues
    if (analysis.h1Tags.length > 1) {
        analysis.issues.push(`Multiple h1 tags found (${analysis.h1Tags.length})`);
        results.filesWithMultipleH1++;
    }

    if (analysis.isArticle && analysis.h1Tags.length > 0) {
        analysis.issues.push('Article contains h1 tag (should come from layout)');
    }

    if (analysis.isLayout && analysis.h1Tags.length === 0) {
        analysis.issues.push('Layout file has no h1 tag');
    }

    // Check heading hierarchy
    if (analysis.h2Tags.length === 0 && analysis.h3Tags.length > 0) {
        analysis.issues.push('Has h3 tags but no h2 tags (poor hierarchy)');
    }

    return analysis;
}

/**
 * Generate a detailed report
 */
function generateReport(analyses) {
    console.log(`${colors.bold}${colors.cyan}=== H1 Tag Validation Report ===${colors.reset}\n`);

    // Summary statistics
    console.log(`${colors.bold}Summary:${colors.reset}`);
    console.log(`  Total PUG files analyzed: ${results.totalFiles}`);
    console.log(`  Article files: ${results.articleFiles}`);
    console.log(`  Layout files: ${results.layoutFiles}`);
    console.log(`  Files with h1 tags: ${results.filesWithH1}`);
    console.log(`  Files with multiple h1 tags: ${colors.red}${results.filesWithMultipleH1}${colors.reset}`);
    console.log(`  Article files with h1 tags: ${colors.yellow}${results.articleFilesWithH1}${colors.reset}\n`);

    // Files with issues
    const problemFiles = analyses.filter(a => a.issues.length > 0);

    if (problemFiles.length > 0) {
        console.log(`${colors.bold}${colors.red}Files with Issues (${problemFiles.length}):${colors.reset}`);
        problemFiles.forEach(analysis => {
            console.log(`\n${colors.yellow}${analysis.file}${colors.reset}`);
            analysis.issues.forEach(issue => {
                console.log(`  ${colors.red}❌${colors.reset} ${issue}`);
            });

            if (analysis.h1Tags.length > 0) {
                console.log(`  H1 tags found:`);
                analysis.h1Tags.forEach(tag => {
                    console.log(`    Line ${tag.line}: ${colors.cyan}${tag.content}${colors.reset}`);
                });
            }
        });
        console.log('');
    }

    // Files following best practices
    const goodFiles = analyses.filter(a =>
        a.issues.length === 0 &&
        (a.isArticle ? a.h1Tags.length === 0 : true) &&
        (a.isLayout ? a.h1Tags.length <= 1 : true)
    );

    if (goodFiles.length > 0) {
        console.log(`${colors.bold}${colors.green}Files Following Best Practices (${goodFiles.length}):${colors.reset}`);
        goodFiles.forEach(analysis => {
            const type = analysis.isArticle ? 'Article' : analysis.isLayout ? 'Layout' : 'Other';
            console.log(`  ${colors.green}✅${colors.reset} ${analysis.file} (${type})`);
        });
        console.log('');
    }

    // Recommendations
    console.log(`${colors.bold}${colors.magenta}Recommendations:${colors.reset}`);

    if (results.articleFilesWithH1 > 0) {
        console.log(`  ${colors.yellow}•${colors.reset} Remove h1 tags from article content - these should come from layout/metadata`);
    }

    if (results.filesWithMultipleH1 > 0) {
        console.log(`  ${colors.yellow}•${colors.reset} Fix files with multiple h1 tags - only one h1 per page for SEO`);
    }

    console.log(`  ${colors.yellow}•${colors.reset} Article content should start with h2 tags for proper hierarchy`);
    console.log(`  ${colors.yellow}•${colors.reset} Use heading hierarchy: h1 (from layout) > h2 > h3 > h4`);

    // Code examples note
    const filesWithHtmlContent = analyses.filter(a =>
        a.codeBlocks.some(block => block.type === 'html-content')
    );

    if (filesWithHtmlContent.length > 0) {
        console.log(`\n${colors.bold}${colors.blue}Files with HTML content in code blocks:${colors.reset}`);
        console.log(`  ${colors.blue}ℹ️${colors.reset} These may be causing Bing's "multiple h1" detection:`);
        filesWithHtmlContent.forEach(analysis => {
            console.log(`    ${analysis.file}`);
        });
    }
}

/**
 * Main execution
 */
function main() {
    console.log(`${colors.bold}Analyzing PUG files for h1 tag usage...${colors.reset}\n`);

    try {
        const pugFiles = findPugFiles(config.srcDir);
        results.totalFiles = pugFiles.length;

        const analyses = pugFiles.map(filePath => {
            const analysis = analyzeFile(filePath);

            // Update results
            if (analysis.h1Tags.length > 0) {
                results.filesWithH1++;
            }

            if (analysis.isArticle) {
                results.articleFiles++;
                if (analysis.h1Tags.length > 0) {
                    results.articleFilesWithH1++;
                }
            }

            if (analysis.isLayout) {
                results.layoutFiles++;
            }

            return analysis;
        });

        generateReport(analyses);

        // Exit with error code if issues found
        const hasIssues = analyses.some(a => a.issues.length > 0);
        process.exit(hasIssues ? 1 : 0);

    } catch (error) {
        console.error(`${colors.red}Error: ${error.message}${colors.reset}`);
        process.exit(1);
    }
}

// Run if called directly
if (require.main === module) {
    main();
}

module.exports = { main, analyzeFile, findPugFiles };
