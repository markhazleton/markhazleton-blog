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
const articles = require('./../src/articles.json');
const projects = require('./../src/projects.json');

// Import SEO Helper
const SEOHelper = require('./seo-helper');
const seoHelper = new SEOHelper(articles, {
    logWarnings: true,
    logErrors: true,
    logInfo: false,
    logScores: false,
    logBuild: false
});

module.exports = async function renderPug(filePath) {
    const destPath = filePath.replace(/src\/pug\//, 'docs/').replace(/\.pug$/, '.html');
    const srcPath = upath.resolve(upath.dirname(__filename), '../src');

    // Derive the currentSlug
    const relativePath = upath.relative(upath.resolve(__dirname, '../src/pug'), filePath);
    const currentSlug = relativePath.replace(/\.pug$/, '.html');

    // Get the last modified date of the file
    const fileStats = fs.statSync(filePath);
    const article = articles.find(article => article.slug === currentSlug);

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
        console.log(`[BUILD] Rendering: ${currentSlug} ${article ? `(Article ID: ${article.id})` : '(No article data)'}`);
    }

    // Build source context for SEO validation
    const sourceContext = {
        pugFile: upath.relative(process.cwd(), filePath),
        buildStep: 'render-pug',
        timestamp: new Date().toISOString()
    };

    // Get SEO data for this article/page
    const seoData = article ? seoHelper.getArticleSEO(currentSlug, sourceContext) : seoHelper.getDefaultSEO();

    const html = pug.renderFile(filePath, {
        doctype: 'html',
        filename: filePath,
        basedir: srcPath,
        articles: articles,
        projects: projects,
        currentSlug: currentSlug,
        lastModified: formattedLastModified,
        article: article,
        seoData: seoData,
        seoHelper: seoHelper
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
