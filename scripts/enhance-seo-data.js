/**
 * Script to enhance articles.json with improved SEO data
 * Analyzes existing data and suggests/implements SEO improvements
 */

const fs = require('fs');
const path = require('path');

// Load current articles data
const articlesPath = path.join(__dirname, '../src/articles.json');
const articles = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

function optimizeTitle(title) {
  // Ensure title is between 30-60 characters for optimal SEO
  if (title.length < 30) {
    console.log(`Title too short (${title.length} chars): ${title}`);
  }
  if (title.length > 60) {
    console.log(`Title too long (${title.length} chars): ${title}`);
  }
  return title;
}

function optimizeDescription(description) {
  // Ensure description is between 120-160 characters for optimal SEO
  if (!description) return description;

  if (description.length < 120) {
    console.log(`Description too short (${description.length} chars): ${description.substring(0, 50)}...`);
  }
  if (description.length > 160) {
    console.log(`Description too long (${description.length} chars), truncating: ${description.substring(0, 50)}...`);
    return description.substring(0, 157) + '...';
  }
  return description;
}

function enhanceArticleSEO(article) {
  // Skip if already has enhanced SEO
  if (article.seo && article.og && article.twitter) {
    return article;
  }

  console.log(`Enhancing SEO for: ${article.name}`);

  // Create enhanced SEO structure
  const enhanced = {
    ...article,
    seo: article.seo || {
      title: optimizeTitle(article.name),
      titleSuffix: ' | Mark Hazleton',
      description: optimizeDescription(article.description),
      keywords: article.keywords || '',
      canonical: `https://markhazleton.com/${article.slug}`,
      robots: 'index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1'
    },
    og: article.og || {
      title: article.name,
      description: article.description ? optimizeDescription(article.description) : '',
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
  let titleIssues = 0;
  let descriptionIssues = 0;

  articles.forEach(article => {
    if (article.name && article.description && article.keywords) {
      hasBasicSEO++;
    }

    if (article.seo && article.og && article.twitter) {
      hasEnhancedSEO++;
    }

    if (!article.name || article.name.length < 30 || article.name.length > 60) {
      titleIssues++;
    }

    if (!article.description || article.description.length < 120 || article.description.length > 160) {
      descriptionIssues++;
    }
  });

  console.log(`Total articles: ${articles.length}`);
  console.log(`Articles with basic SEO: ${hasBasicSEO}`);
  console.log(`Articles with enhanced SEO: ${hasEnhancedSEO}`);
  console.log(`Articles with title issues: ${titleIssues}`);
  console.log(`Articles with description issues: ${descriptionIssues}`);
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
