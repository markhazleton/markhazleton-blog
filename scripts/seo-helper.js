/**
 * SEO Data Helper for Blog
 * Centralizes SEO data management and provides consistent meta tag generation
 */

class SEOHelper {
    constructor(articles = [], defaultConfig = {}) {
        this.articles = articles;
        this.defaultConfig = {
            titleSuffix: '',
            defaultAuthor: 'Solutions Architect',
            defaultRobots: 'index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1',
            baseUrl: 'https://markhazleton.com',
            defaultImage: 'https://markhazleton.com/assets/img/MarkHazleton.jpg',
            defaultImageWidth: 1200,
            defaultImageHeight: 630,
            ...defaultConfig
        };
    }

    /**
     * Get SEO data for a specific article by slug
     */
    getArticleSEO(slug) {
        const article = this.articles.find(a => a.slug === slug);
        if (!article) {
            return this.getDefaultSEO();
        }

        // Build SEO data with fallbacks
        const seo = article.seo || {};
        const title = this.buildTitle(seo.title || article.name, seo.titleSuffix);
        const description = this.truncateDescription(seo.description || article.description || '');
        const keywords = seo.keywords || article.keywords || '';
        const canonical = seo.canonical || `${this.defaultConfig.baseUrl}/${article.slug}`;

        return {
            title,
            description,
            keywords,
            canonical,
            robots: seo.robots || this.defaultConfig.defaultRobots,
            author: article.author || this.defaultConfig.defaultAuthor,

            // Open Graph
            og: {
                title: article.og?.title || title,
                description: article.og?.description || description,
                type: article.og?.type || 'article',
                url: canonical,
                image: article.og?.image || article.img_src || this.defaultConfig.defaultImage,
                imageWidth: this.defaultConfig.defaultImageWidth,
                imageHeight: this.defaultConfig.defaultImageHeight,
                imageAlt: article.og?.imageAlt || article.name,
                siteName: 'Solutions Architect & Technology Leader',
                locale: 'en_US'
            },

            // Twitter Card
            twitter: {
                card: 'summary_large_image',
                site: '@markhazleton',
                creator: '@markhazleton',
                title: article.twitter?.title || title,
                description: article.twitter?.description || description,
                image: article.twitter?.image || article.img_src || this.defaultConfig.defaultImage,
                imageAlt: article.twitter?.imageAlt || article.name
            },

            // Structured Data
            structuredData: this.buildArticleStructuredData(article, canonical)
        };
    }

    /**
     * Get default SEO data for non-article pages
     */
    getDefaultSEO(customData = {}) {
        const title = this.buildTitle(customData.title || 'Solutions Architect & Technology Leader');
        const description = customData.description || 'Solutions Architect specializing in .NET, Azure, and project management. 25+ years experience bridging technology with business goals.';
        const canonical = customData.canonical || this.defaultConfig.baseUrl;

        return {
            title,
            description,
            keywords: customData.keywords || 'Solutions Architect, .NET Developer, Azure Cloud, Project Management',
            canonical,
            robots: this.defaultConfig.defaultRobots,
            author: this.defaultConfig.defaultAuthor,

            og: {
                title: customData.ogTitle || title,
                description: customData.ogDescription || description,
                type: 'website',
                url: canonical,
                image: this.defaultConfig.defaultImage,
                imageWidth: this.defaultConfig.defaultImageWidth,
                imageHeight: this.defaultConfig.defaultImageHeight,
                imageAlt: 'Solutions Architect',
                siteName: 'Solutions Architect & Technology Leader',
                locale: 'en_US'
            },

            twitter: {
                card: 'summary_large_image',
                site: '@markhazleton',
                creator: '@markhazleton',
                title: customData.twitterTitle || title,
                description: customData.twitterDescription || description,
                image: this.defaultConfig.defaultImage,
                imageAlt: 'Solutions Architect'
            }
        };
    }

    /**
     * Build optimized title with suffix
     */
    buildTitle(baseTitle, customSuffix = null) {
        // Use just the base title, no suffix
        const title = baseTitle;

        // Ensure title is within optimal length (30-60 characters)
        if (title.length > 60) {
            console.warn(`Title too long (${title.length} chars): ${title}`);
        } else if (title.length < 30) {
            console.warn(`Title too short (${title.length} chars): ${title}`);
        }

        return title;
    }

    /**
     * Truncate description to optimal length (150-160 characters)
     */
    truncateDescription(description) {
        if (!description) return '';

        if (description.length > 160) {
            console.warn(`Description too long (${description.length} chars), truncating: ${description.substring(0, 50)}...`);
            return description.substring(0, 157) + '...';
        } else if (description.length < 120) {
            console.warn(`Description too short (${description.length} chars): ${description.substring(0, 50)}...`);
        }

        return description;
    }

    /**
     * Build structured data for articles
     */
    buildArticleStructuredData(article, canonical) {
        return {
            "@context": "https://schema.org",
            "@type": "Article",
            "headline": article.name,
            "description": article.description,
            "image": {
                "@type": "ImageObject",
                "url": article.img_src ? `${this.defaultConfig.baseUrl}/${article.img_src}` : this.defaultConfig.defaultImage,
                "width": this.defaultConfig.defaultImageWidth,
                "height": this.defaultConfig.defaultImageHeight
            },
            "author": {
                "@type": "Person",
                "name": article.author || this.defaultConfig.defaultAuthor,
                "url": this.defaultConfig.baseUrl
            },
            "publisher": {
                "@type": "Person",
                "name": this.defaultConfig.defaultAuthor,
                "url": this.defaultConfig.baseUrl
            },
            "datePublished": article.lastmod,
            "dateModified": article.lastmod,
            "mainEntityOfPage": {
                "@type": "WebPage",
                "@id": canonical
            },
            "url": canonical,
            "keywords": article.keywords
        };
    }

    /**
     * Generate PUG mixin for meta tags
     */
    generateMetaTags(seoData) {
        return {
            title: seoData.title,
            metaTags: [
                { name: 'description', content: seoData.description },
                { name: 'keywords', content: seoData.keywords },
                { name: 'author', content: seoData.author },
                { name: 'robots', content: seoData.robots }
            ],
            linkTags: [
                { rel: 'canonical', href: seoData.canonical }
            ],
            ogTags: [
                { property: 'og:title', content: seoData.og.title },
                { property: 'og:description', content: seoData.og.description },
                { property: 'og:type', content: seoData.og.type },
                { property: 'og:url', content: seoData.og.url },
                { property: 'og:image', content: seoData.og.image },
                { property: 'og:image:width', content: seoData.og.imageWidth },
                { property: 'og:image:height', content: seoData.og.imageHeight },
                { property: 'og:image:alt', content: seoData.og.imageAlt },
                { property: 'og:site_name', content: seoData.og.siteName },
                { property: 'og:locale', content: seoData.og.locale }
            ],
            twitterTags: [
                { name: 'twitter:card', content: seoData.twitter.card },
                { name: 'twitter:site', content: seoData.twitter.site },
                { name: 'twitter:creator', content: seoData.twitter.creator },
                { name: 'twitter:title', content: seoData.twitter.title },
                { name: 'twitter:description', content: seoData.twitter.description },
                { name: 'twitter:image', content: seoData.twitter.image },
                { name: 'twitter:image:alt', content: seoData.twitter.imageAlt }
            ],
            structuredData: seoData.structuredData
        };
    }
}

module.exports = SEOHelper;
