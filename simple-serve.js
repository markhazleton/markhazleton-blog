const { execSync } = require('child_process');
const path = require('path');

console.log('Starting a simple web server...');
console.log('The site should open automatically in your browser.');
console.log('If it doesn\'t, try opening http://localhost:3000 manually');

try {
    // Change to the docs directory
    process.chdir(path.join(__dirname, 'docs'));

    // Run browser-sync using npx
    execSync('npx browser-sync start --server --files "*.html, css/*.css, js/*.js" --port 3000 --open',
        { stdio: 'inherit' });
} catch (error) {
    console.error('Error starting the server:', error.message);
}
