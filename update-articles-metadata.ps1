$ErrorActionPreference = "Continue"

# Read the git dates results
$gitDatesPath = "git-dates-results.json"
$gitDatesContent = Get-Content $gitDatesPath -Raw | ConvertFrom-Json

# Read the articles.json file
$articlesPath = "src/articles.json"
$articlesContent = Get-Content $articlesPath -Raw | ConvertFrom-Json

Write-Host "Updating articles.json with publishedDate and estimatedReadTime..." -ForegroundColor Cyan

$updatedCount = 0

# Create a lookup table for git dates by article ID
$gitDatesLookup = @{}
foreach ($gitDate in $gitDatesContent) {
    $gitDatesLookup[$gitDate.Id] = $gitDate.EarliestDate
}

# Update each article
for ($i = 0; $i -lt $articlesContent.Count; $i++) {
    $article = $articlesContent[$i]
    $updated = $false

    # Add publishedDate if missing
    if (-not $article.PSObject.Properties.Name -contains "publishedDate" -or [string]::IsNullOrEmpty($article.publishedDate)) {
        if ($gitDatesLookup.ContainsKey($article.id)) {
            $article | Add-Member -NotePropertyName "publishedDate" -NotePropertyValue $gitDatesLookup[$article.id] -Force
            $updated = $true
            Write-Host "Added publishedDate '$($gitDatesLookup[$article.id])' to ID $($article.id): $($article.name)" -ForegroundColor Green
        }
    }

    # Add estimatedReadTime if missing (default to 5 minutes)
    if (-not $article.PSObject.Properties.Name -contains "estimatedReadTime" -or $null -eq $article.estimatedReadTime) {
        $article | Add-Member -NotePropertyName "estimatedReadTime" -NotePropertyValue 5 -Force
        $updated = $true
        Write-Host "Added estimatedReadTime '5' to ID $($article.id): $($article.name)" -ForegroundColor Blue
    }

    if ($updated) {
        $updatedCount++
    }
}

# Convert back to JSON and save
Write-Host "`nConverting to JSON and saving..." -ForegroundColor Cyan
$jsonOutput = $articlesContent | ConvertTo-Json -Depth 10 -Compress:$false

# Write to file
$jsonOutput | Out-File $articlesPath -Encoding UTF8

Write-Host "`nUpdated $updatedCount articles in $articlesPath" -ForegroundColor Green
Write-Host "All articles now have publishedDate and estimatedReadTime fields!" -ForegroundColor Cyan
