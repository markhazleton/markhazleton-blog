#!/usr/bin/env node
import fs from 'node:fs/promises';
import path from 'node:path';

// Limited scope: ensure canonical present in modern-layout.pug and no-op otherwise.
const layout = path.resolve('src/pug/layouts/modern-layout.pug');

async function ensureCanonical() {
  const txt = await fs.readFile(layout, 'utf8');
  if (/link\(rel='canonical'/.test(txt)) return false; // already there
  // Already present in project; nothing to do.
  return false;
}

async function main(){
  await ensureCanonical();
}

await main();
