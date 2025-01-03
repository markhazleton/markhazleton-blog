'use strict';

const fs = require('fs').promises;
const packageJSON = require('../package.json');
const upath = require('upath');
const sh = require('shelljs');
const Terser = require('terser');

module.exports = async function renderScripts() {
    const sourcePath = upath.resolve(upath.dirname(__filename), '../src/js');
    const destPath = upath.resolve(upath.dirname(__filename), '../docs/.');

    sh.cp('-R', sourcePath, destPath);

    // Path to Bootstrap JS in node_modules
    const bootstrapJSPath = upath.resolve(upath.dirname(__filename), '../node_modules/bootstrap/dist/js/bootstrap.bundle.min.js');

    // Path to PrismJS and the language components in node_modules
    const prismJSPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/prism.js');
    const prismYAMLPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-yaml.min.js');
    const prismXMLPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-xml-doc.min.js');
    const prismPUGPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-pug.min.js');
    const prismCSHARPPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-csharp.min.js');
    const prismPythonPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-python.min.js');
    const prismBashPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-bash.min.js');
    const prismTypeScriptPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-typescript.min.js');
    const prismSCSSPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-scss.min.js'); // SCSS
    const prismJSONPath = upath.resolve(upath.dirname(__filename), '../node_modules/prismjs/components/prism-json.min.js'); // JSON

    // Paths for scripts.js and its destination
    const sourcePathScriptsJS = upath.resolve(upath.dirname(__filename), '../src/js/scripts.js');
    const destPathScriptsJS = upath.resolve(upath.dirname(__filename), '../docs/js/scripts.js');

    // Read all JS files, including the new SCSS and JSON components
    const [
        bootstrapJS, prismJS, prismYAML, prismXML, prismPUG, prismCSHARP,
        prismPython, prismBash, prismTypeScript, prismSCSS, prismJSON,
        scriptsJS
    ] = await Promise.all([
        fs.readFile(bootstrapJSPath, 'utf8'),
        fs.readFile(prismJSPath, 'utf8'),
        fs.readFile(prismYAMLPath, 'utf8'),
        fs.readFile(prismXMLPath, 'utf8'),
        fs.readFile(prismPUGPath, 'utf8'),
        fs.readFile(prismCSHARPPath, 'utf8'),
        fs.readFile(prismPythonPath, 'utf8'),
        fs.readFile(prismBashPath, 'utf8'),
        fs.readFile(prismTypeScriptPath, 'utf8'),
        fs.readFile(prismSCSSPath, 'utf8'), // SCSS
        fs.readFile(prismJSONPath, 'utf8'), // JSON
        fs.readFile(sourcePathScriptsJS, 'utf8')
    ]);

    // Combine all scripts
    const combinedScripts = [
        bootstrapJS, prismJS, prismYAML, prismXML, prismPUG, prismCSHARP,
        prismPython, prismBash, prismTypeScript, prismSCSS, prismJSON, scriptsJS
    ].join('\n');

    // Include copyright notice
    const copyrightNotice = `/*!
* Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
* Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
* Licensed under ${packageJSON.license} (https://github.com/StartBootstrap/${packageJSON.name}/blob/master/LICENSE)
*/
`;

    // Minify combined scripts and handle errors
    const minified = await Terser.minify(combinedScripts);
    if (minified.error) {
        console.error("Terser Error: ", minified.error);
        return;
    }

    await fs.writeFile(destPathScriptsJS, copyrightNotice + minified.code);
};
