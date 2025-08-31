const { execSync, spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

console.log('🚀 Starting Mark Hazleton Blog Development Server...');

// Check if docs directory exists
const docsDir = path.join(__dirname, '..', 'docs');
if (!fs.existsSync(docsDir)) {
    console.error('📁 docs directory not found! Building site first...');

    try {
        console.log('🔨 Running build process...');
        execSync('npm run build', { stdio: 'inherit', cwd: path.join(__dirname, '..') });
        console.log('✅ Build completed successfully');
    } catch (error) {
        console.error('❌ Build failed:', error.message);
        process.exit(1);
    }
}

// Check for watch mode argument
const args = process.argv.slice(2);
const watchMode = args.includes('--watch') || args.includes('-w');

if (watchMode) {
    console.log('👀 Starting in watch mode...');

    // Import and start the watcher
    const BlogBuilder = require('./build');
    const DevelopmentWatcher = require('../build/utils/development-watcher');

    try {
        const builder = new BlogBuilder({ noCache: false });
        const watcher = new DevelopmentWatcher(builder, {
            debounceMs: 500,
            usePolling: true,
            interval: 1000
        });

        watcher.start();

        // Graceful shutdown
        process.on('SIGINT', () => {
            console.log('\n🛑 Shutting down watch mode...');
            watcher.stop();
            process.exit(0);
        });

        process.on('SIGTERM', () => {
            console.log('\n🛑 Shutting down watch mode...');
            watcher.stop();
            process.exit(0);
        });

    } catch (error) {
        console.error('❌ Failed to start watch mode:', error.message);
        console.log('📝 Falling back to standard mode...');
    }
}

try {
    // Change to the docs directory
    process.chdir(docsDir);

    // Prepare browser-sync command
    const command = 'npx';
    const args = [
        'browser-sync', 'start',
        '--server',
        '--files', '*.html,css/*.css,js/*.js',
        '--port', '3000',
        '--open',
        '--no-notify',
        '--no-ui'
    ];

    console.log('🌐 Starting development server on http://localhost:3000');
    console.log('📁 Serving files from docs directory');

    if (watchMode) {
        console.log('👀 File watcher is active - changes will trigger rebuilds');
    } else {
        console.log('📝 Manual rebuild required when source files change');
        console.log('💡 Use "npm start -- --watch" for automatic rebuilds');
    }

    // Start browser-sync
    const browserSync = spawn(command, args, {
        stdio: 'inherit',
        shell: true
    });

    // Handle process termination
    process.on('SIGINT', () => {
        console.log('\n🛑 Shutting down development server...');
        browserSync.kill();
        process.exit(0);
    });

    process.on('SIGTERM', () => {
        console.log('\n🛑 Shutting down development server...');
        browserSync.kill();
        process.exit(0);
    });

    browserSync.on('close', (code) => {
        if (code !== 0) {
            console.error(`❌ Development server exited with code ${code}`);
        } else {
            console.log('✅ Development server stopped');
        }
        process.exit(code);
    });

} catch (error) {
    console.error('❌ Error starting the development server:', error.message);
    console.log('💡 Make sure browser-sync is installed: npm install');
    process.exit(1);
}
