const { execSync } = require('child_process');
const path = require('path');

console.log('Starting a simple HTTP server...');
console.log('The site should be available at http://localhost:8080');

try {
    // Install http-server if it's not already installed
    try {
        execSync('npx http-server --version', { stdio: 'ignore' });
    } catch (e) {
        console.log('Installing http-server...');
        execSync('npm install -g http-server', { stdio: 'inherit' });
    }

    // Change to the docs directory
    process.chdir(path.join(__dirname, 'docs'));

    // Run http-server
    execSync('npx http-server -o -p 8080', { stdio: 'inherit' });
} catch (error) {
    console.error('Error starting the server:', error.message);
}
