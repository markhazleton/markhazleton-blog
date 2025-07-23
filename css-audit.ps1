#!/usr/bin/env pwsh

# CSS Audit Script for MarkHazleton Blog
# This script analyzes CSS usage and identifies potential optimization opportunities

Write-Host "🔍 CSS Audit Report for MarkHazleton Blog" -ForegroundColor Green
Write-Host "=" * 50

# 1. Check CSS file sizes
Write-Host "`n📊 CSS File Sizes:" -ForegroundColor Yellow
Get-ChildItem -Path "docs/css" -Filter "*.css" | ForEach-Object {
    $sizeKB = [math]::Round($_.Length / 1KB, 2)
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  $($_.Name): $sizeKB KB ($sizeMB MB)" -ForegroundColor White
}

# 2. Check font file sizes
Write-Host "`n🔤 Font File Sizes:" -ForegroundColor Yellow
if (Test-Path "docs/css/fonts") {
    Get-ChildItem -Path "docs/css/fonts" | ForEach-Object {
        $sizeKB = [math]::Round($_.Length / 1KB, 2)
        Write-Host "  $($_.Name): $sizeKB KB" -ForegroundColor White
    }
}

# 3. Check which stylesheets are being used
Write-Host "`n📄 Stylesheet Usage Analysis:" -ForegroundColor Yellow

$modernStylesCount = (Get-ChildItem -Path "docs" -Recurse -Filter "*.html" | 
    Select-String -Pattern "modern-styles\.css" | Measure-Object).Count

$stylesCount = (Get-ChildItem -Path "docs" -Recurse -Filter "*.html" | 
    Select-String -Pattern "css/styles\.css" | Measure-Object).Count

Write-Host "  modern-styles.css used in: $modernStylesCount files" -ForegroundColor White
Write-Host "  styles.css used in: $stylesCount files" -ForegroundColor White

# 4. Check icon usage
Write-Host "`n🎨 Icon Usage Analysis:" -ForegroundColor Yellow

$faUsage = (Get-ChildItem -Path "docs" -Recurse -Filter "*.html" | 
    Select-String -Pattern "class=""[^""]*fa[sb]?\s" | Measure-Object).Count

$biUsage = (Get-ChildItem -Path "docs" -Recurse -Filter "*.html" | 
    Select-String -Pattern "class=""[^""]*bi-" | Measure-Object).Count

$deviconUsage = (Get-ChildItem -Path "docs" -Recurse -Filter "*.html" | 
    Select-String -Pattern "class=""[^""]*devicon-" | Measure-Object).Count

Write-Host "  FontAwesome icons found: $faUsage instances" -ForegroundColor White
Write-Host "  Bootstrap icons found: $biUsage instances" -ForegroundColor White
Write-Host "  Devicon usage found: $deviconUsage instances" -ForegroundColor White

# 5. Check for duplicate libraries
Write-Host "`n📚 Library Import Analysis:" -ForegroundColor Yellow

$stylesSCSS = Get-Content "src/scss/styles.scss" -Raw
$modernStylesSCSS = Get-Content "src/scss/modern-styles.scss" -Raw

$commonImports = @("bootstrap", "bootstrap-icons", "fontawesome", "prismjs")
foreach ($import in $commonImports) {
    $inStyles = $stylesSCSS -match $import
    $inModern = $modernStylesSCSS -match $import
    
    if ($inStyles -and $inModern) {
        Write-Host "  ⚠️  $import imported in BOTH stylesheets" -ForegroundColor Red
    } else {
        Write-Host "  ✅ $import imported in one stylesheet only" -ForegroundColor Green
    }
}

# 6. Recommendations
Write-Host "`n💡 Optimization Recommendations:" -ForegroundColor Cyan

Write-Host "`n🎯 High Priority:" -ForegroundColor Red
if ($deviconUsage -eq 0) {
    Write-Host "  • Remove Devicon library (1.4MB+ fonts, 0 usage found)" -ForegroundColor White
}

if ($modernStylesCount -gt 0 -and $stylesCount -gt 0) {
    Write-Host "  • Consider consolidating to one main stylesheet" -ForegroundColor White
    Write-Host "    - modern-styles.css: $modernStylesCount files" -ForegroundColor Gray
    Write-Host "    - styles.css: $stylesCount files" -ForegroundColor Gray
}

Write-Host "`n🎯 Medium Priority:" -ForegroundColor Yellow
Write-Host "  • Consider creating custom FontAwesome build with only used icons" -ForegroundColor White
Write-Host "  • Review Bootstrap components usage vs. file size" -ForegroundColor White

Write-Host "`n🎯 Low Priority:" -ForegroundColor Green
Write-Host "  • Minify and compress stylesheets if not already done" -ForegroundColor White
Write-Host "  • Consider using CSS purging tools for production builds" -ForegroundColor White

Write-Host "`n" + "=" * 50
Write-Host "Audit Complete! 🎉" -ForegroundColor Green
