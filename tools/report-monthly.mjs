#!/usr/bin/env node
import fs from 'node:fs/promises';
import path from 'node:path';
import dayjs from 'dayjs';

const artifactsDir = path.resolve('artifacts');
const reportsDir = path.resolve('maintenance', 'reports');
await fs.mkdir(reportsDir, { recursive: true });

function safeReadJson(p) {
  return fs.readFile(p, 'utf8').then(JSON.parse).catch(() => null);
}

function median(nums) {
  if (!nums?.length) return null;
  const s = [...nums].sort((a,b)=>a-b); const mid = Math.floor(s.length/2);
  return s.length % 2 === 0 ? (s[mid-1]+s[mid])/2 : s[mid];
}

async function extractLhci() {
  try {
    const base = path.resolve('lhci');
    const files = await fs.readdir(base);
    const sums = files.filter(f=>f.endsWith('.json'));
    const perf = [];
    for (const f of sums) {
      const j = await safeReadJson(path.join(base, f));
      if (j?.categories?.performance?.score != null) perf.push(j.categories.performance.score);
    }
    return { perfMedian: perf.length ? median(perf)*100 : null };
  } catch { return { perfMedian: null }; }
}

async function main() {
  const now = dayjs();
  const ym = now.format('YYYY-MM');
  const prevYm = now.subtract(1, 'month').format('YYYY-MM');

  const seo = await safeReadJson(path.join(artifactsDir, 'seo.json'));
  const a11y = await safeReadJson(path.join(artifactsDir, 'a11y.json'));
  const ssl = await safeReadJson(path.join(artifactsDir, 'ssl.json'));
  const lh = await extractLhci();

  const prevReport = await fs.readFile(path.join(reportsDir, `${prevYm}.md`)).catch(() => null);

  const lines = [];
  lines.push(`# Monthly Maintenance Report - ${ym}`);
  lines.push('');
  lines.push('## Summary');
  lines.push(`- Perf median: ${lh.perfMedian ?? 'n/a'}`);
  lines.push(`- SEO pages checked: ${seo?.pages?.length ?? 0}`);
  lines.push(`- A11y report: ${a11y ? 'present' : 'n/a'}`);
  lines.push(`- SSL days remaining: ${ssl?.days_remaining ?? 'n/a'}`);
  lines.push('');
  lines.push('## Details');
  lines.push('### SEO');
  if (seo?.pages) {
    for (const p of seo.pages) {
      lines.push(`- ${p.url}: ${p.issues?.length ?? 0} issues`);
    }
  }
  lines.push('');
  lines.push('### Accessibility');
  lines.push(a11y ? 'See artifacts/a11y.json' : 'No a11y data');
  lines.push('');
  lines.push('### SSL');
  lines.push(ssl ? `Certificate for ${ssl.host} expires in ${ssl.days_remaining} days (warn=${ssl.warn})` : 'No SSL data');
  lines.push('');
  lines.push('### Previous Month');
  lines.push(prevReport ? 'Previous report found.' : 'No previous report.');

  await fs.writeFile(path.join(reportsDir, `${ym}.md`), lines.join('\n'));
}

await main();
