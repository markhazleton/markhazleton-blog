'use strict';

const fs = require('fs');
const fse = require('fs-extra');
const upath = require('upath');
const pug = require('pug');
const sh = require('shelljs');
const prettier = require('prettier');
const Prism = require('prismjs');
require('prismjs/components/')(); // Load all languages
const { execSync } = require('child_process');

// Get List of Articles and Projects
const articles = require('./../../src/articles.json');
const projects = require('./../../src/projects.json');

// Function to load build version info (reload fresh each time)
function getBuildVersion() {
    try {
        const buildVersionPath = upath.resolve(__dirname, '../../build-version.json');
        if (fs.existsSync(buildVersionPath)) {
            // Clear cache to get fresh data
            delete require.cache[require.resolve('../../build-version.json')];
            return require('../../build-version.json');
        }
    } catch (error) {
        console.warn('⚠️ Could not load build version:', error.message);
    }
    return { version: '1.0.0', buildDate: '', buildNumber: 0 };
}

// Import SEO Helper
const SEOHelper = require('./seo-helper');
const seoHelper = new SEOHelper(articles, {
    logWarnings: true,
    logErrors: true,
    logInfo: false,
    logScores: false,
    logBuild: false
});
function buildProjectSEO(project) {
    if (!project) {
        return seoHelper.getDefaultSEO();
    }

    const projectSeo = project.seo || {};
    const baseTitle = projectSeo.title || project.p || project.name || 'Project';
    const suffix = Object.prototype.hasOwnProperty.call(projectSeo, 'titleSuffix') ? projectSeo.titleSuffix : '';
    const title = `${baseTitle}${suffix}`;

    let description = projectSeo.description || project.summary || project.d || '';
    if (description.length > 160) {
        description = `${description.substring(0, 157)}...`;
    }

    const canonical = projectSeo.canonical || `https://markhazleton.com/projects/${project.slug}`;
    const keywords = projectSeo.keywords || project.keywords || '';
    const robots = projectSeo.robots || 'index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1';

    const imagePath = (project.og && project.og.image) || project.image || '/assets/img/MarkHazleton.jpg';
    const normalizedImage = imagePath.startsWith('http') ? imagePath : `https://markhazleton.com/${imagePath.replace(/^\/+/, '')}`;

    const ogTitle = (project.og && project.og.title) || title;
    const ogDescription = (project.og && project.og.description) || description;
    const ogType = (project.og && project.og.type) || 'website';
    const ogImageAlt = (project.og && project.og.imageAlt) || `${baseTitle} - Project preview`;

    const twitterImagePath = (project.twitter && project.twitter.image) || normalizedImage;
    const twitterImage = twitterImagePath.startsWith('http') ? twitterImagePath : `https://markhazleton.com/${twitterImagePath.replace(/^\/+/, '')}`;
    const twitterTitle = (project.twitter && project.twitter.title) || title;
    const twitterDescription = (project.twitter && project.twitter.description) || description;
    const twitterImageAlt = (project.twitter && project.twitter.imageAlt) || ogImageAlt;

    return {
        title,
        description,
        keywords,
        canonical,
        robots,
        author: project.author || 'Mark Hazleton',
        og: {
            title: ogTitle,
            description: ogDescription,
            type: ogType,
            url: canonical,
            image: normalizedImage,
            imageWidth: 1200,
            imageHeight: 630,
            imageAlt: ogImageAlt,
            siteName: 'Mark Hazleton',
            locale: 'en_US'
        },
        twitter: {
            card: 'summary_large_image',
            site: '@markhazleton',
            creator: '@markhazleton',
            title: twitterTitle,
            description: twitterDescription,
            image: twitterImage,
            imageAlt: twitterImageAlt
        },
        structuredData: {
            '@context': 'https://schema.org',
            '@type': 'CreativeWork',
            name: baseTitle,
            description,
            url: canonical,
            image: normalizedImage,
            keywords
        }
    };
}


module.exports = async function renderPug(filePath) {
    const destPath = filePath.replace(/src\/pug\//, 'docs/').replace(/\.pug$/, '.html');
    const srcPath = upath.resolve(upath.dirname(__filename), '../../src');

    // Derive the currentSlug
    const relativePath = upath.relative(upath.resolve(__dirname, '../../src/pug'), filePath);
    const currentSlug = relativePath.replace(/\.pug$/, '.html');

    // Get the last modified date of the file
    const fileStats = fs.statSync(filePath);

    // Find article - handle index.html mapping to folder paths
    let article = articles.find(article => article.slug === currentSlug);

    // If no article found and this is an index.html file, try mapping to folder path
    if (!article && currentSlug.endsWith('/index.html')) {
        const folderSlug = currentSlug.replace('/index.html', '/');
        article = articles.find(article => article.slug === folderSlug);
    }

    let project = null;
    const normalizedSlug = currentSlug.replace(/\\/g, '/');
    if (!article && normalizedSlug.startsWith('projects/')) {
        const projectSlugCandidate = normalizedSlug
            .replace(/^projects\//, '')
            .replace(/index\.html$/, '')
            .replace(/\.html$/, '')
            .replace(/\/$/, '');

        if (projectSlugCandidate) {
            project = projects.find(item => item.slug === projectSlugCandidate) || null;
        }
    }

    // Use Git to retrieve the last commit date for the file
    let lastModified;
    try {
        const gitCommand = `git log -1 --format=%cI -- ${filePath}`;
        lastModified = execSync(gitCommand).toString().trim();
    } catch (error) {
        console.error(`Error retrieving last modified date for ${filePath}:`, error.message);
        lastModified = new Date().toISOString(); // Fallback to current date
    }
    const formattedLastModified = new Date(lastModified).toLocaleDateString(); // Short date

    // Log build info if enabled
    if (seoHelper.defaultConfig.logBuild) {
        const buildLabel = article ? `(Article ID: ${article.id})` : project ? `(Project: ${project.slug})` : '(No content data)';
        console.log(`[BUILD] Rendering: ${currentSlug} ${buildLabel}`);
    }

    // Build source context for SEO validation
    const sourceContext = {
        pugFile: upath.relative(process.cwd(), filePath),
        buildStep: 'render-pug',
        timestamp: new Date().toISOString()
    };

    // Get SEO data for this article/page
    let seoData;
    if (article) {
        seoData = seoHelper.getArticleSEO(currentSlug, sourceContext);
    } else if (project) {
        seoData = buildProjectSEO(project);
    } else {
        seoData = seoHelper.getDefaultSEO();
    }

    // Get fresh build version data for this render
    const buildVersion = getBuildVersion();

    const html = pug.renderFile(filePath, {
        doctype: 'html',
        filename: filePath,
        basedir: srcPath,
        articles: articles,
        projects: projects,
        currentSlug: currentSlug,
        lastModified: formattedLastModified,
        article: article,
        project: project,
        seoData: seoData,
        seoHelper: seoHelper,
        buildVersion: buildVersion.version,
        buildDate: buildVersion.buildDate,
        buildNumber: buildVersion.buildNumber,
        // Article / project properties for template compatibility
        publishedDate: article ? article.publishedDate : (project ? (project.promotion && project.promotion.lastPromotedOn) || project.updatedOn || project.lastModified || null : null),
        pageAuthor: article ? article.author : (project && project.author ? project.author : 'Mark Hazleton'),
        estimatedReadTime: article ? article.estimatedReadTime : (project ? 3 : 5)
    });


    let highlightedHtml = html;
    // Start with your original HTML
    // Here you would find and replace code snippets with highlighted versions
    // This is a simplified example; you might need a more complex solution
    // based on your HTML structure and how code snippets are represented
    highlightedHtml = highlightedHtml.replace(/<code class="language-(\w+)">([\s\S]*?)<\/code>/g, (match, lang, code) => {
        const language = Prism.languages[lang] || Prism.languages.javascript;
        return `<code class="language-${lang}">${Prism.highlight(code, language, lang)}</code>`;
    });

    const destPathDirname = upath.dirname(destPath);
    if (!sh.test('-e', destPathDirname)) {
        sh.mkdir('-p', destPathDirname);
    }

    const prettified = await prettier.format(highlightedHtml, {
        printWidth: 1000,
        tabWidth: 4,
        singleQuote: true,
        proseWrap: 'preserve',
        endOfLine: 'lf',
        parser: 'html',
        htmlWhitespaceSensitivity: 'ignore'
    });

    fs.writeFileSync(destPath, prettified);
};
