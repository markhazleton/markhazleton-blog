# Typeahead Functionality Research - Azure Static Web Apps Issue

**Date**: December 15, 2025  
**Issue**: Header search typeahead works locally but fails on Azure Static Web Apps  
**Status**: Under Investigation

## Problem Summary

The header search typeahead (autocomplete) functionality works perfectly when running the site locally via BrowserSync but does NOT work when deployed to Azure Static Web Apps at https://markhazleton.com.

## Implementation Details

### Core Files Involved

1. **Source JavaScript**: `src/js/scripts.js` (lines 113-280)
2. **Compiled Output**: `docs/js/scripts.js` (minified version)
3. **CSP Configuration**: `src/staticwebapp.config.json` & `docs/staticwebapp.config.json`
4. **HTML Templates**: `src/pug/layouts/modern-layout.pug` & `performance-optimized-layout.pug`

### Typeahead Implementation

The typeahead functionality is implemented in the `initializeHeaderSearch()` function with the following key features:

```javascript
// Multiple event listeners for better browser compatibility
headerSearchInput.addEventListener("input", handleSearchInput);
headerSearchInput.addEventListener("keyup", handleSearchInput);
headerSearchInput.addEventListener("propertychange", handleSearchInput); // IE/Edge legacy support
headerSearchInput.addEventListener("paste", function () {
    setTimeout(handleSearchInput.bind(this), 100);
});

// Articles cache loading
async function showHeaderSuggestions(query) {
    if (!window.articlesCache) {
        const response = await fetch("/articles.json");
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        window.articlesCache = await response.json();
    }
    // ... display suggestions
}
```

### Edge-Specific Compatibility Code

The code includes special handling for Microsoft Edge:

```javascript
if (
    navigator.userAgent.indexOf("Edge") > -1 ||
    navigator.userAgent.indexOf("Edg") > -1
) {
    console.log("Microsoft Edge detected, applying compatibility fixes");
    
    setTimeout(function () {
        if (!window.articlesCache) {
            fetch("/articles.json")
                .then((response) => response.json())
                .then((data) => {
                    window.articlesCache = data;
                    console.log("Articles cache preloaded for Edge");
                })
                .catch((error) =>
                    console.error("Error preloading articles:", error),
                );
        }
    }, 500);
}
```

## Potential Causes

### 1. Content Security Policy (CSP) Restrictions

**Current CSP in `staticwebapp.config.json`:**
```
script-src 'self' 'unsafe-inline' 'unsafe-eval' data: https://www.googletagmanager.com ... https://markhazleton.com;
connect-src 'self' data: https://www.googletagmanager.com ... https://markhazleton.com;
```

**Analysis:**
- ✅ `fetch` API should work with `connect-src 'self'`
- ✅ Articles.json is served from same origin
- ⚠️ Possible MIME type mismatch issue
- ⚠️ CSP might block dynamic style/DOM manipulation

**Testing Required:**
1. Check browser console for CSP violations on production
2. Verify `/articles.json` is accessible directly in browser
3. Check for CORS errors in Network tab
4. Validate response Content-Type header

### 2. Minification/Build Issues

**Current Build Process:**
- Scripts are compiled and minified during build
- The minified version might have scope issues
- Variable references might be lost during minification

**Testing Required:**
1. Compare source map to ensure proper mapping
2. Check if `window.articlesCache` is accessible in production console
3. Verify event listeners are properly attached

### 3. Path Resolution Issues

**Observations:**
- Local: Files served from `docs/` via BrowserSync
- Production: Files served from Azure Static Web Apps CDN
- Articles.json path: `/articles.json` (absolute path)

**Potential Issues:**
- Path might resolve differently on Azure
- CDN caching might serve stale version
- Routing configuration might interfere

**Testing Required:**
1. Check actual URL requested in Network tab: `https://markhazleton.com/articles.json`
2. Verify response headers (Cache-Control, Content-Type)
3. Test with full URL: `https://markhazleton.com/articles.json` instead of relative path

### 4. Timing/Race Conditions

**Edge Preload Code:**
- Runs after 500ms delay
- Only on Edge browser detection
- Might not work for all browsers on production

**Testing Required:**
1. Test in different browsers (Chrome, Firefox, Safari, Edge)
2. Check if delay is sufficient on slower networks
3. Verify DOMContentLoaded event fires correctly

### 5. Static Web Apps Routing

**Current Configuration:**
- `staticwebapp.config.json` defines routes and headers
- Might have routing rules that interfere with fetch requests

**Testing Required:**
1. Check if articles.json is listed in routes
2. Verify no redirect rules affecting .json files
3. Confirm MIME types are properly set

## Debugging Steps

### Step 1: Browser Console Inspection (Production)

Open https://markhazleton.com in browser and check:

1. **Console Errors:**
   ```javascript
   // Look for:
   // - CSP violations
   // - Fetch errors
   // - CORS errors
   // - JavaScript errors
   ```

2. **Network Tab:**
   ```
   // Check for:
   // - GET /articles.json request
   // - Response status (should be 200)
   // - Response headers (Content-Type: application/json)
   // - Response time and size
   ```

3. **Application Tab:**
   ```javascript
   // Check:
   window.articlesCache // Should be undefined initially, then populated
   ```

### Step 2: Test Fetch Manually

In browser console on production:

```javascript
// Test 1: Basic fetch
fetch('/articles.json')
    .then(r => r.json())
    .then(d => console.log('Articles:', d.length))
    .catch(e => console.error('Error:', e));

// Test 2: Check cache
console.log('Cache:', window.articlesCache);

// Test 3: Trigger manually
const input = document.getElementById('headerSearchInput');
input.value = 'test';
input.dispatchEvent(new Event('input'));
```

### Step 3: Check CSP Violations

```javascript
// Add CSP violation reporter
document.addEventListener('securitypolicyviolation', (e) => {
    console.error('CSP Violation:', e.violatedDirective, e.blockedURI);
});
```

### Step 4: Network Debugging

```bash
# Test from command line
curl -I https://markhazleton.com/articles.json

# Check response headers
# Should see:
# Content-Type: application/json
# Access-Control-Allow-Origin: *
```

## Recommended Fixes

### Fix 1: Add Explicit JSON MIME Type

Update `staticwebapp.config.json`:

```json
{
    "mimeTypes": {
        ".json": "application/json"
    }
}
```

Already present ✅

### Fix 2: Add CORS Headers for JSON

Add specific route for articles.json:

```json
{
    "route": "/articles.json",
    "headers": {
        "Content-Type": "application/json",
        "Access-Control-Allow-Origin": "*",
        "Cache-Control": "public, max-age=3600"
    }
}
```

### Fix 3: Improve Error Handling

Update `showHeaderSuggestions` function:

```javascript
async function showHeaderSuggestions(query) {
    try {
        if (!window.articlesCache) {
            console.log('Fetching articles.json...');
            const response = await fetch("/articles.json");
            
            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers.get('Content-Type'));
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            console.log('Articles loaded:', data.length);
            window.articlesCache = data;
        }
        
        // ... rest of function
    } catch (error) {
        console.error("Error loading search suggestions:", error);
        console.error("Error stack:", error.stack);
        // Show user-friendly error
        showErrorMessage("Search suggestions unavailable. Please try full search.");
    }
}
```

### Fix 4: Add Fallback Path

```javascript
async function loadArticlesCache() {
    const paths = [
        '/articles.json',
        'https://markhazleton.com/articles.json',
        '/docs/articles.json'
    ];
    
    for (const path of paths) {
        try {
            console.log(`Trying path: ${path}`);
            const response = await fetch(path);
            if (response.ok) {
                window.articlesCache = await response.json();
                console.log(`Success with path: ${path}`);
                return;
            }
        } catch (error) {
            console.warn(`Failed to load from ${path}:`, error);
        }
    }
    
    throw new Error('Could not load articles.json from any path');
}
```

### Fix 5: Remove 'data:' from connect-src

The `data:` scheme in `connect-src` might be causing issues:

```json
// Current
"connect-src 'self' data: https://..."

// Recommended
"connect-src 'self' https://..."
```

## Next Steps

1. **Deploy Test Build** with enhanced logging
2. **Monitor Console** on production for specific error messages
3. **Test in Multiple Browsers** to isolate browser-specific issues
4. **Check Network Tab** for actual fetch requests and responses
5. **Verify Articles.json** is properly deployed and accessible
6. **Test CSP** by temporarily relaxing restrictions

## Related Files

- [scripts.js](../../src/js/scripts.js) - Main typeahead implementation
- [staticwebapp.config.json](../../src/staticwebapp.config.json) - CSP and routing config
- [modern-layout.pug](../../src/pug/layouts/modern-layout.pug) - Header search input
- [articles.json](../../docs/articles.json) - Data source

## Conclusion

The most likely cause is either:

1. **CSP blocking fetch requests** - Check for CSP violations in console
2. **Path resolution issue** - Articles.json might not be accessible at expected path
3. **Caching issue** - CDN might be serving old/corrupted version
4. **MIME type mismatch** - Server might not be sending correct Content-Type header

**Immediate Action**: Deploy with enhanced console logging and inspect production browser console for specific error messages.
