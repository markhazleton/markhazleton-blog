#!/usr/bin/env node

/**
 * Font Awesome Audit Tool (Standalone)
 * Scans PUG source files for Font Awesome class references
 * No external dependencies required
 */

const fs = require('fs');
const path = require('path');

// Font Awesome to Bootstrap Icons mapping
const iconMappings = {
    // Brand icons
    'fab fa-linkedin-in': 'bi bi-linkedin',
    'fab fa-linkedin': 'bi bi-linkedin',
    'fab fa-github': 'bi bi-github',
    'fab fa-youtube': 'bi bi-youtube',
    'fab fa-instagram': 'bi bi-instagram',
    'fab fa-twitter': 'bi bi-twitter',
    'fab fa-facebook': 'bi bi-facebook',
    'fab fa-microsoft': 'bi bi-microsoft',
    'fab fa-react': 'bi bi-react',
    'fab fa-bootstrap': 'bi bi-bootstrap',
    'fab fa-node-js': 'bi bi-nodejs',
    'fab fa-js': 'bi bi-filetype-js',
    'fab fa-git-alt': 'bi bi-git',

    // Solid icons
    'fas fa-search': 'bi bi-search',
    'fas fa-rss': 'bi bi-rss',
    'fas fa-info-circle': 'bi bi-info-circle',
    'fas fa-external-link-alt': 'bi bi-box-arrow-up-right',
    'fas fa-home': 'bi bi-house',
    'fas fa-user': 'bi bi-person',
    'fas fa-envelope': 'bi bi-envelope',
    'fas fa-phone': 'bi bi-telephone',
    'fas fa-calendar': 'bi bi-calendar',
    'fas fa-clock': 'bi bi-clock',
    'fas fa-download': 'bi bi-download',
    'fas fa-upload': 'bi bi-upload',
    'fas fa-edit': 'bi bi-pencil',
    'fas fa-trash': 'bi bi-trash',
    'fas fa-plus': 'bi bi-plus',
    'fas fa-minus': 'bi bi-dash',
    'fas fa-check': 'bi bi-check',
    'fas fa-times': 'bi bi-x',
    'fas fa-arrow-right': 'bi bi-arrow-right',
    'fas fa-arrow-left': 'bi bi-arrow-left',
    'fas fa-arrow-up': 'bi bi-arrow-up',
    'fas fa-arrow-down': 'bi bi-arrow-down',
    'fas fa-cog': 'bi bi-gear',
    'fas fa-star': 'bi bi-star',
    'fas fa-heart': 'bi bi-heart',
    'fas fa-eye': 'bi bi-eye',
    'fas fa-lock': 'bi bi-lock',
    'fas fa-unlock': 'bi bi-unlock',
    'fas fa-folder': 'bi bi-folder',
    'fas fa-file': 'bi bi-file-text',
    'fas fa-image': 'bi bi-image',
    'fas fa-video': 'bi bi-camera-video',
    'fas fa-music': 'bi bi-music-note',
    'fas fa-play': 'bi bi-play',
    'fas fa-pause': 'bi bi-pause',
    'fas fa-stop': 'bi bi-stop',
    'fas fa-forward': 'bi bi-skip-forward',
    'fas fa-backward': 'bi bi-skip-backward',
    'fas fa-volume-up': 'bi bi-volume-up',
    'fas fa-volume-down': 'bi bi-volume-down',
    'fas fa-volume-mute': 'bi bi-volume-mute',

    // Regular icons
    'far fa-star': 'bi bi-star',
    'far fa-heart': 'bi bi-heart',
    'far fa-eye': 'bi bi-eye',
    'far fa-envelope': 'bi bi-envelope',
    'far fa-folder': 'bi bi-folder',
    'far fa-file': 'bi bi-file-text',
    'far fa-calendar': 'bi bi-calendar',
    'far fa-clock': 'bi bi-clock',
    'far fa-user': 'bi bi-person'
};

class FontAwesomeAuditor {
    constructor() {
        this.sourceDir = path.join(process.cwd(), 'src');
        this.issues = [];
        this.summary = {
            totalFiles: 0,
            filesWithIssues: 0,
            totalIssues: 0,
            fixableIssues: 0
        };
    }

    async audit() {
        console.log('🔍 Font Awesome Audit Tool');
        console.log('===========================');
        console.log(`📁 Scanning: ${this.sourceDir}/**/*.pug`);
        console.log('');

        try {
            const files = this.findPugFiles(this.sourceDir);
            this.summary.totalFiles = files.length;

            if (files.length === 0) {
                console.log('❌ No PUG files found to audit');
                return;
            }

            console.log(`📄 Found ${files.length} PUG files to scan`);
            console.log('');

            for (const file of files) {
                await this.auditFile(file);
            }

            this.generateReport();

        } catch (error) {
            console.error('❌ Error during audit:', error.message);
            process.exit(1);
        }
    }

    findPugFiles(dir) {
        let files = [];

        try {
            const items = fs.readdirSync(dir);

            for (const item of items) {
                const fullPath = path.join(dir, item);
                const stat = fs.statSync(fullPath);

                if (stat.isDirectory()) {
                    // Recursively search subdirectories
                    files = files.concat(this.findPugFiles(fullPath));
                } else if (item.endsWith('.pug')) {
                    files.push(fullPath);
                }
            }
        } catch (error) {
            console.error(`❌ Error reading directory ${dir}:`, error.message);
        }

        return files;
    }

    async auditFile(filePath) {
        try {
            const content = fs.readFileSync(filePath, 'utf8');
            const lines = content.split('\n');
            const relativePath = path.relative(process.cwd(), filePath);
            let fileHasIssues = false;

            lines.forEach((line, index) => {
                const lineNumber = index + 1;
                const matches = this.findFontAwesomeClasses(line);

                if (matches.length > 0) {
                    if (!fileHasIssues) {
                        fileHasIssues = true;
                        this.summary.filesWithIssues++;
                    }

                    matches.forEach(match => {
                        const issue = {
                            file: relativePath,
                            line: lineNumber,
                            content: line.trim(),
                            fontAwesome: match,
                            bootstrapIcon: iconMappings[match] || 'UNKNOWN',
                            fixable: !!iconMappings[match]
                        };

                        this.issues.push(issue);
                        this.summary.totalIssues++;

                        if (issue.fixable) {
                            this.summary.fixableIssues++;
                        }
                    });
                }
            });

        } catch (error) {
            console.error(`❌ Error reading file ${filePath}:`, error.message);
        }
    }

    findFontAwesomeClasses(line) {
        const matches = [];

        // Patterns to match Font Awesome classes
        const patterns = [
            /\bfa[bsr]?\s+fa-[\w-]+/g,  // fab fa-xxx, fas fa-xxx, far fa-xxx, fa fa-xxx
            /\bfa-[\w-]+(?!\w)/g,       // standalone fa-xxx (not followed by word character)
            /\.fab\.fa-[\w-]+/g,        // PUG class syntax .fab.fa-xxx
            /\.fas\.fa-[\w-]+/g,        // PUG class syntax .fas.fa-xxx
            /\.far\.fa-[\w-]+/g,        // PUG class syntax .far.fa-xxx
            /\.fa\.fa-[\w-]+/g,         // PUG class syntax .fa.fa-xxx
            /class="[^"]*\bfa[bsr]?\s+fa-[\w-]+[^"]*"/g,  // class="fab fa-xxx"
            /class='[^']*\bfa[bsr]?\s+fa-[\w-]+[^']*'/g   // class='fab fa-xxx'
        ];

        patterns.forEach(pattern => {
            const found = line.match(pattern);
            if (found) {
                found.forEach(match => {
                    let normalized = match.trim();

                    // Handle different formats
                    if (match.includes('class=')) {
                        // Extract FA classes from class attribute
                        const classMatch = match.match(/\bfa[bsr]?\s+fa-[\w-]+/);
                        if (classMatch) {
                            normalized = classMatch[0];
                        }
                    } else if (match.startsWith('.')) {
                        // PUG class syntax .fab.fa-xxx -> fab fa-xxx
                        normalized = match.replace(/\./g, ' ').trim();
                    } else if (match.startsWith('fa-')) {
                        // Standalone fa-xxx, assume fas
                        normalized = 'fas ' + match;
                    }

                    // Convert to standard format
                    if (normalized.startsWith('fa ')) {
                        normalized = 'fas ' + normalized.substring(3);
                    }

                    if (!matches.includes(normalized)) {
                        matches.push(normalized);
                    }
                });
            }
        });

        return matches;
    }

    generateReport() {
        console.log('📊 AUDIT REPORT');
        console.log('================');
        console.log(`📄 Total files scanned: ${this.summary.totalFiles}`);
        console.log(`⚠️  Files with issues: ${this.summary.filesWithIssues}`);
        console.log(`🚨 Total Font Awesome references: ${this.summary.totalIssues}`);
        console.log(`✅ Automatically fixable: ${this.summary.fixableIssues}`);
        console.log(`❓ Requires manual review: ${this.summary.totalIssues - this.summary.fixableIssues}`);
        console.log('');

        if (this.issues.length === 0) {
            console.log('🎉 No Font Awesome references found! All clean!');
            return;
        }

        // Group issues by file
        const issuesByFile = this.groupIssuesByFile();

        console.log('📋 DETAILED FINDINGS');
        console.log('=====================');

        Object.keys(issuesByFile).forEach(filePath => {
            const fileIssues = issuesByFile[filePath];
            console.log(`\n📄 ${filePath}`);
            console.log('─'.repeat(filePath.length + 2));

            fileIssues.forEach(issue => {
                const status = issue.fixable ? '✅' : '❓';
                const replacement = issue.fixable ? issue.bootstrapIcon : 'NEEDS MANUAL REVIEW';

                console.log(`  ${status} Line ${issue.line}: ${issue.fontAwesome} → ${replacement}`);
                console.log(`     Context: ${issue.content}`);
            });
        });

        // Generate suggestions
        console.log('\n💡 SUGGESTIONS');
        console.log('===============');

        const uniqueClasses = [...new Set(this.issues.map(i => i.fontAwesome))];
        const fixableClasses = uniqueClasses.filter(cls => iconMappings[cls]);
        const unknownClasses = uniqueClasses.filter(cls => !iconMappings[cls]);

        if (fixableClasses.length > 0) {
            console.log('\n✅ Ready to fix (automatic replacements available):');
            fixableClasses.forEach(cls => {
                console.log(`   ${cls} → ${iconMappings[cls]}`);
            });
        }

        if (unknownClasses.length > 0) {
            console.log('\n❓ Requires manual review:');
            unknownClasses.forEach(cls => {
                console.log(`   ${cls} → Find equivalent Bootstrap Icon`);
            });
            console.log('\n   Visit https://icons.getbootstrap.com/ to find alternatives');
        }

        // Save detailed report
        this.saveDetailedReport();
    }

    groupIssuesByFile() {
        const grouped = {};
        this.issues.forEach(issue => {
            if (!grouped[issue.file]) {
                grouped[issue.file] = [];
            }
            grouped[issue.file].push(issue);
        });
        return grouped;
    }

    saveDetailedReport() {
        const reportData = {
            timestamp: new Date().toISOString(),
            summary: this.summary,
            issues: this.issues,
            iconMappings: iconMappings
        };

        const reportPath = path.join(process.cwd(), 'copilot', 'session-2025-10-12', 'fontawesome-audit-report.json');

        try {
            // Ensure directory exists
            const reportDir = path.dirname(reportPath);
            if (!fs.existsSync(reportDir)) {
                fs.mkdirSync(reportDir, { recursive: true });
            }

            fs.writeFileSync(reportPath, JSON.stringify(reportData, null, 2));
            console.log(`\n💾 Detailed report saved: ${path.relative(process.cwd(), reportPath)}`);
        } catch (error) {
            console.error(`❌ Failed to save report: ${error.message}`);
        }
    }
}

// Run the audit if this script is executed directly
if (require.main === module) {
    const auditor = new FontAwesomeAuditor();
    auditor.audit().catch(error => {
        console.error('❌ Audit failed:', error);
        process.exit(1);
    });
}

module.exports = FontAwesomeAuditor;
