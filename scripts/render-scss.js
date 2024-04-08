'use strict';
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');
const fs = require('fs');
const packageJSON = require('../package.json');
const upath = require('upath');
const postcss = require('postcss');
const sass = require('sass');
const sh = require('shelljs');

const stylesPath = '../src/scss/styles.scss';
const destPath = upath.resolve(upath.dirname(__filename), '../docs/css/styles.css');

module.exports = function renderSCSS() {

    const results = sass.renderSync({
        data: entryPoint,
        includePaths: [
            upath.resolve(upath.dirname(__filename), '../node_modules')
        ],
    });

    const destPathDirname = upath.dirname(destPath);
    if (!sh.test('-e', destPathDirname)) {
        sh.mkdir('-p', destPathDirname);
    }

    // Include cssnano in the PostCSS process for minification with explicit configuration
    postcss([autoprefixer, cssnano(
        {
            preset: 'default', // This is the default preset that provides good minification. You can choose other presets or customize the options as needed.
        })]).process(results.css, { from: undefined }).then(result => {
            result.warnings().forEach(warn => {
                console.warn(warn.toString());
            });
            fs.writeFileSync(destPath, result.css.toString()); // Write the minified CSS to the destination file
        }).catch(error => {
            console.error('Error during CSS processing:', error); // Error handling for the PostCSS process
        });

};

const entryPoint = `/*!
* Start Bootstrap - ${packageJSON.title} v${packageJSON.version} (${packageJSON.homepage})
* Copyright 2013-${new Date().getFullYear()} ${packageJSON.author}
* Licensed under ${packageJSON.license} (https://github.com/StartBootstrap/${packageJSON.name}/blob/master/LICENSE)
*/
@import "${stylesPath}"
`
