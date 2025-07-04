const fs = require('fs');
const path = require('path');

// Configuration
const ARTICLES_JSON_PATH = '../src/articles.json';
const OUTPUT_DIR = '../src/pug/articles';

// Load articles.json
function loadArticles() {
    try {
        const data = fs.readFileSync(ARTICLES_JSON_PATH, 'utf8');
        return JSON.parse(data);
    } catch (error) {
        console.error('Error loading articles.json:', error);
        process.exit(1);
    }
}

// Generate PUG content for an article using the dynamic SEO layout
function generatePugContent(article) {
    const slug = article.slug || 'unknown-article';

    return `extends ../layouts/dynamic-seo-layout

block append pageData
  - pageData = getArticleData('${slug}')

block content
  main.container.py-4
    article.row
      .col-12.col-lg-8
        header.article-header.mb-5
          .breadcrumb-wrapper.mb-3
            nav(aria-label='breadcrumb')
              ol.breadcrumb
                li.breadcrumb-item: a(href='/') Home
                li.breadcrumb-item: a(href='/articles') Articles
                li.breadcrumb-item.active(aria-current='page')= pageData.name

          h1.display-4.mb-3= pageData.name

          .article-meta.mb-4
            .d-flex.flex-wrap.align-items-center.gap-3
              .text-muted
                i.bi.bi-calendar3.me-2
                time(datetime=pageData.lastmod)= new Date(pageData.lastmod).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
              .text-muted
                i.bi.bi-folder.me-2
                span= pageData.Section
              .text-muted
                i.bi.bi-tags.me-2
                span= pageData.keywords || 'Technology, Development'

        .article-content.mb-5
          .lead.mb-4= pageData.description

          // Article content goes here
          // This section should be customized for each article
          section.mb-5
            h2 Introduction
            p.
              This article content should be replaced with the actual article content.
              The dynamic SEO layout will handle all meta tags, Open Graph, and Twitter Card data
              based on the article data from articles.json.

          section.mb-5
            h2 Content Section
            p.
              Add your actual article content here. The SEO data will be automatically
              populated from the articles.json file based on the article slug.

      aside.col-12.col-lg-4
        .sticky-top(style='top: 2rem;')
          .card.mb-4
            .card-header
              h5.card-title.mb-0
                i.bi.bi-info-circle.me-2
                | Article Information
            .card-body
              ul.list-unstyled.mb-0
                li.mb-2
                  strong Section:
                  span= pageData.Section
                li.mb-2
                  strong Last Updated:
                  time(datetime=pageData.lastmod)= new Date(pageData.lastmod).toLocaleDateString()
                li.mb-2
                  strong Change Frequency:
                  span= pageData.changefreq

          .card.mb-4
            .card-header
              h5.card-title.mb-0
                i.bi.bi-share.me-2
                | Share This Article
            .card-body.text-center
              .d-grid.gap-2
                a.btn.btn-primary.btn-sm(href=\`https://twitter.com/intent/tweet?url=\${encodeURIComponent('https://markhazleton.com/' + pageData.slug)}&text=\${encodeURIComponent(pageData.name)}\` target='_blank' rel='noopener')
                  i.bi.bi-twitter.me-2
                  | Share on Twitter
                a.btn.btn-info.btn-sm(href=\`https://www.linkedin.com/sharing/share-offsite/?url=\${encodeURIComponent('https://markhazleton.com/' + pageData.slug)}\` target='_blank' rel='noopener')
                  i.bi.bi-linkedin.me-2
                  | Share on LinkedIn
`;
}

// Migrate articles to use dynamic SEO layout
function migrateArticles() {
    const articles = loadArticles();
    let migrated = 0;
    let skipped = 0;
    let errors = 0;

    console.log(`Starting migration of ${articles.length} articles...`);

    articles.forEach((article, index) => {
        const slug = article.slug;
        if (!slug || slug === 'tbd') {
            console.log(`Skipping article ${index + 1}: Invalid slug "${slug}"`);
            skipped++;
            return;
        }

        const outputPath = path.join(OUTPUT_DIR, `${slug}.pug`);

        try {
            // Check if file already exists
            if (fs.existsSync(outputPath)) {
                console.log(`Skipping article ${index + 1}: File already exists for "${slug}"`);
                skipped++;
                return;
            }

            // Generate PUG content
            const pugContent = generatePugContent(article);

            // Ensure output directory exists
            fs.mkdirSync(path.dirname(outputPath), { recursive: true });

            // Write PUG file
            fs.writeFileSync(outputPath, pugContent, 'utf8');

            console.log(`✓ Migrated article ${index + 1}: "${slug}"`);
            migrated++;

        } catch (error) {
            console.error(`✗ Error migrating article ${index + 1} ("${slug}"):`, error.message);
            errors++;
        }
    });

    console.log('\n=== Migration Summary ===');
    console.log(`Total articles: ${articles.length}`);
    console.log(`Migrated: ${migrated}`);
    console.log(`Skipped: ${skipped}`);
    console.log(`Errors: ${errors}`);

    if (migrated > 0) {
        console.log('\n✓ Migration completed successfully!');
        console.log('Next steps:');
        console.log('1. Review the generated PUG files');
        console.log('2. Customize the content sections for each article');
        console.log('3. Run the build process to generate HTML files');
        console.log('4. Test the SEO meta tags in the generated HTML');
    }
}

// Run migration
if (require.main === module) {
    migrateArticles();
}

module.exports = { migrateArticles, generatePugContent };
