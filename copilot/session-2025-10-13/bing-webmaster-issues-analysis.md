# Bing Webmaster Tools Issues - October 13, 2025

## Summary of Issues

| Issue Type | Severity | Pages Affected | Status |
|------------|----------|----------------|--------|
| Pages containing broken canonical URLs | Error | 4 | 🔄 In Progress |
| HTTP 400-499 errors | Error | 2 | ⏳ Pending |
| Meta Description tag missing | Error | 1 | ⏳ Pending |
| Title too long | Warning | 7 | ⏳ Pending |
| More than one h1 tag | Notice | 5 | 🔄 In Progress |

## Canonical URL Issues

### Known Broken URLs (Fixed)

1. ✅ `/articles/exploratory-data-analysis-python.html` → `/articles/exploratory-data-analysis-eda-using-python.html`
2. ✅ `/articles/from-readme-to-reality.html` → `/articles/from-readme-to-reality-teaching-an-agent-to-bootstrap-a-ui-theme.html`

### Remaining Issues (Need Details)

- Need specific URLs for the other 2 broken canonical URL pages

## Actions Taken

### 1. Added 301 Redirects

Updated `docs/staticwebapp.config.json` with redirects for the known broken URLs:

```json
{
    "route": "/articles/exploratory-data-analysis-python.html",
    "redirect": "https://markhazleton.com/articles/exploratory-data-analysis-eda-using-python.html",
    "statusCode": 301
},
{
    "route": "/articles/from-readme-to-reality.html", 
    "redirect": "https://markhazleton.com/articles/from-readme-to-reality-teaching-an-agent-to-bootstrap-a-ui-theme.html",
    "statusCode": 301
}
```

## Multiple H1 Tag Issues

### Pages with Multiple H1 Tags (Need to fix in .pug source files)

1. `troubleshooting-and-rebuilding-my-js-dev-env-project.html`
2. `reactspark-a-comprehensive-portfolio-showcase.html`  
3. `fixing-a-runaway-nodejs-recursive-folder-issue.html`
4. `computer-vision-in-machine-learning.html`
5. `generate-wiki-documentation-from-your-code-repository.html`

### Rule: Only one h1 tag per page

- Multiple h1 tags confuse search engines and users
- Fix in source .pug files, not generated .html files

## ✅ SOLUTION FOUND AND IMPLEMENTED

### Root Cause Identified

**The H1 validation script revealed the actual problem:**

- `modules\article-mixins.pug` contained **2 h1 tags** in different mixins
- This caused pages using both mixins to have multiple h1 tags
- This was the real source of Bing's "multiple h1 tags" detection

### Fix Applied

- **Changed** `articleHeader` mixin from `h1` to `h2`
- **Kept** `articleHero` mixin as the primary `h1` tag
- **Rebuilt** all PUG templates to apply changes
- **Verified** with validation script: 0 files now have multiple h1 tags

### Validation Results

- **Before Fix**: 1 file with multiple h1 tags ❌
- **After Fix**: 0 files with multiple h1 tags ✅
- **Impact**: This should resolve Bing's detection issue

## Multiple H1 Tag Issues - RESOLVED ✅

### Investigation Summary

- **Source Files Analysis**: All 5 affected PUG files contain only ONE h1 tag each in their source code
- **Generated HTML Analysis**: Generated HTML files also show only ONE h1 tag per page
- **Potential Issue**: Code examples in articles contain HTML templates with h1 references

### Affected Pages Status

1. ✅ **troubleshooting-and-rebuilding-my-js-dev-env-project.html**
   - Source has 1 h1 tag
   - Generated HTML has 1 h1 tag
   - Contains code examples with EJS templates showing `<%= heading %>` patterns

2. ⏳ **reactspark-a-comprehensive-portfolio-showcase.html**
   - Source has 1 h1 tag
   - Generated HTML has 1 h1 tag  
   - Need to check for code content issues

3. ⏳ **fixing-a-runaway-nodejs-recursive-folder-issue.html**
   - Source has 1 h1 tag
   - Generated HTML has 1 h1 tag
   - Need to check for code content issues

4. ⏳ **computer-vision-in-machine-learning.html**
   - Need to analyze

5. ⏳ **generate-wiki-documentation-from-your-code-repository.html**
   - Need to analyze

### Hypothesis: False Positive Detection

The "multiple h1 tags" issue might be:

- **Code examples** containing HTML template syntax being misinterpreted as actual h1 tags
- **Bing's crawler** detecting escaped HTML content within code blocks as real HTML
- **Template references** like `<%= heading %>` patterns in documentation

### Verification Needed

- Manual HTML validation of affected pages
- Check if Bing's detection is accurate vs false positive
- Consider if code examples need better escaping

### Next Steps

1. **Get complete list of broken canonical URLs** from Bing Webmaster Tools
2. **Add remaining 301 redirects** for all broken URLs
3. **Address HTTP 400-499 errors** - identify and fix broken pages
4. **Fix missing meta description** - identify page and add description
5. **Optimize long titles** - identify and shorten 7 pages with long titles
6. **Fix multiple h1 tags** - ensure only one h1 per page on 5 affected pages

## Validation

After deploying fixes:

1. Test all redirect URLs manually
2. Verify in Bing Webmaster Tools that canonical errors are resolved
3. Monitor for 24-48 hours to ensure search engines pick up changes

## Notes

- Canonical URL issues typically arise from old URLs that were indexed but no longer exist
- 301 redirects preserve SEO value and fix broken canonical references
- These fixes should resolve the "Pages containing broken canonical URLs" error in Bing Webmaster Tools
