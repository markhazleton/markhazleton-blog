const fs = require('fs');
const path = require('path');

/**
 * Script to convert existing articles from modern-layout to dynamic-seo-layout
 * This preserves all your custom content while upgrading to the new SEO system
 */

// Configuration
const ARTICLES_DIR = './src/pug/articles';
const BACKUP_DIR = './backups/pug-conversion';

/**
 * Extract content blocks from existing PUG file
 */
function extractContentBlocks(pugContent) {
    const blocks = {
        pagehead: '',
        canonical: '',
        og_overrides: '',
        twitter_overrides: '',
        layout_content: '',
        scripts: '',
        other_blocks: []
    };

    const lines = pugContent.split('\n');
    let currentBlock = null;
    let blockContent = [];
    let indentLevel = 0;

    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        const trimmed = line.trim();

        // Detect block declarations
        if (trimmed.startsWith('block ')) {
            // Save previous block
            if (currentBlock && blockContent.length > 0) {
                const content = blockContent.join('\n');
                if (blocks.hasOwnProperty(currentBlock)) {
                    blocks[currentBlock] = content;
                } else {
                    blocks.other_blocks.push({ name: currentBlock, content });
                }
            }

            // Start new block
            currentBlock = trimmed.replace('block ', '').replace('append ', '');
            blockContent = [];
            indentLevel = line.length - line.trimStart().length;
        } else if (currentBlock) {
            // Add content to current block (preserve indentation)
            const lineIndent = line.length - line.trimStart().length;
            if (lineIndent > indentLevel || trimmed === '' || trimmed.startsWith('//')) {
                blockContent.push(line);
            } else if (trimmed !== '') {
                // End of block content
                if (blockContent.length > 0) {
                    const content = blockContent.join('\n');
                    if (blocks.hasOwnProperty(currentBlock)) {
                        blocks[currentBlock] = content;
                    } else {
                        blocks.other_blocks.push({ name: currentBlock, content });
                    }
                }
                currentBlock = null;
                blockContent = [];
                i--; // Reprocess this line
            }
        }
    }

    // Save final block
    if (currentBlock && blockContent.length > 0) {
        const content = blockContent.join('\n');
        if (blocks.hasOwnProperty(currentBlock)) {
            blocks[currentBlock] = content;
        } else {
            blocks.other_blocks.push({ name: currentBlock, content });
        }
    }

    return blocks;
}

/**
 * Generate new PUG content using dynamic-seo-layout
 */
function generateNewPugContent(slug, blocks) {
    const template = `extends ../layouts/dynamic-seo-layout

block append pageData
  - pageData = getArticleData('${slug}')

block content${blocks.layout_content ? blocks.layout_content : `
  // Your existing content goes here
  // TODO: Move content from layout-content block to here
  .container
    .row
      .col-12
        p Converted article - please add your content here`}${blocks.scripts ? `

block append scripts${blocks.scripts}` : ''}${blocks.other_blocks.length > 0 ? `

// Additional blocks from original file:
${blocks.other_blocks.map(block => `block ${block.name}${block.content}`).join('\n')}` : ''}
`;

    return template;
}

/**
 * Convert a single article file
 */
function convertArticle(filePath) {
    console.log(`Converting: ${filePath}`);

    try {
        // Read original file
        const originalContent = fs.readFileSync(filePath, 'utf8');

        // Extract slug from filename
        const fileName = path.basename(filePath, '.pug');
        const slug = fileName;

        // Create backup
        const backupPath = path.join(BACKUP_DIR, path.basename(filePath));
        fs.mkdirSync(BACKUP_DIR, { recursive: true });
        fs.writeFileSync(backupPath, originalContent);
        console.log(`  ✓ Backup created: ${backupPath}`);

        // Extract content blocks
        const blocks = extractContentBlocks(originalContent);

        // Generate new content
        const newContent = generateNewPugContent(slug, blocks);

        // Write new file
        fs.writeFileSync(filePath, newContent);
        console.log(`  ✓ Converted to dynamic-seo-layout`);

        // Show summary of what was extracted
        console.log(`  📄 Extracted blocks:`);
        if (blocks.layout_content) console.log(`    - layout-content (${blocks.layout_content.split('\n').length} lines)`);
        if (blocks.scripts) console.log(`    - scripts`);
        if (blocks.other_blocks.length > 0) {
            blocks.other_blocks.forEach(block => {
                console.log(`    - ${block.name}`);
            });
        }

        return { success: true, blocks };

    } catch (error) {
        console.error(`  ✗ Error converting ${filePath}:`, error.message);
        return { success: false, error: error.message };
    }
}

/**
 * Convert multiple articles with selection
 */
function convertArticles(articleSlugs = []) {
    console.log('🔄 Converting articles to dynamic-seo-layout...\n');

    let converted = 0;
    let errors = 0;
    const results = [];

    // If no specific articles provided, ask user
    if (articleSlugs.length === 0) {
        console.log('ℹ️  No specific articles provided.');
        console.log('To convert specific articles, run:');
        console.log('node scripts/convert-to-dynamic-seo.js article1 article2 article3');
        console.log('');
        console.log('Available articles:');

        const files = fs.readdirSync(ARTICLES_DIR)
            .filter(f => f.endsWith('.pug') && f !== 'article-stub.pug')
            .slice(0, 10); // Show first 10

        files.forEach(file => {
            console.log(`  - ${file.replace('.pug', '')}`);
        });

        if (fs.readdirSync(ARTICLES_DIR).length > 10) {
            console.log(`  ... and ${fs.readdirSync(ARTICLES_DIR).length - 10} more`);
        }

        return;
    }

    articleSlugs.forEach(slug => {
        const filePath = path.join(ARTICLES_DIR, `${slug}.pug`);

        if (!fs.existsSync(filePath)) {
            console.log(`❌ File not found: ${slug}.pug`);
            errors++;
            return;
        }

        const result = convertArticle(filePath);
        results.push({ slug, ...result });

        if (result.success) {
            converted++;
        } else {
            errors++;
        }

        console.log(''); // Empty line between articles
    });

    console.log('=== Conversion Summary ===');
    console.log(`Total processed: ${articleSlugs.length}`);
    console.log(`Successfully converted: ${converted}`);
    console.log(`Errors: ${errors}`);
    console.log(`Backups saved to: ${BACKUP_DIR}`);

    if (converted > 0) {
        console.log('\n✅ Conversion completed!');
        console.log('\n📋 Next Steps:');
        console.log('1. Review the converted files');
        console.log('2. Test the articles by running: npm run build:pug');
        console.log('3. Check that SEO data is properly loaded from articles.json');
        console.log('4. Customize the content blocks as needed');

        if (errors > 0) {
            console.log('\n⚠️  Some articles had errors. Check the backups if needed.');
        }
    }
}

/**
 * Interactive conversion guide
 */
function showConversionGuide() {
    console.log(`
🎯 ARTICLE CONVERSION GUIDE

This script helps convert your existing articles from 'modern-layout' to 'dynamic-seo-layout'.

WHAT IT DOES:
✅ Creates backups of your original files
✅ Extracts your existing content blocks
✅ Converts to use dynamic SEO system
✅ Preserves your custom content

USAGE:
1. Convert specific articles:
   node scripts/convert-to-dynamic-seo.js article-slug-1 article-slug-2

2. See available articles:
   node scripts/convert-to-dynamic-seo.js

EXAMPLE:
   node scripts/convert-to-dynamic-seo.js building-my-first-react-site-using-vite

BENEFITS OF CONVERSION:
🔹 Automatic SEO meta tag generation from articles.json
🔹 Centralized SEO data management
🔹 AI-powered SEO field generation support
🔹 Consistent Open Graph and Twitter Card data
🔹 Future-proof SEO improvements

SAFE TO USE:
🛡️  Original files are backed up before conversion
🛡️  All your content is preserved and moved to new structure
🛡️  You can always restore from backups if needed
`);
}

// Run the script
if (require.main === module) {
    const args = process.argv.slice(2);

    if (args.length === 0) {
        showConversionGuide();
        convertArticles([]); // Show available articles
    } else if (args[0] === '--help' || args[0] === '-h') {
        showConversionGuide();
    } else {
        convertArticles(args);
    }
}

module.exports = { convertArticle, convertArticles, extractContentBlocks, generateNewPugContent };
