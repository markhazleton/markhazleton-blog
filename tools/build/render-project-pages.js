'use strict';

const fs = require('fs');
const upath = require('upath');
const pug = require('pug');
const sh = require('shelljs');
const prettier = require('prettier');
const Prism = require('prismjs');
require('prismjs/components/')();
const { execSync } = require('child_process');

const srcRoot = upath.resolve(__dirname, '../../src');
const articlesPath = upath.join(srcRoot, 'articles.json');
const projectsPath = upath.join(srcRoot, 'projects.json');

const SEOHelper = require('./seo-helper');

let cachedArticles = null;
let cachedProjects = null;
let cachedProjectsModified = null;

function readJson(filePath) {
    return JSON.parse(fs.readFileSync(filePath, 'utf8'));
}

function getArticles() {
    if (!cachedArticles) {
        cachedArticles = readJson(articlesPath);
    }
    return cachedArticles;
}

function getProjects() {
    if (!cachedProjects) {
        cachedProjects = readJson(projectsPath);
    }
    return cachedProjects;
}

const seoHelper = new SEOHelper(getArticles(), {
    logWarnings: true,
    logErrors: true,
    logInfo: false,
    logScores: false,
    logBuild: false
});

function getProjectsJsonModified() {
    if (cachedProjectsModified) {
        return cachedProjectsModified;
    }

    try {
        const result = execSync(`git log -1 --format=%cI -- ${projectsPath}`);
        cachedProjectsModified = result.toString().trim() || new Date().toISOString();
    } catch (error) {
        cachedProjectsModified = new Date().toISOString();
    }

    return cachedProjectsModified;
}

function getProjectLastModified(project) {
    const candidate = project.promotion?.lastPromotedOn || project.updatedOn || project.lastModified;
    if (candidate) {
        const parsed = new Date(candidate);
        if (!Number.isNaN(parsed.getTime())) {
            return {
                iso: parsed.toISOString(),
                display: parsed.toLocaleDateString()
            };
        }
    }

    const fallback = new Date(getProjectsJsonModified());
    return {
        iso: fallback.toISOString(),
        display: fallback.toLocaleDateString()
    };
}

function highlightCode(html) {
    return html.replace(/<code class="language-(\w+)">([\s\S]*?)<\/code>/g, (match, lang, code) => {
        const language = Prism.languages[lang] || Prism.languages.javascript;
        return `<code class="language-${lang}">${Prism.highlight(code, language, lang)}</code>`;
    });
}

async function renderProjectPage({ templatePath, project, destPath, currentSlug }) {
    const articles = getArticles();
    const projects = getProjects();
    const lastModified = getProjectLastModified(project);

    const html = pug.renderFile(templatePath, {
        doctype: 'html',
        filename: templatePath,
        basedir: srcRoot,
        articles,
        projects,
        project,
        currentSlug,
        lastModified: lastModified.display,
        seoHelper,
        pageAuthor: 'Mark Hazleton',
        publishedDate: lastModified.iso,
        estimatedReadTime: 3
    });

    const highlightedHtml = highlightCode(html);

    const prettified = await prettier.format(highlightedHtml, {
        printWidth: 1000,
        tabWidth: 4,
        singleQuote: true,
        proseWrap: 'preserve',
        endOfLine: 'lf',
        parser: 'html',
        htmlWhitespaceSensitivity: 'ignore'
    });

    const destDir = upath.dirname(destPath);
    if (!sh.test('-e', destDir)) {
        sh.mkdir('-p', destDir);
    }

    fs.writeFileSync(destPath, prettified);
}

module.exports = renderProjectPage;
