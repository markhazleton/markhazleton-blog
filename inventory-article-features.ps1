# Article Features Inventory Script
# This script analyzes PUG article files to identify which ones have meta sections and LinkedIn sharing

$articlePath = "src\pug\articles"
$pugFiles = Get-ChildItem -Path $articlePath -Filter "*.pug" | Sort-Object Name

$results = @()

foreach ($file in $pugFiles) {
    $content = Get-Content $file.FullName -Raw

    # Skip the article-stub.pug template file
    if ($file.Name -eq "article-stub.pug") {
        continue
    }

    $hasArticleMeta = $content -match "\.article-meta"
    $hasLinkedInShare = $content -match "\+articleLinkedInShare"
    $hasInlineLinkedIn = $content -match "\+inlineLinkedInPrompt"
    $hasFloatingLinkedIn = $content -match "\+floatingLinkedInShare"

    $linkedInCount = 0
    if ($hasLinkedInShare) { $linkedInCount++ }
    if ($hasInlineLinkedIn) { $linkedInCount++ }
    if ($hasFloatingLinkedIn) { $linkedInCount++ }

    $status = "Missing Both"
    if ($hasArticleMeta -and $linkedInCount -eq 3) {
        $status = "Complete"
    }
    elseif ($hasArticleMeta -and $linkedInCount -gt 0) {
        $status = "Partial LinkedIn"
    }
    elseif ($hasArticleMeta) {
        $status = "Meta Only"
    }
    elseif ($linkedInCount -gt 0) {
        $status = "LinkedIn Only"
    }

    $results += [PSCustomObject]@{
        FileName            = $file.Name
        HasArticleMeta      = $hasArticleMeta
        HasLinkedInShare    = $hasLinkedInShare
        HasInlineLinkedIn   = $hasInlineLinkedIn
        HasFloatingLinkedIn = $hasFloatingLinkedIn
        LinkedInComponents  = $linkedInCount
        Status              = $status
    }
}

# Display summary
Write-Host "`n=== ARTICLE FEATURES INVENTORY SUMMARY ===" -ForegroundColor Green
Write-Host "Total articles analyzed: $($results.Count)" -ForegroundColor Yellow

$statusCounts = $results | Group-Object Status | Sort-Object Count -Descending
foreach ($statusGroup in $statusCounts) {
    Write-Host "$($statusGroup.Name): $($statusGroup.Count) files" -ForegroundColor Cyan
}

Write-Host "`n=== FILES BY STATUS ===" -ForegroundColor Green

# Complete files
$complete = $results | Where-Object { $_.Status -eq "Complete" }
if ($complete) {
    Write-Host "`n✅ COMPLETE (Meta + All 3 LinkedIn Components): $($complete.Count)" -ForegroundColor Green
    $complete | ForEach-Object { Write-Host "   - $($_.FileName)" -ForegroundColor White }
}

# Meta only files
$metaOnly = $results | Where-Object { $_.Status -eq "Meta Only" }
if ($metaOnly) {
    Write-Host "`n📝 META ONLY (Missing LinkedIn): $($metaOnly.Count)" -ForegroundColor Yellow
    $metaOnly | ForEach-Object { Write-Host "   - $($_.FileName)" -ForegroundColor White }
}

# Partial LinkedIn files
$partialLinkedIn = $results | Where-Object { $_.Status -eq "Partial LinkedIn" }
if ($partialLinkedIn) {
    Write-Host "`n⚠️  PARTIAL LINKEDIN (Meta + Some LinkedIn): $($partialLinkedIn.Count)" -ForegroundColor Magenta
    $partialLinkedIn | ForEach-Object {
        Write-Host "   - $($_.FileName) ($($_.LinkedInComponents)/3 components)" -ForegroundColor White
    }
}

# LinkedIn only files
$linkedInOnly = $results | Where-Object { $_.Status -eq "LinkedIn Only" }
if ($linkedInOnly) {
    Write-Host "`n🔗 LINKEDIN ONLY (Missing Meta): $($linkedInOnly.Count)" -ForegroundColor Blue
    $linkedInOnly | ForEach-Object {
        Write-Host "   - $($_.FileName) ($($_.LinkedInComponents)/3 components)" -ForegroundColor White
    }
}

# Missing both
$missingBoth = $results | Where-Object { $_.Status -eq "Missing Both" }
if ($missingBoth) {
    Write-Host "`n❌ MISSING BOTH (No Meta, No LinkedIn): $($missingBoth.Count)" -ForegroundColor Red
    $missingBoth | ForEach-Object { Write-Host "   - $($_.FileName)" -ForegroundColor White }
}

Write-Host "`n=== PRIORITY FOR UPDATES ===" -ForegroundColor Green
Write-Host "1. Meta Only files need LinkedIn sharing components added" -ForegroundColor Yellow
Write-Host "2. Partial LinkedIn files need remaining LinkedIn components" -ForegroundColor Magenta
Write-Host "3. LinkedIn Only files need article meta sections" -ForegroundColor Blue
Write-Host "4. Missing Both files need complete implementation" -ForegroundColor Red

# Export detailed results to CSV for further analysis
$results | Export-Csv -Path "article-features-inventory.csv" -NoTypeInformation
Write-Host "`n📊 Detailed results exported to: article-features-inventory.csv" -ForegroundColor Green
