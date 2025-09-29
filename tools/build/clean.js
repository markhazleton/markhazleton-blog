const sh = require('shelljs');
const upath = require('upath');
const fs = require('fs');

// Resolve the root docs directory (two levels up from this file: tools/build -> project root)
const destPath = upath.resolve(upath.dirname(__filename), '../../docs');

// Safety guard: ensure we only ever clean a folder actually named 'docs'
if (upath.basename(destPath) !== 'docs') {
	console.error(`❌ Refusing to clean unexpected directory: ${destPath}`);
	process.exit(1);
}

if (!fs.existsSync(destPath)) {
	console.log('📁 docs directory does not exist yet, creating...');
	fs.mkdirSync(destPath, { recursive: true });
}

console.log('🧹 Cleaning docs folder...');
console.log(`📁 Removing all content from: ${destPath}`);

// Remove contents but keep the docs folder itself
sh.rm('-rf', upath.join(destPath, '*'));

// Recreate baseline folder structure needed early in the build
const baselineDirs = [
	'css',
	'js'
];

console.log('📂 Creating required folders...');
baselineDirs.forEach(dir => {
	const full = upath.join(destPath, dir);
	if (!fs.existsSync(full)) {
		fs.mkdirSync(full, { recursive: true });
	}
});

console.log('✅ Clean completed successfully (root docs directory)');
