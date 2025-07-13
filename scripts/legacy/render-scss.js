'use strict';
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');
const fs = require('fs');
const packageJSON = require('../package.json');
const upath = require('upath');
const postcss = require('postcss');
const sass = require('sass');
const sh = require('shelljs');

const stylesPath = upath.resolve(upath.dirname(__filename), '../src/scss/styles.scss');
const destPath = upath.resolve(upath.dirname(__filename), '../docs/css/styles.css');

module.exports = function renderSCSS() {
    // Use the new Dart Sass compile method with additional options to suppress warnings
    const result = sass.compile(stylesPath, {
        loadPaths: [
            upath.resolve(upath.dirname(__filename), '../node_modules')
        ],
        quietDeps: true,      // Suppress deprecation warnings from dependencies
        logger: {
            warn: function(message, options) {
                // Only show warnings that don't contain specific deprecation messages
                if (!message.includes('deprecated') && !message.includes('Deprecation')) {
                    console.warn(message);
                }
            }
        }
    });

    const destPathDirname = upath.dirname(destPath);
    if (!sh.test('-e', destPathDirname)) {
        sh.mkdir('-p', destPathDirname);
    }

    // Include cssnano in the PostCSS process for minification with explicit configuration
    postcss([autoprefixer, cssnano(
        {
            preset: 'default', // This is the default preset that provides good minification
        })]).process(result.css, { from: undefined }).then(output => {
            output.warnings().forEach(warn => {
                console.warn(warn.toString());
            });
            fs.writeFileSync(destPath, output.css.toString()); // Write the minified CSS to the destination file
        }).catch(error => {
            console.error('Error during CSS processing:', error); // Error handling for the PostCSS process
        });
};
