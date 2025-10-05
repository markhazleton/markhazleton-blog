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
    const projectsJsonPath = path.join(srcPath, 'projects.json');
    const sitemapXmlPath = path.join(srcPath, 'sitemap.xml');

    try {
        const articlesData = fs.readFileSync(articlesJsonPath, 'utf8');
        const projectsData = fs.readFileSync(projectsJsonPath, 'utf8');
        const articles = JSON.parse(articlesData);
        const projects = JSON.parse(projectsData);

        articles.sort((a, b) => new Date(b.lastmod) - new Date(a.lastmod));
        projects.sort((a, b) => {
            const aDate = new Date(a.promotion?.lastPromotedOn || a.updatedOn || a.lastModified || 0);
            const bDate = new Date(b.promotion?.lastPromotedOn || b.updatedOn || b.lastModified || 0);
            return bDate - aDate;
        });

        let sitemapContent = `<?xml version="1.0" encoding="utf-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://www.sitemaps.org/schemas/sitemap/0.9
            http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd">
`;
        const now = new Date();
        const formattedNow = formatSitemapDate(now);
        let urlCount = 0;

        sitemapContent += `  <url>
    <loc>https://markhazleton.com/</loc>
    <lastmod>${formattedNow}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>1.0</priority>
  </url>
`;
        urlCount++;

        const latestProjectDate = projects.reduce((latest, project) => {
            const candidate = project.promotion?.lastPromotedOn || project.updatedOn || project.lastModified;
            const candidateDate = candidate ? new Date(candidate) : null;
            if (candidateDate && !Number.isNaN(candidateDate.getTime())) {
                if (!latest || candidateDate > latest) {
                    return candidateDate;
                }
            }
            return latest;
        }, null);

        const projectsListingLastMod = formatSitemapDate(latestProjectDate || now);
        sitemapContent += `  <url>
    <loc>https://markhazleton.com/projects.html</loc>
    <lastmod>${projectsListingLastMod}</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.7</priority>
  </url>
`;
        urlCount++;

        articles.forEach(article => {
            const fullLink = `https://markhazleton.com/${article.slug}`;
            const articleDate = new Date(article.lastmod);
            const lastmod = formatSitemapDate(article.lastmod);
            const changefreq = getChangeFrequency(articleDate);

            const diffTime = Math.abs(now - articleDate);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            let priority = 0.5;
            if (diffDays < 30) {
                priority = 0.8;
            } else if (diffDays < 90) {
                priority = 0.6;
            }

            sitemapContent += `  <url>
    <loc>${escapeXml(fullLink)}</loc>
    <lastmod>${lastmod}</lastmod>
    <changefreq>${changefreq}</changefreq>
    <priority>${priority.toFixed(1)}</priority>
  </url>
`;
            urlCount++;
        });

        projects.forEach(project => {
            if (!project || !project.slug) {
                return;
            }

            const projectUrl = `https://markhazleton.com/projects/${project.slug}/`;
            const projectDateRaw = project.promotion?.lastPromotedOn || project.updatedOn || project.lastModified || now.toISOString();
            const projectDateObj = new Date(projectDateRaw);
            const validProjectDate = Number.isNaN(projectDateObj.getTime()) ? now : projectDateObj;
            const lastmod = formatSitemapDate(validProjectDate);
            const changefreq = getChangeFrequency(validProjectDate);

            const diffTime = Math.abs(now - validProjectDate);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            let priority = 0.5;
            if (diffDays < 30) {
                priority = 0.7;
            } else if (diffDays < 90) {
                priority = 0.6;
            }

            sitemapContent += `  <url>
    <loc>${escapeXml(projectUrl)}</loc>
    <lastmod>${lastmod}</lastmod>
    <changefreq>${changefreq}</changefreq>
    <priority>${priority.toFixed(1)}</priority>
  </url>
`;
            urlCount++;
        });

        sitemapContent += `</urlset>`;
        fs.writeFileSync(sitemapXmlPath, sitemapContent);

        console.log(`Sitemap updated successfully with ${urlCount} URLs.`);
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
