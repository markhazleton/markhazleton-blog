/**

* Enhanced SEO Configuration for Articles
* This structure extends your existing articles.json with optimized SEO fields
 */

// Recommended additions to each article object in articles.json:
{
    // ... your existing fields (id, Section, slug, name, content, description, keywords, etc.)

    // Enhanced SEO fields
    "seo": {
        "title": "Custom title (30-60 chars) - defaults to 'name' if not provided",
        "description": "Custom meta description (120-160 chars) - defaults to 'description' if not provided", 
        "keywords": "Custom keywords - defaults to 'keywords' if not provided",
        "canonical": "https://markhazleton.com/slug", // Auto-generated if not provided
        "titleSuffix": " | Mark Hazleton", // Appended to title, can be customized per article
        "robots": "index, follow", // Default: "index, follow"
        "priority": 0.8, // Sitemap priority (0.0-1.0)
        "changefreq": "monthly" // You already have this
    },
    
    // Open Graph overrides (optional)
    "og": {
        "title": "Optional OG title override",
        "description": "Optional OG description override", 
        "image": "Optional OG image override",
        "type": "article" // Default: "article" for articles, "website" for pages
    },
    
    // Twitter Card overrides (optional)
    "twitter": {
        "title": "Optional Twitter title override",
        "description": "Optional Twitter description override",
        "image": "Optional Twitter image override"
    }
}

// For backward compatibility, the system will use these fallbacks:
// - seo.title → name + titleSuffix
// - seo.description → description (truncated to 160 chars if needed)
// - seo.keywords → keywords
// - seo.canonical → auto-generated from slug
