/**
 * Script to update the sitemap.xml based on articles.json data
 * This script follows sitemap protocol standards: https://www.sitemaps.org/protocol.html
 */

const fs = require('fs');
const path = require('path');
const upath = require('upath');

/**
 * Formats a date string in ISO 8601 format required for sitemaps
 * Converts YYYY-MM-DD to proper sitemap date format (YYYY-MM-DDThh:mm:ssTZD)
 * @param {string} dateStr - Date string in YYYY-MM-DD format
 * @returns {string} Date formatted for sitemap
 */
function formatSitemapDate(dateStr) {
    const date = new Date(dateStr);
    // Format as ISO 8601: YYYY-MM-DDThh:mm:ssTZD
    return date.toISOString().replace(/\.\d+Z$/, '-05:00');
}

/**
 * Determines change frequency based on article age
 * @param {Date} articleDate - Article's last modified date
 * @returns {string} Change frequency value (daily, weekly, monthly, etc.)
 */
function getChangeFrequency(articleDate) {
    const now = new Date();
    const diffTime = Math.abs(now - articleDate);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    // Articles less than 30 days old get 'daily'
    if (diffDays < 30) {
        return 'daily';
    }
    // Articles 30-90 days old get 'weekly'
    else if (diffDays < 90) {
        return 'weekly';
    }
    // Older articles get 'monthly'
    else {
        return 'monthly';
    }
}

/**
 * Escapes special XML characters in a string
 * @param {string} text - String to escape
 * @returns {string} XML-escaped string
 */
function escapeXml(text) {
    if (!text) return '';
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&apos;');
}

/**
 * Main function to update the sitemap
 */
function updateSitemap() {
    console.log('Updating sitemap.xml...');

    // Define file paths
    const srcPath = upath.resolve(__dirname, '../../src');
    const articlesJsonPath = path.join(srcPath, 'articles.json');
    const sitemapXmlPath = path.join(srcPath, 'sitemap.xml');

    try {
        // Load articles.json
        const articlesData = fs.readFileSync(articlesJsonPath, 'utf8');
        const articles = JSON.parse(articlesData);

        // Sort articles by date (newest first)
        articles.sort((a, b) => {
            return new Date(b.lastmod) - new Date(a.lastmod);
        });

        // Start building the sitemap XML content
        let sitemapContent = `<?xml version="1.0" encoding="utf-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://www.sitemaps.org/schemas/sitemap/0.9
            http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd">
`;

        // Add homepage as the first entry (always highest priority)
        const now = new Date();
        const formattedNow = formatSitemapDate(now);

        sitemapContent += `  <url>
    <loc>https://markhazleton.com/</loc>
    <lastmod>${formattedNow}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>1.0</priority>
  </url>
`;

        // Process all articles
        articles.forEach(article => {
            const fullLink = `https://markhazleton.com/${article.slug}`;
            const articleDate = new Date(article.lastmod);
            const lastmod = formatSitemapDate(article.lastmod);
            const changefreq = getChangeFrequency(articleDate);

            // Determine priority based on age (newer articles get higher priority)
            const diffTime = Math.abs(now - articleDate);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            // Calculate priority (newer = higher priority)
            // Priority values range from 0.0 to 1.0
            let priority = 0.5; // Default priority

            if (diffDays < 30) {
                priority = 0.8; // Recent articles (less than a month old)
            } else if (diffDays < 90) {
                priority = 0.6; // Articles 1-3 months old
            }

            sitemapContent += `  <url>
    <loc>${escapeXml(fullLink)}</loc>
    <lastmod>${lastmod}</lastmod>
    <changefreq>${changefreq}</changefreq>
    <priority>${priority.toFixed(1)}</priority>
  </url>
`;
        });

        // Close sitemap tag
        sitemapContent += `</urlset>`;

        // Write the updated sitemap XML to file
        fs.writeFileSync(sitemapXmlPath, sitemapContent);

        console.log(`Sitemap updated successfully with ${articles.length + 1} URLs.`);
    } catch (error) {
        console.error('Error updating sitemap:', error);
    }
}

// Execute the function if called directly
if (require.main === module) {
    updateSitemap();
}

// Export for use by other scripts
module.exports = updateSitemap;
