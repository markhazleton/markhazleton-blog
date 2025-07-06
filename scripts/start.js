const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

console.log('Starting Mark Hazleton Blog...');
console.log('Building site and starting development server...');

// Check if docs directory exists
const docsDir = path.join(__dirname, '..', 'docs');
if (!fs.existsSync(docsDir)) {
    console.error('docs directory not found! Please run: npm run build');
    process.exit(1);
}

try {
    // Change to the docs directory
    process.chdir(docsDir);

    // Start browser-sync server
    const command = `npx browser-sync start --server --files "*.html, css/*.css, js/*.js" --port 3000 --open --no-notify`;

    console.log('🚀 Starting development server on http://localhost:3000');
    console.log('📁 Serving files from docs directory');
    console.log('👀 Watching for file changes...');

    execSync(command, { stdio: 'inherit' });
} catch (error) {
    console.error('❌ Error starting the development server:', error.message);
    console.log('Make sure browser-sync is installed: npm install');
    process.exit(1);
}
