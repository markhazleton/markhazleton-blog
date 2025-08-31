const chokidar = require('chokidar');

console.log('🔍 Testing file watcher...');

// Watch the specific file instead of a glob pattern
const watcher = chokidar.watch('src/pug/articles.pug', {
    persistent: true,
    ignoreInitial: true,
    usePolling: true,
    interval: 1000,
    binaryInterval: 1000,
    atomic: true
});

watcher.on('change', (path) => {
    console.log(`✅ File changed: ${path}`);
});

watcher.on('add', (path) => {
    console.log(`➕ File added: ${path}`);
});

watcher.on('unlink', (path) => {
    console.log(`➖ File removed: ${path}`);
});

watcher.on('error', error => {
    console.error(`❌ Watcher error: ${error}`);
});

watcher.on('ready', () => {
    console.log('👀 Watcher ready. Make a change to any .pug file...');
});

console.log('Press Ctrl+C to stop...');
