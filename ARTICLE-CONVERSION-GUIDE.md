# Article Conversion Guide: Modern Layout to Dynamic SEO Layout

This guide will help you convert your existing articles from the `modern-layout` to the new `dynamic-seo-layout` system, which centralizes SEO data management and provides enhanced SEO capabilities.

## 🎯 Benefits of Converting

### ✅ What You Gain

- **Centralized SEO Data**: All SEO information stored in `articles.json`
- **AI-Powered SEO**: Generate meta descriptions, titles, and keywords automatically
- **Consistent SEO**: Standardized meta tags across all articles
- **Enhanced Open Graph**: Better social media sharing
- **Future-Proof**: Easy to update SEO data without touching PUG files
- **Performance**: Faster builds with centralized data

### 📋 What Changes

- **Layout**: From `modern-layout` to `dynamic-seo-layout`
- **SEO Data**: Moved from PUG blocks to `articles.json`
- **Meta Tags**: Generated automatically from centralized data
- **Structure**: Cleaner, more maintainable PUG files

## 🔧 Conversion Process

### Step 1: Check Your Article Data

First, ensure your article exists in `articles.json`:

```bash
# Search for your article in the JSON file
grep -i "your-article-slug" src/articles.json
```

### Step 2: Create Backup (Safe First!)

```bash
# Create backup directory
mkdir -p backups/manual-conversion

# Copy your article
cp src/pug/articles/your-article.pug backups/manual-conversion/
```

### Step 3: Convert the Article Structure

Here's the manual conversion process:

#### Before (Modern Layout)

```pug
extends ../layouts/modern-layout

block pagehead
  title Your Article Title
  meta(name='description', content='Your description')
  meta(name="keywords" content="your, keywords")
  meta(name='author', content='Mark Hazleton')

block canonical
  link(rel='canonical', href='https://markhazleton.com/articles/your-article.html')

block og_overrides
  meta(property='og:title', content='Your Article Title')
  meta(property='og:description', content='Your description')
  meta(property='og:url', content='https://markhazleton.com/articles/your-article.html')
  meta(property='og:type', content='article')

block twitter_overrides
  meta(name='twitter:title', content='Your Article Title')
  meta(name='twitter:description', content='Your description')

block layout-content
  // Your article content here
```

#### After (Dynamic SEO Layout)

```pug
extends ../layouts/dynamic-seo-layout

block append pageData
  - pageData = getArticleData('your-article-slug')

block content
  // Your article content here (moved from layout-content)
```

### Step 4: Update Articles.json

Ensure your article data is complete in `src/articles.json`:

```json
{
  "slug": "your-article-slug",
  "title": "Your Article Title",
  "description": "Your meta description",
  "keywords": "your, keywords, here",
  "publishDate": "2024-01-01",
  "category": "Development",
  "tags": ["tag1", "tag2"],
  "author": "Mark Hazleton",
  "seo": {
    "title": "Your Article Title",
    "description": "Your meta description",
    "keywords": "your, keywords, here",
    "canonical": "https://markhazleton.com/articles/your-article.html",
    "openGraph": {
      "title": "Your Article Title",
      "description": "Your meta description",
      "url": "https://markhazleton.com/articles/your-article.html",
      "type": "article",
      "image": "https://markhazleton.com/assets/img/your-article-image.jpg"
    },
    "twitter": {
      "title": "Your Article Title",
      "description": "Your meta description",
      "image": "https://markhazleton.com/assets/img/your-article-image.jpg"
    }
  }
}
```

## 🚀 Quick Start: Convert Your First Article

### Option 1: Manual Conversion (Recommended for Learning)

1. **Pick an article** to convert (start with a simple one)
2. **Open the PUG file** in your editor
3. **Change the extends line**:

   ```pug
   extends ../layouts/dynamic-seo-layout
   ```

4. **Replace the pagehead/canonical/og_overrides/twitter_overrides blocks** with:

   ```pug
   block append pageData
     - pageData = getArticleData('your-article-slug')
   ```

5. **Move content** from `layout-content` to `content` block
6. **Test the build**: `npm run build:pug`

### Option 2: Using the Automated Script

```bash
# Convert specific articles
node scripts/convert-to-dynamic-seo.js your-article-slug another-article-slug

# See available articles
node scripts/convert-to-dynamic-seo.js
```

## 🧪 Testing Your Conversion

After converting an article, test it:

```bash
# Build PUG files
npm run build:pug

# Check for errors
npm run build

# Test locally
npm start
```

## 🔍 Common Issues and Solutions

### Issue 1: Article Not Found

**Error**: "Article data not found for slug: your-article"
**Solution**: Ensure the article exists in `articles.json` with the correct slug.

### Issue 2: Missing SEO Data

**Error**: Missing meta tags in generated HTML
**Solution**: Add complete SEO data to `articles.json` using the enhance script:

```bash
node scripts/enhance-seo-data.js
```

### Issue 3: PUG Formatting Errors

**Error**: "unexpected text" or indentation errors
**Solution**: Check PUG syntax, ensure proper indentation (2 spaces), and no mixed tabs/spaces.

### Issue 4: Content Not Displaying

**Error**: Article loads but content is missing
**Solution**: Ensure content is properly moved from `layout-content` to `content` block.

## 📊 Recommended Conversion Order

1. **Start Simple**: Pick articles with basic structure first
2. **Test Early**: Convert 1-2 articles and test thoroughly
3. **Batch Convert**: Once comfortable, convert similar articles in batches
4. **Complex Last**: Save articles with custom blocks for last

### Good First Candidates

- Articles with standard structure
- Articles without custom JavaScript
- Articles without complex layouts
- Recent articles (likely better structured)

### Convert Later

- Articles with custom styling
- Articles with embedded scripts
- Articles with complex layouts
- Very old articles (may need content updates)

## 🛠️ Tools Available

### 1. Conversion Script

```bash
node scripts/convert-to-dynamic-seo.js article-slug
```

### 2. SEO Enhancement Script

```bash
node scripts/enhance-seo-data.js
```

### 3. Migration Script (for new articles)

```bash
node scripts/migrate-to-dynamic-seo.js
```

## 📝 Manual Conversion Example

Let's convert `building-my-first-react-site-using-vite.pug`:

### Step 1: Backup

```bash
cp src/pug/articles/building-my-first-react-site-using-vite.pug backups/manual-conversion/
```

### Step 2: Edit the PUG file

Replace the top of the file:

```pug
extends ../layouts/dynamic-seo-layout

block append pageData
  - pageData = getArticleData('building-my-first-react-site-using-vite')

block content
  //- Hero Section
  section#hero.bg-light.py-5
    .container
      .row.align-items-center
        .col-lg-8
          header
            h1.display-4.fw-bold.text-primary
              i.bi.bi-code-square.me-3
              | Building My First React Site Using Vite
```

### Step 3: Test

```bash
npm run build:pug
```

### Step 4: Verify

Check that the article loads properly and all SEO meta tags are present.

## 🎉 Success Indicators

✅ **Conversion Successful When:**

- Article builds without errors
- HTML contains proper meta tags
- Page loads correctly in browser
- SEO data is complete in articles.json
- No console errors

## 📞 Need Help?

If you encounter issues:

1. Check the backup files in `backups/pug-conversion/`
2. Verify the article exists in `articles.json`
3. Test with a simple article first
4. Use the automated scripts for complex articles

## 🔄 Rollback Instructions

If something goes wrong:

```bash
# Restore from backup
cp backups/manual-conversion/your-article.pug src/pug/articles/your-article.pug

# Or restore from automated backup
cp backups/pug-conversion/your-article.pug src/pug/articles/your-article.pug
```

---

## 📋 Quick Reference

### Essential Commands

```bash
# See available articles
node scripts/convert-to-dynamic-seo.js

# Convert specific articles
node scripts/convert-to-dynamic-seo.js article1 article2

# Enhance SEO data
node scripts/enhance-seo-data.js

# Build and test
npm run build:pug
npm run build
npm start
```

### File Locations

- Articles: `src/pug/articles/`
- Article data: `src/articles.json`
- Backups: `backups/pug-conversion/`
- Layout: `src/pug/layouts/dynamic-seo-layout.pug`

This conversion will modernize your SEO management and prepare your site for AI-powered SEO enhancements!

## 🔄 **NEW: Modern Layout with Dynamic SEO**

**Great news!** The `modern-layout.pug` has been updated to use the same dynamic SEO system as `dynamic-seo-layout.pug`. This means:

### ✅ **For Existing Articles Using Modern Layout:**

- **No conversion needed!** Your articles will automatically benefit from dynamic SEO
- **Backward compatible:** Existing `pagehead`, `og_overrides`, and `twitter_overrides` blocks still work
- **Enhanced SEO:** Gets automatic SEO data from `articles.json` while preserving custom overrides

### 📋 **What Changed in Modern Layout:**

- Added dynamic SEO data generation from `articles.json`
- Automatic meta tag generation for title, description, keywords
- Enhanced Open Graph and Twitter Card data
- Backward compatibility with existing block overrides

### 🎯 **Layout Comparison:**

| Feature | Modern Layout (Updated) | Dynamic SEO Layout |
|---------|------------------------|-------------------|
| **Dynamic SEO Data** | ✅ Yes | ✅ Yes |
| **Articles.json Integration** | ✅ Yes | ✅ Yes |
| **Navigation** | ✅ Full site navigation | ❌ Minimal layout |
| **Footer** | ✅ Complete footer | ❌ Minimal layout |
| **Backward Compatibility** | ✅ Full compatibility | ⚠️ Layout-content block only |
| **Use Case** | 🎯 Full site pages & articles | 🎯 Content-focused articles |

### 💡 **Recommendation:**

- **Keep using `modern-layout`** for articles that need full site navigation and footer
- **Use `dynamic-seo-layout`** for content-focused articles or new articles
- **Both layouts now provide the same SEO benefits!**
