/**
 * Generate Placeholder Images - Build Tool
 * Generates local placeholder images to eliminate external dependencies
 */

const fs = require('fs').promises;
const path = require('path');

class PlaceholderGenerator {
    constructor() {
        this.outputDir = path.join(__dirname, '../../src/assets/img/placeholders');
        this.staticDir = path.join(__dirname, '../../docs/assets/img/placeholders');
    }

    /**
     * Generate SVG placeholder image with text
     */
    generateSVGPlaceholder(width, height, text, backgroundColor = '#f8f9fa', textColor = '#6c757d') {
        // Ensure text fits within the image bounds
        const maxTextLength = Math.floor(width / 12); // Approximate character width
        const displayText = text.length > maxTextLength ? text.substring(0, maxTextLength - 3) + '...' : text;

        // Calculate font size based on image dimensions
        const fontSize = Math.min(width / 10, height / 8, 24);

        const svg = `<svg width="${width}" height="${height}" xmlns="http://www.w3.org/2000/svg">
  <rect width="100%" height="100%" fill="${backgroundColor}"/>
  <rect x="2" y="2" width="${width-4}" height="${height-4}" fill="none" stroke="#dee2e6" stroke-width="2" stroke-dasharray="5,5"/>
  <text x="50%" y="50%" text-anchor="middle" dominant-baseline="central"
        font-family="Arial, sans-serif" font-size="${fontSize}" fill="${textColor}"
        font-weight="500">${displayText}</text>
</svg>`;

        return svg;
    }

    /**
     * Generate placeholder images for common sizes
     */
    async generatePlaceholders() {
        console.log('📷 Generating local placeholder images...');

        // Create output directories
        await fs.mkdir(this.outputDir, { recursive: true });
        await fs.mkdir(this.staticDir, { recursive: true });

        // Common placeholder sizes used in the project
        const placeholders = [
            { width: 600, height: 340, name: 'project-card' },
            { width: 120, height: 120, name: 'small-icon' },
            { width: 800, height: 450, name: 'hero-banner' },
            { width: 400, height: 225, name: 'medium-card' },
            { width: 200, height: 150, name: 'thumbnail' }
        ];

        const generated = [];

        for (const placeholder of placeholders) {
            const filename = `placeholder-${placeholder.width}x${placeholder.height}.svg`;
            const text = `${placeholder.width}×${placeholder.height}`;

            const svgContent = this.generateSVGPlaceholder(
                placeholder.width,
                placeholder.height,
                text,
                '#f8f9fa', // Light background
                '#6c757d'  // Muted text color
            );

            // Write to both source and build directories
            const srcPath = path.join(this.outputDir, filename);
            const buildPath = path.join(this.staticDir, filename);

            await fs.writeFile(srcPath, svgContent, 'utf8');
            await fs.writeFile(buildPath, svgContent, 'utf8');

            generated.push({
                name: placeholder.name,
                filename,
                size: `${placeholder.width}×${placeholder.height}`,
                path: `/assets/img/placeholders/${filename}`
            });
        }

        // Generate generic fallback placeholder
        const genericSvg = this.generateSVGPlaceholder(600, 340, 'Image Not Found', '#f8f9fa', '#6c757d');
        await fs.writeFile(path.join(this.outputDir, 'fallback.svg'), genericSvg, 'utf8');
        await fs.writeFile(path.join(this.staticDir, 'fallback.svg'), genericSvg, 'utf8');

        generated.push({
            name: 'fallback',
            filename: 'fallback.svg',
            size: '600×340',
            path: '/assets/img/placeholders/fallback.svg'
        });

        console.log(`✅ Generated ${generated.length} placeholder images:`);
        generated.forEach(img => {
            console.log(`   📄 ${img.filename} (${img.size}) - ${img.name}`);
        });

        return generated;
    }

    /**
     * Generate project-specific placeholder with custom text
     */
    async generateProjectPlaceholder(projectName, width = 600, height = 340) {
        const filename = `project-${projectName.toLowerCase().replace(/[^a-z0-9]/g, '-')}.svg`;
        const svgContent = this.generateSVGPlaceholder(width, height, projectName);

        const srcPath = path.join(this.outputDir, filename);
        const buildPath = path.join(this.staticDir, filename);

        await fs.writeFile(srcPath, svgContent, 'utf8');
        await fs.writeFile(buildPath, svgContent, 'utf8');

        return `/assets/img/placeholders/${filename}`;
    }
}

// Export for use in build system
module.exports = PlaceholderGenerator;

// Run directly if called from command line
if (require.main === module) {
    const generator = new PlaceholderGenerator();
    generator.generatePlaceholders()
        .then(() => {
            console.log('🎉 Placeholder generation completed successfully');
        })
        .catch(error => {
            console.error('❌ Error generating placeholders:', error);
            process.exit(1);
        });
}
