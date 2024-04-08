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

    // Path to Bootstrap JS in node_modules (adjust according to your Bootstrap version)
    const bootstrapJSPath = upath.resolve(upath.dirname(__filename), '../node_modules/bootstrap/dist/js/bootstrap.bundle.min.js');

    // Paths for scripts.js and its destination
    const sourcePathScriptsJS = upath.resolve(upath.dirname(__filename), '../src/js/scripts.js');
    const destPathScriptsJS = upath.resolve(upath.dirname(__filename), '../docs/js/scripts.js');

    // Read the Bootstrap JS and your scripts.js content
    const [bootstrapJS, scriptsJS] = await Promise.all([
        fs.readFile(bootstrapJSPath, 'utf8'),
        fs.readFile(sourcePathScriptsJS, 'utf8')
    ]);

    // Combine Bootstrap JS and your scripts.js
    const combinedScripts = bootstrapJS + '\n' + scriptsJS;

    // Include copyright notice
    const copyrightNotice = `/*!
* Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
* Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
* Licensed under ${packageJSON.license} (https://github.com/StartBootstrap/${packageJSON.name}/blob/master/LICENSE)
*/
`;

    // Minify and other operations follow as before...
    const minified = await Terser.minify(combinedScripts);
    if (minified.error) {
        console.error("Terser Error: ", minified.error);
        return;
    }

    await fs.writeFile(destPathScriptsJS, copyrightNotice + minified.code);
};
