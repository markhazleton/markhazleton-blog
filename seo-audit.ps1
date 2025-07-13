# SEO Audit Script for Mark Hazleton Blog
# This script checks all HTML files in the docs folder for common SEO issues
# Validation logic is consistent with mwhWebAdmin\Services\SeoValidationService.cs

param(
    [string]$DocsPath = "docs",
    [switch]$Verbose = $false
)

# Import centralized SEO validation configuration
Import-Module -Name "$PSScriptRoot\SeoValidationConfig.psm1" -Force

# Get the SEO validation configuration
$SeoConfig = Get-SeoValidationConfig

Write-Host "🔍 Starting SEO Audit for Mark Hazleton Blog..." -ForegroundColor Green
Write-Host "📁 Scanning directory: $DocsPath" -ForegroundColor Cyan
Write-Host ""

# Load articles.json to validate entries
$articlesJsonPath = "src/articles.json"
$articlesData = @()
$articlesLookup = @{}

try {
    if (Test-Path $articlesJsonPath) {
        Write-Host "📄 Loading articles.json..." -ForegroundColor Cyan
        $articlesData = Get-Content $articlesJsonPath -Raw | ConvertFrom-Json

        # Create lookup table for faster access
        foreach ($article in $articlesData) {
            if ($article.slug) {
                $articlesLookup[$article.slug] = $article
            }
        }
        Write-Host "✅ Loaded $($articlesData.Count) articles from articles.json" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️ articles.json not found at $articlesJsonPath" -ForegroundColor Yellow
        Write-Host "   Proceeding with validation for all HTML files..." -ForegroundColor Yellow
    }
}
catch {
    Write-Host "❌ Error loading articles.json: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Proceeding with validation for all HTML files..." -ForegroundColor Yellow
}

Write-Host ""

# Initialize counters and arrays
$totalFiles = 0
$issuesFound = @()
$summary = @{
    MissingTitle               = 0
    MissingDescription         = 0
    MissingKeywords            = 0
    MissingCanonical           = 0
    TitleTooLong               = 0
    TitleTooShort              = 0
    MetaDescriptionTooLong     = 0
    MetaDescriptionTooShort    = 0
    OgDescriptionTooLong       = 0
    OgDescriptionTooShort      = 0
    TwitterDescriptionTooLong  = 0
    TwitterDescriptionTooShort = 0
    MissingH1                  = 0
    MultipleH1                 = 0
    MissingAltText             = 0
    EmptyTitle                 = 0
    EmptyDescription           = 0
    EmptyOgDescription         = 0
    EmptyTwitterDescription    = 0
    MissingOpenGraph           = 0
    MissingTwitterCard         = 0
    TooFewKeywords             = 0
    TooManyKeywords            = 0
}

# Function to check if file should be validated based on articles.json
function Should-ValidateFile {
    param($HtmlFilePath)

    # If articles.json wasn't loaded, validate all files
    if ($articlesLookup.Count -eq 0) {
        return $true
    }

    # Extract the relative path from docs folder
    $relativePath = $HtmlFilePath.Replace((Get-Location).Path + "\docs\", "").Replace("\", "/")

    # Check direct match first
    if ($articlesLookup.ContainsKey($relativePath)) {
        return $true
    }

    # Check if this is an index.html file that should match a directory slug ending with /
    if ($relativePath.EndsWith("/index.html")) {
        $directoryPath = $relativePath.Replace("/index.html", "/")
        if ($articlesLookup.ContainsKey($directoryPath)) {
            return $true
        }
    }

    return $false
}

# Function to find corresponding PUG file for HTML file
function Get-PugFilePath {
    param($HtmlFilePath)

    $relativePath = $HtmlFilePath.Replace((Get-Location).Path + "\docs\", "")
    $pugFileName = [System.IO.Path]::GetFileNameWithoutExtension($relativePath) + ".pug"

    # Handle different directory structures
    if ($relativePath.Contains("articles\")) {
        $articlePath = $relativePath.Replace("articles\", "").Replace(".html", ".pug")
        $pugPath = "src\pug\articles\$articlePath"
    }
    elseif ($relativePath.Contains("projectmechanics\")) {
        $projectPath = $relativePath.Replace("projectmechanics\", "").Replace(".html", ".pug")
        $pugPath = "src\pug\projectmechanics\$projectPath"
    }
    else {
        $pugPath = "src\pug\$pugFileName"
    }

    # Check if the PUG file exists
    if (Test-Path $pugPath) {
        return $pugPath
    }
    else {
        return "PUG file not found"
    }
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
            elseif ($titleLength -gt $SeoConfig.Title.MaxLength) {
                $issues += "Title too long ($titleLength chars, should be ≤$($SeoConfig.Title.MaxLength))"
                $summary.TitleTooLong++
            }
            elseif ($titleLength -lt $SeoConfig.Title.MinLength) {
                $issues += "Title too short ($titleLength chars, should be ≥$($SeoConfig.Title.MinLength))"
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
            elseif ($descLength -gt $SeoConfig.MetaDescription.MaxLength) {
                $issues += "Meta description too long ($descLength chars, should be ≤$($SeoConfig.MetaDescription.MaxLength))"
                $summary.MetaDescriptionTooLong++
            }
            elseif ($descLength -lt $SeoConfig.MetaDescription.MinLength) {
                $issues += "Meta description too short ($descLength chars, should be ≥$($SeoConfig.MetaDescription.MinLength))"
                $summary.MetaDescriptionTooShort++
            }
        }
    }

    # Check for meta keywords with count validation
    if ($content -notmatch 'name=[\"\x27]keywords[\"\x27]') {
        $issues += "Missing meta keywords"
        $summary.MissingKeywords++
    }
    else {
        $keywordPattern = 'name=[\"\x27]keywords[\"\x27][^>]*content=[\"\x27]([^\"]*?)[\"\x27]'
        $keywordMatch = [regex]::Match($content, $keywordPattern)
        if ($keywordMatch.Success) {
            $keywordText = $keywordMatch.Groups[1].Value.Trim()
            if (-not [string]::IsNullOrWhiteSpace($keywordText)) {
                $keywordList = $keywordText.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
                if ($keywordList.Count -lt $SeoConfig.Keywords.MinCount) {
                    $issues += "Too few keywords ($($keywordList.Count) found, recommended: $($SeoConfig.Keywords.MinCount)-$($SeoConfig.Keywords.MaxCount))"
                    $summary.TooFewKeywords++
                }
                elseif ($keywordList.Count -gt $SeoConfig.Keywords.MaxCount) {
                    $issues += "Too many keywords ($($keywordList.Count) found, recommended: $($SeoConfig.Keywords.MinCount)-$($SeoConfig.Keywords.MaxCount))"
                    $summary.TooManyKeywords++
                }
            }
        }
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

    # Check for Open Graph metadata
    $ogChecks = @(
        @{Pattern = 'property=[\"\x27]og:title[\"\x27]'; Message = "Missing Open Graph title" },
        @{Pattern = 'property=[\"\x27]og:image[\"\x27]'; Message = "Missing Open Graph image" },
        @{Pattern = 'property=[\"\x27]og:type[\"\x27]'; Message = "Missing Open Graph type" }
    )

    foreach ($check in $ogChecks) {
        if ($content -notmatch $check.Pattern) {
            $issues += $check.Message
            $summary.MissingOpenGraph++
        }
    }

    # Check Open Graph description separately for length validation
    if ($content -notmatch 'property=[\"\x27]og:description[\"\x27]') {
        $issues += "Missing Open Graph description"
        $summary.MissingOpenGraph++
    }
    else {
        $ogDescPattern = 'property=[\"\x27]og:description[\"\x27][^>]*content=[\"\x27]([^\"]*?)[\"\x27]'
        $ogDescMatch = [regex]::Match($content, $ogDescPattern)
        if ($ogDescMatch.Success) {
            $ogDescText = $ogDescMatch.Groups[1].Value.Trim()
            $ogDescLength = $ogDescText.Length

            if ([string]::IsNullOrWhiteSpace($ogDescText)) {
                $issues += "Empty Open Graph description"
                $summary.EmptyOgDescription++
            }
            elseif ($ogDescLength -gt $SeoConfig.OpenGraphDescription.MaxLength) {
                $issues += "Open Graph description too long ($ogDescLength chars, should be ≤$($SeoConfig.OpenGraphDescription.MaxLength))"
                $summary.OgDescriptionTooLong++
            }
            elseif ($ogDescLength -lt $SeoConfig.OpenGraphDescription.MinLength) {
                $issues += "Open Graph description too short ($ogDescLength chars, should be ≥$($SeoConfig.OpenGraphDescription.MinLength))"
                $summary.OgDescriptionTooShort++
            }
        }
    }

    # Check for Twitter Card metadata
    $twitterChecks = @(
        @{Pattern = 'name=[\"\x27]twitter:card[\"\x27]'; Message = "Missing Twitter Card type" },
        @{Pattern = 'name=[\"\x27]twitter:title[\"\x27]'; Message = "Missing Twitter Card title" },
        @{Pattern = 'name=[\"\x27]twitter:image[\"\x27]'; Message = "Missing Twitter Card image" }
    )

    foreach ($check in $twitterChecks) {
        if ($content -notmatch $check.Pattern) {
            $issues += $check.Message
            $summary.MissingTwitterCard++
        }
    }

    # Check Twitter Card description separately for length validation
    if ($content -notmatch 'name=[\"\x27]twitter:description[\"\x27]') {
        $issues += "Missing Twitter Card description"
        $summary.MissingTwitterCard++
    }
    else {
        $twitterDescPattern = 'name=[\"\x27]twitter:description[\"\x27][^>]*content=[\"\x27]([^\"]*?)[\"\x27]'
        $twitterDescMatch = [regex]::Match($content, $twitterDescPattern)
        if ($twitterDescMatch.Success) {
            $twitterDescText = $twitterDescMatch.Groups[1].Value.Trim()
            $twitterDescLength = $twitterDescText.Length

            if ([string]::IsNullOrWhiteSpace($twitterDescText)) {
                $issues += "Empty Twitter Card description"
                $summary.EmptyTwitterDescription++
            }
            elseif ($twitterDescLength -gt $SeoConfig.TwitterDescription.MaxLength) {
                $issues += "Twitter Card description too long ($twitterDescLength chars, should be ≤$($SeoConfig.TwitterDescription.MaxLength))"
                $summary.TwitterDescriptionTooLong++
            }
            elseif ($twitterDescLength -lt $SeoConfig.TwitterDescription.MinLength) {
                $issues += "Twitter Card description too short ($twitterDescLength chars, should be ≥$($SeoConfig.TwitterDescription.MinLength))"
                $summary.TwitterDescriptionTooShort++
            }
        }
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
    $validatedFiles = 0
    $skippedFiles = 0
    foreach ($file in $htmlFiles) {
        $progress++
        $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")

        if ($Verbose) {
            Write-Progress -Activity "SEO Audit" -Status "Analyzing $relativePath" -PercentComplete (($progress / $totalFiles) * 100)
        }

        # Check if this file should be validated based on articles.json
        if (-not (Should-ValidateFile -HtmlFilePath $file.FullName)) {
            $skippedFiles++
            if ($Verbose) {
                Write-Host "⏭️ $relativePath (not in articles.json, skipping)" -ForegroundColor Gray
            }
            continue
        }

        $validatedFiles++
        $fileIssues = Test-SEOCompliance -FilePath $file.FullName

        if ($fileIssues.Count -gt 0) {
            $pugFilePath = Get-PugFilePath -HtmlFilePath $file.FullName
            $issuesFound += [PSCustomObject]@{
                File       = $relativePath
                PugFile    = $pugFilePath
                Issues     = $fileIssues
                IssueCount = $fileIssues.Count
            }

            if ($Verbose) {
                Write-Host "❌ $relativePath" -ForegroundColor Red
                Write-Host "   📄 Source PUG: $pugFilePath" -ForegroundColor Cyan
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

    Write-Host ""
    Write-Host "📊 PROCESSING SUMMARY:" -ForegroundColor Yellow
    Write-Host "• Total HTML files found: $totalFiles" -ForegroundColor White
    Write-Host "• Files validated: $validatedFiles" -ForegroundColor Green
    Write-Host "• Files skipped (not in articles.json): $skippedFiles" -ForegroundColor Gray

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
    Write-Host "🎉 EXCELLENT! No SEO issues found in any of the $validatedFiles validated HTML files!" -ForegroundColor Green
}
else {
    Write-Host "📊 SUMMARY:" -ForegroundColor Cyan
    Write-Host "• Files validated: $validatedFiles" -ForegroundColor White
    Write-Host "• Files with issues: $($issuesFound.Count)" -ForegroundColor Yellow
    Write-Host "• Files without issues: $($validatedFiles - $issuesFound.Count)" -ForegroundColor Green
    Write-Host ""

    Write-Host "🔍 ISSUE BREAKDOWN:" -ForegroundColor Cyan
    if ($summary.MissingTitle -gt 0) { Write-Host "• Missing title tags: $($summary.MissingTitle)" -ForegroundColor Red }
    if ($summary.EmptyTitle -gt 0) { Write-Host "• Empty title tags: $($summary.EmptyTitle)" -ForegroundColor Red }
    if ($summary.TitleTooLong -gt 0) { Write-Host "• Titles too long: $($summary.TitleTooLong)" -ForegroundColor Yellow }
    if ($summary.TitleTooShort -gt 0) { Write-Host "• Titles too short: $($summary.TitleTooShort)" -ForegroundColor Yellow }
    if ($summary.MissingDescription -gt 0) { Write-Host "• Missing meta descriptions: $($summary.MissingDescription)" -ForegroundColor Red }
    if ($summary.EmptyDescription -gt 0) { Write-Host "• Empty meta descriptions: $($summary.EmptyDescription)" -ForegroundColor Red }
    if ($summary.MetaDescriptionTooLong -gt 0) { Write-Host "• Meta descriptions too long: $($summary.MetaDescriptionTooLong)" -ForegroundColor Yellow }
    if ($summary.MetaDescriptionTooShort -gt 0) { Write-Host "• Meta descriptions too short: $($summary.MetaDescriptionTooShort)" -ForegroundColor Yellow }
    if ($summary.OgDescriptionTooLong -gt 0) { Write-Host "• Open Graph descriptions too long: $($summary.OgDescriptionTooLong)" -ForegroundColor Yellow }
    if ($summary.OgDescriptionTooShort -gt 0) { Write-Host "• Open Graph descriptions too short: $($summary.OgDescriptionTooShort)" -ForegroundColor Yellow }
    if ($summary.EmptyOgDescription -gt 0) { Write-Host "• Empty Open Graph descriptions: $($summary.EmptyOgDescription)" -ForegroundColor Red }
    if ($summary.TwitterDescriptionTooLong -gt 0) { Write-Host "• Twitter Card descriptions too long: $($summary.TwitterDescriptionTooLong)" -ForegroundColor Yellow }
    if ($summary.TwitterDescriptionTooShort -gt 0) { Write-Host "• Twitter Card descriptions too short: $($summary.TwitterDescriptionTooShort)" -ForegroundColor Yellow }
    if ($summary.EmptyTwitterDescription -gt 0) { Write-Host "• Empty Twitter Card descriptions: $($summary.EmptyTwitterDescription)" -ForegroundColor Red }
    if ($summary.MissingKeywords -gt 0) { Write-Host "• Missing meta keywords: $($summary.MissingKeywords)" -ForegroundColor Yellow }
    if ($summary.MissingCanonical -gt 0) { Write-Host "• Missing canonical URLs: $($summary.MissingCanonical)" -ForegroundColor Yellow }
    if ($summary.MissingH1 -gt 0) { Write-Host "• Missing H1 tags: $($summary.MissingH1)" -ForegroundColor Red }
    if ($summary.MultipleH1 -gt 0) { Write-Host "• Multiple H1 tags: $($summary.MultipleH1)" -ForegroundColor Yellow }
    if ($summary.MissingAltText -gt 0) { Write-Host "• Images without alt text: $($summary.MissingAltText)" -ForegroundColor Yellow }
    if ($summary.MissingOpenGraph -gt 0) { Write-Host "• Missing Open Graph metadata: $($summary.MissingOpenGraph)" -ForegroundColor Yellow }
    if ($summary.MissingTwitterCard -gt 0) { Write-Host "• Missing Twitter Card metadata: $($summary.MissingTwitterCard)" -ForegroundColor Yellow }
    if ($summary.TooFewKeywords -gt 0) { Write-Host "• Too few keywords: $($summary.TooFewKeywords)" -ForegroundColor Yellow }
    if ($summary.TooManyKeywords -gt 0) { Write-Host "• Too many keywords: $($summary.TooManyKeywords)" -ForegroundColor Yellow }

    Write-Host ""
    Write-Host "📝 DETAILED ISSUES:" -ForegroundColor Cyan

    $issuesFound | Sort-Object IssueCount -Descending | ForEach-Object {
        Write-Host ""
        Write-Host "❌ $($_.File) ($($_.IssueCount) issues):" -ForegroundColor Red
        Write-Host "   📄 Source PUG: $($_.PugFile)" -ForegroundColor Cyan
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
Write-Host "• Meta descriptions should be 150-160 characters (max 320)" -ForegroundColor White
Write-Host "• Open Graph descriptions should be 200-300 characters (max 300)" -ForegroundColor White
Write-Host "• Twitter Card descriptions should be 200 characters (max 200)" -ForegroundColor White
Write-Host "• Keywords should be 3-8 relevant terms" -ForegroundColor White
Write-Host "• Each page should have exactly one H1 tag" -ForegroundColor White
Write-Host "• All images should have descriptive alt text" -ForegroundColor White
Write-Host "• Every page should have a canonical URL" -ForegroundColor White
Write-Host "• Include Open Graph and Twitter Card metadata for social sharing" -ForegroundColor White
