#!/usr/bin/env node

/**
 * Lighthouse Performance Fallback
 * Alternative performance testing when full Lighthouse fails on Windows
 */

const https = require('https');
const fs = require('fs');

class PerformanceFallback {
    constructor() {
        this.urls = [
            'https://markhazleton.com/',
            'https://markhazleton.com/articles.html',
            'https://markhazleton.com/web-project-mechanics.html'
        ];
    }

    async measurePageLoad(url) {
        return new Promise((resolve) => {
            const startTime = Date.now();

            const req = https.get(url, (res) => {
                let data = '';

                res.on('data', chunk => {
                    data += chunk;
                });

                res.on('end', () => {
                    const loadTime = Date.now() - startTime;

                    resolve({
                        url,
                        statusCode: res.statusCode,
                        loadTime,
                        contentLength: data.length,
                        hasTitle: data.includes('<title>'),
                        hasMetaDescription: data.includes('meta name="description"'),
                        hasViewport: data.includes('meta name="viewport"'),
                        isCompressed: res.headers['content-encoding'] === 'gzip',
                        cacheControl: res.headers['cache-control'] || 'none',
                        contentType: res.headers['content-type'] || 'unknown'
                    });
                });

            }).on('error', (err) => {
                resolve({
                    url,
                    error: err.message,
                    loadTime: Date.now() - startTime
                });
            });

            req.setTimeout(10000, () => {
                req.destroy();
                resolve({
                    url,
                    error: 'Timeout',
                    loadTime: 10000
                });
            });
        });
    }

    async runFallbackPerformanceTest() {
        console.log('🔄 Running fallback performance tests...');

        const results = [];

        for (const url of this.urls) {
            console.log(`📊 Testing ${url}...`);
            const result = await this.measurePageLoad(url);
            results.push(result);

            if (result.error) {
                console.log(`❌ ${url}: ${result.error}`);
            } else {
                console.log(`✅ ${url}: ${result.loadTime}ms, ${result.statusCode}, ${Math.round(result.contentLength/1024)}KB`);
            }
        }

        // Generate summary
        const successful = results.filter(r => !r.error);
        const avgLoadTime = successful.length > 0
            ? successful.reduce((sum, r) => sum + r.loadTime, 0) / successful.length
            : 0;

        const report = {
            timestamp: new Date().toISOString(),
            totalUrls: this.urls.length,
            successfulTests: successful.length,
            averageLoadTime: Math.round(avgLoadTime),
            results: results,
            summary: {
                allPagesReachable: successful.length === this.urls.length,
                averagePerformance: avgLoadTime < 3000 ? 'good' : avgLoadTime < 5000 ? 'fair' : 'poor',
                hasBasicSeo: successful.every(r => r.hasTitle && r.hasMetaDescription),
                hasViewport: successful.every(r => r.hasViewport),
                hasCompression: successful.some(r => r.isCompressed)
            }
        };

        // Save fallback performance report
        fs.writeFileSync(
            'artifacts/performance-fallback.json',
            JSON.stringify(report, null, 2)
        );

        console.log('📊 Fallback Performance Summary:');
        console.log(`   🌐 Pages tested: ${report.totalUrls}`);
        console.log(`   ✅ Successful: ${report.successfulTests}`);
        console.log(`   ⏱️  Average load time: ${report.averageLoadTime}ms`);
        console.log(`   📈 Performance rating: ${report.summary.averagePerformance}`);

        return report.successfulTests >= 2; // At least 2 out of 3 pages should work
    }
}

// Export for use in main test runner
module.exports = PerformanceFallback;

// CLI execution
if (require.main === module) {
    const fallback = new PerformanceFallback();

    fallback.runFallbackPerformanceTest()
        .then(success => {
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('❌ Fallback performance test failed:', error);
            process.exit(1);
        });
}
