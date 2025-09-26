#!/usr/bin/env node

/**
 * Lighthouse Windows Permission Fix
 * Handles temp directory permissions on Windows
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🔧 Fixing Lighthouse Windows Permissions...');

try {
    // 1. Kill any running Chrome processes
    console.log('🛑 Stopping Chrome processes...');
    try {
        execSync('taskkill /f /im chrome.exe /t', { stdio: 'ignore' });
    } catch (e) {
        console.log('   No Chrome processes found');
    }

    // 2. Clear lighthouse temp files
    console.log('🧹 Cleaning lighthouse temp files...');
    const tempDir = process.env.TEMP || process.env.TMP;
    try {
        execSync(`powershell -Command "Get-ChildItem '${tempDir}\\lighthouse*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue"`, { stdio: 'ignore' });
    } catch (e) {
        console.log('   Temp cleanup completed');
    }

    // 3. Set environment variables for better Chrome behavior
    process.env.CHROME_DEVEL_SANDBOX = '/opt/google/chrome/chrome-sandbox';
    process.env.PUPPETEER_DISABLE_DEV_SHM_USAGE = 'true';

    // 4. Try lighthouse with minimal configuration
    console.log('🚀 Running Lighthouse with Windows compatibility...');

    const lighthouseCmd = [
        'npx lighthouse',
        'https://markhazleton.com/',
        '--output=json',
        '--output-path=./artifacts/lighthouse-fixed.json',
        '--chrome-flags="--headless --no-sandbox --disable-dev-shm-usage --disable-gpu --no-first-run --disable-extensions --disable-default-apps"',
        '--max-wait-for-load=45000',
        '--timeout=90000'
    ].join(' ');

    const result = execSync(lighthouseCmd, {
        encoding: 'utf8',
        timeout: 120000,
        stdio: ['pipe', 'pipe', 'pipe']
    });

    console.log('✅ Lighthouse completed successfully!');
    console.log('📄 Report saved to: ./artifacts/lighthouse-fixed.json');

    // 5. Show basic performance metrics if available
    try {
        const report = JSON.parse(fs.readFileSync('./artifacts/lighthouse-fixed.json', 'utf8'));
        const metrics = report.lhr.audits;

        console.log('\n📊 Performance Metrics:');
        console.log('========================');
        if (metrics['first-contentful-paint']) {
            console.log(`🎨 First Contentful Paint: ${metrics['first-contentful-paint'].displayValue}`);
        }
        if (metrics['largest-contentful-paint']) {
            console.log(`📏 Largest Contentful Paint: ${metrics['largest-contentful-paint'].displayValue}`);
        }
        if (metrics['speed-index']) {
            console.log(`⚡ Speed Index: ${metrics['speed-index'].displayValue}`);
        }

        const performanceScore = report.lhr.categories.performance.score * 100;
        console.log(`🏆 Performance Score: ${Math.round(performanceScore)}/100`);

    } catch (e) {
        console.log('📊 Report generated but unable to parse metrics');
    }

} catch (error) {
    console.error('❌ Lighthouse fix failed:', error.message);
    console.log('\n💡 Alternative options:');
    console.log('1. Run: npm run audit:seo && npm run audit:ssl');
    console.log('2. Use online tools: https://pagespeed.web.dev/');
    console.log('3. Try running as Administrator');
    process.exit(1);
}
