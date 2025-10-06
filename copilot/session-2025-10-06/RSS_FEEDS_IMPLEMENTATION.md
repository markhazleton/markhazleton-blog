# RSS Feeds Implementation

## Overview

This blog now supports **two separate RSS feeds** to provide targeted content feeds for different audiences while maintaining backward compatibility.

## Available RSS Feeds

### 1. Articles RSS Feed

- **URL**: `https://markhazleton.com/rss.xml` (backward compatibility)
- **URL**: `https://markhazleton.com/articles-rss.xml` (new specific URL)
- **Content**: Latest articles and insights from Mark Hazleton
- **Items**: Up to 500 most recent articles
- **Update frequency**: Updated automatically when articles are published

### 2. Projects RSS Feed

- **URL**: `https://markhazleton.com/projects-rss.xml`
- **Content**: Latest project updates and releases from Mark Hazleton's portfolio
- **Items**: Up to 50 most recent projects
- **Update frequency**: Updated when projects are modified or new projects added

## RSS Feed Features

### Articles RSS Feed

- **Title**: "Mark Hazleton Articles"
- **Description**: "Latest articles and insights from Mark Hazleton"
- **Categories**: Includes article sections (Development, AI & Machine Learning, etc.)
- **Content**: Full article content included in `<content:encoded>` tags
- **Metadata**: Publication date, author, categories, and descriptions

### Projects RSS Feed

- **Title**: "Mark Hazleton Projects"
- **Description**: "Latest project updates and releases from Mark Hazleton's portfolio"
- **Categories**: All items categorized as "Projects"
- **Content**: Rich project details including:
  - Project summary and description
  - Technology stack and keywords
  - Live demo links
  - Source code repository links
  - Release notes and updates
- **Metadata**: Last modified date, project stage, and promotion information

## Technical Implementation

### Build Process

Both RSS feeds are generated automatically during the build process:

```bash
# Build all RSS feeds
npm run build

# Build only articles RSS feed
npm run build --rss

# Build only projects RSS feed
npm run build --projectsRss

# Build both RSS feeds
npm run build --rss --projectsRss
```

### Feed Generation Scripts

- **Articles**: `tools/build/update-rss.js`
- **Projects**: `tools/build/update-projects-rss.js`

### Data Sources

- **Articles RSS**: Generated from `src/articles.json`
- **Projects RSS**: Generated from `src/projects.json`

## Backward Compatibility

The original `rss.xml` file continues to serve the articles feed to maintain compatibility with existing subscribers and feed readers.

## Usage Examples

### Subscribe to Articles

```xml
<link rel="alternate" type="application/rss+xml" title="Mark Hazleton Articles" href="https://markhazleton.com/rss.xml" />
```

### Subscribe to Projects

```xml
<link rel="alternate" type="application/rss+xml" title="Mark Hazleton Projects" href="https://markhazleton.com/projects-rss.xml" />
```

### HTML Meta Tags

```html
<!-- Articles RSS -->
<link rel="alternate" type="application/rss+xml" 
      title="Mark Hazleton Articles" 
      href="https://markhazleton.com/rss.xml" />

<!-- Projects RSS -->
<link rel="alternate" type="application/rss+xml" 
      title="Mark Hazleton Projects" 
      href="https://markhazleton.com/projects-rss.xml" />
```

## Benefits

1. **Targeted Content**: Readers can subscribe to specific content types
2. **Reduced Noise**: Project-focused readers won't get article updates and vice versa
3. **Better Organization**: Clear separation between articles and project updates
4. **Backward Compatibility**: Existing subscribers continue to receive article updates
5. **SEO Benefits**: Dedicated feeds improve content discoverability

## Future Enhancements

- Category-specific RSS feeds (e.g., AI & Machine Learning articles only)
- Technology-specific project feeds (e.g., React projects only)
- Combined feed with both articles and projects
- RSS feed statistics and analytics

---

**Last Updated**: October 6, 2025
