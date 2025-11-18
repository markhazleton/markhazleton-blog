const fs = require('fs-extra');
const path = require('path');
const { marked } = require('marked');
const matter = require('gray-matter');

// Configuration
const config = {
  contentDir: path.join(__dirname, 'content'),
  publicDir: path.join(__dirname, 'public'),
  templatesDir: path.join(__dirname, 'templates'),
  siteTitle: 'Project Mechanics',
  siteUrl: 'https://projectmechanics.com',
  author: 'Mark Hazleton'
};

// Configure marked options
marked.setOptions({
  gfm: true,
  breaks: true,
  headerIds: true,
  mangle: false
});

// Template function
function createTemplate(title, content, meta = {}) {
  const nav = `
    <nav>
      <a href="index.html" class="logo">
        <span>📊</span>
        <span>Project Mechanics</span>
      </a>
      <button class="mobile-menu-toggle" aria-label="Toggle menu">☰</button>
      <ul class="nav-links">
        <li><a href="index.html">Home</a></li>
        <li><a href="overview.html">Overview</a></li>
        <li><a href="project-life-cycle.html">Project Life Cycle</a></li>
        <li><a href="leadership-skills.html">Leadership</a></li>
        <li><a href="change-management-strategies.html">Change Management</a></li>
        <li><a href="conflict-management-strategies.html">Conflict Management</a></li>
      </ul>
    </nav>
  `;

  const breadcrumb = meta.slug && meta.slug !== 'index' ? `
    <div class="breadcrumb">
      <a href="index.html">Home</a>
      <span>/</span>
      <span>${title}</span>
    </div>
  ` : '';

  return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta name="description" content="${meta.description || 'Project Mechanics - Effective Project Management Methodology'}">
  <meta name="author" content="${meta.author || config.author}">
  <meta name="keywords" content="project management, project mechanics, software development, agile, waterfall, PMI, PMP">

  <!-- Open Graph -->
  <meta property="og:title" content="${title} | ${config.siteTitle}">
  <meta property="og:description" content="${meta.description || 'Project Mechanics - Effective Project Management Methodology'}">
  <meta property="og:type" content="article">
  <meta property="og:url" content="${config.siteUrl}/${meta.slug || 'index'}.html">

  <!-- Twitter Card -->
  <meta name="twitter:card" content="summary_large_image">
  <meta name="twitter:title" content="${title} | ${config.siteTitle}">
  <meta name="twitter:description" content="${meta.description || 'Project Mechanics - Effective Project Management Methodology'}">

  <title>${title} | ${config.siteTitle}</title>
  <link rel="stylesheet" href="css/styles.css">
  <link rel="icon" type="image/x-icon" href="assets/favicon.ico">
</head>
<body>
  <a href="#main-content" class="skip-link">Skip to main content</a>

  <header>
    ${nav}
  </header>

  <main id="main-content">
    ${breadcrumb}
    <div class="content-wrapper">
      <article>
        ${content}
      </article>
    </div>
  </main>

  <footer>
    <p>&copy; ${new Date().getFullYear()} Mark Hazleton. All rights reserved.</p>
    <p>
      <a href="https://markhazleton.com" target="_blank" rel="noopener noreferrer">Visit Mark's Blog</a> |
      <a href="https://www.linkedin.com/in/markhazleton/" target="_blank" rel="noopener noreferrer">LinkedIn</a> |
      <a href="https://github.com/markhazleton" target="_blank" rel="noopener noreferrer">GitHub</a>
    </p>
  </footer>

  <script src="js/main.js"></script>
</body>
</html>`;
}

// Build homepage with cards
function createHomepage(content, meta) {
  const contentFiles = fs.readdirSync(config.contentDir)
    .filter(file => 
      file.endsWith('.md') && 
      file !== 'index.md' && 
      file !== 'README.md' // Exclude README from cards
    )
    .map(file => {
      const filePath = path.join(config.contentDir, file);
      const fileContent = fs.readFileSync(filePath, 'utf-8');
      const { data } = matter(fileContent);
      
      // Determine slug from frontmatter or filename
      let slug;
      if (data.slug) {
        slug = data.slug;
      } else if (file.startsWith('projectmechanics-')) {
        slug = file.replace('projectmechanics-', '').replace('.md', '');
      } else {
        slug = file.replace('.md', '');
      }

      // Skip files with invalid slugs
      if (!slug || slug === '-' || slug.endsWith('-') || slug.includes('--')) {
        return null;
      }

      return {
        title: data.title || slug.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
        description: data.description || '',
        slug: slug,
        filename: slug + '.html'
      };
    })
    .filter(item => item && item.title && item.slug); // Filter out null items and those without titles

  const cardsHtml = contentFiles.map(item => `
    <div class="card">
      <h3>${item.title}</h3>
      <p>${item.description}</p>
      <a href="${item.filename}">Read more →</a>
    </div>
  `).join('\n');

  const heroSection = `
    <div class="hero">
      <h1>Project Mechanics</h1>
      <p>Mastering the art and science of effective project management through structured methodologies and adaptive problem-solving</p>
      <a href="#explore" class="cta-button">Explore Project Mechanics</a>
    </div>
  `;

  const exploreSection = `
    <div id="explore" class="content-wrapper">
      <h2>Explore Project Management Topics</h2>
      <div class="cards-grid">
        ${cardsHtml}
      </div>
    </div>
  `;

  const fullContent = `${heroSection}${content}${exploreSection}`;

  return createTemplate(meta.title || 'Home', fullContent, meta);
}

// Process markdown files
async function buildSite() {
  console.log('🚀 Building Project Mechanics site...');

  // Ensure public directory exists
  await fs.ensureDir(config.publicDir);
  await fs.ensureDir(path.join(config.publicDir, 'css'));
  await fs.ensureDir(path.join(config.publicDir, 'js'));
  await fs.ensureDir(path.join(config.publicDir, 'assets'));

  // Get all markdown files
  const contentFiles = await fs.readdir(config.contentDir);
  const markdownFiles = contentFiles.filter(file => 
    file.endsWith('.md') && 
    file !== 'README.md' // Skip README files
  );

  console.log(`📄 Found ${markdownFiles.length} markdown files`);

  // Process each markdown file
  for (const file of markdownFiles) {
    const filePath = path.join(config.contentDir, file);
    const fileContent = await fs.readFile(filePath, 'utf-8');

    // Parse frontmatter and content
    const { data: meta, content } = matter(fileContent);

    // Convert markdown to HTML
    const htmlContent = marked(content);

    // Determine output filename
    let outputFile;
    if (file === 'index.md') {
      outputFile = 'index.html';
    } else if (meta.slug) {
      // Use slug from frontmatter if available
      outputFile = meta.slug + '.html';
    } else if (file.startsWith('projectmechanics-')) {
      outputFile = file.replace('projectmechanics-', '').replace('.md', '.html');
    } else {
      outputFile = file.replace('.md', '.html');
    }

    // Skip files with invalid output names
    if (outputFile === '.html' || outputFile === '-.html' || outputFile.includes('-.html')) {
      console.log(`⚠️  Skipping invalid file: ${file} (would generate: ${outputFile})`);
      continue;
    }

    // Create full HTML page
    let html;
    if (file === 'index.md') {
      html = createHomepage(htmlContent, meta);
    } else {
      html = createTemplate(meta.title || 'Project Mechanics', htmlContent, meta);
    }

    // Write HTML file
    const outputPath = path.join(config.publicDir, outputFile);
    await fs.writeFile(outputPath, html);

    console.log(`✅ Generated: ${outputFile}`);
  }

  // Create main.js if it doesn't exist
  const jsPath = path.join(config.publicDir, 'js', 'main.js');
  if (!await fs.pathExists(jsPath)) {
    const js = `// Project Mechanics - Main JavaScript
document.addEventListener('DOMContentLoaded', function() {
  // Mobile menu toggle
  const menuToggle = document.querySelector('.mobile-menu-toggle');
  const navLinks = document.querySelector('.nav-links');

  if (menuToggle && navLinks) {
    menuToggle.addEventListener('click', function() {
      navLinks.classList.toggle('active');
    });
  }

  // Smooth scrolling for anchor links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        e.preventDefault();
        target.scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
      }
    });
  });

  // Set active nav link
  const currentPage = window.location.pathname.split('/').pop() || 'index.html';
  document.querySelectorAll('.nav-links a').forEach(link => {
    if (link.getAttribute('href') === currentPage) {
      link.classList.add('active');
    }
  });

  console.log('✨ Project Mechanics site initialized');
});
`;
    await fs.writeFile(jsPath, js);
    console.log('✅ Generated: js/main.js');
  }

  console.log('✨ Build complete!');
  console.log(`📁 Output directory: ${config.publicDir}`);
}

// Run the build
buildSite().catch(console.error);
