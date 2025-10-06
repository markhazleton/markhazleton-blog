/**
 * Download Google Fonts for Local Hosting
 * Downloads Google Fonts to eliminate external font dependencies
 */

const https = require('https');
const fs = require('fs').promises;
const path = require('path');

class FontDownloader {
    constructor() {
        this.fontsDir = path.join(__dirname, '../../src/assets/fonts');
        this.staticDir = path.join(__dirname, '../../docs/assets/fonts');
        this.cssOutputPath = path.join(__dirname, '../../src/assets/css/fonts.css');
        this.staticCssPath = path.join(__dirname, '../../docs/assets/css/fonts.css');
    }

    /**
     * Download a file from URL
     */
    downloadFile(url, filepath) {
        return new Promise((resolve, reject) => {
            const file = require('fs').createWriteStream(filepath);

            https.get(url, (response) => {
                if (response.statusCode === 200) {
                    response.pipe(file);
                    file.on('finish', () => {
                        file.close();
                        resolve();
                    });
                } else if (response.statusCode === 301 || response.statusCode === 302) {
                    // Follow redirect
                    file.close();
                    require('fs').unlinkSync(filepath);
                    this.downloadFile(response.headers.location, filepath).then(resolve).catch(reject);
                } else {
                    file.close();
                    require('fs').unlinkSync(filepath);
                    reject(new Error(`Failed to download ${url}: ${response.statusCode}`));
                }
            }).on('error', (err) => {
                file.close();
                reject(err);
            });
        });
    }

    /**
     * Get Google Fonts CSS with proper User-Agent
     */
    async getGoogleFontsCss(fontFamily) {
        return new Promise((resolve, reject) => {
            const url = `https://fonts.googleapis.com/css2?family=${fontFamily}&display=swap`;

            const options = {
                headers: {
                    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
                }
            };

            https.get(url, options, (response) => {
                let data = '';

                response.on('data', (chunk) => {
                    data += chunk;
                });

                response.on('end', () => {
                    if (response.statusCode === 200) {
                        resolve(data);
                    } else {
                        reject(new Error(`Failed to fetch CSS: ${response.statusCode}`));
                    }
                });
            }).on('error', reject);
        });
    }

    /**
     * Extract font URLs from CSS
     */
    extractFontUrls(css) {
        const urlRegex = /url\((https:\/\/fonts\.gstatic\.com\/[^)]+)\)/g;
        const urls = [];
        let match;

        while ((match = urlRegex.exec(css)) !== null) {
            urls.push(match[1]);
        }

        return urls;
    }

    /**
     * Download Inter font family
     */
    async downloadInterFont() {
        console.log('🔤 Downloading Inter font family...');

        // Create directories
        await fs.mkdir(this.fontsDir, { recursive: true });
        await fs.mkdir(this.staticDir, { recursive: true });
        await fs.mkdir(path.dirname(this.cssOutputPath), { recursive: true });
        await fs.mkdir(path.dirname(this.staticCssPath), { recursive: true });

        try {
            // Get the CSS for Inter font
            const fontFamily = 'Inter:wght@300;400;500;600;700';
            const css = await this.getGoogleFontsCss(fontFamily);

            // Extract font URLs
            const fontUrls = this.extractFontUrls(css);
            console.log(`📄 Found ${fontUrls.length} font files to download`);

            // Download each font file
            const downloadedFonts = [];
            for (let i = 0; i < fontUrls.length; i++) {
                const url = fontUrls[i];
                const filename = `inter-${i + 1}.woff2`;
                const filepath = path.join(this.fontsDir, filename);
                const staticpath = path.join(this.staticDir, filename);

                console.log(`   📥 Downloading ${filename}...`);
                await this.downloadFile(url, filepath);

                // Copy to static directory
                await fs.copyFile(filepath, staticpath);

                downloadedFonts.push({
                    originalUrl: url,
                    filename,
                    localPath: `/assets/fonts/${filename}`
                });
            }

            // Generate local CSS
            let localCss = css;
            downloadedFonts.forEach(font => {
                localCss = localCss.replace(font.originalUrl, font.localPath);
            });

            // Write CSS files
            await fs.writeFile(this.cssOutputPath, localCss, 'utf8');
            await fs.writeFile(this.staticCssPath, localCss, 'utf8');

            console.log(`✅ Downloaded ${downloadedFonts.length} font files`);
            console.log(`📝 Generated local CSS: ${this.cssOutputPath}`);

            return {
                fonts: downloadedFonts,
                cssPath: '/assets/css/fonts.css'
            };

        } catch (error) {
            console.error('❌ Error downloading fonts:', error.message);
            console.log('⚠️  Falling back to system fonts...');

            // Create fallback CSS with system fonts
            const fallbackCss = `
/* Inter Font Family - Local Fallback */
@font-face {
  font-family: 'Inter';
  font-style: normal;
  font-weight: 300 700;
  font-display: swap;
  src: local('Inter'), local('system-ui'), local('-apple-system'), local('Segoe UI'), local('Roboto'), local('sans-serif');
}

/* System font stack fallback */
.inter-fallback {
  font-family: Inter, system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif;
}
`.trim();

            await fs.writeFile(this.cssOutputPath, fallbackCss, 'utf8');
            await fs.writeFile(this.staticCssPath, fallbackCss, 'utf8');

            return {
                fonts: [],
                cssPath: '/assets/css/fonts.css',
                fallback: true
            };
        }
    }
}

// Export for use in build system
module.exports = FontDownloader;

// Run directly if called from command line
if (require.main === module) {
    const downloader = new FontDownloader();
    downloader.downloadInterFont()
        .then((result) => {
            if (result.fallback) {
                console.log('🎉 Font fallback system created successfully');
            } else {
                console.log('🎉 Font download completed successfully');
            }
        })
        .catch(error => {
            console.error('❌ Error downloading fonts:', error);
            process.exit(1);
        });
}
