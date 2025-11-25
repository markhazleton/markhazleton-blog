#!/usr/bin/env node

/**
 * Build Version Incrementor
 * Increments the patch version and updates the build date
 * Usage: node tools/build/increment-version.js
 */

const fs = require('fs');
const path = require('path');

const versionFilePath = path.resolve(__dirname, '../../build-version.json');

function incrementVersion() {
    try {
        // Read current version data
        let versionData;
        if (fs.existsSync(versionFilePath)) {
            versionData = JSON.parse(fs.readFileSync(versionFilePath, 'utf8'));
        } else {
            // Initialize if file doesn't exist
            versionData = {
                version: '1.0.0',
                buildDate: '',
                buildNumber: 0
            };
        }

        // Parse version string (e.g., "1.0.0" -> [1, 0, 0])
        const versionParts = versionData.version.split('.').map(Number);

        // Increment patch version (third number)
        versionParts[2] = (versionParts[2] || 0) + 1;

        // Update version data
        versionData.version = versionParts.join('.');
        versionData.buildNumber = (versionData.buildNumber || 0) + 1;
        versionData.buildDate = new Date().toISOString();

        // Write updated version back to file
        fs.writeFileSync(versionFilePath, JSON.stringify(versionData, null, 2), 'utf8');

        console.log(`✅ Version incremented to v${versionData.version} (Build #${versionData.buildNumber})`);
        console.log(`📅 Build date: ${new Date(versionData.buildDate).toLocaleString()}`);

        return versionData;
    } catch (error) {
        console.error('❌ Error incrementing version:', error.message);
        throw error;
    }
}

// Run if called directly
if (require.main === module) {
    incrementVersion();
}

module.exports = incrementVersion;
