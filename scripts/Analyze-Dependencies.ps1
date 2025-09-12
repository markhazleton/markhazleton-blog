#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Analyzes .NET dependencies in the BSW.EHR solution for security vulnerabilities and compliance

.DESCRIPTION
    This script scans all .NET projects in the BSW.EHR solution to:
    - Identify outdated NuGet packages
    - Check for known security vulnerabilities
    - Analyze dependency compliance for healthcare environments
    - Generate recommendations for package updates
    - Assess licensing compliance

.PARAMETER SolutionPath
    Path to the solution file or directory to analyze (default: current directory)

.PARAMETER OutputPath
    Path where the report will be saved (default: current directory)

.PARAMETER Format
    Output format: HTML, JSON, or Console (default: HTML)

.PARAMETER CheckVulnerabilities
    Check for known security vulnerabilities (requires internet connection)

.EXAMPLE
    .\Analyze-Dependencies.ps1
    .\Analyze-Dependencies.ps1 -SolutionPath ".\BSW.EHR.sln" -Format JSON
    .\Analyze-Dependencies.ps1 -CheckVulnerabilities $true

.NOTES
    Author: BSW.EHR Development Team
    Version: 1.0
    Requires: PowerShell 5.1+, .NET CLI
    Focus: Healthcare compliance and security assessment
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$SolutionPath = ".",
    
    [Parameter()]
    [string]$OutputPath = ".",
    
    [Parameter()]
    [ValidateSet("HTML", "JSON", "Console")]
    [string]$Format = "HTML",
    
    [Parameter()]
    [bool]$CheckVulnerabilities = $true
)

# Healthcare-specific package recommendations
$HealthcarePackageGuidelines = @{
    'Security' = @(
        'Microsoft.AspNetCore.Authentication.JwtBearer',
        'Microsoft.AspNetCore.DataProtection',
        'System.Security.Cryptography.Algorithms',
        'IdentityModel'
    )
    'Logging' = @(
        'Serilog',
        'Serilog.AspNetCore',
        'Serilog.Sinks.File',
        'Microsoft.Extensions.Logging'
    )
    'Validation' = @(
        'FluentValidation',
        'System.ComponentModel.Annotations'
    )
    'HealthcareStandards' = @(
        'HL7.Fhir.R4',
        'HL7.FhirPath'
    )
}

# Initialize analysis results
$analysisResults = @{
    GeneratedAt = Get-Date
    SolutionPath = (Resolve-Path $SolutionPath).Path
    Projects = @{}
    DependencySummary = @{
        TotalPackages = 0
        OutdatedPackages = 0
        VulnerablePackages = 0
        SecurityPackages = 0
    }
    Recommendations = @()
    ComplianceIssues = @()
    VulnerabilityReport = @{}
}

Write-Host "📦 Starting BSW.EHR Dependency Analysis..." -ForegroundColor Cyan
Write-Host "Solution Path: $SolutionPath" -ForegroundColor Yellow

# Find all project files
$projectFiles = @()
if (Test-Path $SolutionPath -PathType Leaf) {
    # If it's a solution file, parse it to find projects
    $solutionContent = Get-Content $SolutionPath
    $projectPattern = 'Project\("\{[^}]+\}"\) = "[^"]+", "([^"]+\.csproj)"'
    $matches = [regex]::Matches($solutionContent -join "`n", $projectPattern)
    foreach ($match in $matches) {
        $projectPath = $match.Groups[1].Value
        if (-not [System.IO.Path]::IsPathRooted($projectPath)) {
            $projectPath = Join-Path (Split-Path $SolutionPath) $projectPath
        }
        $projectFiles += $projectPath
    }
} else {
    # Search for project files in directory
    $projectFiles = Get-ChildItem -Path $SolutionPath -Recurse -Filter "*.csproj" | ForEach-Object { $_.FullName }
}

Write-Host "Found $($projectFiles.Count) project files" -ForegroundColor Green

# Analyze each project
foreach ($projectFile in $projectFiles) {
    if (-not (Test-Path $projectFile)) {
        Write-Warning "Project file not found: $projectFile"
        continue
    }
    
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectFile)
    Write-Host "Analyzing project: $projectName" -ForegroundColor Cyan
    
    $projectAnalysis = @{
        Name = $projectName
        Path = $projectFile
        Packages = @{}
        Framework = ""
        PackageCount = 0
        OutdatedCount = 0
        SecurityPackages = @()
        Issues = @()
    }
    
    # Parse project file to get target framework and packages
    try {
        [xml]$projectXml = Get-Content $projectFile
        
        # Get target framework
        $targetFramework = $projectXml.Project.PropertyGroup.TargetFramework
        if ($targetFramework) {
            $projectAnalysis.Framework = $targetFramework
        }
        
        # Get package references
        $packageReferences = $projectXml.Project.ItemGroup.PackageReference
        if ($packageReferences) {
            foreach ($package in $packageReferences) {
                $packageName = $package.Include
                $version = $package.Version
                
                if ($packageName -and $version) {
                    $projectAnalysis.Packages[$packageName] = @{
                        Version = $version
                        IsSecurityRelated = $false
                        IsOutdated = $false
                        Category = "Unknown"
                        Vulnerabilities = @()
                    }
                    
                    # Check if it's a security-related package
                    foreach ($category in $HealthcarePackageGuidelines.Keys) {
                        if ($HealthcarePackageGuidelines[$category] -contains $packageName) {
                            $projectAnalysis.Packages[$packageName].IsSecurityRelated = $true
                            $projectAnalysis.Packages[$packageName].Category = $category
                            $projectAnalysis.SecurityPackages += $packageName
                            break
                        }
                    }
                    
                    $projectAnalysis.PackageCount++
                }
            }
        }
    }
    catch {
        Write-Warning "Could not parse project file: $projectFile - $_"
        $projectAnalysis.Issues += "Failed to parse project file: $_"
    }
    
    # Check for outdated packages using dotnet CLI
    if ($projectAnalysis.PackageCount -gt 0) {
        try {
            $outdatedOutput = & dotnet list $projectFile package --outdated --format json 2>$null
            if ($LASTEXITCODE -eq 0 -and $outdatedOutput) {
                $outdatedJson = $outdatedOutput | ConvertFrom-Json
                if ($outdatedJson.projects) {
                    foreach ($project in $outdatedJson.projects) {
                        if ($project.frameworks) {
                            foreach ($framework in $project.frameworks) {
                                if ($framework.topLevelPackages) {
                                    foreach ($package in $framework.topLevelPackages) {
                                        if ($projectAnalysis.Packages[$package.id]) {
                                            $projectAnalysis.Packages[$package.id].IsOutdated = $true
                                            $projectAnalysis.Packages[$package.id].LatestVersion = $package.latestVersion
                                            $projectAnalysis.OutdatedCount++
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch {
            Write-Warning "Could not check for outdated packages in $projectName"
            $projectAnalysis.Issues += "Failed to check outdated packages: $_"
        }
    }
    
    # Check for vulnerabilities if enabled
    if ($CheckVulnerabilities -and $projectAnalysis.PackageCount -gt 0) {
        try {
            $auditOutput = & dotnet list $projectFile package --vulnerable --format json 2>$null
            if ($LASTEXITCODE -eq 0 -and $auditOutput) {
                $auditJson = $auditOutput | ConvertFrom-Json
                if ($auditJson.projects) {
                    foreach ($project in $auditJson.projects) {
                        if ($project.frameworks) {
                            foreach ($framework in $project.frameworks) {
                                if ($framework.vulnerablePackages) {
                                    foreach ($vulnPackage in $framework.vulnerablePackages) {
                                        if ($projectAnalysis.Packages[$vulnPackage.id]) {
                                            $vulnerabilities = @()
                                            foreach ($vuln in $vulnPackage.vulnerabilities) {
                                                $vulnerabilities += @{
                                                    Id = $vuln.advisoryUrl
                                                    Severity = $vuln.severity
                                                    Description = $vuln.title
                                                }
                                            }
                                            $projectAnalysis.Packages[$vulnPackage.id].Vulnerabilities = $vulnerabilities
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch {
            Write-Warning "Could not check for vulnerabilities in $projectName"
            $projectAnalysis.Issues += "Failed to check vulnerabilities: $_"
        }
    }
    
    $analysisResults.Projects[$projectName] = $projectAnalysis
}

# Calculate summary statistics
foreach ($project in $analysisResults.Projects.Values) {
    $analysisResults.DependencySummary.TotalPackages += $project.PackageCount
    $analysisResults.DependencySummary.OutdatedPackages += $project.OutdatedCount
    $analysisResults.DependencySummary.SecurityPackages += $project.SecurityPackages.Count
    
    foreach ($package in $project.Packages.Values) {
        if ($package.Vulnerabilities.Count -gt 0) {
            $analysisResults.DependencySummary.VulnerablePackages++
        }
    }
}

# Generate recommendations
function Get-DependencyRecommendations {
    param([hashtable]$Results)
    
    $recommendations = @()
    
    # Outdated packages
    if ($Results.DependencySummary.OutdatedPackages -gt 0) {
        $recommendations += "📦 UPDATE: $($Results.DependencySummary.OutdatedPackages) packages need updating. Prioritize security-related packages."
    }
    
    # Vulnerable packages
    if ($Results.DependencySummary.VulnerablePackages -gt 0) {
        $recommendations += "🔴 CRITICAL: $($Results.DependencySummary.VulnerablePackages) packages have known vulnerabilities. Update immediately."
    }
    
    # Healthcare-specific recommendations
    $recommendations += "🏥 HEALTHCARE: Ensure all packages handling PHI are up-to-date and compliant"
    $recommendations += "🏥 HEALTHCARE: Consider using packages specifically designed for FHIR/HL7 standards"
    
    # Security recommendations
    $recommendations += "🔒 SECURITY: Implement dependency scanning in CI/CD pipeline"
    $recommendations += "🔒 SECURITY: Regularly audit dependencies for compliance with healthcare regulations"
    
    # .NET specific recommendations
    $recommendations += "⚙️ .NET: Keep .NET runtime and SDK versions current for security patches"
    $recommendations += "⚙️ .NET: Consider Long Term Support (LTS) versions for production healthcare applications"
    
    return $recommendations
}

$analysisResults.Recommendations = Get-DependencyRecommendations -Results $analysisResults

# Generate HTML report
function Generate-DependencyHtmlReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "bsw-ehr-dependency-analysis-$timestamp.html"
    $filePath = Join-Path $OutputPath $fileName
    
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BSW.EHR Dependency Analysis Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, #2980b9 0%, #3498db 100%);
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
            background: linear-gradient(135deg, #2980b9 0%, #3498db 100%); 
            color: white; 
            padding: 30px; 
            text-align: center; 
        }
        .header h1 { font-size: 2.5em; margin-bottom: 10px; }
        .header p { font-size: 1.2em; opacity: 0.9; }
        .content { padding: 30px; }
        .summary-grid { 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); 
            gap: 20px; 
            margin-bottom: 30px; 
        }
        .summary-card { 
            background: #f8f9ff; 
            border: 1px solid #e1e5fe; 
            border-radius: 10px; 
            padding: 20px; 
            text-align: center;
            transition: transform 0.3s ease;
        }
        .summary-card:hover { transform: translateY(-5px); }
        .summary-value { font-size: 2em; font-weight: bold; margin-bottom: 5px; }
        .summary-label { color: #666; font-size: 0.9em; text-transform: uppercase; letter-spacing: 1px; }
        .critical { color: #dc3545; }
        .warning { color: #ffc107; }
        .info { color: #17a2b8; }
        .good { color: #28a745; }
        .section { margin-bottom: 40px; }
        .section h2 { 
            color: #333; 
            border-bottom: 3px solid #2980b9; 
            padding-bottom: 10px; 
            margin-bottom: 20px; 
            font-size: 1.8em;
        }
        .project-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 20px; }
        .project-card { 
            background: #ffffff; 
            border: 1px solid #e0e0e0; 
            border-radius: 8px; 
            padding: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
        }
        .project-title { font-weight: bold; font-size: 1.2em; margin-bottom: 10px; color: #2980b9; }
        .project-stats { font-size: 0.9em; color: #666; margin-bottom: 15px; }
        .package-list { max-height: 200px; overflow-y: auto; }
        .package-item { 
            display: flex; 
            justify-content: space-between; 
            padding: 5px 0; 
            border-bottom: 1px solid #f0f0f0; 
            font-size: 0.85em;
        }
        .package-name { font-weight: 500; }
        .package-version { color: #666; }
        .outdated { color: #ffc107; }
        .vulnerable { color: #dc3545; font-weight: bold; }
        .security-related { color: #28a745; }
        .recommendations { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; }
        .recommendations ul { margin-left: 20px; }
        .recommendations li { margin-bottom: 8px; }
        .healthcare-highlight { background: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; padding: 15px; margin-bottom: 20px; }
        .footer { 
            text-align: center; 
            padding: 20px; 
            background: #f8f9fa; 
            color: #666; 
            border-top: 1px solid #eee;
        }
        table { width: 100%; border-collapse: collapse; margin-top: 15px; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f8f9ff; font-weight: 600; }
        tr:hover { background: #f5f5f5; }
        .legend { 
            display: flex; 
            gap: 15px; 
            margin-bottom: 20px; 
            font-size: 0.9em;
            justify-content: center;
        }
        .legend-item { display: flex; align-items: center; gap: 5px; }
        .legend-color { width: 12px; height: 12px; border-radius: 2px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📦 BSW.EHR Dependency Analysis</h1>
            <p>Healthcare Application Dependency & Security Assessment</p>
            <p>Generated on: $($Results.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss"))</p>
        </div>
        
        <div class="content">
            <!-- Healthcare Compliance Notice -->
            <div class="healthcare-highlight">
                <h3>🏥 Healthcare Compliance Notice</h3>
                <p>This dependency analysis focuses on security and compliance requirements for healthcare applications. 
                Critical vulnerabilities and outdated security packages should be addressed immediately to maintain HIPAA compliance.</p>
            </div>

            <!-- Dependency Summary -->
            <div class="section">
                <h2>📊 Dependency Summary</h2>
                <div class="summary-grid">
                    <div class="summary-card">
                        <div class="summary-value">$($Results.DependencySummary.TotalPackages)</div>
                        <div class="summary-label">Total Packages</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value outdated">$($Results.DependencySummary.OutdatedPackages)</div>
                        <div class="summary-label">Outdated</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value critical">$($Results.DependencySummary.VulnerablePackages)</div>
                        <div class="summary-label">Vulnerable</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value good">$($Results.DependencySummary.SecurityPackages)</div>
                        <div class="summary-label">Security Packages</div>
                    </div>
                </div>
            </div>

            <!-- Legend -->
            <div class="legend">
                <div class="legend-item">
                    <div class="legend-color" style="background: #28a745;"></div>
                    <span>Security-Related</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color" style="background: #ffc107;"></div>
                    <span>Outdated</span>
                </div>
                <div class="legend-item">
                    <div class="legend-color" style="background: #dc3545;"></div>
                    <span>Vulnerable</span>
                </div>
            </div>

            <!-- Projects Analysis -->
            <div class="section">
                <h2>🏗️ Project Dependencies</h2>
                <div class="project-grid">
"@

    foreach ($projectName in $Results.Projects.Keys | Sort-Object) {
        $project = $Results.Projects[$projectName]
        $html += @"
                    <div class="project-card">
                        <div class="project-title">$projectName</div>
                        <div class="project-stats">
                            Framework: <strong>$($project.Framework)</strong><br>
                            Packages: <strong>$($project.PackageCount)</strong> | 
                            Outdated: <span class="outdated">$($project.OutdatedCount)</span> | 
                            Security: <span class="security-related">$($project.SecurityPackages.Count)</span>
                        </div>
                        <div class="package-list">
"@
        
        foreach ($packageName in $project.Packages.Keys | Sort-Object) {
            $package = $project.Packages[$packageName]
            $cssClass = ""
            $indicator = ""
            
            if ($package.Vulnerabilities.Count -gt 0) {
                $cssClass = "vulnerable"
                $indicator = "🔴"
            } elseif ($package.IsOutdated) {
                $cssClass = "outdated"
                $indicator = "🟡"
            } elseif ($package.IsSecurityRelated) {
                $cssClass = "security-related"
                $indicator = "🔒"
            }
            
            $html += @"
                            <div class="package-item">
                                <span class="package-name $cssClass">$indicator $packageName</span>
                                <span class="package-version">$($package.Version)</span>
                            </div>
"@
        }
        
        $html += @"
                        </div>
                    </div>
"@
    }

    $html += @"
                </div>
            </div>

            <!-- Recommendations -->
            <div class="section">
                <h2>💡 Dependency Recommendations</h2>
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

            <!-- Detailed Package Analysis -->
            <div class="section">
                <h2>📋 Package Details</h2>
                <table>
                    <tr><th>Package</th><th>Project</th><th>Version</th><th>Status</th><th>Category</th></tr>
"@

    foreach ($projectName in $Results.Projects.Keys | Sort-Object) {
        $project = $Results.Projects[$projectName]
        foreach ($packageName in $project.Packages.Keys | Sort-Object) {
            $package = $project.Packages[$packageName]
            $status = "Current"
            $statusClass = "good"
            
            if ($package.Vulnerabilities.Count -gt 0) {
                $status = "Vulnerable"
                $statusClass = "critical"
            } elseif ($package.IsOutdated) {
                $status = "Outdated"
                $statusClass = "warning"
            }
            
            $html += @"
                    <tr>
                        <td>$packageName</td>
                        <td>$projectName</td>
                        <td>$($package.Version)</td>
                        <td class="$statusClass">$status</td>
                        <td>$($package.Category)</td>
                    </tr>
"@
        }
    }

    $html += @"
                </table>
            </div>
        </div>
        
        <div class="footer">
            <p>Generated by BSW.EHR Dependency Analyzer | PowerShell Script v1.0</p>
            <p>Healthcare Application Security & Compliance Assessment</p>
        </div>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Generate JSON report
function Generate-DependencyJsonReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "bsw-ehr-dependency-analysis-$timestamp.json"
    $filePath = Join-Path $OutputPath $fileName
    
    $Results | ConvertTo-Json -Depth 10 | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Generate console report
function Show-DependencyConsoleReport {
    param([hashtable]$Results)
    
    Write-Host "`n" + "="*80 -ForegroundColor Blue
    Write-Host "  📦 BSW.EHR DEPENDENCY ANALYSIS REPORT" -ForegroundColor Blue
    Write-Host "="*80 -ForegroundColor Blue
    
    Write-Host "`n🏥 HEALTHCARE COMPLIANCE FOCUS" -ForegroundColor Yellow
    Write-Host "Analyzing dependencies for healthcare security and HIPAA compliance" -ForegroundColor Cyan
    
    Write-Host "`n📊 DEPENDENCY SUMMARY" -ForegroundColor Yellow
    Write-Host "  Total Packages: $($Results.DependencySummary.TotalPackages)"
    Write-Host "  Outdated: " -NoNewline
    Write-Host $Results.DependencySummary.OutdatedPackages -ForegroundColor Yellow
    Write-Host "  Vulnerable: " -NoNewline
    Write-Host $Results.DependencySummary.VulnerablePackages -ForegroundColor Red
    Write-Host "  Security-Related: " -NoNewline
    Write-Host $Results.DependencySummary.SecurityPackages -ForegroundColor Green
    
    if ($Results.DependencySummary.VulnerablePackages -gt 0) {
        Write-Host "`n🔴 CRITICAL: Vulnerable packages detected!" -ForegroundColor Red
    }
    
    Write-Host "`n🏗️ PROJECT BREAKDOWN" -ForegroundColor Yellow
    foreach ($projectName in $Results.Projects.Keys | Sort-Object) {
        $project = $Results.Projects[$projectName]
        Write-Host "  $projectName ($($project.Framework)): $($project.PackageCount) packages, $($project.OutdatedCount) outdated" -ForegroundColor Cyan
    }
    
    if ($Results.Recommendations.Count -gt 0) {
        Write-Host "`n💡 KEY RECOMMENDATIONS" -ForegroundColor Yellow
        foreach ($recommendation in $Results.Recommendations | Select-Object -First 5) {
            Write-Host "  • $recommendation" -ForegroundColor Green
        }
    }
    
    Write-Host "`n" + "="*80 -ForegroundColor Blue
}

# Main execution
try {
    # Generate output
    switch ($Format) {
        "HTML" {
            $outputFile = Generate-DependencyHtmlReport -Results $analysisResults -OutputPath $OutputPath
            Write-Host "✅ Dependency analysis report generated: $outputFile" -ForegroundColor Green
            
            # Open the report in default browser
            if (Get-Command "Start-Process" -ErrorAction SilentlyContinue) {
                Start-Process $outputFile
            }
        }
        "JSON" {
            $outputFile = Generate-DependencyJsonReport -Results $analysisResults -OutputPath $OutputPath
            Write-Host "✅ Dependency analysis JSON generated: $outputFile" -ForegroundColor Green
        }
        "Console" {
            Show-DependencyConsoleReport -Results $analysisResults
        }
    }
    
    # Dependency summary
    if ($analysisResults.DependencySummary.VulnerablePackages -gt 0) {
        Write-Host "`n⚠️  DEPENDENCY ALERT: $($analysisResults.DependencySummary.VulnerablePackages) vulnerable packages found!" -ForegroundColor Red
    } elseif ($analysisResults.DependencySummary.OutdatedPackages -gt 0) {
        Write-Host "`n⚠️  Outdated packages detected: $($analysisResults.DependencySummary.OutdatedPackages) packages need updating" -ForegroundColor Yellow
    } else {
        Write-Host "`n✅ All dependencies are current and secure" -ForegroundColor Green
    }
    
    Write-Host "`n🎉 Dependency analysis complete!" -ForegroundColor Green
}
catch {
    Write-Error "❌ Error during dependency analysis: $_"
    exit 1
}
