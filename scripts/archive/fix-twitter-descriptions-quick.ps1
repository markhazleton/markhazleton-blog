# Quick Fix for Twitter Card Description Issues
# This script provides a simple fix for Twitter descriptions that are flagged by seo-audit.ps1
# Usage: .\fix-twitter-descriptions-quick.ps1 [-DryRun] [-Verbose]

param(
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

$ArticlesJsonPath = "src/articles.json"
$MinLength = 120
$MaxLength = 160

Write-Host "🚀 Quick Twitter Card Description Fix" -ForegroundColor Green
Write-Host "📁 File: $ArticlesJsonPath" -ForegroundColor Cyan
Write-Host "📏 Target: $MinLength-$MaxLength characters" -ForegroundColor Cyan
if ($DryRun) { Write-Host "🔍 DRY RUN MODE" -ForegroundColor Yellow }
Write-Host ""

# Load and process articles
try {
    $articles = Get-Content $ArticlesJsonPath -Raw | ConvertFrom-Json
    $updated = 0
    $issues = 0

    foreach ($article in $articles) {
        $needsUpdate = $false
        $currentDesc = ""
        $newDesc = ""
        $source = ""

        # Check current Twitter description
        if ($article.twitter -and $article.twitter.description) {
            $currentDesc = $article.twitter.description.Trim()
            if ($currentDesc.Length -lt $MinLength -or $currentDesc.Length -gt $MaxLength) {
                $needsUpdate = $true
            }
        } else {
            $needsUpdate = $true
        }

        if ($needsUpdate) {
            $issues++

            # Find best replacement description
            $candidates = @()

            if ($article.seo -and $article.seo.description) {
                $candidates += @{ text = $article.seo.description; source = "seo.description" }
            }
            if ($article.og -and $article.og.description) {
                $candidates += @{ text = $article.og.description; source = "og.description" }
            }
            if ($article.description) {
                $candidates += @{ text = $article.description; source = "description" }
            }

            # Select best candidate
            $bestCandidate = $candidates | Where-Object { $_.text.Length -ge $MinLength } | Select-Object -First 1
            if (-not $bestCandidate) {
                $bestCandidate = $candidates | Sort-Object { $_.text.Length } -Descending | Select-Object -First 1
            }

            if ($bestCandidate) {
                $newDesc = $bestCandidate.text.Trim()
                $source = $bestCandidate.source

                # Truncate if too long
                if ($newDesc.Length -gt $MaxLength) {
                    $lastSpace = $newDesc.Substring(0, $MaxLength).LastIndexOf(' ')
                    if ($lastSpace -gt ($MaxLength * 0.8)) {
                        $newDesc = $newDesc.Substring(0, $lastSpace).Trim()
                    } else {
                        $newDesc = $newDesc.Substring(0, $MaxLength - 3).Trim() + "..."
                    }
                }

                # Apply update
                if (-not $DryRun) {
                    if (-not $article.twitter) {
                        $article | Add-Member -MemberType NoteProperty -Name "twitter" -Value @{}
                    }
                    $article.twitter.description = $newDesc
                    $updated++
                }

                # Report
                $status = if ($DryRun) { "WOULD UPDATE" } else { "UPDATED" }
                $color = if ($DryRun) { "Cyan" } else { "Green" }

                Write-Host "✅ $status - ID: $($article.id)" -ForegroundColor $color
                if ($Verbose) {
                    Write-Host "   📝 $($article.name)" -ForegroundColor Gray
                    Write-Host "   📏 $($currentDesc.Length) → $($newDesc.Length) chars" -ForegroundColor Gray
                    Write-Host "   🔗 Source: $source" -ForegroundColor Gray
                }
            } else {
                Write-Host "⚠️  No suitable description found for ID: $($article.id)" -ForegroundColor Yellow
            }
        }
    }

    # Save changes
    if (-not $DryRun -and $updated -gt 0) {
        # Create backup
        $backup = $ArticlesJsonPath + ".backup." + (Get-Date -Format "yyyyMMdd-HHmmss")
        Copy-Item $ArticlesJsonPath $backup

        # Save updated file
        $articles | ConvertTo-Json -Depth 10 | Set-Content $ArticlesJsonPath -Encoding UTF8
        Write-Host ""
        Write-Host "💾 Backup created: $backup" -ForegroundColor Green
        Write-Host "✅ Updated $updated articles" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "📊 Summary: $issues issues found, $updated articles updated" -ForegroundColor Yellow

} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "🎉 Complete!" -ForegroundColor Green
