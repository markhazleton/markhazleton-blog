#!/usr/bin/env node
import fs from 'node:fs/promises';
import path from 'node:path';
import { parse } from 'node-html-parser';
import dayjs from 'dayjs';

const ART_DIR = path.resolve('artifacts');
await fs.mkdir(ART_DIR, { recursive: true });

const siteUrl = process.env.SITE_URL || process.env.npm_package_config_siteUrl || 'https://markhazleton.com';
const sitemapUrl = process.env.SITEMAP_URL || process.env.npm_package_config_sitemapUrl || `${siteUrl.replace(/\/$/,'')}/sitemap.xml`;

function absolute(u) {
  try { new URL(u); return true; } catch { return false; }
}

async function fetchText(url) {
  const res = await fetch(url, { redirect: 'follow' });
  if (!res.ok) throw new Error(`${url} -> ${res.status}`);
  return res.text();
}

async function tryFetch(url) {
  try { return await fetchText(url); } catch (e) { return null; }
}

function extractUrlsFromSitemap(xml, limit = 10) {
  const urls = Array.from(xml.matchAll(/<loc>(.*?)<\/loc>/g)).map(m => m[1]);
  return urls.slice(0, limit);
}

function checkSeo(html, url) {
  const root = parse(html);
  const title = root.querySelector('title')?.text?.trim() || '';
  const desc = root.querySelector('meta[name="description"]')?.getAttribute('content') || '';
  const h1s = root.querySelectorAll('h1');
  const htmlEl = root.querySelector('html');
  const lang = htmlEl?.getAttribute('lang') || '';
  const canonical = root.querySelector('link[rel="canonical"]')?.getAttribute('href') || '';
  const ogTitle = root.querySelector('meta[property="og:title"]')?.getAttribute('content') || '';
  const ogDesc = root.querySelector('meta[property="og:description"]')?.getAttribute('content') || '';
  const ogType = root.querySelector('meta[property="og:type"]')?.getAttribute('content') || '';
  const ogUrl = root.querySelector('meta[property="og:url"]')?.getAttribute('content') || '';

  const issues = [];
  if (title.length < 30 || title.length > 65) issues.push({ rule: 'title_length', title });
  if (desc.length < 70 || desc.length > 160) issues.push({ rule: 'meta_desc_length', desc });
  if (h1s.length !== 1) issues.push({ rule: 'single_h1', count: h1s.length });
  if (!lang) issues.push({ rule: 'html_lang_missing' });
  if (!canonical || !absolute(canonical)) issues.push({ rule: 'canonical_absent_or_relative', canonical });
  if (!ogTitle) issues.push({ rule: 'og_title_missing' });
  if (!ogDesc) issues.push({ rule: 'og_desc_missing' });
  if (!ogType) issues.push({ rule: 'og_type_missing' });
  if (!ogUrl) issues.push({ rule: 'og_url_missing' });

  return { url, title, desc, lang, canonical, issues };
}

async function main() {
  const out = { runAt: dayjs().toISOString(), siteUrl, sitemapUrl, pages: [] };

  const robotsTxtUrl = `${siteUrl.replace(/\/$/, '')}/robots.txt`;
  const robots = await tryFetch(robotsTxtUrl);
  out.robots = { ok: !!robots, url: robotsTxtUrl };
  if (robots && /Disallow:\s*\//i.test(robots)) {
    out.robots.blockingAll = true;
  }

  let urls = [];
  const sm = await tryFetch(sitemapUrl);
  if (sm) {
    out.sitemap = { ok: true, url: sitemapUrl };
    urls = extractUrlsFromSitemap(sm, 10);
  } else {
    out.sitemap = { ok: false, url: sitemapUrl };
    urls = [siteUrl];
  }

  for (const u of urls) {
    try {
      const html = await fetchText(u);
      out.pages.push(checkSeo(html, u));
    } catch (e) {
      out.pages.push({ url: u, error: String(e) });
    }
  }

  await fs.writeFile(path.join(ART_DIR, 'seo.json'), JSON.stringify(out, null, 2));
}

await main().catch(async (e) => {
  await fs.writeFile(path.join(ART_DIR, 'seo.json'), JSON.stringify({ error: String(e) }, null, 2));
  process.exitCode = 1;
});
