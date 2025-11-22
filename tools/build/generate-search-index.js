const fs = require('fs');
const upath = require('upath');

module.exports = function generateSearchIndex() {
    const srcPath = upath.resolve(__dirname, '../../src/articles.json');
    const destPath = upath.resolve(__dirname, '../../docs/search-index.json');

    if (!fs.existsSync(srcPath)) {
        console.error('❌ articles.json not found');
        return;
    }

    try {
        const articles = JSON.parse(fs.readFileSync(srcPath, 'utf8'));
        
        // Map only necessary fields for search and listing
        const searchIndex = articles.map(article => ({
            name: article.name,
            description: article.description,
            slug: article.slug,
            publishedDate: article.publishedDate,
            Section: article.Section,
            keywords: article.keywords,
            lastmod: article.lastmod
        }));

        fs.writeFileSync(destPath, JSON.stringify(searchIndex));
        
        // Calculate savings
        const originalSize = fs.statSync(srcPath).size;
        const newSize = fs.statSync(destPath).size;
        const savings = ((originalSize - newSize) / originalSize * 100).toFixed(1);
        
        console.log(`🔍 Generated search-index.json: ${searchIndex.length} articles`);
        console.log(`📉 Size reduced from ${(originalSize/1024).toFixed(1)}KB to ${(newSize/1024).toFixed(1)}KB (${savings}% savings)`);
        
    } catch (error) {
        console.error('❌ Error generating search index:', error.message);
    }
};
