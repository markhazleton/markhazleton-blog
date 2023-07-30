const sh = require('shelljs');
const upath = require('upath');
const fs = require('fs');

const destPath = upath.resolve(upath.dirname(__filename), '../docs');

// Clean the entire 'docs' folder
sh.rm('-rf', `${destPath}/*`);

// Create empty 'css' and 'js' folders inside the 'docs' folder
const cssFolder = upath.join(destPath, 'css');
const jsFolder = upath.join(destPath, 'js');

fs.mkdirSync(cssFolder);
fs.mkdirSync(jsFolder);
