const sh = require('shelljs');
const upath = require('upath');
const fs = require('fs');

const destPath = upath.resolve(upath.dirname(__filename), '../docs');

console.log('🧹 Cleaning docs folder...');
console.log(`📁 Removing all content from: ${destPath}`);

// Clean the entire 'docs' folder
sh.rm('-rf', `${destPath}/*`);

// Create empty 'css' and 'js' folders inside the 'docs' folder
const cssFolder = upath.join(destPath, 'css');
const jsFolder = upath.join(destPath, 'js');

console.log('📂 Creating required folders...');
fs.mkdirSync(cssFolder);
fs.mkdirSync(jsFolder);

console.log('✅ Clean completed successfully');
