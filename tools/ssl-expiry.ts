#!/usr/bin/env tsx
import tls from 'node:tls';
import { URL } from 'node:url';
import fs from 'node:fs/promises';
import path from 'node:path';

const ART_DIR = path.resolve('artifacts');
await fs.mkdir(ART_DIR, { recursive: true });

const siteUrl = process.env.SITE_URL || process.env.npm_package_config_siteUrl || 'https://markhazleton.com';
const { hostname, port } = new URL(siteUrl);

function getCert(host: string, portNum: number): Promise<tls.PeerCertificate> {
  return new Promise((resolve, reject) => {
    const socket = tls.connect({ host, port: portNum || 443, servername: host }, () => {
      const cert = socket.getPeerCertificate();
      socket.end();
      if (cert && cert.valid_to) resolve(cert);
      else reject(new Error('No certificate'));
    });
    socket.on('error', reject);
  });
}

function daysUntil(dateStr: string): number {
  const exp = new Date(dateStr).getTime();
  const now = Date.now();
  return Math.ceil((exp - now) / (1000 * 60 * 60 * 24));
}

(async () => {
  try {
    const cert = await getCert(hostname, Number(port) || 443);
    const days = daysUntil(cert.valid_to);
    const result = {
      host: hostname,
      valid_to: cert.valid_to,
      days_remaining: days,
      warn: days < 30
    };
    await fs.writeFile(path.join(ART_DIR, 'ssl.json'), JSON.stringify(result, null, 2));
  } catch (e) {
    await fs.writeFile(path.join(ART_DIR, 'ssl.json'), JSON.stringify({ error: String(e) }, null, 2));
    process.exitCode = 1;
  }
})();
