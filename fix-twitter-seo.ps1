# SEO Fix: Twitter Card & Open Graph Description Updater
# This script works in conjunction with seo-audit.ps1 to fix Twitter Card and Open Graph description issues
# It updates Twitter and OG descriptions that are too short/long by using fallback descriptions

param(
    [string]$ArticlesJsonPath = "src/articles.json",
    [int]$TwitterMinLength = 200,
    [int]$TwitterMaxLength = 200,
    [int]$OGMinLength = 200,
    [int]$OGMaxLength = 300,
    [switch]$DryRun = $false,
    [switch]$Verbose = $false,
    [switch]$RunSeoAudit = $false
)

# Constants
$SCRIPT_NAME = "SEO Fix: Twitter Card & Open Graph Description Updater"
$BACKUP_PREFIX = "articles.json.backup."

function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = "White",
        [string]$Prefix = ""
    )

    if ($Prefix) {
        Write-Host "$Prefix " -NoNewline -ForegroundColor Gray
    }
    Write-Host $Message -ForegroundColor $Color
}

function Get-OptimalTwitterDescription {
    param(
        [PSCustomObject]$Article,
        [int]$MinLength,
        [int]$MaxLength
    )

    $fallbackOrder = @(
        @{ Property = "seo.description"; Path = { $Article.seo.description } },
        @{ Property = "og.description"; Path = { $Article.og.description } },
        @{ Property = "description"; Path = { $Article.description } },
        @{ Property = "summary"; Path = { $Article.summary } }
    )

    foreach ($fallback in $fallbackOrder) {
        try {
            $text = & $fallback.Path
            if ([string]::IsNullOrWhiteSpace($text)) { continue }

            $text = $text.Trim()
            if ($text.Length -ge $MinLength) {
                return @{
                    Text         = (Get-TruncatedText -Text $text -MaxLength $MaxLength)
                    Source       = $fallback.Property
                    WasTruncated = $text.Length -gt $MaxLength
                }
            }
        }
        catch {
            # Skip if property doesn't exist
            continue
        }
    }

    # If no description meets minimum length, use the longest available
    $longestDesc = $null
    $longestLength = 0

    foreach ($fallback in $fallbackOrder) {
        try {
            $text = & $fallback.Path
            if ([string]::IsNullOrWhiteSpace($text)) { continue }

            $text = $text.Trim()
            if ($text.Length -gt $longestLength) {
                $longestLength = $text.Length
                $longestDesc = @{
                    Text         = (Get-TruncatedText -Text $text -MaxLength $MaxLength)
                    Source       = $fallback.Property
                    WasTruncated = $text.Length -gt $MaxLength
                }
            }
        }
        catch {
            continue
        }
    }

    return $longestDesc
}

function Get-OptimalOGDescription {
    param(
        [PSCustomObject]$Article,
        [int]$MinLength,
        [int]$MaxLength
    )

    $fallbackOrder = @(
        @{ Property = "seo.description"; Path = { $Article.seo.description } },
        @{ Property = "twitter.description"; Path = { $Article.twitter.description } },
        @{ Property = "description"; Path = { $Article.description } },
        @{ Property = "summary"; Path = { $Article.summary } }
    )

    foreach ($fallback in $fallbackOrder) {
        try {
            $text = & $fallback.Path
            if ([string]::IsNullOrWhiteSpace($text)) { continue }

            $text = $text.Trim()
            if ($text.Length -ge $MinLength) {
                return @{
                    Text         = (Get-TruncatedText -Text $text -MaxLength $MaxLength)
                    Source       = $fallback.Property
                    WasTruncated = $text.Length -gt $MaxLength
                }
            }
        }
        catch {
            # Skip if property doesn't exist
            continue
        }
    }

    # If no description meets minimum length, use the longest available
    $longestDesc = $null
    $longestLength = 0

    foreach ($fallback in $fallbackOrder) {
        try {
            $text = & $fallback.Path
            if ([string]::IsNullOrWhiteSpace($text)) { continue }

            $text = $text.Trim()
            if ($text.Length -gt $longestLength) {
                $longestLength = $text.Length
                $longestDesc = @{
                    Text         = (Get-TruncatedText -Text $text -MaxLength $MaxLength)
                    Source       = $fallback.Property
                    WasTruncated = $text.Length -gt $MaxLength
                }
            }
        }
        catch {
            continue
        }
    }

    return $longestDesc
}

function Get-TruncatedText {
    param(
        [string]$Text,
        [int]$MaxLength
    )

    if ($Text.Length -le $MaxLength) {
        return $Text
    }

    # Find the last space before the max length to preserve word boundaries
    $truncated = $Text.Substring(0, $MaxLength - 3)  # Reserve 3 chars for "..."
    $lastSpace = $truncated.LastIndexOf(' ')

    if ($lastSpace -gt ($MaxLength * 0.7)) {
        return $truncated.Substring(0, $lastSpace).Trim() + "..."
    }
    else {
        return $truncated.Trim() + "..."
    }
}

function Test-TwitterDescription {
    param(
        [PSCustomObject]$Article,
        [int]$MinLength,
        [int]$MaxLength
    )

    $issues = @()

    if (-not $Article.twitter -or -not $Article.twitter.description) {
        $issues += "Missing Twitter Card description"
        return $issues
    }

    $desc = $Article.twitter.description.Trim()

    if ([string]::IsNullOrWhiteSpace($desc)) {
        $issues += "Empty Twitter Card description"
    }
    elseif ($desc.Length -gt $MaxLength) {
        $issues += "Twitter Card description too long ($($desc.Length) chars, should be ≤$MaxLength)"
    }
    elseif ($desc.Length -lt $MinLength) {
        $issues += "Twitter Card description too short ($($desc.Length) chars, should be ≥$MinLength)"
    }

    return $issues
}

function Test-OGDescription {
    param(
        [PSCustomObject]$Article,
        [int]$MinLength,
        [int]$MaxLength
    )

    $issues = @()

    if (-not $Article.og -or -not $Article.og.description) {
        $issues += "Missing Open Graph description"
        return $issues
    }

    $desc = $Article.og.description.Trim()

    if ([string]::IsNullOrWhiteSpace($desc)) {
        $issues += "Empty Open Graph description"
    }
    elseif ($desc.Length -gt $MaxLength) {
        $issues += "Open Graph description too long ($($desc.Length) chars, should be ≤$MaxLength)"
    }
    elseif ($desc.Length -lt $MinLength) {
        $issues += "Open Graph description too short ($($desc.Length) chars, should be ≥$MinLength)"
    }

    return $issues
}

function Backup-ArticlesFile {
    param(
        [string]$FilePath
    )

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = "$FilePath.$BACKUP_PREFIX$timestamp"

    try {
        Copy-Item $FilePath $backupPath
        Write-ColoredOutput "Backup created: $backupPath" "Green" "💾"
        return $backupPath
    }
    catch {
        Write-ColoredOutput "Failed to create backup: $($_.Exception.Message)" "Red" "❌"
        return $null
    }
}

# Main script execution
Write-Host ""
Write-Host "🔧 $SCRIPT_NAME" -ForegroundColor Green
Write-Host "=" * 60 -ForegroundColor Gray
Write-ColoredOutput "Processing: $ArticlesJsonPath" "Cyan" "📁"
Write-ColoredOutput "Twitter length: $TwitterMinLength-$TwitterMaxLength characters" "Cyan" "🐦"
Write-ColoredOutput "Open Graph length: $OGMinLength-$OGMaxLength characters" "Cyan" "�"

if ($DryRun) {
    Write-ColoredOutput "DRY RUN MODE - No changes will be made" "Yellow" "🔍"
}

Write-Host ""

# Load articles.json
try {
    if (-not (Test-Path $ArticlesJsonPath)) {
        Write-ColoredOutput "articles.json not found at: $ArticlesJsonPath" "Red" "❌"
        exit 1
    }

    Write-ColoredOutput "Loading articles.json..." "Cyan" "📄"
    $articles = Get-Content $ArticlesJsonPath -Raw | ConvertFrom-Json
    Write-ColoredOutput "Loaded $($articles.Count) articles" "Green" "✅"
}
catch {
    Write-ColoredOutput "Error loading articles.json: $($_.Exception.Message)" "Red" "❌"
    exit 1
}

# Process articles
Write-Host ""
Write-ColoredOutput "Analyzing Twitter Card and Open Graph descriptions..." "Yellow" "🔍"
Write-Host ""

$stats = @{
    Total                 = $articles.Count
    TwitterIssuesFound    = 0
    OGIssuesFound         = 0
    TwitterUpdated        = 0
    OGUpdated             = 0
    Skipped               = 0
    NoSuitableDescription = 0
}

$results = @()

foreach ($article in $articles) {
    $articleId = if ($article.id) { $article.id } else { "N/A" }
    $articleName = if ($article.name) { $article.name } else { "Unnamed Article" }

    # Test current Twitter description
    $twitterIssues = Test-TwitterDescription -Article $article -MinLength $TwitterMinLength -MaxLength $TwitterMaxLength
    $ogIssues = Test-OGDescription -Article $article -MinLength $OGMinLength -MaxLength $OGMaxLength

    $hasTwitterIssues = $twitterIssues.Count -gt 0
    $hasOGIssues = $ogIssues.Count -gt 0

    if ($hasTwitterIssues -or $hasOGIssues) {
        if ($hasTwitterIssues) { $stats.TwitterIssuesFound++ }
        if ($hasOGIssues) { $stats.OGIssuesFound++ }

        if ($Verbose) {
            Write-ColoredOutput "Article ID: $articleId - $articleName" "Gray" "📝"
            foreach ($issue in $twitterIssues) {
                Write-ColoredOutput $issue "Yellow" "   🐦"
            }
            foreach ($issue in $ogIssues) {
                Write-ColoredOutput $issue "Yellow" "   🔗"
            }
        }

        $updated = $false

        # Handle Twitter description issues
        if ($hasTwitterIssues) {
            $optimalTwitterDesc = Get-OptimalTwitterDescription -Article $article -MinLength $TwitterMinLength -MaxLength $TwitterMaxLength

            if ($optimalTwitterDesc) {
                $currentTwitterDesc = if ($article.twitter -and $article.twitter.description) { $article.twitter.description } else { "" }

                # Apply Twitter update
                if (-not $DryRun) {
                    if (-not $article.twitter) {
                        $article | Add-Member -MemberType NoteProperty -Name "twitter" -Value @{}
                    }
                    $article.twitter.description = $optimalTwitterDesc.Text
                    $stats.TwitterUpdated++
                    $updated = $true
                }

                if ($Verbose) {
                    $status = if ($DryRun) { "Would update Twitter" } else { "Updated Twitter" }
                    Write-ColoredOutput "$status description" "Green" "   🐦"
                    Write-ColoredOutput "Length: $($currentTwitterDesc.Length) → $($optimalTwitterDesc.Text.Length) chars" "Gray" "      📏"
                    Write-ColoredOutput "Source: $($optimalTwitterDesc.Source)" "Gray" "      🔗"
                    if ($optimalTwitterDesc.WasTruncated) {
                        Write-ColoredOutput "Text was truncated" "Yellow" "      ✂️"
                    }
                }
            }
            else {
                $stats.NoSuitableDescription++
                Write-ColoredOutput "No suitable Twitter description found for Article ID: $articleId" "Yellow" "⚠️"
            }
        }

        # Handle Open Graph description issues
        if ($hasOGIssues) {
            $optimalOGDesc = Get-OptimalOGDescription -Article $article -MinLength $OGMinLength -MaxLength $OGMaxLength

            if ($optimalOGDesc) {
                $currentOGDesc = if ($article.og -and $article.og.description) { $article.og.description } else { "" }

                # Apply OG update
                if (-not $DryRun) {
                    if (-not $article.og) {
                        $article | Add-Member -MemberType NoteProperty -Name "og" -Value @{}
                    }
                    $article.og.description = $optimalOGDesc.Text
                    $stats.OGUpdated++
                    $updated = $true
                }

                if ($Verbose) {
                    $status = if ($DryRun) { "Would update OG" } else { "Updated OG" }
                    Write-ColoredOutput "$status description" "Green" "   🔗"
                    Write-ColoredOutput "Length: $($currentOGDesc.Length) → $($optimalOGDesc.Text.Length) chars" "Gray" "      📏"
                    Write-ColoredOutput "Source: $($optimalOGDesc.Source)" "Gray" "      🔗"
                    if ($optimalOGDesc.WasTruncated) {
                        Write-ColoredOutput "Text was truncated" "Yellow" "      ✂️"
                    }
                }
            }
            else {
                $stats.NoSuitableDescription++
                Write-ColoredOutput "No suitable OG description found for Article ID: $articleId" "Yellow" "⚠️"
            }
        }

        # Report overall status
        if ($updated -or $DryRun) {
            $status = if ($DryRun) { "Would update" } else { "Updated" }
            $color = if ($DryRun) { "Cyan" } else { "Green" }
            Write-ColoredOutput "$status Article ID: $articleId" $color "✅"
        }
    }
}

# Save changes
if (-not $DryRun -and ($stats.TwitterUpdated -gt 0 -or $stats.OGUpdated -gt 0)) {
    Write-Host ""
    Write-ColoredOutput "Saving changes..." "Cyan" "💾"

    # Create backup
    $backupPath = Backup-ArticlesFile -FilePath $ArticlesJsonPath

    if ($backupPath) {
        try {
            $articles | ConvertTo-Json -Depth 10 | Set-Content $ArticlesJsonPath -Encoding UTF8
            Write-ColoredOutput "Successfully saved updated articles.json" "Green" "✅"
        }
        catch {
            Write-ColoredOutput "Error saving articles.json: $($_.Exception.Message)" "Red" "❌"
            exit 1
        }
    }
    else {
        Write-ColoredOutput "Skipping save due to backup failure" "Yellow" "⚠️"
    }
}

# Summary
Write-Host ""
Write-Host "📊 Summary" -ForegroundColor Green
Write-Host "=" * 40 -ForegroundColor Gray
Write-ColoredOutput "Total articles: $($stats.Total)" "Gray" "📄"
Write-ColoredOutput "Twitter issues found: $($stats.TwitterIssuesFound)" "Yellow" "🐦"
Write-ColoredOutput "Open Graph issues found: $($stats.OGIssuesFound)" "Yellow" "🔗"
Write-ColoredOutput "Twitter descriptions updated: $($stats.TwitterUpdated)" "Green" "✅"
Write-ColoredOutput "Open Graph descriptions updated: $($stats.OGUpdated)" "Green" "✅"
Write-ColoredOutput "No suitable description: $($stats.NoSuitableDescription)" "Yellow" "⚠️"

# Run SEO audit if requested
if ($RunSeoAudit) {
    Write-Host ""
    Write-ColoredOutput "Running SEO audit..." "Cyan" "🔍"

    if (Test-Path "seo-audit.ps1") {
        & ".\seo-audit.ps1"
    }
    else {
        Write-ColoredOutput "seo-audit.ps1 not found in current directory" "Yellow" "⚠️"
    }
}

Write-Host ""
Write-ColoredOutput "Twitter Card & Open Graph Description Fix Complete!" "Green" "🎉"

# Exit codes
$totalIssues = $stats.TwitterIssuesFound + $stats.OGIssuesFound
if ($totalIssues -gt 0 -and $DryRun) {
    exit 1  # Issues found in dry run
}
elseif ($stats.NoSuitableDescription -gt 0) {
    exit 2  # Some articles couldn't be fixed
}
else {
    exit 0  # Success
}
