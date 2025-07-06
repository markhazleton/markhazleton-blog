$ErrorActionPreference = "Continue"

# Read the articles.json file
$articlesPath = "src/articles.json"
$articlesContent = Get-Content $articlesPath -Raw | ConvertFrom-Json

# Create an array to store results
$results = @()

# Process each article
foreach ($article in $articlesContent) {
    $source = $article.source
    $hasPublishedDate = $null -ne $article.publishedDate

    if ($source -and -not $hasPublishedDate) {
        # Remove leading slash if present and construct full path
        $pugFile = $source -replace "^/", ""
        $fullPath = $pugFile

        Write-Host "Processing: $($article.name)" -ForegroundColor Yellow
        Write-Host "  Source: $source" -ForegroundColor Gray
        Write-Host "  Has publishedDate: $hasPublishedDate" -ForegroundColor Gray

        # Check if file exists
        if (Test-Path $fullPath) {
            try {
                # Get the earliest commit date for this file
                $gitLog = git log --follow --format="%ad" --date=short --reverse -- $fullPath | Select-Object -First 1

                if ($gitLog) {
                    Write-Host "  Earliest commit date: $gitLog" -ForegroundColor Green

                    $results += @{
                        Id = $article.id
                        Name = $article.name
                        Source = $source
                        EarliestDate = $gitLog
                        HasPublishedDate = $hasPublishedDate
                    }
                } else {
                    Write-Host "  No git history found" -ForegroundColor Red
                }
            }
            catch {
                Write-Host "  Error getting git history: $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            Write-Host "  File not found: $fullPath" -ForegroundColor Red
        }
        Write-Host ""
    }
}

# Display summary
Write-Host "=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Articles without publishedDate: $($results.Count)" -ForegroundColor White

foreach ($result in $results | Sort-Object Id) {
    Write-Host "ID $($result.Id): $($result.Name)" -ForegroundColor White
    Write-Host "  Date: $($result.EarliestDate)" -ForegroundColor Green
    Write-Host "  Source: $($result.Source)" -ForegroundColor Gray
}

# Export results for use in updating the JSON
$results | ConvertTo-Json -Depth 2 | Out-File "git-dates-results.json" -Encoding UTF8
Write-Host "`nResults saved to git-dates-results.json" -ForegroundColor Cyan
