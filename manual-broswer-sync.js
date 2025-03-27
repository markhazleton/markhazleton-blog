const browserSync = require('browser-sync').create();

// Start the server
browserSync.init({
    server: './docs',
    port: 3000,
    open: true,
    files: ['./docs/**/*.html', './docs/css/**/*.css', './docs/js/**/*.js'],
    logLevel: 'info',
    ui: {
        port: 3001
    }
});

console.log('BrowserSync running at:');
console.log('- Local: http://localhost:3000');
console.log('- UI: http://localhost:3001');
