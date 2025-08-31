/**
 * Script to update the RSS feed based on articles.json data
 * This script reads the articles.json file and generates an updated rss.xml file
 * with the most recent articles in the src folder.
 */

const fs = require('fs');
const path = require('path');
const upath = require('upath');

// Number of items to include in the RSS feed
const MAX_RSS_ITEMS = 500;

/**
 * Formats a date string in RFC822 format required for RSS
 * Converts YYYY-MM-DD to proper RSS date format
 * @param {string} dateStr - Date string in YYYY-MM-DD format
 * @returns {string} Date formatted for RSS
 */
function formatRssDate(dateStr) {
    const date = new Date(dateStr);
    return date.toUTCString();
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
 * Main function to update the RSS feed
 */
function updateRssFeed() {
    console.log('Updating RSS feed...');

    // Define file paths
    const srcPath = upath.resolve(__dirname, '../../src');
    const articlesJsonPath = path.join(srcPath, 'articles.json');
    const rssXmlPath = path.join(srcPath, 'rss.xml');

    // Load articles.json
    try {
        const articlesData = fs.readFileSync(articlesJsonPath, 'utf8');
        const articles = JSON.parse(articlesData);

        // Sort articles by date (newest first)
        articles.sort((a, b) => {
            return new Date(b.lastmod) - new Date(a.lastmod);
        });

        // Get current date for lastBuildDate
        const now = new Date();
        const buildDate = now.toUTCString();

        // Start building the RSS XML content
        let rssContent = `<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:content="http://purl.org/rss/1.0/modules/content/">
  <channel>
    <title>Mark Hazleton Articles</title>
    <link>https://markhazleton.com/</link>
    <description>Latest articles from Mark Hazleton.</description>
    <language>en-us</language>
    <lastBuildDate>${buildDate}</lastBuildDate>
    <atom:link href="https://markhazleton.com/rss.xml" rel="self" type="application/rss+xml" />
    <image>
      <url>https://markhazleton.com/apple-touch-icon.png</url>
      <title>Mark Hazleton Articles</title>
      <link>https://markhazleton.com/</link>
    </image>
`;

        // Add items (limited to MAX_RSS_ITEMS)
        const itemsToInclude = articles.slice(0, MAX_RSS_ITEMS);

        itemsToInclude.forEach(article => {
            const fullLink = `https://markhazleton.com/${article.slug}`;
            const pubDate = formatRssDate(article.lastmod);
            const title = escapeXml(article.name);
            const description = escapeXml(article.description);
            const category = article.Section ? escapeXml(article.Section) : '';

            rssContent += `    <item>
      <title>${title}</title>
      <link>${fullLink}</link>
      <description>${description}</description>
      <pubDate>${pubDate}</pubDate>
      <guid isPermaLink="true">${fullLink}</guid>`;

            // Add category if available
            if (category) {
                rssContent += `
      <category>${category}</category>`;
            }

            // Add optional content:encoded if content field is not null
            if (article.content) {
                rssContent += `
      <content:encoded><![CDATA[${article.content}]]></content:encoded>`;
            }

            rssContent += `
    </item>
`;
        });

        // Close RSS tags
        rssContent += `  </channel>
</rss>`;

        // Write the updated RSS XML to file
        fs.writeFileSync(rssXmlPath, rssContent);

        console.log(`RSS feed updated successfully with ${itemsToInclude.length} articles.`);
    } catch (error) {
        console.error('Error updating RSS feed:', error);
    }
}

// Execute the function if called directly
if (require.main === module) {
    updateRssFeed();
}

// Export for use by other scripts
module.exports = updateRssFeed;
