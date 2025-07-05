/**
 * Script to enhance articles.json with improved SEO data
 * Analyzes existing data and suggests/implements SEO improvements
 */

const fs = require('fs');
const path = require('path');

// Load current articles data
const articlesPath = path.join(__dirname, '../src/articles.json');
const articles = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

function optimizeTitle(title, articleInfo = {}) {
  // Ensure title is between 30-60 characters for optimal SEO
  const location = articleInfo.slug ? ` [Article: ${articleInfo.slug}${articleInfo.id ? `, ID: ${articleInfo.id}` : ''}]` : '';

  if (title.length < 30) {
    console.log(`[SEO WARNING]${location} Title too short (${title.length} chars, should be ≥30): ${title}`);
  }
  if (title.length > 60) {
    console.log(`[SEO WARNING]${location} Title too long (${title.length} chars, should be ≤60): ${title}`);
  }
  return title;
}

function optimizeDescription(description, articleInfo = {}) {
  // Ensure description is between 120-160 characters for optimal SEO
  if (!description) return description;

  const location = articleInfo.slug ? ` [Article: ${articleInfo.slug}${articleInfo.id ? `, ID: ${articleInfo.id}` : ''}]` : '';

  if (description.length < 120) {
    console.log(`[SEO WARNING]${location} Description too short (${description.length} chars, should be ≥120): ${description.substring(0, 50)}...`);
  }
  if (description.length > 160) {
    console.log(`[SEO WARNING]${location} Description too long (${description.length} chars, should be ≤160), truncating: ${description.substring(0, 50)}...`);
    return description.substring(0, 157) + '...';
  }
  return description;
}

function enhanceArticleSEO(article) {
  // Skip if already has enhanced SEO
  if (article.seo && article.og && article.twitter) {
    return article;
  }

  const articleInfo = { slug: article.slug, id: article.id, name: article.name };
  console.log(`[SEO ENHANCEMENT] Processing article: ${article.name} [ID: ${article.id}, Slug: ${article.slug}]`);

  // Create enhanced SEO structure
  const enhanced = {
    ...article,
    seo: article.seo || {
      title: optimizeTitle(article.name, articleInfo),
      titleSuffix: ' | Mark Hazleton',
      description: optimizeDescription(article.description, articleInfo),
      keywords: article.keywords || '',
      canonical: `https://markhazleton.com/${article.slug}`,
      robots: 'index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1'
    },
    og: article.og || {
      title: article.name,
      description: article.description ? optimizeDescription(article.description, articleInfo) : '',
      type: 'article',
      image: article.img_src ? `https://markhazleton.com/${article.img_src}` : 'https://markhazleton.com/assets/img/MarkHazleton.jpg',
      imageAlt: `${article.name} - Mark Hazleton`
    },
    twitter: article.twitter || {
      title: article.name.length > 50 ? article.name.substring(0, 47) + '...' : article.name,
      description: article.description ? (article.description.length > 120 ? article.description.substring(0, 117) + '...' : article.description) : '',
      image: article.img_src ? `https://markhazleton.com/${article.img_src}` : 'https://markhazleton.com/assets/img/MarkHazleton.jpg',
      imageAlt: `${article.name} - Mark Hazleton`
    }
  };

  return enhanced;
}

function enhanceAllArticles() {
  console.log(`Processing ${articles.length} articles...`);

  const enhancedArticles = articles.map(enhanceArticleSEO);

  // Create backup
  const backupPath = articlesPath + '.backup.' + Date.now();
  fs.writeFileSync(backupPath, JSON.stringify(articles, null, 2));
  console.log(`Backup created: ${backupPath}`);

  // Write enhanced articles
  fs.writeFileSync(articlesPath, JSON.stringify(enhancedArticles, null, 2));
  console.log(`Enhanced articles.json saved`);

  // Report statistics
  const enhanced = enhancedArticles.filter(a => a.seo && a.og && a.twitter).length;
  console.log(`${enhanced}/${articles.length} articles now have enhanced SEO data`);
}

function analyzeCurrentSEO() {
  console.log('\n=== SEO Analysis Report ===');

  let hasBasicSEO = 0;
  let hasEnhancedSEO = 0;
  let titleIssues = [];
  let descriptionIssues = [];

  articles.forEach(article => {
    const articleInfo = `[ID: ${article.id}, Slug: ${article.slug}]`;

    if (article.name && article.description && article.keywords) {
      hasBasicSEO++;
    }

    if (article.seo && article.og && article.twitter) {
      hasEnhancedSEO++;
    }

    if (!article.name || article.name.length < 30 || article.name.length > 60) {
      const issue = !article.name ?
        `Missing title ${articleInfo}` :
        `Title length issue (${article.name.length} chars) ${articleInfo}: ${article.name.substring(0, 50)}...`;
      titleIssues.push(issue);
    }

    if (!article.description || article.description.length < 120 || article.description.length > 160) {
      const issue = !article.description ?
        `Missing description ${articleInfo}` :
        `Description length issue (${article.description.length} chars) ${articleInfo}: ${article.description.substring(0, 50)}...`;
      descriptionIssues.push(issue);
    }
  });

  console.log(`Total articles: ${articles.length}`);
  console.log(`Articles with basic SEO: ${hasBasicSEO}`);
  console.log(`Articles with enhanced SEO: ${hasEnhancedSEO}`);
  console.log(`Articles with title issues: ${titleIssues.length}`);
  console.log(`Articles with description issues: ${descriptionIssues.length}`);

  if (titleIssues.length > 0) {
    console.log('\n🔍 TITLE ISSUES:');
    titleIssues.slice(0, 10).forEach(issue => console.log(`  • ${issue}`));
    if (titleIssues.length > 10) {
      console.log(`  ... and ${titleIssues.length - 10} more`);
    }
  }

  if (descriptionIssues.length > 0) {
    console.log('\n🔍 DESCRIPTION ISSUES:');
    descriptionIssues.slice(0, 10).forEach(issue => console.log(`  • ${issue}`));
    if (descriptionIssues.length > 10) {
      console.log(`  ... and ${descriptionIssues.length - 10} more`);
    }
  }

  console.log('========================\n');
}

// Run analysis
analyzeCurrentSEO();

// Uncomment to run enhancement (currently just analyzes)
// enhanceAllArticles();

console.log('Run with "node scripts/enhance-seo-data.js enhance" to apply improvements');

// Check command line argument
if (process.argv.includes('enhance')) {
  enhanceAllArticles();
}
