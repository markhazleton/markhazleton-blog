# Check for articles in JSON without corresponding HTML files

$articlesData = Get-Content 'src/articles.json' | ConvertFrom-Json
$htmlFiles = Get-ChildItem -Path 'docs' -Filter '*.html' -Recurse
$htmlPaths = $htmlFiles | ForEach-Object { $_.FullName.Replace((Get-Location).Path + '\docs\', '').Replace('\', '/') }

Write-Host 'Articles in JSON without corresponding HTML files:' -ForegroundColor Yellow
Write-Host '=================================================' -ForegroundColor Yellow

$missingCount = 0
foreach ($article in $articlesData) {
    if ($article.slug -and $article.slug -notin $htmlPaths) {
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
foreach ($htmlPath in $htmlPaths) {
    if ($htmlPath -notin $articleSlugs) {
        $orphanCount++
        Write-Host "Orphan HTML: $htmlPath" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Total orphan HTML files: $orphanCount" -ForegroundColor Cyan
