const crypto = require('crypto');
const fs = require('fs-extra');
const path = require('path');

/**
 * Build Cache Manager
 * Provides intelligent caching for build processes to avoid unnecessary rebuilds
 */
class BuildCache {
    constructor(options = {}) {
        this.cacheDir = options.cacheDir || '.build-cache';
        this.manifestPath = path.join(this.cacheDir, 'manifest.json');
        this.enabled = options.enabled !== false;
        this.ttl = options.ttl || (24 * 60 * 60 * 1000); // 24 hours default
        this.ensureCacheDir();
    }

    ensureCacheDir() {
        if (this.enabled) {
            fs.ensureDirSync(this.cacheDir);
        }
    }

    /**
     * Generate MD5 hash for file content
     */
    getFileHash(filePath) {
        if (!fs.existsSync(filePath)) return null;
        const content = fs.readFileSync(filePath);
        return crypto.createHash('md5').update(content).digest('hex');
    }

    /**
     * Get hash for multiple files (useful for dependency checking)
     */
    getMultiFileHash(filePaths) {
        const hashes = filePaths
            .filter(fp => fs.existsSync(fp))
            .map(fp => this.getFileHash(fp))
            .filter(Boolean);

        if (hashes.length === 0) return null;
        return crypto.createHash('md5').update(hashes.join('')).digest('hex');
    }

    /**
     * Check if source file(s) need rebuilding
     */
    shouldRebuild(sourceFile, targetFile, dependencies = []) {
        if (!this.enabled) return true;
        if (!fs.existsSync(targetFile)) return true;

        const manifest = this.loadManifest();
        const cacheKey = this.getCacheKey(sourceFile);
        const cachedEntry = manifest[cacheKey];

        if (!cachedEntry) return true;

        // Check if cache entry is expired
        if (this.isCacheExpired(cachedEntry.timestamp)) return true;

        // Check source file hash
        const currentHash = this.getFileHash(sourceFile);
        if (currentHash !== cachedEntry.sourceHash) return true;

        // Check dependencies hash
        if (dependencies.length > 0) {
            const currentDepHash = this.getMultiFileHash(dependencies);
            if (currentDepHash !== cachedEntry.dependencyHash) return true;
        }

        return false;
    }

    /**
     * Mark file as built and cache its state
     */
    markBuilt(sourceFile, targetFile, dependencies = []) {
        if (!this.enabled) return;

        const manifest = this.loadManifest();
        const cacheKey = this.getCacheKey(sourceFile);

        manifest[cacheKey] = {
            sourceFile,
            targetFile,
            sourceHash: this.getFileHash(sourceFile),
            dependencyHash: dependencies.length > 0 ? this.getMultiFileHash(dependencies) : null,
            timestamp: Date.now()
        };

        this.saveManifest(manifest);
    }

    /**
     * Get cache statistics
     */
    getStats() {
        if (!this.enabled) return { enabled: false };

        const manifest = this.loadManifest();
        const entries = Object.values(manifest);
        const expired = entries.filter(entry => this.isCacheExpired(entry.timestamp));

        return {
            enabled: true,
            totalEntries: entries.length,
            expiredEntries: expired.length,
            cacheSize: this.getCacheSize()
        };
    }

    /**
     * Clean expired cache entries
     */
    cleanExpired() {
        if (!this.enabled) return 0;

        const manifest = this.loadManifest();
        const before = Object.keys(manifest).length;

        Object.keys(manifest).forEach(key => {
            if (this.isCacheExpired(manifest[key].timestamp)) {
                delete manifest[key];
            }
        });

        this.saveManifest(manifest);
        const after = Object.keys(manifest).length;

        return before - after;
    }

    /**
     * Clear all cache
     */
    clear() {
        if (fs.existsSync(this.cacheDir)) {
            fs.removeSync(this.cacheDir);
        }
        this.ensureCacheDir();
    }

    /**
     * Private helper methods
     */
    getCacheKey(filePath) {
        return path.relative(process.cwd(), filePath).replace(/\\/g, '/');
    }

    isCacheExpired(timestamp) {
        return (Date.now() - timestamp) > this.ttl;
    }

    loadManifest() {
        if (!fs.existsSync(this.manifestPath)) return {};
        try {
            return fs.readJsonSync(this.manifestPath);
        } catch (error) {
            console.warn('⚠️  Cache manifest corrupted, clearing cache');
            return {};
        }
    }

    saveManifest(manifest) {
        try {
            fs.writeJsonSync(this.manifestPath, manifest, { spaces: 2 });
        } catch (error) {
            console.warn('⚠️  Failed to save cache manifest:', error.message);
        }
    }

    getCacheSize() {
        try {
            if (!fs.existsSync(this.cacheDir)) return '0 B';
            const stats = fs.statSync(this.manifestPath);
            const bytes = stats.size;

            if (bytes === 0) return '0 B';
            const k = 1024;
            const sizes = ['B', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
        } catch {
            return 'Unknown';
        }
    }
}

module.exports = BuildCache;
