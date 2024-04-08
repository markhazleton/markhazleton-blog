'use strict';

const fs = require('fs');
const fse = require('fs');
const upath = require('upath');
const pug = require('pug');
const sh = require('shelljs');
const prettier = require('prettier');
const Prism = require('prismjs');
require('prismjs/components/')(); // Load all languages

// Get List of Articles
const articles = require('./../src/articles.json');
const projects = require('./../src/projects.json');

module.exports = async function renderPug(filePath) {
    const destPath = filePath.replace(/src\/pug\//, 'docs/').replace(/\.pug$/, '.html');
    const srcPath = upath.resolve(upath.dirname(__filename), '../src');

    const html = pug.renderFile(filePath, {
        doctype: 'html',
        filename: filePath,
        basedir: srcPath,
        articles: articles,
        projects: projects
    });

    let highlightedHtml = html; // Start with your original HTML
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
