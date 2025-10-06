/**
 * Script to update the Projects RSS feed based on projects.json data
 * This script reads the projects.json file and generates an updated projects-rss.xml file
 * with project updates and releases.
 */

const fs = require('fs');
const path = require('path');
const upath = require('upath');

// Number of items to include in the RSS feed
const MAX_RSS_ITEMS = 50;

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
 * Gets the last modified date for a project
 * @param {Object} project - Project object
 * @returns {string} Last modified date string
 */
function getProjectLastModified(project) {
    // Priority order: lastPromotedOn, updatedOn, lastModified, or fallback to current date
    return project.promotion?.lastPromotedOn ||
           project.updatedOn ||
           project.lastModified ||
           new Date().toISOString();
}

/**
 * Generates a description for the project RSS item
 * @param {Object} project - Project object
 * @returns {string} Project description
 */
function generateProjectDescription(project) {
    let description = project.summary || project.d || 'Project update';

    // Add technology stack if available
    if (project.keywords) {
        const technologies = project.keywords.split(',').map(k => k.trim()).slice(0, 5);
        description += ` | Technologies: ${technologies.join(', ')}`;
    }

    // Add stage information if available
    if (project.promotion?.currentStage) {
        description += ` | Stage: ${project.promotion.currentStage}`;
    }

    return description;
}

/**
 * Main function to update the Projects RSS feed
 */
function updateProjectsRssFeed() {
    console.log('Updating Projects RSS feed...');

    // Define file paths
    const srcPath = upath.resolve(__dirname, '../../src');
    const projectsJsonPath = path.join(srcPath, 'projects.json');
    const projectsRssXmlPath = path.join(srcPath, 'projects-rss.xml');

    // Load projects.json
    try {
        const projectsData = fs.readFileSync(projectsJsonPath, 'utf8');
        const projects = JSON.parse(projectsData);

        // Sort projects by last modified date (newest first)
        projects.sort((a, b) => {
            const dateA = new Date(getProjectLastModified(a));
            const dateB = new Date(getProjectLastModified(b));
            return dateB - dateA;
        });

        // Get current date for lastBuildDate
        const now = new Date();
        const buildDate = now.toUTCString();

        // Start building the RSS XML content
        let rssContent = `<?xml version="1.0" encoding="utf-8"?>
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:content="http://purl.org/rss/1.0/modules/content/">
  <channel>
    <title>Mark Hazleton Projects</title>
    <link>https://markhazleton.com/projects.html</link>
    <description>Latest project updates and releases from Mark Hazleton's portfolio.</description>
    <language>en-us</language>
    <lastBuildDate>${buildDate}</lastBuildDate>
    <atom:link href="https://markhazleton.com/projects-rss.xml" rel="self" type="application/rss+xml" />
    <image>
      <url>https://markhazleton.com/apple-touch-icon.png</url>
      <title>Mark Hazleton Projects</title>
      <link>https://markhazleton.com/projects.html</link>
    </image>
`;

        // Add items (limited to MAX_RSS_ITEMS)
        const itemsToInclude = projects.slice(0, MAX_RSS_ITEMS);

        itemsToInclude.forEach(project => {
            const projectSlug = project.slug || project.p?.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'project';
            const fullLink = `https://markhazleton.com/projects/${projectSlug}/`;
            const lastModified = getProjectLastModified(project);
            const pubDate = formatRssDate(lastModified);
            const title = escapeXml(project.p || project.name || 'Project Update');
            const description = escapeXml(generateProjectDescription(project));
            const category = 'Projects';

            rssContent += `    <item>
      <title>${title}</title>
      <link>${fullLink}</link>
      <description>${description}</description>
      <pubDate>${pubDate}</pubDate>
      <guid isPermaLink="true">${fullLink}</guid>
      <category>${category}</category>`;

            // Add optional content with project details
            const contentParts = [];

            if (project.summary && project.d && project.summary !== project.d) {
                contentParts.push(`<h2>Summary</h2><p>${escapeXml(project.summary)}</p>`);
                contentParts.push(`<h2>Description</h2><p>${escapeXml(project.d)}</p>`);
            } else if (project.d) {
                contentParts.push(`<h2>Description</h2><p>${escapeXml(project.d)}</p>`);
            }

            if (project.keywords) {
                const technologies = project.keywords.split(',').map(k => k.trim());
                contentParts.push(`<h2>Technologies</h2><p>${technologies.join(', ')}</p>`);
            }

            if (project.h) {
                contentParts.push(`<h2>Live Demo</h2><p><a href="${escapeXml(project.h)}" target="_blank">View Project</a></p>`);
            }

            if (project.repository?.url) {
                contentParts.push(`<h2>Source Code</h2><p><a href="${escapeXml(project.repository.url)}" target="_blank">View Repository</a></p>`);
            }

            if (project.promotion?.notes) {
                contentParts.push(`<h2>Release Notes</h2><p>${escapeXml(project.promotion.notes)}</p>`);
            }

            if (contentParts.length > 0) {
                const content = contentParts.join('\n');
                rssContent += `
      <content:encoded><![CDATA[${content}]]></content:encoded>`;
            }

            rssContent += `
    </item>
`;
        });

        // Close RSS tags
        rssContent += `  </channel>
</rss>`;

        // Write the updated RSS XML to file
        fs.writeFileSync(projectsRssXmlPath, rssContent);

        console.log(`Projects RSS feed updated successfully with ${itemsToInclude.length} projects.`);
    } catch (error) {
        console.error('Error updating Projects RSS feed:', error);
    }
}

// Execute the function if called directly
if (require.main === module) {
    updateProjectsRssFeed();
}

// Export for use by other scripts
module.exports = updateProjectsRssFeed;
