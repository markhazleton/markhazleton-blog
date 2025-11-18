/**
 * SEO Improvements Script for ProjectMechanics Site
 * Generates sitemap.xml, robots.txt, and enhanced build with SEO optimizations
 */

const fs = require('fs-extra');
const path = require('path');
const { marked } = require('marked');
const matter = require('gray-matter');

// Configuration
const config = {
  contentDir: path.join(__dirname, 'content'),
  publicDir: path.join(__dirname, 'public'),
  siteTitle: 'Project Mechanics',
  siteUrl: 'https://projectmechanics.com',
  author: 'Mark Hazleton',
  siteLogo: '/assets/img/logo.png',
  ogImage: '/assets/og-images/og-default.jpg'
};

/**
 * Generate robots.txt
 */
function generateRobotsTxt() {
  const robotsTxt = `# projectmechanics.com robots.txt
User-agent: *
Allow: /
Disallow: /assets/temp/

# Crawl-delay for specific bots
User-agent: Googlebot
Crawl-delay: 0

User-agent: Bingbot
Crawl-delay: 1

# Sitemap location
Sitemap: ${config.siteUrl}/sitemap.xml

# Good bots
User-agent: Googlebot
User-agent: Googlebot-Image
User-agent: Bingbot
User-agent: Slurp
User-agent: DuckDuckBot
Allow: /
`;

  fs.writeFileSync(path.join(config.publicDir, 'robots.txt'), robotsTxt);
  console.log('✅ Generated: robots.txt');
}

/**
 * Generate sitemap.xml
 */
async function generateSitemap() {
  const contentFiles = await fs.readdir(config.contentDir);
  const markdownFiles = contentFiles.filter(file =>
    file.endsWith('.md') && file !== 'README.md'
  );

  const pages = [];

  for (const file of markdownFiles) {
    const filePath = path.join(config.contentDir, file);
    const fileContent = await fs.readFile(filePath, 'utf-8');
    const { data: meta } = matter(fileContent);

    // Determine slug
    let slug = 'index';
    if (file !== 'index.md') {
      slug = meta.slug || file.replace('projectmechanics-', '').replace('.md', '');
    }

    // Skip invalid slugs
    if (slug === '-' || slug.endsWith('-')) continue;

    // Set priority based on page type
    let priority = 0.8;
    let changefreq = 'monthly';

    if (file === 'index.md') {
      priority = 1.0;
      changefreq = 'weekly';
    } else if (slug.includes('project-life-cycle') || slug.includes('leadership') || slug.includes('change-management')) {
      priority = 0.9;
      changefreq = 'monthly';
    }

    pages.push({
      slug,
      priority,
      changefreq,
      lastmod: meta.date || new Date().toISOString().split('T')[0]
    });
  }

  const sitemap = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
        xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
${pages.map(page => `  <url>
    <loc>${config.siteUrl}/${page.slug === 'index' ? '' : page.slug + '.html'}</loc>
    <lastmod>${page.lastmod}</lastmod>
    <changefreq>${page.changefreq}</changefreq>
    <priority>${page.priority}</priority>
  </url>`).join('\n')}
</urlset>`;

  fs.writeFileSync(path.join(config.publicDir, 'sitemap.xml'), sitemap);
  console.log(`✅ Generated: sitemap.xml (${pages.length} pages)`);
}

/**
 * Get JSON-LD structured data for a page
 */
function getStructuredData(meta, pageType = 'article') {
  if (pageType === 'homepage') {
    return {
      "@context": "https://schema.org",
      "@type": "WebSite",
      "name": config.siteTitle,
      "url": config.siteUrl,
      "description": meta.description || "Effective project management methodology for software development",
      "publisher": {
        "@type": "Person",
        "name": config.author,
        "url": "https://markhazleton.com"
      },
      "potentialAction": {
        "@type": "SearchAction",
        "target": {
          "@type": "EntryPoint",
          "urlTemplate": `${config.siteUrl}/search?q={search_term_string}`
        },
        "query-input": "required name=search_term_string"
      }
    };
  }

  return {
    "@context": "https://schema.org",
    "@type": "Article",
    "headline": meta.title,
    "description": meta.description,
    "author": {
      "@type": "Person",
      "name": meta.author || config.author,
      "url": "https://markhazleton.com"
    },
    "publisher": {
      "@type": "Person",
      "name": config.author
    },
    "datePublished": meta.date || new Date().toISOString(),
    "dateModified": meta.modified || new Date().toISOString(),
    "mainEntityOfPage": {
      "@type": "WebPage",
      "@id": `${config.siteUrl}/${meta.slug || 'index'}.html`
    },
    "keywords": Array.isArray(meta.keywords) ? meta.keywords.join(', ') : (meta.keywords || ''),
    "articleSection": meta.category || "Project Management",
    "inLanguage": "en-US"
  };
}

/**
 * Get optimized meta tags for SEO
 */
function getMetaTags(meta, isHomepage = false) {
  const title = isHomepage
    ? `${meta.title || config.siteTitle}: Master Project Management | ${config.author}`
    : `${meta.title || 'Page'} | ${config.siteTitle}`;

  const description = meta.description || 'Effective project management methodology for software development';
  const keywords = meta.keywords || 'project management, project mechanics, software development, agile, waterfall, PMI, PMP';
  const url = `${config.siteUrl}/${meta.slug === 'index' ? '' : (meta.slug || 'page') + '.html'}`;
  const ogImage = meta.ogImage || `${config.siteUrl}${config.ogImage}`;

  return {
    title,
    description,
    keywords,
    url,
    ogImage,
    ogTitle: meta.ogTitle || title,
    ogDescription: meta.ogDescription || description,
    twitterTitle: meta.twitterTitle || title,
    twitterDescription: meta.twitterDescription || description
  };
}

/**
 * Enhanced metadata recommendations for each page
 */
const pageMetadata = {
  'index': {
    title: 'Project Mechanics: Master the Art and Science of Project Management',
    description: 'Master project management with Project Mechanics - a proven methodology combining structured techniques and adaptive problem-solving for software development success. Learn PMI best practices.',
    keywords: 'project management methodology, project mechanics, software project management, PMI, PMP, agile project management, project delivery'
  },
  'overview': {
    title: 'Project Mechanics Overview: Understanding the Methodology',
    description: 'An introduction to Project Mechanics methodology - combining art and science of project management for successful software delivery and client satisfaction.',
    keywords: 'project mechanics, project management framework, software development methodology, project delivery approach'
  },
  'project-life-cycle': {
    title: 'Project Life Cycle: 7 Stages of Project Management Success',
    description: 'Understand the complete project life cycle from initiation to closure. Learn the 7 stages of effective project management including planning, execution, and portfolio management (PPM).',
    keywords: 'project life cycle, project phases, project stages, project lifecycle management, PPM, project portfolio management'
  },
  'project-meetings': {
    title: 'Effective Project Meetings: Boost Team Productivity',
    description: 'Run more effective project meetings with proven strategies. Learn how to boost team productivity through structured agendas, clear objectives, and actionable outcomes.',
    keywords: 'project meetings, effective meetings, team productivity, meeting agenda, meeting best practices'
  },
  'program-management-office': {
    title: 'PMO Best Practices: Program Management Office Guide',
    description: 'Establish a high-performing Program Management Office (PMO) with best practices for governance, resource optimization, and strategic project portfolio management.',
    keywords: 'PMO, program management office, PMO best practices, project governance, portfolio management'
  },
  'leadership-skills': {
    title: 'Leadership Skills for Project Managers | Project Success Guide',
    description: 'Develop essential leadership skills for project success. Master emotional intelligence, decision-making, communication, and team motivation strategies for project managers.',
    keywords: 'project management leadership, leadership skills, project manager skills, team leadership, PM leadership'
  },
  'change-management-strategies': {
    title: 'Change Management Strategies for Projects | 10 Proven Methods',
    description: 'Master project change management with 10 proven strategies. Learn best practices for managing project changes through clear communication, stakeholder involvement, and strategic planning.',
    keywords: 'change management, project change management, change control, managing change, change management strategies'
  },
  'conflict-management-strategies': {
    title: 'Conflict Resolution for Project Teams | Management Strategies',
    description: 'Essential strategies for managing conflicts in project teams to improve team dynamics, enhance productivity, and drive innovation. Learn proven conflict resolution techniques.',
    keywords: 'conflict management, conflict resolution, team conflict, project team management, resolving conflicts'
  },
  'solution-architect-technology-decisions-that-impact-business': {
    title: 'Solution Architecture: Technology Decisions That Impact Business',
    description: 'Make better technology decisions as a solution architect. Learn how technical choices impact business outcomes, scalability, and long-term project success.',
    keywords: 'solution architect, technology decisions, architecture decisions, technical architecture, business impact'
  }
};

/**
 * Generate enhanced meta tags based on recommendations
 */
function getEnhancedMetaTags(slug, currentMeta) {
  const recommended = pageMetadata[slug] || {};
  return {
    ...currentMeta,
    title: currentMeta.title || recommended.title || 'Project Mechanics',
    description: currentMeta.description || recommended.description || 'Effective project management methodology',
    keywords: currentMeta.keywords || recommended.keywords || 'project management'
  };
}

/**
 * Main execution
 */
async function improveSEO() {
  console.log('🔍 Starting SEO improvements...\n');

  try {
    // Generate robots.txt
    generateRobotsTxt();

    // Generate sitemap.xml
    await generateSitemap();

    console.log('\n✨ SEO improvements complete!');
    console.log('\n📋 Next steps:');
    console.log('1. Review generated sitemap.xml and robots.txt');
    console.log('2. Add Google Analytics tracking code to build.js');
    console.log('3. Create Open Graph images in /public/assets/og-images/');
    console.log('4. Update meta descriptions using recommendations in SEO_AUDIT.md');
    console.log('5. Fix duplicate H1 tags in index.md');
    console.log('6. Submit sitemap to Google Search Console after deployment');

  } catch (error) {
    console.error('❌ Error during SEO improvements:', error);
  }
}

// Run if executed directly
if (require.main === module) {
  improveSEO();
}

module.exports = {
  generateSitemap,
  generateRobotsTxt,
  getStructuredData,
  getMetaTags,
  getEnhancedMetaTags,
  pageMetadata
};
