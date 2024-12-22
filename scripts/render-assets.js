'use strict';
const fs = require('fs');
const upath = require('upath');
const sh = require('shelljs');
const glob = require("glob");

module.exports = function renderAssets() {
    // Define source and destination paths
    const sourcePath = upath.resolve(upath.dirname(__filename), '../src/assets');
    const destPath = upath.resolve(upath.dirname(__filename), '../docs/.');

    // Copy all assets from source to destination
    sh.cp('-R', sourcePath, destPath);

    const srcPath = upath.resolve(upath.dirname(__filename), "../src");

    // Copy all XML files maintaining the directory structure
    const xmlFiles = glob.sync("**/*.xml", { cwd: srcPath });
    xmlFiles.forEach((file) => {
        const sourceFile = upath.join(srcPath, file);
        const destFile = upath.join(destPath, file);
        sh.mkdir("-p", upath.dirname(destFile)); // Ensure the destination directory exists
        sh.cp(sourceFile, destFile);
    });

    // Function to copy a file with error handling
    const copyFile = (source, dest) => {
        try {
            sh.cp(source, dest);
        } catch (err) {
            console.error(`Error copying ${source} to ${dest}:`, err);
        }
    };

    // Copy specific files
    const filesToCopy = ["CNAME", "robots.txt", "web.config"];
    filesToCopy.forEach((file) => {
        const sourceFile = upath.join(srcPath, file);
        const destFile = upath.join(destPath, file);
        copyFile(sourceFile, destFile);
    });

    // Copy Bootstrap fonts
    const bsiSourcePath = upath.resolve(upath.dirname(__filename), '../node_modules/bootstrap-icons/font/fonts/');
    const bsiDestPath = upath.resolve(destPath, 'css/');
    sh.mkdir("-p", bsiDestPath);
    sh.cp('-R', bsiSourcePath, bsiDestPath);

    // Copy FontAwesome fonts
    const fontSourcePath = upath.resolve(upath.dirname(__filename), '../node_modules/fontawesome-free/webfonts/');
    const fontDestPath = upath.resolve(destPath, '');
    sh.mkdir("-p", fontDestPath);
    sh.cp('-R', fontSourcePath, fontDestPath);

    // Copy Devicon fonts
    const deviconFontSourcePath = upath.resolve(upath.dirname(__filename), '../node_modules/devicon/fonts/');
    const deviconFontDestPath = upath.resolve(destPath, 'css/');
    sh.mkdir("-p", deviconFontDestPath);
    sh.cp('-R', deviconFontSourcePath, deviconFontDestPath);

    // Move any files in the root of the src folder to the dest folder
    const rootFiles = glob.sync("*.*", { cwd: srcPath, nodir: true });
    rootFiles.forEach((file) => {
        const sourceFile = upath.join(srcPath, file);
        const destFile = upath.join(destPath, file);
        copyFile(sourceFile, destFile);
    });
};
