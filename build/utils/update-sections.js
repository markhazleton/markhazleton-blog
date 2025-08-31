#!/usr/bin/env node

/**
 * Update sections.json with articles data
 * This script reads articles.json and populates sections.json with article lists
 */

const fs = require('fs');
const path = require('path');
const upath = require('upath');

function updateSectionsWithArticles() {
    const srcPath = upath.resolve(upath.dirname(__filename), '../../src');
    const sectionsPath = upath.join(srcPath, 'sections.json');
    const articlesPath = upath.join(srcPath, 'articles.json');

    console.log('📚 Updating sections.json with articles data...');

    try {
        // Read the current sections.json
        const sectionsData = JSON.parse(fs.readFileSync(sectionsPath, 'utf8'));

        // Read the articles.json
        const articlesData = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

        // Create a map of sections to articles
        const sectionArticleMap = {};

        // Initialize each section with an empty articles array
        sectionsData.forEach(section => {
            sectionArticleMap[section.name] = [];
        });

        // Group articles by section
        articlesData.forEach(article => {
            const sectionName = article.Section;
            if (sectionName && sectionArticleMap.hasOwnProperty(sectionName)) {
                sectionArticleMap[sectionName].push({
                    name: article.name,
                    url: article.slug
                });
            }
        });

        // Update sections with articles
        sectionsData.forEach(section => {
            const articles = sectionArticleMap[section.name] || [];
            // Sort articles by name for consistent ordering
            articles.sort((a, b) => a.name.localeCompare(b.name));
            section.articles = articles;
            section.articleCount = articles.length;
        });

        // Write the updated sections.json
        fs.writeFileSync(sectionsPath, JSON.stringify(sectionsData, null, 2));

        // Log summary
        console.log('✅ Sections updated with articles:');
        sectionsData.forEach(section => {
            console.log(`   ${section.name}: ${section.articleCount} articles`);
        });

        const totalArticles = sectionsData.reduce((sum, section) => sum + section.articleCount, 0);
        console.log(`📊 Total articles processed: ${totalArticles}`);

    } catch (error) {
        console.error('❌ Error updating sections with articles:', error.message);
        throw error;
    }
}

// Export for use in other scripts
module.exports = updateSectionsWithArticles;

// Run directly if called from command line
if (require.main === module) {
    updateSectionsWithArticles();
}
