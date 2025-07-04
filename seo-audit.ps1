# SEO Audit Script for Mark Hazleton Blog
# This script checks all HTML files in the docs folder for common SEO issues

param(
    [string]$DocsPath = "docs",
    [switch]$Verbose = $false
)

Write-Host "🔍 Starting SEO Audit for Mark Hazleton Blog..." -ForegroundColor Green
Write-Host "📁 Scanning directory: $DocsPath" -ForegroundColor Cyan
Write-Host ""

# Initialize counters and arrays
$totalFiles = 0
$issuesFound = @()
$summary = @{
    MissingTitle        = 0
    MissingDescription  = 0
    MissingKeywords     = 0
    MissingCanonical    = 0
    TitleTooLong        = 0
    TitleTooShort       = 0
    DescriptionTooLong  = 0
    DescriptionTooShort = 0
    MissingH1           = 0
    MultipleH1          = 0
    MissingAltText      = 0
    EmptyTitle          = 0
    EmptyDescription    = 0
}

# Function to check individual file
function Test-SEOCompliance {
    param($FilePath)

    $issues = @()
    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue

    if (-not $content) {
        $issues += "Could not read file content"
        return $issues
    }

    # Check for title tag
    if ($content -notmatch '<title[^>]*>([^<]*)</title>') {
        $issues += "Missing title tag"
        $summary.MissingTitle++
    }
    else {
        $titleMatch = [regex]::Match($content, '<title[^>]*>([^<]*)</title>')
        if ($titleMatch.Success) {
            $titleText = $titleMatch.Groups[1].Value.Trim()
            $titleLength = $titleText.Length

            if ([string]::IsNullOrWhiteSpace($titleText)) {
                $issues += "Empty title tag"
                $summary.EmptyTitle++
            }
            elseif ($titleLength -gt 60) {
                $issues += "Title too long ($titleLength chars, should be ≤60)"
                $summary.TitleTooLong++
            }
            elseif ($titleLength -lt 30) {
                $issues += "Title too short ($titleLength chars, should be ≥30)"
                $summary.TitleTooShort++
            }
        }
    }

    # Check for meta description
    if ($content -notmatch 'name=[\"\x27]description[\"\x27]') {
        $issues += "Missing meta description"
        $summary.MissingDescription++
    }
    else {
        $descPattern = 'name=[\"\x27]description[\"\x27][^>]*content=[\"\x27]([^\"]*?)[\"\x27]'
        $descMatch = [regex]::Match($content, $descPattern)
        if ($descMatch.Success) {
            $descText = $descMatch.Groups[1].Value.Trim()
            $descLength = $descText.Length

            if ([string]::IsNullOrWhiteSpace($descText)) {
                $issues += "Empty meta description"
                $summary.EmptyDescription++
            }
            elseif ($descLength -gt 160) {
                $issues += "Description too long ($descLength chars, should be ≤160)"
                $summary.DescriptionTooLong++
            }
            elseif ($descLength -lt 120) {
                $issues += "Description too short ($descLength chars, should be ≥120)"
                $summary.DescriptionTooShort++
            }
        }
    }

    # Check for meta keywords
    if ($content -notmatch 'name=[\"\x27]keywords[\"\x27]') {
        $issues += "Missing meta keywords"
        $summary.MissingKeywords++
    }

    # Check for canonical URL
    if ($content -notmatch 'rel=[\"\x27]canonical[\"\x27]') {
        $issues += "Missing canonical URL"
        $summary.MissingCanonical++
    }

    # Check for H1 tags
    $h1Matches = [regex]::Matches($content, '<h1[^>]*>', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($h1Matches.Count -eq 0) {
        $issues += "Missing H1 tag"
        $summary.MissingH1++
    }
    elseif ($h1Matches.Count -gt 1) {
        $issues += "Multiple H1 tags ($($h1Matches.Count) found, should be 1)"
        $summary.MultipleH1++
    }

    # Check for images without alt text
    $imgMatches = [regex]::Matches($content, '<img[^>]*>', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $imagesWithoutAlt = 0
    foreach ($imgMatch in $imgMatches) {
        if ($imgMatch.Value -notmatch 'alt\s*=') {
            $imagesWithoutAlt++
        }
    }
    if ($imagesWithoutAlt -gt 0) {
        $issues += "Images without alt text ($imagesWithoutAlt found)"
        $summary.MissingAltText += $imagesWithoutAlt
    }

    return $issues
}

# Get all HTML files
try {
    $htmlFiles = Get-ChildItem -Path $DocsPath -Filter "*.html" -Recurse -ErrorAction Stop
    $totalFiles = $htmlFiles.Count

    Write-Host "📊 Found $totalFiles HTML files to analyze" -ForegroundColor Yellow
    Write-Host ""

    # Process each file
    $progress = 0
    foreach ($file in $htmlFiles) {
        $progress++
        $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")

        if ($Verbose) {
            Write-Progress -Activity "SEO Audit" -Status "Analyzing $relativePath" -PercentComplete (($progress / $totalFiles) * 100)
        }

        $fileIssues = Test-SEOCompliance -FilePath $file.FullName

        if ($fileIssues.Count -gt 0) {
            $issuesFound += [PSCustomObject]@{
                File       = $relativePath
                Issues     = $fileIssues
                IssueCount = $fileIssues.Count
            }

            if ($Verbose) {
                Write-Host "❌ $relativePath" -ForegroundColor Red
                foreach ($issue in $fileIssues) {
                    Write-Host "   • $issue" -ForegroundColor Yellow
                }
                Write-Host ""
            }
        }
        elseif ($Verbose) {
            Write-Host "✅ $relativePath" -ForegroundColor Green
        }
    }

    if ($Verbose) {
        Write-Progress -Activity "SEO Audit" -Completed
    }

}
catch {
    Write-Host "❌ Error accessing directory '$DocsPath': $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Display results
Write-Host "📋 SEO AUDIT RESULTS" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host ""

if ($issuesFound.Count -eq 0) {
    Write-Host "🎉 EXCELLENT! No SEO issues found in any of the $totalFiles HTML files!" -ForegroundColor Green
}
else {
    Write-Host "📊 SUMMARY:" -ForegroundColor Cyan
    Write-Host "• Total files analyzed: $totalFiles" -ForegroundColor White
    Write-Host "• Files with issues: $($issuesFound.Count)" -ForegroundColor Yellow
    Write-Host "• Files without issues: $($totalFiles - $issuesFound.Count)" -ForegroundColor Green
    Write-Host ""

    Write-Host "🔍 ISSUE BREAKDOWN:" -ForegroundColor Cyan
    if ($summary.MissingTitle -gt 0) { Write-Host "• Missing title tags: $($summary.MissingTitle)" -ForegroundColor Red }
    if ($summary.EmptyTitle -gt 0) { Write-Host "• Empty title tags: $($summary.EmptyTitle)" -ForegroundColor Red }
    if ($summary.TitleTooLong -gt 0) { Write-Host "• Titles too long: $($summary.TitleTooLong)" -ForegroundColor Yellow }
    if ($summary.TitleTooShort -gt 0) { Write-Host "• Titles too short: $($summary.TitleTooShort)" -ForegroundColor Yellow }
    if ($summary.MissingDescription -gt 0) { Write-Host "• Missing meta descriptions: $($summary.MissingDescription)" -ForegroundColor Red }
    if ($summary.EmptyDescription -gt 0) { Write-Host "• Empty meta descriptions: $($summary.EmptyDescription)" -ForegroundColor Red }
    if ($summary.DescriptionTooLong -gt 0) { Write-Host "• Descriptions too long: $($summary.DescriptionTooLong)" -ForegroundColor Yellow }
    if ($summary.DescriptionTooShort -gt 0) { Write-Host "• Descriptions too short: $($summary.DescriptionTooShort)" -ForegroundColor Yellow }
    if ($summary.MissingKeywords -gt 0) { Write-Host "• Missing meta keywords: $($summary.MissingKeywords)" -ForegroundColor Yellow }
    if ($summary.MissingCanonical -gt 0) { Write-Host "• Missing canonical URLs: $($summary.MissingCanonical)" -ForegroundColor Yellow }
    if ($summary.MissingH1 -gt 0) { Write-Host "• Missing H1 tags: $($summary.MissingH1)" -ForegroundColor Red }
    if ($summary.MultipleH1 -gt 0) { Write-Host "• Multiple H1 tags: $($summary.MultipleH1)" -ForegroundColor Yellow }
    if ($summary.MissingAltText -gt 0) { Write-Host "• Images without alt text: $($summary.MissingAltText)" -ForegroundColor Yellow }

    Write-Host ""
    Write-Host "📝 DETAILED ISSUES:" -ForegroundColor Cyan

    $issuesFound | Sort-Object IssueCount -Descending | ForEach-Object {
        Write-Host ""
        Write-Host "❌ $($_.File) ($($_.IssueCount) issues):" -ForegroundColor Red
        foreach ($issue in $_.Issues) {
            Write-Host "   • $issue" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "✅ SEO Audit Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "💡 RECOMMENDATIONS:" -ForegroundColor Cyan
Write-Host "• Title tags should be 30-60 characters" -ForegroundColor White
Write-Host "• Meta descriptions should be 120-160 characters" -ForegroundColor White
Write-Host "• Each page should have exactly one H1 tag" -ForegroundColor White
Write-Host "• All images should have descriptive alt text" -ForegroundColor White
Write-Host "• Every page should have a canonical URL" -ForegroundColor White
