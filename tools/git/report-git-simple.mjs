#!/usr/bin/env node

/**
 * Git Activity Report Generator - Pure Node.js Implementation
 *
 * Generates comprehensive Git repository activity reports including:
 * - Developer productivity and commit analysis
 * - File modification patterns
 * - Recent activity summary
 * - Statistical analysis
 */

import { execSync } from 'node:child_process';
import { promises as fs } from 'node:fs';
import path from 'node:path';
import process from 'node:process';

const DEFAULT_DAYS = 90;
const TEMP_REPORTS_DIR = path.resolve('temp/reports');
const REPORTS_DIR = path.resolve('reports');

// Ensure output directories exist
await fs.mkdir(TEMP_REPORTS_DIR, { recursive: true });
await fs.mkdir(REPORTS_DIR, { recursive: true });

/**
 * Execute git command and return output
 */
function gitCommand(command, options = {}) {
  try {
    return execSync(`git ${command}`, {
      encoding: 'utf8',
      cwd: process.cwd(),
      ...options
    }).trim();
  } catch (error) {
    console.error(`Git command failed: git ${command}`);
    throw error;
  }
}

/**
 * Categorize files as source code or published content
 */
function categorizeFile(filename) {
  // Published/generated content (docs folder and output files)
  if (filename.startsWith('docs/') ||
      filename.startsWith('lhci/') ||
      filename.startsWith('temp/') ||
      filename.startsWith('reports/') ||
      filename.startsWith('artifacts/') ||
      filename.includes('.min.') ||
      filename.endsWith('.map') ||
      filename.endsWith('.log')) {
    return 'published';
  }

  // Source code (everything else - src/, tools/, scripts/, config files, etc.)
  return 'source';
}

/**
 * Analyze commit timing patterns for quality of life insights
 */
function analyzeCommitTiming(commits) {
  const timing = {
    dayOfWeek: new Array(7).fill(0), // Sunday = 0, Monday = 1, etc.
    hourOfDay: new Array(24).fill(0),
    dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
  };

  commits.forEach(commit => {
    const day = commit.date.getDay();
    const hour = commit.date.getHours();
    timing.dayOfWeek[day]++;
    timing.hourOfDay[hour]++;
  });

  // Find preferred day and time
  const preferredDayIndex = timing.dayOfWeek.indexOf(Math.max(...timing.dayOfWeek));
  const preferredHour = timing.hourOfDay.indexOf(Math.max(...timing.hourOfDay));

  // Categorize time of day
  let timeCategory;
  if (preferredHour >= 6 && preferredHour < 12) timeCategory = 'Morning (6AM-12PM)';
  else if (preferredHour >= 12 && preferredHour < 18) timeCategory = 'Afternoon (12PM-6PM)';
  else if (preferredHour >= 18 && preferredHour < 22) timeCategory = 'Evening (6PM-10PM)';
  else timeCategory = 'Night (10PM-6AM)';

  // Categorize work pattern
  const weekdayCommits = timing.dayOfWeek.slice(1, 6).reduce((sum, count) => sum + count, 0);
  const weekendCommits = timing.dayOfWeek[0] + timing.dayOfWeek[6];
  const workPattern = weekdayCommits > weekendCommits ? 'Weekday-focused' : 'Weekend warrior';

  return {
    preferredDay: timing.dayNames[preferredDayIndex],
    preferredHour,
    timeCategory,
    workPattern,
    weekdayCommits,
    weekendCommits,
    dayDistribution: timing.dayOfWeek.map((count, i) => ({ day: timing.dayNames[i], count })),
    hourDistribution: timing.hourOfDay
  };
}

/**
 * Get git log data for analysis
 */
function getGitLogData(days) {
  const since = new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

  // Get commit data with detailed information
  const logFormat = '--pretty=format:"%H|%an|%ae|%ad|%s|%D" --date=iso';
  const commits = gitCommand(`log --since="${since}" ${logFormat} --numstat`);

  if (!commits) {
    return { commits: [], authors: new Map(), fileStats: new Map() };
  }

  const lines = commits.split('\n');
  const commitData = [];
  const authors = new Map();
  const fileStats = new Map();

  let currentCommit = null;

  for (const line of lines) {
    if (line.includes('|') && !line.match(/^\d+\s+\d+\s+/)) {
      // This is a commit header line
      const parts = line.replace(/"/g, '').split('|');
      if (parts.length >= 5) {
        currentCommit = {
          hash: parts[0],
          author: parts[1],
          email: parts[2],
          date: new Date(parts[3]),
          subject: parts[4],
          refs: parts[5] || '',
          filesChanged: [],
          insertions: 0,
          deletions: 0
        };
        commitData.push(currentCommit);

        // Track authors
        if (!authors.has(parts[1])) {
          authors.set(parts[1], {
            name: parts[1],
            email: parts[2],
            commits: 0,
            insertions: 0,
            deletions: 0,
            firstCommit: currentCommit.date,
            lastCommit: currentCommit.date,
            commitHistory: [], // Track all commits for timing analysis
            sourceStats: { commits: 0, insertions: 0, deletions: 0, files: 0 },
            publishedStats: { commits: 0, insertions: 0, deletions: 0, files: 0 }
          });
        }
        const authorData = authors.get(parts[1]);
        authorData.commits++;
        authorData.commitHistory.push(currentCommit);
        if (currentCommit.date < authorData.firstCommit) authorData.firstCommit = currentCommit.date;
        if (currentCommit.date > authorData.lastCommit) authorData.lastCommit = currentCommit.date;
      }
    } else if (line.match(/^\d+\s+\d+\s+/) && currentCommit) {
      // This is a file change line (insertions deletions filename)
      const parts = line.split('\t');
      if (parts.length >= 3) {
        const insertions = parseInt(parts[0]) || 0;
        const deletions = parseInt(parts[1]) || 0;
        const filename = parts[2];
        const fileCategory = categorizeFile(filename);

        currentCommit.filesChanged.push({ filename, insertions, deletions, category: fileCategory });
        currentCommit.insertions += insertions;
        currentCommit.deletions += deletions;

        // Update author stats
        const authorData = authors.get(currentCommit.author);
        if (authorData) {
          authorData.insertions += insertions;
          authorData.deletions += deletions;

          // Track source vs published statistics
          if (fileCategory === 'source') {
            authorData.sourceStats.insertions += insertions;
            authorData.sourceStats.deletions += deletions;
            authorData.sourceStats.files++;
          } else {
            authorData.publishedStats.insertions += insertions;
            authorData.publishedStats.deletions += deletions;
            authorData.publishedStats.files++;
          }
        }

        // Track file statistics with category
        if (!fileStats.has(filename)) {
          fileStats.set(filename, { changes: 0, insertions: 0, deletions: 0, category: fileCategory });
        }
        const fileData = fileStats.get(filename);
        fileData.changes++;
        fileData.insertions += insertions;
        fileData.deletions += deletions;
      }
    }
  }

  // Count commits by category for each author
  commitData.forEach(commit => {
    const authorData = authors.get(commit.author);
    if (authorData) {
      const hasSourceFiles = commit.filesChanged.some(f => f.category === 'source');
      const hasPublishedFiles = commit.filesChanged.some(f => f.category === 'published');

      if (hasSourceFiles) authorData.sourceStats.commits++;
      if (hasPublishedFiles) authorData.publishedStats.commits++;
    }
  });

  // Add timing analysis for each author
  authors.forEach((authorData) => {
    authorData.timing = analyzeCommitTiming(authorData.commitHistory);
  });

  return { commits: commitData, authors, fileStats };
}

/**
 * Get repository information
 */
function getRepositoryInfo() {
  try {
    const remoteUrl = gitCommand('config --get remote.origin.url');
    const currentBranch = gitCommand('rev-parse --abbrev-ref HEAD');
    const totalCommits = gitCommand('rev-list --count HEAD');
    const repoName = path.basename(process.cwd());

    return {
      name: repoName,
      remoteUrl,
      currentBranch,
      totalCommits: parseInt(totalCommits),
      analysisDate: new Date()
    };
  } catch (error) {
    return {
      name: path.basename(process.cwd()),
      currentBranch: 'unknown',
      totalCommits: 0,
      analysisDate: new Date()
    };
  }
}

/**
 * Generate HTML report
 */
function generateHTMLReport(repoInfo, gitData, days) {
  const { commits, authors, fileStats } = gitData;

  // Sort data for top lists
  const topAuthors = Array.from(authors.values())
    .sort((a, b) => b.commits - a.commits)
    .slice(0, 10);

  const topFiles = Array.from(fileStats.entries())
    .map(([filename, stats]) => ({ filename, ...stats }))
    .filter(file => file.category === 'source') // Only include source files
    .sort((a, b) => b.changes - a.changes)
    .slice(0, 20);

  const recentCommits = commits
    .sort((a, b) => b.date - a.date)
    .slice(0, 10); // Limited to last 10 commits

  const totalInsertions = commits.reduce((sum, c) => sum + c.insertions, 0);
  const totalDeletions = commits.reduce((sum, c) => sum + c.deletions, 0);

  // Calculate source files count (excluding published/generated files)
  const sourceFilesCount = Array.from(fileStats.entries())
    .filter(([filename, stats]) => stats.category === 'source')
    .length;

  // Calculate repository-wide source vs published statistics
  const repoSourceStats = {
    commits: 0,
    files: 0,
    insertions: 0,
    deletions: 0
  };
  const repoPublishedStats = {
    commits: 0,
    files: 0,
    insertions: 0,
    deletions: 0
  };

  // Aggregate all author stats
  for (const [, authorData] of authors) {
    repoSourceStats.commits += authorData.sourceStats.commits;
    repoSourceStats.files += authorData.sourceStats.files;
    repoSourceStats.insertions += authorData.sourceStats.insertions;
    repoSourceStats.deletions += authorData.sourceStats.deletions;

    repoPublishedStats.commits += authorData.publishedStats.commits;
    repoPublishedStats.files += authorData.publishedStats.files;
    repoPublishedStats.insertions += authorData.publishedStats.insertions;
    repoPublishedStats.deletions += authorData.publishedStats.deletions;
  }

  const html = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Git Activity Report - ${repoInfo.name}</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
        h2 { color: #34495e; margin-top: 30px; }
        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin: 20px 0; }
        .stat-card { background: #ecf0f1; padding: 20px; border-radius: 6px; text-align: center; }
        .stat-number { font-size: 2em; font-weight: bold; color: #3498db; }
        .stat-label { color: #7f8c8d; font-size: 0.9em; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background-color: #f8f9fa; font-weight: 600; }
        tr:hover { background-color: #f5f5f5; }
        .author { font-weight: bold; color: #2980b9; }
        .hash { font-family: 'Courier New', monospace; font-size: 0.8em; color: #7f8c8d; }
        .date { color: #27ae60; }
        .insertions { color: #27ae60; font-weight: bold; }
        .deletions { color: #e74c3c; font-weight: bold; }
        .filename { font-family: 'Courier New', monospace; font-size: 0.9em; }
        .meta-info { background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 30px; }
        .meta-info h3 { margin-top: 0; color: #2c3e50; }
        .timing-stats { background-color: #f0f8ff; padding: 15px; border-radius: 4px; margin: 10px 0; }
        .timing-stats h4 { margin: 0 0 10px 0; color: #2c3e50; font-size: 1.1em; }
        .timing-item { margin: 5px 0; padding: 5px; background: #e8f4fd; border-radius: 3px; }
        .timing-label { font-weight: bold; color: #2980b9; }
        .work-pattern { display: inline-block; padding: 3px 8px; border-radius: 12px; font-size: 0.85em; font-weight: bold; }
        .weekday-focused { background: #e8f5e8; color: #27ae60; }
        .weekend-warrior { background: #fff3cd; color: #856404; }
        .code-breakdown { background-color: #f8f9ff; padding: 15px; border-radius: 4px; margin: 10px 0; }
        .code-breakdown h4 { margin: 0 0 10px 0; color: #2c3e50; font-size: 1.1em; }
        .breakdown-item { margin: 5px 0; padding: 8px; background: #f0f8ff; border-radius: 3px; display: flex; justify-content: space-between; align-items: center; }
        .breakdown-label { font-weight: bold; color: #3498db; }
        .breakdown-stats { font-size: 0.9em; color: #2c3e50; }
        .source-files { background: #e8f5e8 !important; }
        .published-files { background: #fff3cd !important; }
        .category-badge { display: inline-block; padding: 2px 6px; border-radius: 10px; font-size: 0.75em; font-weight: bold; margin-left: 8px; }
        .source-badge { background: #27ae60; color: white; }
        .published-badge { background: #f39c12; color: white; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🔍 Git Activity Report</h1>

        <div class="meta-info">
            <h3>📊 Repository Information</h3>
            <p><strong>Repository:</strong> ${repoInfo.name}</p>
            <p><strong>Current Branch:</strong> ${repoInfo.currentBranch}</p>
            <p><strong>Analysis Period:</strong> Last ${days} days</p>
            <p><strong>Generated:</strong> ${repoInfo.analysisDate.toLocaleString()}</p>
            <p><strong>Total Commits (All Time):</strong> ${repoInfo.totalCommits}</p>
        </div>

        <div class="stats">
            <div class="stat-card">
                <div class="stat-number">${commits.length}</div>
                <div class="stat-label">Commits</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${authors.size}</div>
                <div class="stat-label">Authors</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">${sourceFilesCount}</div>
                <div class="stat-label">Source Files Changed</div>
            </div>
            <div class="stat-card">
                <div class="stat-number insertions">+${totalInsertions.toLocaleString()}</div>
                <div class="stat-label">Lines Added</div>
            </div>
            <div class="stat-card">
                <div class="stat-number deletions">-${totalDeletions.toLocaleString()}</div>
                <div class="stat-label">Lines Removed</div>
            </div>
        </div>

        <div class="meta-info">
            <h3>🔧 Source vs Published Code Breakdown</h3>
            <div class="code-breakdown">
                <div class="breakdown-item source-files">
                    <span class="breakdown-label">🔧 Source Code Activity</span>
                    <span class="breakdown-stats">
                        ${repoSourceStats.commits} commits, ${repoSourceStats.files} files changed
                        <span class="category-badge source-badge">+${repoSourceStats.insertions.toLocaleString()}/-${repoSourceStats.deletions.toLocaleString()}</span>
                    </span>
                </div>
                <div class="breakdown-item published-files">
                    <span class="breakdown-label">📄 Published/Generated Activity</span>
                    <span class="breakdown-stats">
                        ${repoPublishedStats.commits} commits, ${repoPublishedStats.files} files changed
                        <span class="category-badge published-badge">+${repoPublishedStats.insertions.toLocaleString()}/-${repoPublishedStats.deletions.toLocaleString()}</span>
                    </span>
                </div>
            </div>
        </div>

        <h2>👥 Top Contributors</h2>
        <table>
            <thead>
                <tr>
                    <th>Author</th>
                    <th>Commits</th>
                    <th>Lines Added</th>
                    <th>Lines Removed</th>
                    <th>Quality of Life Stats</th>
                    <th>First/Last Commit</th>
                </tr>
            </thead>
            <tbody>
                ${topAuthors.map(author => `
                <tr>
                    <td class="author">${author.name}</td>
                    <td>${author.commits}</td>
                    <td class="insertions">+${author.insertions.toLocaleString()}</td>
                    <td class="deletions">-${author.deletions.toLocaleString()}</td>
                    <td>
                        <div class="timing-stats">
                            <h4>🕒 Coding Habits</h4>
                            <div class="timing-item">
                                <span class="timing-label">Preferred Day:</span> ${author.timing.preferredDay}
                            </div>
                            <div class="timing-item">
                                <span class="timing-label">Peak Time:</span> ${author.timing.timeCategory} (${author.timing.preferredHour}:00)
                            </div>
                            <div class="timing-item">
                                <span class="timing-label">Work Pattern:</span>
                                <span class="work-pattern ${author.timing.workPattern.toLowerCase().replace(/[\s-]/g, '-')}">${author.timing.workPattern}</span>
                            </div>
                            <div class="timing-item">
                                <span class="timing-label">Schedule:</span> ${author.timing.weekdayCommits} weekday, ${author.timing.weekendCommits} weekend commits
                            </div>
                        </div>
                        <div class="code-breakdown">
                            <h4>📊 Code Activity Breakdown</h4>
                            <div class="breakdown-item source-files">
                                <span class="breakdown-label">🔧 Source Code</span>
                                <span class="breakdown-stats">
                                    ${author.sourceStats.commits} commits, ${author.sourceStats.files} files
                                    <span class="category-badge source-badge">+${author.sourceStats.insertions}/-${author.sourceStats.deletions}</span>
                                </span>
                            </div>
                            <div class="breakdown-item published-files">
                                <span class="breakdown-label">📄 Published/Generated</span>
                                <span class="breakdown-stats">
                                    ${author.publishedStats.commits} commits, ${author.publishedStats.files} files
                                    <span class="category-badge published-badge">+${author.publishedStats.insertions}/-${author.publishedStats.deletions}</span>
                                </span>
                            </div>
                        </div>
                    </td>
                    <td>
                        <div class="date">${author.firstCommit.toLocaleDateString()}</div>
                        <div class="date">${author.lastCommit.toLocaleDateString()}</div>
                    </td>
                </tr>
                `).join('')}
            </tbody>
        </table>

        <h2>� Most Changed Source Files</h2>
        <table>
            <thead>
                <tr>
                    <th>File</th>
                    <th>Changes</th>
                    <th>Lines Added</th>
                    <th>Lines Removed</th>
                </tr>
            </thead>
            <tbody>
                ${topFiles.map(file => `
                <tr>
                    <td class="filename">${file.filename}</td>
                    <td>${file.changes}</td>
                    <td class="insertions">+${file.insertions.toLocaleString()}</td>
                    <td class="deletions">-${file.deletions.toLocaleString()}</td>
                </tr>
                `).join('')}
            </tbody>
        </table>

        <h2>📝 Recent Commits (Last 10)</h2>
        <table>
            <thead>
                <tr>
                    <th>Hash</th>
                    <th>Author</th>
                    <th>Date</th>
                    <th>Message</th>
                    <th>Files</th>
                    <th>+/-</th>
                </tr>
            </thead>
            <tbody>
                ${recentCommits.map(commit => `
                <tr>
                    <td class="hash">${commit.hash.substring(0, 8)}</td>
                    <td class="author">${commit.author}</td>
                    <td class="date">${commit.date.toLocaleDateString()}</td>
                    <td>${commit.subject}</td>
                    <td>${commit.filesChanged.length}</td>
                    <td>
                        <span class="insertions">+${commit.insertions}</span>
                        <span class="deletions">-${commit.deletions}</span>
                    </td>
                </tr>
                `).join('')}
            </tbody>
        </table>
    </div>
</body>
</html>`;

  return html;
}

/**
 * Check if required tools are available
 */
async function checkPrerequisites() {
  try {
    // Check if we're in a Git repository
    await fs.access('.git');
  } catch {
    throw new Error('This script must be run from within a Git repository.');
  }

  try {
    gitCommand('--version');
  } catch {
    throw new Error('Git is not installed or not accessible from command line.');
  }

  return true;
}

/**
 * Main execution
 */
async function main() {
  try {
    console.log('🚀 Git Activity Report Generator');
    console.log('===============================================');

    await checkPrerequisites();

    console.log(`🔍 Analyzing repository activity for last ${DEFAULT_DAYS} days...`);

    const repoInfo = getRepositoryInfo();
    console.log(`📂 Repository: ${repoInfo.name} (${repoInfo.currentBranch})`);

    const gitData = getGitLogData(DEFAULT_DAYS);
    console.log(`📊 Found ${gitData.commits.length} commits from ${gitData.authors.size} authors`);

    const html = generateHTMLReport(repoInfo, gitData, DEFAULT_DAYS);

    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').split('T')[0];
    const filename = `git-activity-report-${timestamp}.html`;
    const filepath = path.join(TEMP_REPORTS_DIR, filename);

    await fs.writeFile(filepath, html, 'utf8');

    console.log('');
    console.log('✅ Git activity report generated successfully!');
    console.log(`📂 Report location: ${filepath}`);
    console.log(`📊 Analysis summary:`);
    console.log(`   - Commits: ${gitData.commits.length}`);
    console.log(`   - Authors: ${gitData.authors.size}`);
    console.log(`   - Files changed: ${gitData.fileStats.size}`);
    console.log(`   - Period: Last ${DEFAULT_DAYS} days`);
    console.log('');
    console.log(`💡 Open the HTML file in your browser to view the detailed report`);

  } catch (error) {
    console.error('❌ Error generating Git activity report:');
    console.error(error.message);
    process.exit(1);
  }
}

// Run if called directly
await main();

export { main };
