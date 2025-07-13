# Check for articles in JSON without corresponding HTML files

$articlesData = Get-Content 'src/articles.json' | ConvertFrom-Json
$htmlFiles = Get-ChildItem -Path 'docs' -Filter '*.html' -Recurse

# Create two arrays: original paths and mapped paths for slug matching
$htmlPaths = @()
$mappedPaths = @()

foreach ($file in $htmlFiles) {
    $originalPath = $file.FullName.Replace((Get-Location).Path + '\docs\', '').Replace('\', '/')
    $htmlPaths += $originalPath

    # Create mapped path for slug comparison (convert /index.html to /)
    if ($originalPath.EndsWith('/index.html')) {
        $mappedPath = $originalPath.Replace('/index.html', '/')
    }
    else {
        $mappedPath = $originalPath
    }
    $mappedPaths += $mappedPath
}

Write-Host 'Articles in JSON without corresponding HTML files:' -ForegroundColor Yellow
Write-Host '=================================================' -ForegroundColor Yellow

$missingCount = 0
foreach ($article in $articlesData) {
    if ($article.slug -and $article.slug -notin $mappedPaths) {
        $missingCount++
        Write-Host "Missing HTML: $($article.slug)" -ForegroundColor Red
        Write-Host "  Article: $($article.name)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Total missing HTML files: $missingCount" -ForegroundColor Yellow

# Also check for HTML files without articles.json entries
Write-Host ""
Write-Host 'HTML files without corresponding JSON entries:' -ForegroundColor Cyan
Write-Host '=============================================' -ForegroundColor Cyan

$articleSlugs = $articlesData | Where-Object { $_.slug } | ForEach-Object { $_.slug }
$orphanCount = 0
for ($i = 0; $i -lt $htmlPaths.Count; $i++) {
    $originalPath = $htmlPaths[$i]
    $mappedPath = $mappedPaths[$i]

    if ($mappedPath -notin $articleSlugs) {
        $orphanCount++
        Write-Host "Orphan HTML: $originalPath" -ForegroundColor Yellow
        if ($originalPath -ne $mappedPath) {
            Write-Host "  Maps to slug: $mappedPath" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "Total orphan HTML files: $orphanCount" -ForegroundColor Cyan
