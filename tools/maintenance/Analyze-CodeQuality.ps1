#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Analyzes code quality metrics for the BSW.EHR healthcare application

.DESCRIPTION
    This script performs comprehensive code quality analysis including:
    - Code metrics (complexity, maintainability)
    - Coding standards compliance
    - Architecture patterns analysis
    - Technical debt assessment
    - Healthcare-specific quality checks

.PARAMETER ScanPath
    Path to scan (default: current directory)

.PARAMETER OutputPath
    Path where the report will be saved (default: current directory)

.PARAMETER Format
    Output format: HTML, JSON, or Console (default: HTML)

.PARAMETER IncludeMetrics
    Include detailed code metrics in the report (default: true)

.EXAMPLE
    .\Analyze-CodeQuality.ps1
    .\Analyze-CodeQuality.ps1 -ScanPath ".\BSW.EHR.Api" -Format JSON

.NOTES
    Author: BSW.EHR Development Team
    Version: 1.0
    Requires: PowerShell 5.1+
    Focus: Healthcare application quality standards
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$ScanPath = ".",
    
    [Parameter()]
    [string]$OutputPath = ".",
    
    [Parameter()]
    [ValidateSet("HTML", "JSON", "Console")]
    [string]$Format = "HTML",
    
    [Parameter()]
    [bool]$IncludeMetrics = $true
)

Write-Host "📊 Starting BSW.EHR Code Quality Analysis..." -ForegroundColor Cyan

# Initialize results structure
$qualityResults = @{
    GeneratedAt = Get-Date
    ScanPath = (Resolve-Path $ScanPath).Path
    Summary = @{
        TotalFiles = 0
        TotalLines = 0
        AverageComplexity = 0
        QualityScore = 0
    }
    Files = @{}
    Issues = @()
    Recommendations = @()
}

# Get source files
$sourceFiles = Get-ChildItem -Path $ScanPath -Recurse -Include "*.cs" | 
               Where-Object { $_.FullName -notmatch '(\\bin\\|\\obj\\|\\packages\\)' }

$qualityResults.Summary.TotalFiles = $sourceFiles.Count
Write-Host "Analyzing $($sourceFiles.Count) C# files..." -ForegroundColor Green

# Analyze each file
foreach ($file in $sourceFiles) {
    $relativePath = $file.FullName.Replace($qualityResults.ScanPath, "").TrimStart('\')
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    
    if (-not $content) { continue }
    
    $lines = $content -split "`n"
    $qualityResults.Summary.TotalLines += $lines.Count
    
    # Analyze file metrics
    $fileAnalysis = @{
        Path = $relativePath
        Lines = $lines.Count
        Methods = 0
        Classes = 0
        Complexity = 0
        Issues = @()
        QualityScore = 0
    }
    
    # Count classes and methods
    $fileAnalysis.Classes = ([regex]::Matches($content, '\bclass\s+\w+', [Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
    $fileAnalysis.Methods = ([regex]::Matches($content, '\b(public|private|protected|internal)\s+.*\s+\w+\s*\(', [Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
    
    # Calculate basic complexity (simplified McCabe)
    $complexityIndicators = @(
        'if\s*\(',
        'while\s*\(',
        'for\s*\(',
        'foreach\s*\(',
        'switch\s*\(',
        'case\s+',
        'catch\s*\(',
        '\?\s*[^:]*:',  # ternary
        '&&',
        '\|\|'
    )
    
    foreach ($indicator in $complexityIndicators) {
        $fileAnalysis.Complexity += ([regex]::Matches($content, $indicator, [Text.RegularExpressions.RegexOptions]::IgnoreCase)).Count
    }
    
    # Check for quality issues
    $qualityChecks = @{
        'Large Methods' = @{
            Pattern = '(\{[^{}]*){50,}'
            Severity = 'Warning'
            Message = 'Large method detected - consider refactoring'
        }
        'Long Lines' = @{
            Pattern = '.{120,}'
            Severity = 'Info'
            Message = 'Line length exceeds 120 characters'
        }
        'Magic Numbers' = @{
            Pattern = '\b\d{2,}\b(?!\s*(ms|seconds?|minutes?|hours?|days?))'
            Severity = 'Warning'
            Message = 'Magic number detected - consider using named constants'
        }
        'TODO Comments' = @{
            Pattern = '(TODO|FIXME|HACK|XXX)'
            Severity = 'Info'
            Message = 'Technical debt marker found'
        }
        'Exception Swallowing' = @{
            Pattern = 'catch\s*\([^)]*\)\s*\{\s*\}'
            Severity = 'Warning'
            Message = 'Empty catch block - exceptions being swallowed'
        }
        'SQL Injection Risk' = @{
            Pattern = '(string\.Format|StringBuilder.*Append).*SELECT.*FROM'
            Severity = 'Critical'
            Message = 'Potential SQL injection vulnerability'
        }
        'Missing Async Pattern' = @{
            Pattern = '\.Result\b|\.Wait\(\)'
            Severity = 'Warning'
            Message = 'Blocking async call - potential deadlock risk'
        }
    }
    
    foreach ($checkName in $qualityChecks.Keys) {
        $check = $qualityChecks[$checkName]
        $issueMatches = [regex]::Matches($content, $check.Pattern, [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        if ($issueMatches.Count -gt 0) {
            $fileAnalysis.Issues += @{
                Type = $checkName
                Severity = $check.Severity
                Message = $check.Message
                Count = $issueMatches.Count
            }
        }
    }
    
    # Calculate quality score (0-100)
    $baseScore = 100
    foreach ($issue in $fileAnalysis.Issues) {
        switch ($issue.Severity) {
            'Critical' { $baseScore -= ($issue.Count * 20) }
            'Warning' { $baseScore -= ($issue.Count * 10) }
            'Info' { $baseScore -= ($issue.Count * 2) }
        }
    }
    
    # Complexity penalty
    if ($fileAnalysis.Lines -gt 0) {
        $complexityRatio = $fileAnalysis.Complexity / $fileAnalysis.Lines
        if ($complexityRatio -gt 0.1) {
            $baseScore -= 20
        }
    }
    
    $fileAnalysis.QualityScore = [Math]::Max(0, $baseScore)
    $qualityResults.Files[$relativePath] = $fileAnalysis
}

# Calculate overall metrics
if ($qualityResults.Files.Count -gt 0) {
    $totalComplexity = ($qualityResults.Files.Values | Measure-Object -Property Complexity -Sum).Sum
    $qualityResults.Summary.AverageComplexity = [Math]::Round($totalComplexity / $qualityResults.Files.Count, 2)
    
    $averageQuality = ($qualityResults.Files.Values | Measure-Object -Property QualityScore -Average).Average
    $qualityResults.Summary.QualityScore = [Math]::Round($averageQuality, 1)
}

# Generate recommendations
$recommendations = @()

if ($qualityResults.Summary.QualityScore -lt 70) {
    $recommendations += "🔴 CRITICAL: Overall code quality is below acceptable threshold (70%)"
}

$criticalIssues = $qualityResults.Files.Values | ForEach-Object { $_.Issues | Where-Object { $_.Severity -eq 'Critical' } }
if ($criticalIssues.Count -gt 0) {
    $recommendations += "🔴 CRITICAL: Address all critical security and reliability issues immediately"
}

if ($qualityResults.Summary.AverageComplexity -gt 15) {
    $recommendations += "⚠️  HIGH COMPLEXITY: Consider refactoring complex methods and classes"
}

$recommendations += "📋 HEALTHCARE: Implement comprehensive unit testing for patient data handling"
$recommendations += "📋 HEALTHCARE: Ensure all clinical algorithms are thoroughly documented"
$recommendations += "📋 HEALTHCARE: Review error handling patterns for patient safety"

$qualityResults.Recommendations = $recommendations

# Output generation functions
function Write-QualityHtmlReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "bsw-ehr-code-quality-$timestamp.html"
    $filePath = Join-Path $OutputPath $fileName
    
    $qualityColor = if ($Results.Summary.QualityScore -ge 80) { "#28a745" } 
                   elseif ($Results.Summary.QualityScore -ge 70) { "#ffc107" }
                   else { "#dc3545" }
    
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BSW.EHR Code Quality Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, #8e44ad 0%, #9b59b6 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .container { 
            max-width: 1200px; 
            margin: 0 auto; 
            background: white; 
            border-radius: 15px; 
            box-shadow: 0 20px 40px rgba(0,0,0,0.2);
            overflow: hidden;
        }
        .header { 
            background: linear-gradient(135deg, #8e44ad 0%, #9b59b6 100%); 
            color: white; 
            padding: 30px; 
            text-align: center; 
        }
        .content { padding: 30px; }
        .metrics-grid { 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); 
            gap: 20px; 
            margin-bottom: 30px; 
        }
        .metric-card { 
            background: #f8f9ff; 
            border: 1px solid #e1e5fe; 
            border-radius: 10px; 
            padding: 20px; 
            text-align: center;
        }
        .metric-value { font-size: 2em; font-weight: bold; margin-bottom: 5px; }
        .metric-label { color: #666; font-size: 0.9em; }
        .section { margin-bottom: 30px; }
        .section h2 { 
            color: #333; 
            border-bottom: 3px solid #8e44ad; 
            padding-bottom: 10px; 
            margin-bottom: 20px; 
        }
        .quality-score { color: $qualityColor; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f8f9ff; }
        .critical { color: #dc3545; font-weight: bold; }
        .warning { color: #ffc107; }
        .info { color: #17a2b8; }
        .recommendations { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📊 BSW.EHR Code Quality Report</h1>
            <p>Healthcare Application Quality Assessment</p>
        </div>
        
        <div class="content">
            <div class="section">
                <h2>📈 Quality Metrics</h2>
                <div class="metrics-grid">
                    <div class="metric-card">
                        <div class="metric-value quality-score">$($Results.Summary.QualityScore)%</div>
                        <div class="metric-label">Overall Quality Score</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">$($Results.Summary.TotalFiles)</div>
                        <div class="metric-label">Files Analyzed</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">$($Results.Summary.TotalLines.ToString("N0"))</div>
                        <div class="metric-label">Lines of Code</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">$($Results.Summary.AverageComplexity)</div>
                        <div class="metric-label">Avg Complexity</div>
                    </div>
                </div>
            </div>

            <div class="section">
                <h2>💡 Recommendations</h2>
                <div class="recommendations">
                    <ul>
"@

    foreach ($recommendation in $Results.Recommendations) {
        $html += "<li>$recommendation</li>"
    }

    $html += @"
                    </ul>
                </div>
            </div>

            <div class="section">
                <h2>📁 File Quality Analysis</h2>
                <table>
                    <tr><th>File</th><th>Lines</th><th>Complexity</th><th>Quality Score</th><th>Issues</th></tr>
"@

    foreach ($filePath in $Results.Files.Keys | Sort-Object) {
        $file = $Results.Files[$filePath]
        $criticalCount = ($file.Issues | Where-Object { $_.Severity -eq 'Critical' }).Count
        $warningCount = ($file.Issues | Where-Object { $_.Severity -eq 'Warning' }).Count
        $infoCount = ($file.Issues | Where-Object { $_.Severity -eq 'Info' }).Count
        
        $issuesText = ""
        if ($criticalCount -gt 0) { $issuesText += "<span class='critical'>C:$criticalCount</span> " }
        if ($warningCount -gt 0) { $issuesText += "<span class='warning'>W:$warningCount</span> " }
        if ($infoCount -gt 0) { $issuesText += "<span class='info'>I:$infoCount</span>" }
        
        $scoreClass = if ($file.QualityScore -ge 80) { "quality-score" } else { "warning" }
        
        $html += @"
                    <tr>
                        <td>$filePath</td>
                        <td>$($file.Lines)</td>
                        <td>$($file.Complexity)</td>
                        <td class="$scoreClass">$($file.QualityScore)%</td>
                        <td>$issuesText</td>
                    </tr>
"@
    }

    $html += @"
                </table>
            </div>
        </div>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

function Write-QualityConsoleReport {
    param([hashtable]$Results)
    
    Write-Host "`n" + "="*80 -ForegroundColor Magenta
    Write-Host "  📊 BSW.EHR CODE QUALITY REPORT" -ForegroundColor Magenta
    Write-Host "="*80 -ForegroundColor Magenta
    
    $scoreColor = if ($Results.Summary.QualityScore -ge 80) { "Green" } 
                 elseif ($Results.Summary.QualityScore -ge 70) { "Yellow" }
                 else { "Red" }
    
    Write-Host "`n📈 QUALITY METRICS" -ForegroundColor Yellow
    Write-Host "  Overall Score: " -NoNewline
    Write-Host "$($Results.Summary.QualityScore)%" -ForegroundColor $scoreColor
    Write-Host "  Files Analyzed: $($Results.Summary.TotalFiles)"
    Write-Host "  Lines of Code: $($Results.Summary.TotalLines.ToString("N0"))"
    Write-Host "  Average Complexity: $($Results.Summary.AverageComplexity)"
    
    if ($Results.Recommendations.Count -gt 0) {
        Write-Host "`n💡 KEY RECOMMENDATIONS" -ForegroundColor Yellow
        foreach ($rec in $Results.Recommendations | Select-Object -First 3) {
            Write-Host "  • $rec" -ForegroundColor Green
        }
    }
    
    Write-Host "`n" + "="*80 -ForegroundColor Magenta
}

# Generate output
try {
    switch ($Format) {
        "HTML" {
            $outputFile = Write-QualityHtmlReport -Results $qualityResults -OutputPath $OutputPath
            Write-Host "✅ Code quality report generated: $outputFile" -ForegroundColor Green
            
            if (Get-Command "Start-Process" -ErrorAction SilentlyContinue) {
                Start-Process $outputFile
            }
        }
        "JSON" {
            $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
            $fileName = "bsw-ehr-code-quality-$timestamp.json"
            $filePath = Join-Path $OutputPath $fileName
            $qualityResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $filePath -Encoding UTF8
            Write-Host "✅ Code quality JSON generated: $filePath" -ForegroundColor Green
        }
        "Console" {
            Write-QualityConsoleReport -Results $qualityResults
        }
    }
    
    Write-Host "`n🎉 Code quality analysis complete!" -ForegroundColor Green
    Write-Host "Quality Score: $($qualityResults.Summary.QualityScore)% | Files: $($qualityResults.Summary.TotalFiles)" -ForegroundColor Cyan
}
catch {
    Write-Error "❌ Error during code quality analysis: $_"
    exit 1
}
