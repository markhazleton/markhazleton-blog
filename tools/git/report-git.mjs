#!/usr/bin/env node
/**
 * Consolidated Git Activity & GitHub Integration Report (Node.js)
 * ------------------------------------------------------------------
 * Combines capabilities of previous PowerShell (Generate-GitActivityReport.ps1)
 * and simple Node report (report-git-simple.mjs) into a single portable script.
 *
 * Features:
 *  - Repository summary & key metrics
 *  - Commit / author / file churn analysis
 *  - Source vs Published/Generated categorization
 *  - GitHub integration patterns (issues, PR merges, conventional commits)
 *  - Code review & collaboration metrics
 *  - Timeline & frequency analysis (weekly, streaks, gaps)
 *  - Commit timing habits (preferred day/hour, weekday vs weekend)
 *  - Developer insight cards
 *  - Recommendations & health assessment
 *  - Output formats: HTML (default), JSON, Console
 *  - CLI options: --days, --format, --commit-count, --output
 *
 * Usage:
 *    node tools/git/report-git.mjs --days=30 --format=html
 *    node tools/git/report-git.mjs --format=json
 *    node tools/git/report-git.mjs --format=console --days=14
 */

import { execSync } from 'node:child_process';
import fs from 'node:fs/promises';
import path from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

// ---------------- Configuration & CLI Parsing -------------------
const REPORT_VERSION = '2.0.0';
const DEFAULT_DAYS = 1460;
const DEFAULT_COMMIT_SUMMARY_COUNT = 10;
const TEMP_REPORTS_DIR = path.resolve('temp/reports');
const REPORTS_DIR = path.resolve('reports');

// Default heuristic thresholds (override with --config)
const DEFAULT_CONFIG = {
  largeCommitLines: 500,
  smallCommitLines: 50,
  hotspotAuthorThreshold: 3,
  staleBranchDays: 30,
  ownershipDominancePct: 0.8,
  afterHoursStart: 19, // 7pm
  afterHoursEnd: 7,    // 7am
  burstWindowMinutes: 5,
  burstMinCommits: 4,
  largeFileKB: 300,
  couplingMaxPairs: 25,
  riskRecentHalfLifeDays: 30,
  riskWeights: { churn: 0.35, recency: 0.25, ownership: 0.2, entropy: 0.1, coupling: 0.1 },
  governanceWeights: { conventional: 0.4, traceability: 0.25, length: 0.15, wipPenalty: 0.1, revertPenalty: 0.05, shortPenalty: 0.05 }
};

function parseArgs() {
  const args = process.argv.slice(2);
  const opts = { days: DEFAULT_DAYS, format: 'html', commitCount: DEFAULT_COMMIT_SUMMARY_COUNT, output: TEMP_REPORTS_DIR, heavy: false, since: null, until: null, configPath: null };
  for (const arg of args) {
    if (arg.startsWith('--days=')) opts.days = parseInt(arg.split('=')[1]) || DEFAULT_DAYS;
    else if (arg.startsWith('--format=')) opts.format = arg.split('=')[1].toLowerCase();
    else if (arg.startsWith('--commit-count=')) opts.commitCount = parseInt(arg.split('=')[1]) || DEFAULT_COMMIT_SUMMARY_COUNT;
    else if (arg.startsWith('--output=')) opts.output = path.resolve(arg.split('=')[1]);
    else if (arg.startsWith('--since=')) opts.since = arg.split('=')[1];
    else if (arg.startsWith('--until=')) opts.until = arg.split('=')[1];
    else if (arg.startsWith('--config=')) opts.configPath = path.resolve(arg.split('=')[1]);
    else if (arg === '--heavy') opts.heavy = true;
    else if (arg === '--json') opts.format = 'json';
    else if (arg === '--console') opts.format = 'console';
    else if (arg === '--help' || arg === '-h') {
      console.log(`Git Report Usage:\n  --days=N            Days to analyze (default ${DEFAULT_DAYS})\n  --since=YYYY-MM-DD  Explicit start date (overrides --days)\n  --until=YYYY-MM-DD  Explicit end date (default: today)\n  --format=html|json|console  Output format (default html)\n  --commit-count=N    Recent commit summary depth (default ${DEFAULT_COMMIT_SUMMARY_COUNT})\n  --output=PATH       Directory for output files (default temp/reports)\n  --heavy             Enable expensive analyses (temporal coupling)\n  --config=path.json  Load threshold overrides\n  --json / --console  Shorthands\n`);
      process.exit(0);
    }
  }
  return opts;
}

// ---------------- Utility Functions -----------------------------
const DEFAULT_MAX_BUFFER = (parseInt(process.env.GIT_REPORT_MAX_BUFFER_MB || '200', 10) || 200) * 1024 * 1024; // 200MB default
function git(command) {
  try {
    return execSync(`git ${command}`, {
      encoding: 'utf8',
      stdio: ['ignore', 'pipe', 'ignore'],
      maxBuffer: DEFAULT_MAX_BUFFER
    }).trim();
  } catch (e) {
    // Provide guidance if buffer overflow likely
    if (e.message && e.message.includes('ENOBUFS')) {
      throw new Error(`Git command failed due to buffer size (ENOBUFS). Try setting GIT_REPORT_MAX_BUFFER_MB to a higher value. Command: git ${command}`);
    }
    throw new Error(`Git command failed: git ${command}\n${e.message}`);
  }
}

function escapeHtml(str = '') {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function categorizeFile(filename) {
  if (
    filename.startsWith('docs/') ||
    filename.startsWith('lhci/') ||
    filename.startsWith('temp/') ||
    filename.startsWith('reports/') ||
    filename.startsWith('artifacts/') ||
    filename.includes('.min.') ||
    filename.endsWith('.map') ||
    filename.endsWith('.log')
  ) {
    return 'published';
  }
  return 'source';
}

function analyzeCommitTiming(commits) {
  const timing = {
    dayOfWeek: new Array(7).fill(0),
    hourOfDay: new Array(24).fill(0),
    dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
  };
  for (const c of commits) {
    const d = c.date.getDay();
    const h = c.date.getHours();
    timing.dayOfWeek[d]++;
    timing.hourOfDay[h]++;
  }
  const preferredDayIndex = timing.dayOfWeek.indexOf(Math.max(...timing.dayOfWeek));
  const preferredHour = timing.hourOfDay.indexOf(Math.max(...timing.hourOfDay));
  let timeCategory;
  if (preferredHour >= 6 && preferredHour < 12) timeCategory = 'Morning (6AM-12PM)';
  else if (preferredHour >= 12 && preferredHour < 18) timeCategory = 'Afternoon (12PM-6PM)';
  else if (preferredHour >= 18 && preferredHour < 22) timeCategory = 'Evening (6PM-10PM)';
  else timeCategory = 'Night (10PM-6AM)';
  const weekdayCommits = timing.dayOfWeek.slice(1, 6).reduce((a, b) => a + b, 0);
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

// ---------------- Data Collection -------------------------------
function getRepositoryInfo() {
  let remoteUrl = 'unknown';
  let currentBranch = 'unknown';
  let totalCommits = 0;
  try { remoteUrl = git('config --get remote.origin.url'); } catch {}
  try { currentBranch = git('rev-parse --abbrev-ref HEAD'); } catch {}
  try { totalCommits = parseInt(git('rev-list --count HEAD')) || 0; } catch {}
  return { name: path.basename(process.cwd()), remoteUrl, currentBranch, totalCommits, analysisDate: new Date() };
}

function resolveDateRange(opts) {
  if (opts.since) {
    const untilDate = opts.until ? new Date(opts.until) : new Date();
    const sinceDate = new Date(opts.since);
    return { since: sinceDate.toISOString().split('T')[0], until: untilDate.toISOString().split('T')[0] };
  }
  const untilDate = new Date();
  const sinceDate = new Date(Date.now() - opts.days * 86400000);
  return { since: sinceDate.toISOString().split('T')[0], until: untilDate.toISOString().split('T')[0] };
}

function getCommitData(range) {
  const { since, until } = range;
  const logFormat = '--pretty=format:"%H|%an|%ae|%ad|%s|%P" --date=iso';
  const raw = git(`log --since="${since}" --until="${until} 23:59:59" ${logFormat} --numstat`);
  if (!raw) return { commits: [], authors: new Map(), fileStats: new Map(), perFileAuthorLines: new Map() };
  const lines = raw.split('\n');
  const commits = [];
  const authors = new Map();
  const fileStats = new Map();
  const perFileAuthorLines = new Map();
  let current = null;
  for (const line of lines) {
    if (/^[a-f0-9]{40}\|/.test(line)) {
      if (current) commits.push(current);
      const [hash, author, email, date, subject, parents = ''] = line.replace(/"/g, '').split('|');
      current = { hash, author, email, date: new Date(date), subject, parents: parents ? parents.split(' ') : [], isMerge: (parents.split(' ').filter(Boolean).length > 1), filesChanged: [], insertions: 0, deletions: 0 };
      if (!authors.has(author)) {
        authors.set(author, { name: author, email, commits: 0, insertions: 0, deletions: 0, firstCommit: current.date, lastCommit: current.date, mergeCommits: 0, commitHistory: [], filesChanged: new Set(), sourceStats: { commits: 0, insertions: 0, deletions: 0, files: 0 }, publishedStats: { commits: 0, insertions: 0, deletions: 0, files: 0 }, messages: [] });
      }
      const ad = authors.get(author);
      ad.commits++;
      if (current.isMerge) ad.mergeCommits++;
      ad.commitHistory.push(current);
      ad.messages.push(subject);
      if (current.date < ad.firstCommit) ad.firstCommit = current.date;
      if (current.date > ad.lastCommit) ad.lastCommit = current.date;
    } else if (/^(\d+|-)\s+(\d+|-)\s+/.test(line) && current) {
      const [addedRaw, deletedRaw, filename] = line.split('\t');
      const insertions = addedRaw === '-' ? 0 : parseInt(addedRaw) || 0;
      const deletions = deletedRaw === '-' ? 0 : parseInt(deletedRaw) || 0;
      const category = categorizeFile(filename);
      current.filesChanged.push({ filename, insertions, deletions, category });
      current.insertions += insertions;
      current.deletions += deletions;
      const ad = authors.get(current.author);
      ad.insertions += insertions;
      ad.deletions += deletions;
      ad.filesChanged.add(filename);
      const target = category === 'source' ? ad.sourceStats : ad.publishedStats;
      target.insertions += insertions; target.deletions += deletions; target.files++;
      if (!fileStats.has(filename)) fileStats.set(filename, { changes: 0, insertions: 0, deletions: 0, category });
      const fsd = fileStats.get(filename);
      fsd.changes++; fsd.insertions += insertions; fsd.deletions += deletions;
      if (!perFileAuthorLines.has(filename)) perFileAuthorLines.set(filename, new Map());
      const fam = perFileAuthorLines.get(filename);
      fam.set(current.author, (fam.get(current.author) || 0) + insertions + deletions);
    }
  }
  if (current) commits.push(current);

  // Count source/published commits per author
  for (const commit of commits) {
    const ad = authors.get(commit.author);
    if (!ad) continue;
    const hasSource = commit.filesChanged.some(f => f.category === 'source');
    const hasPublished = commit.filesChanged.some(f => f.category === 'published');
    if (hasSource) ad.sourceStats.commits++;
    if (hasPublished) ad.publishedStats.commits++;
  }

  // Timing analysis per author
  for (const ad of authors.values()) {
    ad.timing = analyzeCommitTiming(ad.commitHistory);
  }
  return { commits, authors, fileStats, perFileAuthorLines };
}

// -------------- GitHub Integration & Metrics --------------------
function analyzeGitHubIntegration(commits, authorsMap) {
  const integration = {
    issueReferences: { total: 0, unique: new Set(), byAuthor: new Map(), patterns: { fixes: 0, closes: 0, resolves: 0, references: 0 } },
    pullRequestPatterns: { mergeCommits: 0, pullRequestMerges: 0, squashMerges: 0, directCommits: 0, revertCommits: 0 },
    commitMessageQuality: { total: 0, averageLength: 0, withIssueReferences: 0, withConventional: 0, short: 0, withCoAuthoredBy: 0, withBreaking: 0 },
    conventionalCommits: { feat: 0, fix: 0, docs: 0, style: 0, refactor: 0, perf: 0, test: 0, chore: 0, build: 0, ci: 0 }
  };
  const issueRefRegex = /\b(?:#|GH-|gh-)(\d+)\b/i;
  const conventionalRegex = /^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?!?:\s.+/;
  for (const c of commits) {
    integration.commitMessageQuality.total++;
    const msg = c.subject || '';
    if (msg.length < 10) integration.commitMessageQuality.short++;
    const issueMatchAll = msg.matchAll(/\b#(\d+)\b/g);
    for (const m of issueMatchAll) {
      integration.issueReferences.total++;
      integration.issueReferences.unique.add(m[1]);
      const a = c.author;
      if (!integration.issueReferences.byAuthor.has(a)) integration.issueReferences.byAuthor.set(a, { count: 0, issues: new Set() });
      const entry = integration.issueReferences.byAuthor.get(a);
      entry.count++; entry.issues.add(m[1]);
    }
    if (/\b(fix|fixes|fixed)\s+#?\d+/i.test(msg)) { integration.issueReferences.patterns.fixes++; integration.commitMessageQuality.withIssueReferences++; }
    if (/\b(close|closes|closed)\s+#?\d+/i.test(msg)) { integration.issueReferences.patterns.closes++; integration.commitMessageQuality.withIssueReferences++; }
    if (/\b(resolve|resolves|resolved)\s+#?\d+/i.test(msg)) { integration.issueReferences.patterns.resolves++; integration.commitMessageQuality.withIssueReferences++; }
    if (/\b(ref|reference|references|see|relates?\s+to)\s+#?\d+/i.test(msg)) { integration.issueReferences.patterns.references++; integration.commitMessageQuality.withIssueReferences++; }
    if (/Co-authored-by:/i.test(msg)) integration.commitMessageQuality.withCoAuthoredBy++;
    if (/BREAKING CHANGE:?/i.test(msg) || /!:\s/.test(msg)) integration.commitMessageQuality.withBreaking++;
    if (c.isMerge) {
      integration.pullRequestPatterns.mergeCommits++;
      if (/Merge pull request #\d+/i.test(msg) || /Merged PR #\d+/i.test(msg)) integration.pullRequestPatterns.pullRequestMerges++;
      if (/\(#\d+\)$/.test(msg)) integration.pullRequestPatterns.squashMerges++;
    } else integration.pullRequestPatterns.directCommits++;
    if (/^Revert\s+/i.test(msg)) integration.pullRequestPatterns.revertCommits++;
    if (conventionalRegex.test(msg)) {
      integration.commitMessageQuality.withConventional++;
      const type = msg.match(conventionalRegex)[1];
      if (integration.conventionalCommits[type] !== undefined) integration.conventionalCommits[type]++;
    }
  }
  if (integration.commitMessageQuality.total > 0) {
    const totalLength = commits.reduce((s, c) => s + (c.subject?.length || 0), 0);
    integration.commitMessageQuality.averageLength = +(totalLength / integration.commitMessageQuality.total).toFixed(1);
  }
  return integration;
}

function computeChurnMetrics(commits) {
  const metrics = { totalCommits: commits.length, linesAdded: 0, linesDeleted: 0, linesChanged: 0, filesChanged: 0, avgChurnPerCommit: 0, churnRate: 0, fileChurn: new Map(), daily: new Map() };
  const uniqueFiles = new Set();
  for (const c of commits) {
    let added = 0, deleted = 0;
    for (const f of c.filesChanged) {
      added += f.insertions; deleted += f.deletions; uniqueFiles.add(f.filename);
      if (!metrics.fileChurn.has(f.filename)) metrics.fileChurn.set(f.filename, { added: 0, deleted: 0, commits: 0 });
      const fc = metrics.fileChurn.get(f.filename); fc.added += f.insertions; fc.deleted += f.deletions; fc.commits++;
    }
    metrics.linesAdded += added; metrics.linesDeleted += deleted;
    const dayKey = c.date.toISOString().slice(0, 10);
    if (!metrics.daily.has(dayKey)) metrics.daily.set(dayKey, { commits: 0, linesAdded: 0, linesDeleted: 0, files: 0 });
    const day = metrics.daily.get(dayKey); day.commits++; day.linesAdded += added; day.linesDeleted += deleted; day.files += c.filesChanged.length;
  }
  metrics.linesChanged = metrics.linesAdded + metrics.linesDeleted;
  metrics.filesChanged = uniqueFiles.size;
  metrics.avgChurnPerCommit = metrics.totalCommits ? +(metrics.linesChanged / metrics.totalCommits).toFixed(2) : 0;
  metrics.churnRate = metrics.linesAdded ? metrics.linesDeleted / metrics.linesAdded : 0;
  return metrics;
}

// ---------------- Advanced Analyses (Enhancements 1-10) ----------------
function getTagCadence() {
  let tags = [];
  try { const raw = git('tag --sort=creatordate'); tags = raw ? raw.split('\n').filter(Boolean) : []; } catch {}
  if (!tags.length) return { count: 0, avgDaysBetween: null, latest: null };
  const dates = [];
  for (const t of tags) { try { const d = git(`log -1 --format=%ad --date=iso ${t}`); dates.push(new Date(d)); } catch {} }
  const sorted = dates.sort((a,b)=>a-b);
  if (sorted.length < 2) return { count: tags.length, avgDaysBetween: null, latest: sorted.at(-1), tags };
  let gaps=0; for (let i=1;i<sorted.length;i++) gaps += (sorted[i]-sorted[i-1])/86400000;
  return { count: tags.length, avgDaysBetween: +(gaps/(sorted.length-1)).toFixed(1), latest: sorted.at(-1), tags };
}

function getBranchInfo(cfg) {
  let raw='';
  try { raw = git('for-each-ref --format="%(refname:short)|%(committerdate:iso8601)" refs/heads/'); } catch {}
  const branches = raw? raw.split('\n').filter(Boolean).map(l=>{ const [name,date] = l.split('|'); return { name, lastCommit: date? new Date(date): null };}) : [];
  const now = Date.now();
  const stale = branches.filter(b=> b.lastCommit && (now - b.lastCommit.getTime())/86400000 > cfg.staleBranchDays);
  const namingRe = /^(feature|feat|fix|bugfix|hotfix|release|chore|refactor)\//i;
  let compliant=0; branches.forEach(b=> { if (namingRe.test(b.name)) compliant++; });
  return { total: branches.length, staleCount: stale.length, stale, namingCompliancePct: branches.length? +(compliant/branches.length*100).toFixed(1):0 };
}

function computeOwnership(perFileAuthorLines, dominancePct) {
  const ownership=[]; let dominantFiles=0; let totalFiles=0; const busFactorMap=new Map();
  for (const [file, amap] of perFileAuthorLines.entries()) {
    totalFiles++;
    const entries = [...amap.entries()].sort((a,b)=> b[1]-a[1]);
    const total = entries.reduce((s,e)=>s+e[1],0)||1;
    const leader=entries[0];
    const dominance = leader[1]/total;
    if (dominance >= dominancePct) dominantFiles++;
    ownership.push({ file, leader: leader[0], dominance: +dominance.toFixed(2), authors: entries.length });
    for (const [author, lines] of entries) busFactorMap.set(author,(busFactorMap.get(author)||0)+lines);
  }
  const totalLines = [...busFactorMap.values()].reduce((a,b)=>a+b,0)||1;
  const topAuthorsShare = [...busFactorMap.entries()].sort((a,b)=>b[1]-a[1]).slice(0,3).reduce((s,e)=>s+e[1],0)/totalLines;
  return { ownership, dominantFiles, totalFiles, dominanceRatio: totalFiles? +(dominantFiles/totalFiles*100).toFixed(1):0, busFactorTop3Share: +(topAuthorsShare*100).toFixed(1) };
}

function computeHotspots(churnMetrics, ownership) {
  const hotspots=[];
  for (const [file, stats] of churnMetrics.fileChurn.entries()) {
    const ownerInfo = ownership.ownership.find(o=>o.file===file);
    const churn = stats.added + stats.deleted;
    const authors = ownerInfo? ownerInfo.authors:1;
    const score = churn * authors;
    hotspots.push({ file, churn, authors, score });
  }
  return hotspots.sort((a,b)=> b.score - a.score).slice(0,25);
}

function computeTemporalCoupling(commits, maxPairs) {
  const pairMap=new Map(); const fileCommitCounts=new Map();
  for (const c of commits) {
    const files=[...new Set(c.filesChanged.filter(f=>f.category==='source').map(f=>f.filename))];
    for (const f of files) fileCommitCounts.set(f,(fileCommitCounts.get(f)||0)+1);
    for (let i=0;i<files.length;i++) for (let j=i+1;j<files.length;j++) { const a=files[i], b=files[j]; const key = a < b ? `${a}||${b}` : `${b}||${a}`; pairMap.set(key,(pairMap.get(key)||0)+1); }
  }
  const pairs=[];
  for (const [key,count] of pairMap.entries()) { const [a,b]=key.split('||'); const minChanges = Math.min(fileCommitCounts.get(a)||1, fileCommitCounts.get(b)||1); const strength = +(count/minChanges*100).toFixed(1); pairs.push({ a, b, count, strength }); }
  return pairs.sort((x,y)=> y.strength - x.strength).slice(0,maxPairs);
}

function languageBreakdown() {
  let files=[]; try { const raw = git('ls-files'); files = raw? raw.split('\n').filter(Boolean):[]; } catch {}
  const map=new Map(); const extRe=/\.([a-zA-Z0-9_]+)$/;
  for (const f of files) { const m=f.match(extRe); const ext=m? m[1].toLowerCase():'none'; map.set(ext,(map.get(ext)||0)+1); }
  const total = files.length || 1;
  const breakdown = [...map.entries()].map(([ext,count])=>({ ext, count, pct:+(count/total*100).toFixed(1) })).sort((a,b)=> b.count - a.count).slice(0,15);
  return { totalFiles: files.length, breakdown };
}

async function detectLargeChangedFiles(commits, sizeKBThreshold) {
  const changed=new Set(); for (const c of commits) for (const f of c.filesChanged) changed.add(f.filename);
  const large=[]; for (const f of changed) { try { const st = await fs.stat(f); const kb = st.size/1024; if (kb > sizeKBThreshold) large.push({ file: f, kb: +kb.toFixed(1) }); } catch {} }
  return large.sort((a,b)=> b.kb - a.kb).slice(0,20);
}

function commitSizeDistribution(commits) {
  const buckets=[{label:'0-50',min:0,max:50,count:0},{label:'51-200',min:51,max:200,count:0},{label:'201-500',min:201,max:500,count:0},{label:'501-1000',min:501,max:1000,count:0},{label:'>1000',min:1001,max:Infinity,count:0}];
  for (const c of commits) { const size=c.insertions + c.deletions; const b = buckets.find(b=> size>=b.min && size<=b.max); if (b) b.count++; }
  const total = commits.length || 1; buckets.forEach(b=> b.pct=+(b.count/total*100).toFixed(1)); return buckets;
}

function workPatternMetrics(commits, cfg) {
  let afterHours=0, weekend=0; const bursts=[]; const times=commits.map(c=>c.date).sort((a,b)=>a-b);
  for (const c of commits) { const h=c.date.getHours(); const isAfter = cfg.afterHoursStart <= cfg.afterHoursEnd ? (h>=cfg.afterHoursStart && h<cfg.afterHoursEnd) : (h>=cfg.afterHoursStart || h<cfg.afterHoursEnd); if (isAfter) afterHours++; const d=c.date.getDay(); if (d===0||d===6) weekend++; }
  let streak=[times[0]]; for (let i=1;i<times.length;i++){ const delta=(times[i]-times[i-1])/60000; if (delta <= cfg.burstWindowMinutes) streak.push(times[i]); else { if (streak.length >= cfg.burstMinCommits) bursts.push({ start: streak[0], end: streak.at(-1), count: streak.length }); streak=[times[i]]; } }
  if (streak.length >= cfg.burstMinCommits) bursts.push({ start: streak[0], end: streak.at(-1), count: streak.length });
  const total=commits.length||1;
  return { afterHoursPct: +(afterHours/total*100).toFixed(1), weekendPct: +(weekend/total*100).toFixed(1), bursts };
}

function correctiveVsFeature(commits) { let corrective=0, feature=0, docs=0, refactor=0; for (const c of commits){ const m=c.subject.toLowerCase(); if (/^(fix|hotfix|bug|revert)/.test(m)) corrective++; else if (/^feat/.test(m)) feature++; else if (/^docs/.test(m)) docs++; else if (/^refactor/.test(m)) refactor++; } const total=commits.length||1; return { corrective, feature, docs, refactor, correctiveRatio: +(corrective/total*100).toFixed(1) }; }

function issueLeadTime(commits) {
  const issues=new Map(); const closePattern=/(fixe?s|close[sd]?|resolve[sd]?)/i;
  for (const c of commits) { const ids=[...c.subject.matchAll(/#(\d+)/g)].map(m=>m[1]); if (!ids.length) continue; for (const id of ids){ if (!issues.has(id)) issues.set(id,{ first:c.date, last:c.date, closed:null }); const entry=issues.get(id); if (c.date < entry.first) entry.first=c.date; if (c.date > entry.last) entry.last=c.date; if (closePattern.test(c.subject)) entry.closed=c.date; } }
  const completed=[...issues.values()].filter(i=>i.closed); if (!completed.length) return { count: issues.size, completed: 0, avgLeadTimeDays: null };
  const sum=completed.reduce((s,i)=> s + (i.closed - i.first)/86400000,0); return { count: issues.size, completed: completed.length, avgLeadTimeDays: +(sum/completed.length).toFixed(1) };
}

function riskScores(churnMetrics, ownership, commits, cfg, couplingPairs) {
  const latest = commits.length ? commits.map(c=>c.date).sort((a,b)=>b-a)[0] : new Date();
  const latestTime = latest.getTime();
  const files=[];
  let maxChurn=0; for (const [, stats] of churnMetrics.fileChurn.entries()) maxChurn=Math.max(maxChurn, stats.added+stats.deleted);
  maxChurn = maxChurn || 1;
  // Build ownership / last touched maps
  const ownershipMap = new Map(ownership.ownership.map(o=>[o.file,o]));
  const lastTouched=new Map();
  for (const c of commits) for (const f of c.filesChanged) lastTouched.set(f.filename, c.date);
  // Coupling strength lookup (max strength per file)
  const couplingStrength = new Map();
  for (const p of couplingPairs||[]) {
    couplingStrength.set(p.a, Math.max(couplingStrength.get(p.a)||0, p.strength));
    couplingStrength.set(p.b, Math.max(couplingStrength.get(p.b)||0, p.strength));
  }
  const weights = cfg.riskWeights || { churn:0.35, recency:0.25, ownership:0.2, entropy:0.1, coupling:0.1 };
  for (const [file, stats] of churnMetrics.fileChurn.entries()) {
    const churn = stats.added + stats.deleted;
    const normChurn = churn / maxChurn; // 0..1
    const own = ownershipMap.get(file);
    const dominance = own? own.dominance : null; // fraction of leading author
    const authorsCount = own? own.authors : 1;
    // Ownership risk: high when dominance low & many authors (bus factor risk)
    const ownershipRisk = own ? (1 - dominance) * Math.min(authorsCount/10,1) : 0.3;
    // Entropy of contribution distribution (if we had distribution lines; approximated using authors count vs dominance)
    // Approximation: higher authors & moderate dominance -> higher entropy.
    const entropyApprox = own ? Math.min(1, (authorsCount>1? (1 - Math.abs(0.5 - dominance))*2 : 0)) : 0;
    const lt = lastTouched.get(file) || latest;
    const ageDays = (latestTime - lt.getTime())/86400000;
    const recencyFactor = Math.exp(-ageDays / cfg.riskRecentHalfLifeDays); // 1 if today, decays
    const couplingNorm = (couplingStrength.get(file)||0)/100; // 0..1
    const scoreComposite = (normChurn * weights.churn) + (recencyFactor * weights.recency) + (ownershipRisk * weights.ownership) + (entropyApprox * weights.entropy) + (couplingNorm * weights.coupling);
    const score = +(Math.min(1, scoreComposite) * 100).toFixed(1);
    files.push({ file, score, churn, authors: authorsCount, lastTouched: lt, dominance, components: { normChurn:+normChurn.toFixed(3), recency:+recencyFactor.toFixed(3), ownershipRisk:+ownershipRisk.toFixed(3), entropy:+entropyApprox.toFixed(3), coupling:+couplingNorm.toFixed(3) } });
  }
  return files.sort((a,b)=> b.score - a.score).slice(0,25);
}

function governanceScores(integration, authorsMap, cfg) {
  const perAuthor=[];
  const weights = cfg.governanceWeights || { conventional:0.4, traceability:0.25, length:0.15, wipPenalty:0.1, revertPenalty:0.05, shortPenalty:0.05 };
  for (const a of authorsMap.values()) {
    const total = a.commits || 1;
    let conventional=0, withIssues=0, short=0, wip=0, revert=0, goodLength=0;
    for (const c of a.commitHistory) {
      const msg = c.subject || '';
      if (/^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?!?:\s.+/.test(msg)) conventional++;
      if (/#\d+/.test(msg)) withIssues++;
      if (msg.length < 10) short++;
      if (/\bwip\b/i.test(msg)) wip++;
      if (/^revert\b/i.test(msg)) revert++;
      if (msg.length >= 20 && msg.length <= 72) goodLength++;
    }
    const conventionalPct = conventional/total;
    const issueRefPct = withIssues/total;
    const shortPct = short/total;
    const wipPct = wip/total;
    const revertPct = revert/total;
    const lengthQuality = goodLength/total; // proportion in ideal length range
    const positive = (conventionalPct * weights.conventional) + (issueRefPct * weights.traceability) + (lengthQuality * weights.length);
    const penalties = (wipPct * weights.wipPenalty) + (revertPct * weights.revertPenalty) + (shortPct * weights.shortPenalty);
    const rawScore = Math.max(0, Math.min(1, positive - penalties));
    const score = +(rawScore * 100).toFixed(1);
    perAuthor.push({
      author: a.name,
      governanceScore: score,
      conventionalPct: +(conventionalPct*100).toFixed(1),
      issueRefPct: +(issueRefPct*100).toFixed(1),
      lengthQualityPct: +(lengthQuality*100).toFixed(1),
      wipPct: +(wipPct*100).toFixed(1),
      revertPct: +(revertPct*100).toFixed(1),
      shortPct: +(shortPct*100).toFixed(1)
    });
  }
  return perAuthor.sort((a,b)=> b.governanceScore - a.governanceScore);
}

function timelineAnalysis(commits) {
  const timeline = { weekly: new Map(), patterns: { first: null, last: null, totalSpan: 0, largestGap: { days: 0, start: null, end: null }, activityStreak: { longest: 0, longestStart: null, longestEnd: null } }, stats: { avgCommitsPerWeek: 0, medianCommitsPerWeek: 0, avgGap: 0, mostActiveWeek: { week: '', commits: 0, lines: 0 }, consistency: { score: 0, rating: 'Unknown', description: '' } } };
  if (!commits.length) return timeline;
  const sorted = [...commits].sort((a, b) => a.date - b.date);
  timeline.patterns.first = sorted[0].date; timeline.patterns.last = sorted.at(-1).date; timeline.patterns.totalSpan = Math.round((timeline.patterns.last - timeline.patterns.first) / 86400000);
  for (const c of sorted) {
    const weekStart = new Date(c.date); weekStart.setDate(c.date.getDate() - c.date.getDay()); weekStart.setHours(0,0,0,0);
    const key = weekStart.toISOString().slice(0,10);
    if (!timeline.weekly.has(key)) timeline.weekly.set(key, { start: weekStart, commits: 0, lines: 0 });
    const w = timeline.weekly.get(key); w.commits++; w.lines += c.insertions + c.deletions;
  }
  for (let i = 1; i < sorted.length; i++) {
    const gap = Math.round((sorted[i].date - sorted[i-1].date)/86400000);
    if (gap > timeline.patterns.largestGap.days) {
      timeline.patterns.largestGap = { days: gap, start: sorted[i-1].date, end: sorted[i].date };
    }
  }
  // Activity streak (allow up to 7 day gap like original logic)
  const activeDays = [...new Set(sorted.map(c => c.date.toISOString().slice(0,10)))].sort();
  let currentStreak = 1, longest = 1, streakStart = new Date(activeDays[0]), longestStart = streakStart, longestEnd = streakStart;
  for (let i=1;i<activeDays.length;i++) {
    const d = new Date(activeDays[i]); const prev = new Date(activeDays[i-1]);
    if ((d - prev)/86400000 <= 7) { currentStreak++; } else { if (currentStreak > longest) { longest = currentStreak; longestStart = streakStart; longestEnd = prev; } currentStreak = 1; streakStart = d; }
  }
  if (currentStreak > longest) { longest = currentStreak; longestStart = streakStart; longestEnd = new Date(activeDays.at(-1)); }
  timeline.patterns.activityStreak = { longest, longestStart, longestEnd };
  // Weekly stats
  const weeklyArr = [...timeline.weekly.values()];
  const counts = weeklyArr.map(w => w.commits).sort((a,b)=>a-b);
  if (counts.length) {
    timeline.stats.avgCommitsPerWeek = +(counts.reduce((a,b)=>a+b,0)/counts.length).toFixed(2);
    const mid = Math.floor(counts.length/2);
    timeline.stats.medianCommitsPerWeek = counts.length %2 ? counts[mid] : +((counts[mid-1]+counts[mid])/2).toFixed(2);
  }
  const mostActive = weeklyArr.sort((a,b)=>b.commits - a.commits)[0];
  if (mostActive) timeline.stats.mostActiveWeek = { week: mostActive.start.toISOString().slice(0,10), commits: mostActive.commits, lines: mostActive.lines };
  if (sorted.length>1) {
    let totalGaps = 0; for (let i=1;i<sorted.length;i++) totalGaps += (sorted[i].date - sorted[i-1].date)/86400000; timeline.stats.avgGap = +(totalGaps/(sorted.length-1)).toFixed(2);
  }
  // Consistency score (similar weighting)
  const gapScore = timeline.stats.avgGap ? Math.min(100 / Math.max(timeline.stats.avgGap/7,1), 40) : 40;
  const streakScore = Math.min(longest*2, 30);
  const weeklyScore = Math.min(timeline.stats.avgCommitsPerWeek*10, 30);
  const score = +(gapScore + streakScore + weeklyScore).toFixed(1);
  timeline.stats.consistency.score = score;
  if (score >=80) { timeline.stats.consistency.rating='Excellent'; timeline.stats.consistency.description='Very consistent commit patterns'; }
  else if (score>=60) { timeline.stats.consistency.rating='Good'; timeline.stats.consistency.description='Good consistency with regular activity'; }
  else if (score>=40) { timeline.stats.consistency.rating='Fair'; timeline.stats.consistency.description='Moderate consistency with gaps'; }
  else { timeline.stats.consistency.rating='Poor'; timeline.stats.consistency.description='Irregular activity with significant gaps'; }
  return timeline;
}

function codeReviewMetrics(commits, authorsMap) {
  const metrics = { mergeCommitRatio: 0, avgCommitSize: 0, largeThreshold: 500, large: 0, small: 0, reviewable: 0, collaborationScore: 0 };
  if (!commits.length) return metrics;
  let totalChurn = 0; let mergeCommits = 0; for (const c of commits) { const size = c.insertions + c.deletions; totalChurn += size; if (c.isMerge) mergeCommits++; if (size > metrics.largeThreshold) metrics.large++; else if (size < 50) metrics.small++; else metrics.reviewable++; }
  metrics.mergeCommitRatio = +(mergeCommits / commits.length).toFixed(3);
  metrics.avgCommitSize = +(totalChurn / commits.length).toFixed(1);
  const authorsCount = authorsMap.size; // simplistic collaboration score
  metrics.collaborationScore = +(Math.min(metrics.mergeCommitRatio*100,40) + Math.min(authorsCount*10,30) + (metrics.reviewable? Math.min(metrics.reviewable/commits.length*30,30):0)).toFixed(1);
  return metrics;
}

function authorInsights(authorsMap, integration) {
  const cards = [];
  for (const a of authorsMap.values()) {
    const totalLines = a.insertions + a.deletions;
    const avgSize = a.commits ? +(totalLines / a.commits).toFixed(2) : 0;
    const daysSpan = Math.max(1, Math.round((a.lastCommit - a.firstCommit)/86400000) + 1);
    const commitsPerDay = +(a.commits / daysSpan).toFixed(2);
    const issueRefs = integration.issueReferences.byAuthor.get(a.name)?.count || 0;
    const prRatio = a.commits ? +(a.mergeCommits / a.commits * 100).toFixed(1) : 0;
    // Biggest commit
    let biggest = { size: 0, hash: '', date: null, message: '' };
    for (const c of a.commitHistory) { const size = c.insertions + c.deletions; if (size > biggest.size) biggest = { size, hash: c.hash, date: c.date, message: c.subject }; }
    // Author-specific insights
    const insights = [];
    if (avgSize > 500) insights.push('Large average commit size - consider smaller commits');
    if (prRatio === 100) insights.push('All commits via merges - strong PR workflow');
    if (prRatio === 0 && a.commits > 2) insights.push('Low PR usage - consider more Pull Requests');
    if (!issueRefs && a.commits > 2) insights.push('No issue references - improve traceability');
    if (a.filesChanged.size > 20) insights.push('High file diversity - broad codebase knowledge');
    if (!insights.length) insights.push('Solid development practices');
    cards.push({ name: a.name, email: a.email, firstCommit: a.firstCommit, lastCommit: a.lastCommit, commits: a.commits, additions: a.insertions, deletions: a.deletions, files: a.filesChanged.size, avgCommitSize: avgSize, commitsPerDay, issueRefs, prIntegration: prRatio, biggestCommit: biggest, timing: a.timing, sourceStats: a.sourceStats, publishedStats: a.publishedStats, insights });
  }
  return cards.sort((a,b)=> b.commits - a.commits);
}

function overallInsights(metrics, authorsMap, integration, churnMetrics) {
  const insights = { overallHealth: 'Good', keyFindings: [], recommendations: [] };
  if (!metrics.totalCommits) { insights.overallHealth='Poor'; insights.keyFindings.push('No commits in analysis period'); insights.recommendations.push('Increase development activity'); }
  else if (metrics.totalCommits < 5) { insights.keyFindings.push(`Low commit frequency (${metrics.totalCommits} commits)`); insights.recommendations.push('Prefer more frequent, smaller commits'); }
  if (churnMetrics.avgChurnPerCommit > 500) { insights.keyFindings.push(`High average churn (${churnMetrics.avgChurnPerCommit} lines/commit)`); insights.recommendations.push('Split large changes into smaller reviewable commits'); }
  if (churnMetrics.churnRate > 0.5) { insights.keyFindings.push('High deletion rate suggests significant refactoring'); insights.recommendations.push('Ensure adequate tests cover refactors'); }
  if (authorsMap.size === 1) { insights.keyFindings.push('Single author project'); insights.recommendations.push('Adopt PR workflow & branch protections (even solo)'); }
  // Conventional commits adoption
  const convRatio = integration.commitMessageQuality.total ? integration.commitMessageQuality.withConventional / integration.commitMessageQuality.total : 0;
  if (convRatio > 0.7) insights.keyFindings.push(`Excellent Conventional Commits adoption (${(convRatio*100).toFixed(1)}%)`);
  else if (convRatio > 0.3) { insights.keyFindings.push(`Moderate Conventional Commits adoption (${(convRatio*100).toFixed(1)}%)`); insights.recommendations.push('Increase Conventional Commits usage for automation'); }
  else { insights.recommendations.push('Adopt Conventional Commits for better changelogs'); }
  if (integration.commitMessageQuality.averageLength < 20) { insights.keyFindings.push('Short commit messages'); insights.recommendations.push('Use descriptive commit messages explaining WHY'); }
  return insights;
}

// ---------------- Output Generators -----------------------------
function generateHTML(repo, opts, data) {
  const { commits, authors, fileStats, integration, churn, timeline, codeReview, authorCards, insights, advanced } = data;
  const totalInsertions = commits.reduce((s,c)=>s+c.insertions,0); const totalDeletions = commits.reduce((s,c)=>s+c.deletions,0);
  const sourceFilesCount = [...fileStats.values()].filter(f=>f.category==='source').length;
  // Repo source vs published aggregate
  const repoSource = { commits:0, files:0, insertions:0, deletions:0 };
  const repoPublished = { commits:0, files:0, insertions:0, deletions:0 };
  for (const a of authors.values()) { repoSource.commits += a.sourceStats.commits; repoSource.files += a.sourceStats.files; repoSource.insertions += a.sourceStats.insertions; repoSource.deletions += a.sourceStats.deletions; repoPublished.commits += a.publishedStats.commits; repoPublished.files += a.publishedStats.files; repoPublished.insertions += a.publishedStats.insertions; repoPublished.deletions += a.publishedStats.deletions; }
  const topFiles = [...fileStats.entries()].filter(([f,s])=>s.category==='source').map(([filename,stats])=>({filename,...stats})).sort((a,b)=> b.changes - a.changes).slice(0,20);
  const recentCommits = [...commits].sort((a,b)=> b.date - a.date).slice(0, opts.commitCount);
  const weeklyLabels = [...timeline.weekly.keys()].sort();
  const weeklyCommits = weeklyLabels.map(k => timeline.weekly.get(k).commits);
  const weeklyAvgLines = weeklyLabels.map(k => { const w = timeline.weekly.get(k); return +(w.lines / Math.max(w.commits,1)).toFixed(0); });
  const style = `body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Arial,sans-serif;margin:0;padding:24px;background:#f5f7fb;color:#222;}h1{margin-top:0}table{width:100%;border-collapse:collapse;margin:16px 0}th,td{padding:8px 10px;border-bottom:1px solid #e3e6eb;text-align:left}th{background:#f0f3f7}tr:hover{background:#fafcff}.grid{display:grid;gap:16px}.metrics{grid-template-columns:repeat(auto-fit,minmax(190px,1fr));margin:20px 0}.card{background:#fff;border:1px solid #dde3ea;border-radius:8px;padding:16px;box-shadow:0 2px 4px rgba(0,0,0,.04)}.stat-number{font-size:1.9rem;font-weight:600;color:#0077c8}.insert{color:#1b8f3a;font-weight:600}.delete{color:#d33f3f;font-weight:600}.badge{display:inline-block;padding:2px 6px;border-radius:12px;font-size:.7rem;font-weight:600;margin-left:6px;color:#fff}.source{background:#1b8f3a}.published{background:#f39c12}.author-card{display:flex;flex-direction:column;gap:6px;margin:12px 0;padding:14px;background:#fff;border:1px solid #e0e6ed;border-radius:8px}.small{font-size:.8rem;color:#666}.timing{background:#f4f9ff;padding:8px 10px;border-radius:6px;font-size:.75rem;display:inline-block;margin:2px 4px 2px 0}.workpattern{padding:2px 8px;border-radius:14px;font-size:.65rem;font-weight:600}.Weekday-focused{background:#e3f7e8;color:#1b8f3a}.Weekendwarrior{background:#fff1c2;color:#8a6d00}.recommendations{background:#fff7e0;border:1px solid #ffe4a3;padding:12px;border-radius:8px}.health{display:inline-block;padding:4px 14px;border-radius:18px;font-weight:600}.Good{background:#e3f7e8;color:#1b8f3a}.Fair{background:#fff4cc;color:#8a6d00}.Poor{background:#ffe1e1;color:#b21e1e}.section{margin-top:42px}`;
  return `<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8"/><title>Git Activity Report - ${escapeHtml(repo.name)}</title><meta name="viewport" content="width=device-width,initial-scale=1"/><style>${style}</style><script src="https://cdn.jsdelivr.net/npm/chart.js"></script></head><body><h1>🔍 Git Activity Report</h1><div class="card"><strong>Repository:</strong> ${escapeHtml(repo.name)}<br/><strong>Branch:</strong> ${escapeHtml(repo.currentBranch)}<br/><strong>Range:</strong> ${advanced.range.since} → ${advanced.range.until}<br/><strong>Generated:</strong> ${repo.analysisDate.toLocaleString()}<br/><strong>Total Commits (All Time):</strong> ${repo.totalCommits}<br/><strong>Report Version:</strong> ${REPORT_VERSION}</div><div class="grid metrics">${metricCard(commits.length,'Commits')+metricCard(authors.size,'Authors')+metricCard(sourceFilesCount,'Source Files Changed')+metricCard(`<span class="insert">+${totalInsertions.toLocaleString()}</span>`,'Lines Added')+metricCard(`<span class="delete">-${totalDeletions.toLocaleString()}</span>`,'Lines Removed')}</div><div class="card"><h2>Source vs Published</h2><div class="grid metrics" style="margin-top:0">${metricCard(`${repoSource.commits} commits / ${repoSource.files} files <span class='badge source'>+${repoSource.insertions}/-${repoSource.deletions}</span>`,'Source Activity')+metricCard(`${repoPublished.commits} commits / ${repoPublished.files} files <span class='badge published'>+${repoPublished.insertions}/-${repoPublished.deletions}</span>`,'Published Activity')+metricCard(churn.avgChurnPerCommit,'Avg Churn / Commit')+metricCard(churn.churnRate.toFixed(2),'Churn Rate')}</div></div><div class="card"><h2>Advanced Health Metrics</h2><div class="grid metrics" style="margin-top:0">${metricCard(advanced.tags.count,'Tags')+metricCard(advanced.tags.avgDaysBetween ?? '—','Avg Tag Days')+metricCard(advanced.branches.staleCount,'Stale Branches')+metricCard(advanced.branches.namingCompliancePct+'%','Branch Naming OK')+metricCard(advanced.ownership.dominanceRatio+'%','Dominant Files')+metricCard(advanced.ownership.busFactorTop3Share+'%','Top3 Bus Share')+metricCard(advanced.work.afterHoursPct+'%','After Hours')+metricCard(advanced.work.weekendPct+'%','Weekend Commits')}</div></div><h2 class="section">👥 Top Contributors</h2>${authorCards.map(a=>authorCard(a)).join('')}<h2 class="section">📈 Timeline & Consistency</h2><div class="card"><canvas id="timeline" height="220"></canvas><div style="display:flex;flex-wrap:wrap;gap:18px;margin-top:14px">${miniStat(timeline.stats.avgCommitsPerWeek,'Avg Commits / Week')+miniStat(timeline.stats.mostActiveWeek.commits,'Peak Week Commits')+miniStat(timeline.stats.consistency.score+'%','Consistency Score')+miniStat(timeline.patterns.largestGap.days,'Largest Gap (days)')}</div></div><h2 class="section">📦 Most Changed Source Files</h2><div class="card"><table><thead><tr><th>File</th><th>Changes</th><th>+Lines</th><th>-Lines</th></tr></thead><tbody>${topFiles.map(f=>`<tr><td><code>${escapeHtml(f.filename)}</code></td><td>${f.changes}</td><td class='insert'>+${f.insertions}</td><td class='delete'>-${f.deletions}</td></tr>`).join('')}</tbody></table></div><h2 class="section">🔥 Hotspots (Churn * Authors)</h2><div class="card"><table><thead><tr><th>File</th><th>Churn</th><th>Authors</th><th>Score</th></tr></thead><tbody>${advanced.hotspots.map(h=>`<tr><td><code>${escapeHtml(h.file)}</code></td><td>${h.churn}</td><td>${h.authors}</td><td>${h.score}</td></tr>`).join('')||'<tr><td colspan=4>No data</td></tr>'}</tbody></table></div>${opts.heavy?`<h2 class=\"section\">🔗 Temporal Coupling (Top ${advanced.coupling.length})</h2><div class=\"card\"><table><thead><tr><th>File A</th><th>File B</th><th>Co-Changes</th><th>Strength %</th></tr></thead><tbody>${advanced.coupling.map(p=>`<tr><td><code>${escapeHtml(p.a)}</code></td><td><code>${escapeHtml(p.b)}</code></td><td>${p.count}</td><td>${p.strength}</td></tr>`).join('')}</tbody></table></div>`:''}<h2 class="section">🛡 Ownership & Bus Factor</h2><div class="card"><table><thead><tr><th>File</th><th>Lead Author</th><th>Dominance</th><th>Authors</th></tr></thead><tbody>${advanced.ownership.ownership.slice(0,25).map(o=>`<tr><td><code>${escapeHtml(o.file)}</code></td><td>${escapeHtml(o.leader)}</td><td>${(o.dominance*100).toFixed(1)}%</td><td>${o.authors}</td></tr>`).join('')}</tbody></table></div><h2 class="section">🌐 Language Breakdown</h2><div class="card"><table><thead><tr><th>Ext</th><th>Files</th><th>%</th></tr></thead><tbody>${advanced.languages.breakdown.map(l=>`<tr><td>${escapeHtml(l.ext)}</td><td>${l.count}</td><td>${l.pct}</td></tr>`).join('')}</tbody></table>${advanced.largeFiles.length?`<h3>Large Changed Files (&gt;${advanced.config.largeFileKB}KB)</h3><ul>${advanced.largeFiles.map(f=>`<li><code>${escapeHtml(f.file)}</code> - ${f.kb} KB</li>`).join('')}</ul>`:''}</div><h2 class="section">📐 Commit Size Distribution</h2><div class="card"><table><thead><tr><th>Bucket</th><th>Commits</th><th>%</th></tr></thead><tbody>${advanced.commitSizes.map(b=>`<tr><td>${b.label}</td><td>${b.count}</td><td>${b.pct}</td></tr>`).join('')}</tbody></table></div><h2 class="section">⚙ Change Quality & Stability</h2><div class="card"><ul><li>Revert Commits: ${integration.pullRequestPatterns.revertCommits}</li><li>Corrective Ratio: ${advanced.corrective.correctiveRatio}%</li><li>Average Issue Lead Time: ${advanced.issueLead.avgLeadTimeDays ?? '—'} days (completed ${advanced.issueLead.completed}/${advanced.issueLead.count})</li><li>Bursts Detected: ${advanced.work.bursts.length}</li></ul></div><h2 class="section">🧪 Code Review Metrics</h2><div class="grid metrics">${metricCard(codeReview.mergeCommitRatio,'Merge Commit Ratio')+metricCard(codeReview.avgCommitSize,'Avg Commit Size')+metricCard(codeReview.large,'Large Commits')+metricCard(codeReview.reviewable,'Reviewable Commits')+metricCard(codeReview.collaborationScore,'Collaboration Score')}</div><h2 class="section">📝 Recent Commits (Latest ${opts.commitCount})</h2><div class="card"><table><thead><tr><th>Hash</th><th>Author</th><th>Date</th><th>Message</th><th>Files</th><th>+/-</th></tr></thead><tbody>${recentCommits.map(c=>`<tr><td><code>${c.hash.slice(0,8)}</code></td><td>${escapeHtml(c.author)}</td><td>${c.date.toLocaleDateString()}</td><td>${escapeHtml(c.subject)}</td><td>${c.filesChanged.length}</td><td><span class='insert'>+${c.insertions}</span> <span class='delete'>-${c.deletions}</span></td></tr>`).join('')}</tbody></table></div><h2 class="section">🛠 Governance Scores</h2><div class="card"><table><thead><tr><th>Author</th><th>Score</th><th>Conventional %</th><th>Issue Ref %</th></tr></thead><tbody>${advanced.governance.map(g=>`<tr><td>${escapeHtml(g.author)}</td><td>${g.governanceScore}</td><td>${g.conventionalPct}</td><td>${g.issueRefPct}</td></tr>`).join('')}</tbody></table></div><h2 class="section">🚨 Risk Scores (Top 25)</h2><div class="card"><table><thead><tr><th>File</th><th>Score</th><th>Churn</th><th>Authors</th><th>Last Touched</th><th>Dominance</th></tr></thead><tbody>${advanced.risk.map(r=>`<tr><td><code>${escapeHtml(r.file)}</code></td><td>${r.score}</td><td>${r.churn}</td><td>${r.authors}</td><td>${r.lastTouched.toISOString().slice(0,10)}</td><td>${r.dominance!=null?(r.dominance*100).toFixed(1)+'%':'—'}</td></tr>`).join('')}</tbody></table></div><h2 class="section">🔍 Insights & Health</h2><div class="card"><p><strong>Overall Health:</strong> <span class="health ${insights.overallHealth}">${insights.overallHealth}</span></p><h3>Key Findings</h3><ul>${insights.keyFindings.map(f=>`<li>${escapeHtml(f)}</li>`).join('') || '<li>No significant findings</li>'}</ul><h3>Recommendations</h3><div class="recommendations"><ul>${insights.recommendations.map(r=>`<li>${escapeHtml(r)}</li>`).join('') || '<li>No recommendations at this time</li>'}</ul></div></div><script>const ctx=document.getElementById('timeline').getContext('2d');new Chart(ctx,{type:'line',data:{labels:${JSON.stringify(weeklyLabels.map(l=>l.slice(5)))},datasets:[{label:'Commits / Week',data:${JSON.stringify(weeklyCommits)},borderColor:'#0077c8',backgroundColor:'rgba(0,119,200,0.12)',borderWidth:3,fill:true,tension:.35,yAxisID:'y'},{label:'Avg Lines / Commit',data:${JSON.stringify(weeklyAvgLines)},borderColor:'#d33f3f',backgroundColor:'rgba(211,63,63,0.08)',borderWidth:2,fill:false,tension:.35,yAxisID:'y1'}]},options:{responsive:true,interaction:{mode:'index',intersect:false},scales:{y:{beginAtZero:true,title:{display:true,text:'Commits'}},y1:{beginAtZero:true,position:'right',grid:{drawOnChartArea:false},title:{display:true,text:'Avg Lines'}}}}});</script></body></html>`;
}

function metricCard(value,label){return `<div class="card"><div class="stat-number">${value}</div><div class="small">${label}</div></div>`;}
function miniStat(value,label){return `<div style='min-width:140px'><div style='font-size:1.3rem;font-weight:600;'>${value}</div><div class='small'>${label}</div></div>`;}
function authorCard(a){return `<div class='author-card'><div><strong>${escapeHtml(a.name)}</strong> <span class='small'>${escapeHtml(a.email)}</span></div><div class='small'>${a.firstCommit.toLocaleDateString()} → ${a.lastCommit.toLocaleDateString()} • ${a.commits} commits • +${a.additions}/-${a.deletions}</div><div class='small'>Files: ${a.files} • Avg Size: ${a.avgCommitSize} • Commits/Day: ${a.commitsPerDay}</div><div class='small'>Issue Refs: ${a.issueRefs} • PR Integration: ${a.prIntegration}%</div><div>${a.timing ? `<span class='timing'>Preferred: ${a.timing.preferredDay}</span><span class='timing'>Peak: ${a.timing.timeCategory}</span><span class='timing workpattern ${a.timing.workPattern.replace(/\s+/g,'')}'><span class='workpattern ${a.timing.workPattern.replace(/\s+/g,'')}'></span>${a.timing.workPattern}</span>`:''}</div><div class='small'>Source: +${a.sourceStats.insertions}/-${a.sourceStats.deletions} (${a.sourceStats.commits} commits) | Published: +${a.publishedStats.insertions}/-${a.publishedStats.deletions}</div><details style='margin-top:6px'><summary style='cursor:pointer;font-size:.85rem'>Insights</summary><ul class='small'>${a.insights.map(i=>`<li>${escapeHtml(i)}</li>`).join('')}</ul><div class='small'>Biggest Commit: ${a.biggestCommit.size} lines (${a.biggestCommit.hash.slice(0,8)})</div></details></div>`;}

function generateJSON(repo, opts, data) {
  return JSON.stringify({ schemaVersion: 2, reportVersion: REPORT_VERSION, generatedAt: new Date().toISOString(), repository: repo, range: data.advanced.range, options: opts, totals: { commits: data.commits.length, authors: data.authors.size }, integration: data.integration, churn: data.churn, timeline: data.timeline, codeReview: data.codeReview, insights: data.insights, advanced: { tags: data.advanced.tags, branches: data.advanced.branches, ownership: { dominanceRatio: data.advanced.ownership.dominanceRatio, busFactorTop3Share: data.advanced.ownership.busFactorTop3Share }, hotspots: data.advanced.hotspots, coupling: opts.heavy? data.advanced.coupling: undefined, languages: data.advanced.languages, commitSizes: data.advanced.commitSizes, corrective: data.advanced.corrective, issueLead: data.advanced.issueLead, governance: data.advanced.governance, risk: data.advanced.risk }, authors: data.authorCards.map(a=>({ name:a.name, email:a.email, commits:a.commits, additions:a.additions, deletions:a.deletions, avgCommitSize:a.avgCommitSize, commitsPerDay:a.commitsPerDay, issueRefs:a.issueRefs, prIntegration:a.prIntegration, timing:a.timing })) }, null, 2);
}

function generateConsole(repo, opts, data) {
  const lines = [];
  lines.push('='.repeat(78));
  lines.push(` Git Activity Report - ${repo.name} (Last ${opts.days} days)`);
  lines.push('='.repeat(78));
  lines.push(`Commits: ${data.commits.length}  Authors: ${data.authors.size}  Lines +${data.commits.reduce((s,c)=>s+c.insertions,0)}/-${data.commits.reduce((s,c)=>s+c.deletions,0)}`);
  lines.push('--- Top Authors ---');
  for (const a of data.authorCards.slice(0,5)) { lines.push(`${a.name}: ${a.commits} commits +${a.additions}/-${a.deletions} avg:${a.avgCommitSize}`); }
  lines.push('--- Insights ---');
  lines.push(`Overall Health: ${data.insights.overallHealth}`);
  for (const f of data.insights.keyFindings) lines.push(` * Finding: ${f}`);
  for (const r of data.insights.recommendations) lines.push(` * Recommend: ${r}`);
  return lines.join('\n');
}

// ---------------- Main ------------------------------------------
async function main() {
  try {
    await fs.mkdir(TEMP_REPORTS_DIR, { recursive: true });
    await fs.mkdir(REPORTS_DIR, { recursive: true });
    try { await fs.access('.git'); } catch { throw new Error('Not inside a Git repository'); }
    const opts = parseArgs();
    // Load config overrides if provided
    let userConfig = {};
    if (opts.configPath) {
      try { const raw = await fs.readFile(opts.configPath,'utf8'); userConfig = JSON.parse(raw); } catch (e) { console.warn('Config load failed:', e.message); }
    }
    const cfg = { ...DEFAULT_CONFIG, ...userConfig };
    const range = resolveDateRange(opts);
    const repo = getRepositoryInfo();
    console.log(`Analyzing range ${range.since} → ${range.until} for repo ${repo.name} ...`);
    const { commits, authors, fileStats, perFileAuthorLines } = getCommitData(range);
    console.log(`Collected ${commits.length} commits from ${authors.size} authors`);
    const integration = analyzeGitHubIntegration(commits, authors);
    const churn = computeChurnMetrics(commits);
    const timeline = timelineAnalysis(commits);
    const codeReview = codeReviewMetrics(commits, authors);
    const authorCards = authorInsights(authors, integration);
    const insights = overallInsights({ totalCommits: commits.length }, authors, integration, churn);
    // Advanced computations
    const tags = getTagCadence();
    const branches = getBranchInfo(cfg);
    const ownership = computeOwnership(perFileAuthorLines, cfg.ownershipDominancePct);
    const hotspots = computeHotspots(churn, ownership);
    const coupling = opts.heavy ? computeTemporalCoupling(commits, cfg.couplingMaxPairs) : [];
    const languages = languageBreakdown();
    const largeFiles = await detectLargeChangedFiles(commits, cfg.largeFileKB);
    const commitSizes = commitSizeDistribution(commits, cfg);
    const work = workPatternMetrics(commits, cfg);
    const corrective = correctiveVsFeature(commits);
    const issueLead = issueLeadTime(commits);
  const governance = governanceScores(integration, authors, cfg);
  const risk = riskScores(churn, ownership, commits, cfg, coupling);
    const advanced = { tags, branches, ownership, hotspots, coupling, languages, largeFiles, commitSizes, work, corrective, issueLead, governance, risk, config: cfg, range };
    const data = { commits, authors, fileStats, integration, churn, timeline, codeReview, authorCards, insights, advanced };
    const timestamp = new Date().toISOString().replace(/[:.]/g,'-').split('T')[0];
    const baseName = `git-activity-report-${timestamp}`;
    if (opts.format === 'json') {
      const json = generateJSON(repo, opts, data);
      const file = path.join(opts.output, baseName + '.json');
      await fs.writeFile(file, json, 'utf8');
      console.log('✅ JSON report written to', file);
    } else if (opts.format === 'console') {
      console.log(generateConsole(repo, opts, data));
    } else { // html default
      const html = generateHTML(repo, opts, data);
      const file = path.join(opts.output, baseName + '.html');
      await fs.writeFile(file, html, 'utf8');
      console.log('✅ HTML report written to', file);
    }
  } catch (err) {
    console.error('❌ Error generating git report');
    console.error(err.message);
    process.exit(1);
  }
}

// Invoke main when executed directly (not when imported)
try {
  const thisFile = fileURLToPath(import.meta.url);
  if (process.argv[1] && path.resolve(process.argv[1]) === thisFile) {
    await main();
  }
} catch {
  // Fallback: just run main
  await main();
}

export { main };
