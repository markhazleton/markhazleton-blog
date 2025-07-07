# Fix Twitter Card Description Issues
# This script updates Twitter Card descriptions that are too short by using fallback values
# from description, seo:description, or og:description fields

param(
    [string]$ArticlesJsonPath = "src/articles.json",
    [int]$MinLength = 200,
    [int]$MaxLength = 200,
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

Write-Host "🔧 Starting Twitter Card Description Fix..." -ForegroundColor Green
Write-Host "📁 Processing file: $ArticlesJsonPath" -ForegroundColor Cyan
Write-Host "📏 Min length: $MinLength chars, Max length: $MaxLength chars" -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
}
Write-Host ""

# Load articles.json
try {
    if (-not (Test-Path $ArticlesJsonPath)) {
        Write-Host "❌ Error: articles.json not found at $ArticlesJsonPath" -ForegroundColor Red
        exit 1
    }

    Write-Host "📄 Loading articles.json..." -ForegroundColor Cyan
    $articlesData = Get-Content $ArticlesJsonPath -Raw | ConvertFrom-Json
    Write-Host "✅ Loaded $($articlesData.Count) articles" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error loading articles.json: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Initialize counters
$totalProcessed = 0
$updatedCount = 0
$skippedCount = 0
$issuesFound = @()

# Function to truncate text to fit within max length while preserving word boundaries
function Get-TruncatedText {
    param(
        [string]$Text,
        [int]$MaxLength
    )

    if ($Text.Length -le $MaxLength) {
        return $Text
    }

    # Find the last space before the max length
    $truncated = $Text.Substring(0, $MaxLength)
    $lastSpace = $truncated.LastIndexOf(' ')

    if ($lastSpace -gt ($MaxLength * 0.8)) {
        # If the last space is reasonably close to the end, use it
        return $truncated.Substring(0, $lastSpace).Trim()
    }
    else {
        # Otherwise, just truncate and add ellipsis
        return $truncated.Substring(0, $MaxLength - 3).Trim() + "..."
    }
}

# Function to get the best description from available sources
function Get-BestDescription {
    param(
        [PSCustomObject]$Article,
        [int]$MinLength,
        [int]$MaxLength
    )

    $descriptions = @()

    # Priority order: seo.description, og.description, description
    if ($Article.seo -and $Article.seo.description) {
        $descriptions += @{
            Source = "seo.description"
            Text = $Article.seo.description.Trim()
        }
    }

    if ($Article.og -and $Article.og.description) {
        $descriptions += @{
            Source = "og.description"
            Text = $Article.og.description.Trim()
        }
    }

    if ($Article.description) {
        $descriptions += @{
            Source = "description"
            Text = $Article.description.Trim()
        }
    }

    # Find the best description that meets our criteria
    foreach ($desc in $descriptions) {
        if ($desc.Text.Length -ge $MinLength) {
            $finalText = Get-TruncatedText -Text $desc.Text -MaxLength $MaxLength
            return @{
                Source = $desc.Source
                Text = $finalText
                Original = $desc.Text
            }
        }
    }

    # If no description meets minimum length, use the longest one available
    $longestDesc = $descriptions | Sort-Object { $_.Text.Length } -Descending | Select-Object -First 1
    if ($longestDesc) {
        $finalText = Get-TruncatedText -Text $longestDesc.Text -MaxLength $MaxLength
        return @{
            Source = $longestDesc.Source
            Text = $finalText
            Original = $longestDesc.Text
        }
    }

    return $null
}

# Process each article
Write-Host "🔍 Analyzing articles for Twitter Card description issues..." -ForegroundColor Yellow
Write-Host ""

foreach ($article in $articlesData) {
    $totalProcessed++
    $hasIssue = $false
    $currentTwitterDesc = ""
    $articleId = if ($article.id) { $article.id } else { "N/A" }
    $articleName = if ($article.name) { $article.name } else { "Unnamed Article" }

    if ($Verbose) {
        Write-Host "📝 Processing Article ID: $articleId - $articleName" -ForegroundColor Gray
    }

    # Check if Twitter Card description exists and its length
    if ($article.twitter -and $article.twitter.description) {
        $currentTwitterDesc = $article.twitter.description.Trim()

        if ($currentTwitterDesc.Length -lt $MinLength) {
            $hasIssue = $true
            if ($Verbose) {
                Write-Host "   ⚠️  Twitter description too short: $($currentTwitterDesc.Length) chars" -ForegroundColor Yellow
            }
        }
        elseif ($currentTwitterDesc.Length -gt $MaxLength) {
            $hasIssue = $true
            if ($Verbose) {
                Write-Host "   ⚠️  Twitter description too long: $($currentTwitterDesc.Length) chars" -ForegroundColor Yellow
            }
        }
    }
    else {
        $hasIssue = $true
        if ($Verbose) {
            Write-Host "   ⚠️  Missing Twitter description" -ForegroundColor Yellow
        }
    }

    if ($hasIssue) {
        # Get the best description from available sources
        $bestDesc = Get-BestDescription -Article $article -MinLength $MinLength -MaxLength $MaxLength

        if ($bestDesc) {
            $issueDetails = @{
                ArticleId = $articleId
                ArticleName = $articleName
                Slug = $article.slug
                CurrentTwitterDesc = $currentTwitterDesc
                CurrentLength = $currentTwitterDesc.Length
                NewTwitterDesc = $bestDesc.Text
                NewLength = $bestDesc.Text.Length
                Source = $bestDesc.Source
                WasTruncated = $bestDesc.Original.Length -ne $bestDesc.Text.Length
            }

            $issuesFound += $issueDetails

            # Update the article if not in dry run mode
            if (-not $DryRun) {
                # Ensure twitter object exists
                if (-not $article.twitter) {
                    $article | Add-Member -MemberType NoteProperty -Name "twitter" -Value @{}
                }

                # Update the description
                $article.twitter.description = $bestDesc.Text
                $updatedCount++

                Write-Host "✅ Updated Article ID: $articleId" -ForegroundColor Green
                Write-Host "   📝 Title: $articleName" -ForegroundColor Gray
                Write-Host "   📏 Length: $($currentTwitterDesc.Length) → $($bestDesc.Text.Length) chars" -ForegroundColor Gray
                Write-Host "   🔗 Source: $($bestDesc.Source)" -ForegroundColor Gray
                if ($bestDesc.WasTruncated) {
                    Write-Host "   ✂️  Text was truncated to fit" -ForegroundColor Yellow
                }
                Write-Host ""
            }
            else {
                Write-Host "🔍 Would update Article ID: $articleId" -ForegroundColor Cyan
                Write-Host "   📝 Title: $articleName" -ForegroundColor Gray
                Write-Host "   📏 Length: $($currentTwitterDesc.Length) → $($bestDesc.Text.Length) chars" -ForegroundColor Gray
                Write-Host "   🔗 Source: $($bestDesc.Source)" -ForegroundColor Gray
                if ($bestDesc.WasTruncated) {
                    Write-Host "   ✂️  Text would be truncated to fit" -ForegroundColor Yellow
                }
                Write-Host ""
            }
        }
        else {
            Write-Host "⚠️  No suitable description found for Article ID: $articleId" -ForegroundColor Yellow
            Write-Host "   📝 Title: $articleName" -ForegroundColor Gray
            $skippedCount++
        }
    }
}

Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Green
Write-Host "   Total articles processed: $totalProcessed" -ForegroundColor Gray
Write-Host "   Issues found: $($issuesFound.Count)" -ForegroundColor Gray
Write-Host "   Articles updated: $updatedCount" -ForegroundColor Gray
Write-Host "   Articles skipped: $skippedCount" -ForegroundColor Gray

if ($issuesFound.Count -gt 0) {
    Write-Host ""
    Write-Host "📋 Detailed Issues Found:" -ForegroundColor Yellow

    foreach ($issue in $issuesFound) {
        Write-Host "   Article ID: $($issue.ArticleId)" -ForegroundColor White
        Write-Host "   Title: $($issue.ArticleName)" -ForegroundColor Gray
        Write-Host "   Slug: $($issue.Slug)" -ForegroundColor Gray
        Write-Host "   Current Length: $($issue.CurrentLength) chars" -ForegroundColor Gray
        Write-Host "   New Length: $($issue.NewLength) chars" -ForegroundColor Gray
        Write-Host "   Source: $($issue.Source)" -ForegroundColor Gray
        if ($issue.WasTruncated) {
            Write-Host "   Truncated: Yes" -ForegroundColor Yellow
        }
        Write-Host ""
    }
}

# Save the updated articles.json if not in dry run mode
if (-not $DryRun -and $updatedCount -gt 0) {
    try {
        Write-Host "💾 Saving updated articles.json..." -ForegroundColor Cyan

        # Create backup
        $backupPath = $ArticlesJsonPath + ".backup." + (Get-Date -Format "yyyyMMdd-HHmmss")
        Copy-Item $ArticlesJsonPath $backupPath
        Write-Host "📄 Backup created: $backupPath" -ForegroundColor Green

        # Save updated file
        $articlesData | ConvertTo-Json -Depth 10 | Set-Content $ArticlesJsonPath -Encoding UTF8
        Write-Host "✅ Successfully saved updated articles.json" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Error saving articles.json: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}
elseif ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No changes were made to the file" -ForegroundColor Yellow
}
elseif ($updatedCount -eq 0) {
    Write-Host "✅ No updates were needed" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 Twitter Card Description Fix Complete!" -ForegroundColor Green

# Exit with appropriate code
if ($issuesFound.Count -gt 0 -and $DryRun) {
    exit 1  # Issues found in dry run mode
}
elseif ($skippedCount -gt 0) {
    exit 2  # Some articles couldn't be fixed
}
else {
    exit 0  # Success
}
