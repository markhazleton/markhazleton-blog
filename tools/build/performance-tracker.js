const fs = require('fs-extra');
const path = require('path');

/**
 * Performance Tracker for Build System
 * Tracks and reports build performance metrics
 */
class PerformanceTracker {
    constructor(options = {}) {
        this.enabled = options.enabled !== false;
        this.reportPath = options.reportPath || '.build-cache/performance.json';
        this.metrics = {};
        this.startTimes = {};
        this.buildStartTime = null;

        if (this.enabled) {
            fs.ensureDirSync(path.dirname(this.reportPath));
        }
    }

    /**
     * Start timing a task
     */
    start(taskName) {
        if (!this.enabled) return;

        this.startTimes[taskName] = process.hrtime.bigint();

        // Track overall build start time
        if (!this.buildStartTime) {
            this.buildStartTime = this.startTimes[taskName];
        }
    }

    /**
     * End timing a task
     */
    end(taskName) {
        if (!this.enabled) return;

        const startTime = this.startTimes[taskName];
        if (!startTime) {
            console.warn(`⚠️  No start time found for task: ${taskName}`);
            return;
        }

        const duration = Number(process.hrtime.bigint() - startTime) / 1000000; // Convert to milliseconds
        this.metrics[taskName] = {
            duration,
            timestamp: new Date().toISOString()
        };

        delete this.startTimes[taskName];

        // Log individual task completion
        const seconds = (duration / 1000).toFixed(2);
        console.log(`⏱️  ${taskName} completed in ${seconds}s`);
    }

    /**
     * Mark a task as cached (no actual work done)
     */
    markCached(taskName) {
        if (!this.enabled) return;

        this.metrics[taskName] = {
            duration: 0,
            cached: true,
            timestamp: new Date().toISOString()
        };

        console.log(`💾 ${taskName} (cached)`);
    }

    /**
     * Get total build time
     */
    getTotalTime() {
        return Object.values(this.metrics)
            .filter(metric => !metric.cached)
            .reduce((sum, metric) => sum + metric.duration, 0);
    }

    /**
     * Get cache hit rate
     */
    getCacheHitRate() {
        const total = Object.keys(this.metrics).length;
        if (total === 0) return 0;

        const cached = Object.values(this.metrics).filter(m => m.cached).length;
        return (cached / total) * 100;
    }

    /**
     * Display performance report
     */
    report() {
        if (!this.enabled) return;

        const totalTime = this.getTotalTime();
        const cacheHitRate = this.getCacheHitRate();

        console.log('\n📊 Build Performance Report');
        console.log('================================');

        // Sort tasks by duration (excluding cached)
        const taskMetrics = Object.entries(this.metrics)
            .sort(([,a], [,b]) => (b.duration || 0) - (a.duration || 0));

        taskMetrics.forEach(([task, metric]) => {
            const seconds = ((metric.duration || 0) / 1000).toFixed(2);
            const percentage = totalTime > 0 ? ((metric.duration || 0) / totalTime * 100).toFixed(1) : '0.0';
            const status = metric.cached ? '💾' : '⚡';
            const timeDisplay = metric.cached ? 'cached' : `${seconds}s`;
            const percentDisplay = metric.cached ? '' : ` (${percentage}%)`;

            console.log(`${status} ${task.padEnd(15)} ${timeDisplay}${percentDisplay}`);
        });

        console.log('================================');
        console.log(`🏁 Total: ${(totalTime / 1000).toFixed(2)}s`);

        if (cacheHitRate > 0) {
            console.log(`💾 Cache hit rate: ${cacheHitRate.toFixed(1)}%`);
        }

        console.log('');
    }

    /**
     * Save performance data to file
     */
    saveToFile() {
        if (!this.enabled) return;

        const timestamp = new Date().toISOString();
        const totalTime = this.getTotalTime();
        const cacheHitRate = this.getCacheHitRate();

        const report = {
            timestamp,
            totalTime,
            cacheHitRate,
            metrics: this.metrics,
            summary: {
                totalTasks: Object.keys(this.metrics).length,
                cachedTasks: Object.values(this.metrics).filter(m => m.cached).length,
                totalDuration: totalTime,
                averageTaskTime: totalTime / Math.max(1, Object.values(this.metrics).filter(m => !m.cached).length)
            }
        };

        try {
            // Load existing history
            let history = [];
            if (fs.existsSync(this.reportPath)) {
                const existing = fs.readJsonSync(this.reportPath);
                history = Array.isArray(existing) ? existing : [existing];
            }

            // Add current report
            history.push(report);

            // Keep only last 50 builds
            if (history.length > 50) {
                history = history.slice(-50);
            }

            fs.writeJsonSync(this.reportPath, history, { spaces: 2 });
        } catch (error) {
            console.warn('⚠️  Failed to save performance report:', error.message);
        }
    }

    /**
     * Get performance trends from history
     */
    getTrends() {
        if (!this.enabled || !fs.existsSync(this.reportPath)) {
            return { available: false };
        }

        try {
            const history = fs.readJsonSync(this.reportPath);
            const reports = Array.isArray(history) ? history : [history];

            if (reports.length < 2) {
                return { available: false, reason: 'Insufficient data' };
            }

            const recent = reports.slice(-10); // Last 10 builds
            const avgTime = recent.reduce((sum, r) => sum + r.totalTime, 0) / recent.length;
            const avgCacheRate = recent.reduce((sum, r) => sum + (r.cacheHitRate || 0), 0) / recent.length;

            return {
                available: true,
                averageBuildTime: avgTime,
                averageCacheHitRate: avgCacheRate,
                totalBuilds: reports.length,
                recentBuilds: recent.length
            };
        } catch (error) {
            return { available: false, reason: error.message };
        }
    }

    /**
     * Display trends if available
     */
    showTrends() {
        const trends = this.getTrends();

        if (!trends.available) {
            return;
        }

        console.log('📈 Performance Trends (last 10 builds)');
        console.log('======================================');
        console.log(`⏱️  Average build time: ${(trends.averageBuildTime / 1000).toFixed(2)}s`);
        console.log(`💾 Average cache hit rate: ${trends.averageCacheHitRate.toFixed(1)}%`);
        console.log(`📊 Total builds tracked: ${trends.totalBuilds}`);
        console.log('');
    }
}

module.exports = PerformanceTracker;
