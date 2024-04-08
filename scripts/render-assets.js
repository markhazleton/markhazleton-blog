'use strict';
const fs = require('fs');
const upath = require('upath');
const sh = require('shelljs');
const glob = require("glob");
const path = require("path");


module.exports = function renderAssets() {
    const sourcePath = upath.resolve(upath.dirname(__filename), '../src/assets');
    const destPath = upath.resolve(upath.dirname(__filename), '../docs/.');
    sh.cp('-R', sourcePath, destPath)

    const srcPath = upath.resolve(
        upath.dirname(__filename),
        "../src"
    );

    const xmlFiles = glob.sync("**/*.xml", { cwd: srcPath });
    xmlFiles.forEach((file) => {
        const sourceFile = path.join(srcPath, file);
        const destFile = path.join(destPath, file);
        sh.mkdir("-p", path.dirname(destFile));
        sh.cp(sourceFile, destFile);
    });

    const sourceCNAMEFile = path.join(srcPath, "CNAME");
    const destCNAMEFile = path.join(destPath, "CNAME");
    sh.cp(sourceCNAMEFile, destCNAMEFile);

    const sourcerobotsFile = path.join(srcPath, "robots.txt");
    const destrobotsFile = path.join(destPath, "robots.txt");
    sh.cp(sourcerobotsFile, destrobotsFile);

    const sourceConfigFile = path.join(srcPath, "web.config");
    const destConfigFile = path.join(destPath, "web.config");
    sh.cp(sourceConfigFile, destConfigFile);


    // Get the fontawesome fonts
    const fontSourcePath = upath.resolve(upath.dirname(__filename), '../node_modules/fontawesome-free/webfonts/');
    const fontDestPath = upath.resolve(destPath);
    sh.mkdir("-p", fontDestPath); // Ensure the destination directory exists
    sh.cp('-R', fontSourcePath, fontDestPath);

    // Get the devicon fonts
    const deviconfontSourcePath = upath.resolve(upath.dirname(__filename), '../node_modules/devicon/fonts/');
    const deviconfontDestPath = upath.resolve(destPath, 'css/');
    sh.mkdir("-p", deviconfontDestPath);
    sh.cp('-R', deviconfontSourcePath, deviconfontDestPath);

};
