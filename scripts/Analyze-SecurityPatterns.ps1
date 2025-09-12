#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Analyzes the BSW.EHR codebase for security patterns and HIPAA compliance considerations

.DESCRIPTION
    This script scans the BSW.EHR repository for common security patterns, vulnerabilities,
    and HIPAA compliance considerations relevant to healthcare applications including:
    - Data encryption patterns
    - Authentication and authorization usage
    - Logging and audit trail patterns
    - Input validation and sanitization
    - PHI (Protected Health Information) handling
    - Database security patterns
    - API security implementation

.PARAMETER ScanPath
    Path to scan (default: current directory)

.PARAMETER OutputPath
    Path where the report will be saved (default: current directory)

.PARAMETER Format
    Output format: HTML, JSON, or Console (default: HTML)

.PARAMETER IncludeRemediation
    Include remediation suggestions in the report (default: true)

.EXAMPLE
    .\Analyze-SecurityPatterns.ps1
    .\Analyze-SecurityPatterns.ps1 -ScanPath ".\BSW.EHR.Api" -Format JSON
    .\Analyze-SecurityPatterns.ps1 -OutputPath ".\security-reports" -IncludeRemediation $true

.NOTES
    Author: BSW.EHR Development Team
    Version: 1.0
    Requires: PowerShell 5.1+
    Focus: Healthcare HIPAA compliance and security best practices
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
    [bool]$IncludeRemediation = $true
)

# Security pattern definitions for healthcare applications
$SecurityPatterns = @{
    # Encryption patterns
    'Encryption' = @{
        'AES_Usage' = @{
            Pattern = '(AES|Aes)\.(Create|CreateEncryptor|CreateDecryptor)'
            Description = 'AES encryption usage'
            Severity = 'Info'
            Category = 'Encryption'
        }
        'RSA_Usage' = @{
            Pattern = '(RSA|Rsa)\.(Create|ImportParameters|ExportParameters)'
            Description = 'RSA encryption usage'
            Severity = 'Info'
            Category = 'Encryption'
        }
        'Hashing_Algorithms' = @{
            Pattern = '(SHA256|SHA512|MD5|SHA1)\.(Create|ComputeHash)'
            Description = 'Hashing algorithm usage'
            Severity = 'Warning'
            Category = 'Encryption'
            Note = 'Ensure strong hashing algorithms (SHA256+) for PHI'
        }
    }
    
    # Authentication patterns
    'Authentication' = @{
        'JWT_Token' = @{
            Pattern = '(JwtSecurityToken|jwt|JWT)'
            Description = 'JWT token handling'
            Severity = 'Info'
            Category = 'Authentication'
        }
        'Identity_Framework' = @{
            Pattern = '(UserManager|SignInManager|IdentityUser)'
            Description = 'ASP.NET Identity usage'
            Severity = 'Info'
            Category = 'Authentication'
        }
        'OAuth_Implementation' = @{
            Pattern = '(OAuth|oauth|OpenIdConnect)'
            Description = 'OAuth/OpenID Connect implementation'
            Severity = 'Info'
            Category = 'Authentication'
        }
    }
    
    # Authorization patterns
    'Authorization' = @{
        'Authorize_Attribute' = @{
            Pattern = '\[Authorize'
            Description = 'Authorization attribute usage'
            Severity = 'Info'
            Category = 'Authorization'
        }
        'Role_Based_Auth' = @{
            Pattern = '(Roles\s*=|IsInRole|HasRole)'
            Description = 'Role-based authorization'
            Severity = 'Info'
            Category = 'Authorization'
        }
        'Claims_Based_Auth' = @{
            Pattern = '(Claims|Claim\.|ClaimsPrincipal)'
            Description = 'Claims-based authorization'
            Severity = 'Info'
            Category = 'Authorization'
        }
    }
    
    # Data validation patterns
    'DataValidation' = @{
        'Input_Validation' = @{
            Pattern = '(ModelState\.IsValid|ValidationAttribute|Required\])'
            Description = 'Input validation implementation'
            Severity = 'Info'
            Category = 'Data Validation'
        }
        'SQL_Injection_Protection' = @{
            Pattern = '(SqlParameter|@\w+|parameterized)'
            Description = 'SQL injection protection'
            Severity = 'Info'
            Category = 'Data Validation'
        }
        'XSS_Protection' = @{
            Pattern = '(HtmlEncoder|AntiXss|ValidateAntiForgeryToken)'
            Description = 'XSS protection implementation'
            Severity = 'Info'
            Category = 'Data Validation'
        }
    }
    
    # Logging and auditing
    'AuditLogging' = @{
        'Logging_Framework' = @{
            Pattern = '(ILogger|Log\.|LogInformation|LogWarning|LogError)'
            Description = 'Logging framework usage'
            Severity = 'Info'
            Category = 'Audit & Logging'
        }
        'Audit_Trail' = @{
            Pattern = '(audit|Audit|AuditLog|patient.*access)'
            Description = 'Audit trail implementation'
            Severity = 'Info'
            Category = 'Audit & Logging'
        }
        'Security_Events' = @{
            Pattern = '(login|Login|authentication|failed.*attempt)'
            Description = 'Security event logging'
            Severity = 'Info'
            Category = 'Audit & Logging'
        }
    }
    
    # HIPAA-specific patterns
    'HIPAA_Compliance' = @{
        'PHI_References' = @{
            Pattern = '(PHI|protected.*health|patient.*data|medical.*record)'
            Description = 'Protected Health Information references'
            Severity = 'Warning'
            Category = 'HIPAA Compliance'
            Note = 'Ensure proper encryption and access controls for PHI'
        }
        'Patient_Identifiers' = @{
            Pattern = '(SSN|social.*security|patient.*id|medical.*record.*number)'
            Description = 'Patient identifier handling'
            Severity = 'Critical'
            Category = 'HIPAA Compliance'
            Note = 'Patient identifiers require special protection'
        }
        'Minimum_Necessary' = @{
            Pattern = '(minimum.*necessary|least.*privilege|need.*to.*know)'
            Description = 'Minimum necessary principle implementation'
            Severity = 'Info'
            Category = 'HIPAA Compliance'
        }
    }
    
    # Potential vulnerabilities
    'Vulnerabilities' = @{
        'Hardcoded_Secrets' = @{
            Pattern = '(password\s*=\s*["\'''][^"'\'']+["\''']|connectionString\s*=\s*["\'''][^"'\'']*password[^"'\'']*["\'''])'
            Description = 'Potential hardcoded secrets'
            Severity = 'Critical'
            Category = 'Security Vulnerability'
            Note = 'Use secure configuration management'
        }
        'Weak_Random' = @{
            Pattern = '(Random\(\)|DateTime\.Now\.Ticks)'
            Description = 'Weak random number generation'
            Severity = 'Warning'
            Category = 'Security Vulnerability'
            Note = 'Use cryptographically secure random generators'
        }
        'HTTP_Usage' = @{
            Pattern = '(http://|HttpClient.*http://)'
            Description = 'HTTP (non-encrypted) usage'
            Severity = 'Warning'
            Category = 'Security Vulnerability'
            Note = 'Use HTTPS for all communications'
        }
    }
}

# Initialize analysis results
$analysisResults = @{
    GeneratedAt = Get-Date
    ScanPath = (Resolve-Path $ScanPath).Path
    TotalFilesScanned = 0
    PatternsFound = @{}
    SecuritySummary = @{
        Critical = 0
        Warning = 0
        Info = 0
    }
    FileResults = @{}
    Recommendations = @()
}

Write-Host "🔒 Starting BSW.EHR Security Pattern Analysis..." -ForegroundColor Cyan
Write-Host "Scan Path: $ScanPath" -ForegroundColor Yellow

# Get all source files
$sourceFiles = Get-ChildItem -Path $ScanPath -Recurse -Include "*.cs","*.js","*.ts","*.json","*.config","*.xml" | 
               Where-Object { $_.FullName -notmatch '(\\bin\\|\\obj\\|\\node_modules\\|\\packages\\)' }

$analysisResults.TotalFilesScanned = $sourceFiles.Count
Write-Host "Found $($sourceFiles.Count) files to analyze" -ForegroundColor Green

# Analyze each file
foreach ($file in $sourceFiles) {
    $relativePath = $file.FullName.Replace($analysisResults.ScanPath, "").TrimStart('\')
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    
    if (-not $content) { continue }
    
    $fileResults = @{
        Path = $relativePath
        Findings = @()
    }
    
    # Check each security pattern category
    foreach ($categoryName in $SecurityPatterns.Keys) {
        $category = $SecurityPatterns[$categoryName]
        
        foreach ($patternName in $category.Keys) {
            $pattern = $category[$patternName]
            $matches = [regex]::Matches($content, $pattern.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            
            if ($matches.Count -gt 0) {
                $finding = @{
                    Pattern = $patternName
                    Description = $pattern.Description
                    Severity = $pattern.Severity
                    Category = $pattern.Category
                    Count = $matches.Count
                    Lines = @()
                }
                
                if ($pattern.Note) {
                    $finding.Note = $pattern.Note
                }
                
                # Get line numbers for matches
                $lines = $content -split "`n"
                for ($i = 0; $i -lt $lines.Count; $i++) {
                    if ($lines[$i] -match $pattern.Pattern) {
                        $finding.Lines += ($i + 1)
                    }
                }
                
                $fileResults.Findings += $finding
                
                # Update pattern counts
                $patternKey = "$categoryName.$patternName"
                if (-not $analysisResults.PatternsFound[$patternKey]) {
                    $analysisResults.PatternsFound[$patternKey] = @{
                        Description = $pattern.Description
                        Severity = $pattern.Severity
                        Category = $pattern.Category
                        Count = 0
                        Files = @()
                    }
                }
                
                $analysisResults.PatternsFound[$patternKey].Count += $matches.Count
                $analysisResults.PatternsFound[$patternKey].Files += $relativePath
                
                # Update severity counts
                switch ($pattern.Severity) {
                    'Critical' { $analysisResults.SecuritySummary.Critical++ }
                    'Warning' { $analysisResults.SecuritySummary.Warning++ }
                    'Info' { $analysisResults.SecuritySummary.Info++ }
                }
            }
        }
    }
    
    if ($fileResults.Findings.Count -gt 0) {
        $analysisResults.FileResults[$relativePath] = $fileResults
    }
}

# Generate recommendations
function Get-SecurityRecommendations {
    param([hashtable]$Results)
    
    $recommendations = @()
    
    # Critical findings recommendations
    if ($Results.SecuritySummary.Critical -gt 0) {
        $recommendations += "🔴 CRITICAL: Address all critical security findings immediately, particularly hardcoded secrets and patient identifier exposure"
    }
    
    # HIPAA-specific recommendations
    $phiFindings = $Results.PatternsFound.Keys | Where-Object { $_ -match 'HIPAA' }
    if ($phiFindings.Count -gt 0) {
        $recommendations += "🏥 HIPAA: Implement comprehensive PHI protection measures including encryption at rest and in transit"
        $recommendations += "🏥 HIPAA: Ensure all patient data access is logged and auditable"
        $recommendations += "🏥 HIPAA: Implement role-based access controls following minimum necessary principle"
    }
    
    # Encryption recommendations
    $encryptionFindings = $Results.PatternsFound.Keys | Where-Object { $_ -match 'Encryption' }
    if ($encryptionFindings.Count -gt 0) {
        $recommendations += "🔐 ENCRYPTION: Use AES-256 for all PHI encryption requirements"
        $recommendations += "🔐 ENCRYPTION: Implement proper key management practices"
    }
    
    # Authentication recommendations
    $authFindings = $Results.PatternsFound.Keys | Where-Object { $_ -match 'Authentication' }
    if ($authFindings.Count -gt 0) {
        $recommendations += "🔑 AUTH: Implement multi-factor authentication for all healthcare personnel"
        $recommendations += "🔑 AUTH: Use secure session management with appropriate timeout values"
    }
    
    # General security recommendations
    $recommendations += "📋 GENERAL: Conduct regular security assessments and penetration testing"
    $recommendations += "📋 GENERAL: Implement continuous security monitoring and alerting"
    $recommendations += "📋 GENERAL: Maintain an incident response plan for security breaches"
    
    return $recommendations
}

if ($IncludeRemediation) {
    $analysisResults.Recommendations = Get-SecurityRecommendations -Results $analysisResults
}

# Generate HTML report
function Generate-SecurityHtmlReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "bsw-ehr-security-analysis-$timestamp.html"
    $filePath = Join-Path $OutputPath $fileName
    
    $criticalColor = if ($Results.SecuritySummary.Critical -gt 0) { "#dc3545" } else { "#28a745" }
    $warningColor = if ($Results.SecuritySummary.Warning -gt 0) { "#ffc107" } else { "#28a745" }
    
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BSW.EHR Security Analysis Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%);
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
            background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); 
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
            border-bottom: 3px solid #e74c3c; 
            padding-bottom: 10px; 
            margin-bottom: 20px; 
            font-size: 1.8em;
        }
        .findings-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 15px; }
        .finding-card { 
            background: #ffffff; 
            border: 1px solid #e0e0e0; 
            border-radius: 8px; 
            padding: 15px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
        }
        .finding-title { font-weight: bold; margin-bottom: 5px; }
        .finding-category { 
            display: inline-block; 
            background: #e9ecef; 
            color: #495057; 
            padding: 2px 8px; 
            border-radius: 12px; 
            font-size: 0.8em;
            margin-bottom: 8px;
        }
        .finding-files { font-size: 0.9em; color: #666; max-height: 100px; overflow-y: auto; }
        .recommendations { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; }
        .recommendations ul { margin-left: 20px; }
        .recommendations li { margin-bottom: 8px; }
        .hipaa-highlight { background: #d4edda; border: 1px solid #c3e6cb; border-radius: 8px; padding: 15px; margin-bottom: 20px; }
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
        .severity-critical { border-left: 4px solid #dc3545; }
        .severity-warning { border-left: 4px solid #ffc107; }
        .severity-info { border-left: 4px solid #17a2b8; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>🔒 BSW.EHR Security Analysis</h1>
            <p>Healthcare Security & HIPAA Compliance Assessment</p>
            <p>Generated on: $($Results.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss"))</p>
        </div>
        
        <div class="content">
            <!-- HIPAA Compliance Notice -->
            <div class="hipaa-highlight">
                <h3>🏥 HIPAA Compliance Notice</h3>
                <p>This report analyzes security patterns relevant to HIPAA compliance for Protected Health Information (PHI). 
                Critical and Warning findings should be addressed immediately to maintain compliance with healthcare regulations.</p>
            </div>

            <!-- Security Summary -->
            <div class="section">
                <h2>📊 Security Summary</h2>
                <div class="summary-grid">
                    <div class="summary-card">
                        <div class="summary-value critical">$($Results.SecuritySummary.Critical)</div>
                        <div class="summary-label">Critical Issues</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value warning">$($Results.SecuritySummary.Warning)</div>
                        <div class="summary-label">Warnings</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value info">$($Results.SecuritySummary.Info)</div>
                        <div class="summary-label">Info</div>
                    </div>
                    <div class="summary-card">
                        <div class="summary-value">$($Results.TotalFilesScanned)</div>
                        <div class="summary-label">Files Scanned</div>
                    </div>
                </div>
            </div>

            <!-- Security Findings -->
            <div class="section">
                <h2>🔍 Security Pattern Findings</h2>
                <div class="findings-grid">
"@

    foreach ($patternKey in $Results.PatternsFound.Keys | Sort-Object) {
        $pattern = $Results.PatternsFound[$patternKey]
        $severityClass = "severity-$($pattern.Severity.ToLower())"
        $severityIcon = switch ($pattern.Severity) {
            'Critical' { '🔴' }
            'Warning' { '🟡' }
            'Info' { '🔵' }
        }
        
        $html += @"
                    <div class="finding-card $severityClass">
                        <div class="finding-title">$severityIcon $($pattern.Description)</div>
                        <div class="finding-category">$($pattern.Category)</div>
                        <p><strong>Occurrences:</strong> $($pattern.Count)</p>
                        <p><strong>Files:</strong> $($pattern.Files.Count)</p>
                        <div class="finding-files">
"@
        
        foreach ($file in $pattern.Files | Select-Object -First 10) {
            $html += "<div>• $file</div>"
        }
        
        if ($pattern.Files.Count -gt 10) {
            $html += "<div>... and $($pattern.Files.Count - 10) more files</div>"
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
                <h2>💡 Security Recommendations</h2>
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

            <!-- File Analysis Details -->
            <div class="section">
                <h2>📁 File Analysis Summary</h2>
                <table>
                    <tr><th>File</th><th>Critical</th><th>Warning</th><th>Info</th><th>Total</th></tr>
"@

    foreach ($filePath in $Results.FileResults.Keys | Sort-Object) {
        $fileResult = $Results.FileResults[$filePath]
        $critical = ($fileResult.Findings | Where-Object { $_.Severity -eq 'Critical' } | Measure-Object -Property Count -Sum).Sum
        $warning = ($fileResult.Findings | Where-Object { $_.Severity -eq 'Warning' } | Measure-Object -Property Count -Sum).Sum
        $info = ($fileResult.Findings | Where-Object { $_.Severity -eq 'Info' } | Measure-Object -Property Count -Sum).Sum
        $total = $critical + $warning + $info
        
        $html += @"
                    <tr>
                        <td>$filePath</td>
                        <td class="critical">$critical</td>
                        <td class="warning">$warning</td>
                        <td class="info">$info</td>
                        <td><strong>$total</strong></td>
                    </tr>
"@
    }

    $html += @"
                </table>
            </div>
        </div>
        
        <div class="footer">
            <p>Generated by BSW.EHR Security Pattern Analyzer | PowerShell Script v1.0</p>
            <p>Focused on Healthcare Security & HIPAA Compliance</p>
        </div>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Generate JSON report
function Generate-SecurityJsonReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "bsw-ehr-security-analysis-$timestamp.json"
    $filePath = Join-Path $OutputPath $fileName
    
    $Results | ConvertTo-Json -Depth 10 | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Generate console report
function Show-SecurityConsoleReport {
    param([hashtable]$Results)
    
    Write-Host "`n" + "="*80 -ForegroundColor Red
    Write-Host "  🔒 BSW.EHR SECURITY ANALYSIS REPORT" -ForegroundColor Red
    Write-Host "="*80 -ForegroundColor Red
    
    Write-Host "`n🏥 HIPAA COMPLIANCE NOTICE" -ForegroundColor Yellow
    Write-Host "This analysis focuses on healthcare security patterns and HIPAA compliance" -ForegroundColor Cyan
    
    Write-Host "`n📊 SECURITY SUMMARY" -ForegroundColor Yellow
    Write-Host "  Files Scanned: $($Results.TotalFilesScanned)"
    Write-Host "  Critical Issues: " -NoNewline
    Write-Host $Results.SecuritySummary.Critical -ForegroundColor Red
    Write-Host "  Warnings: " -NoNewline  
    Write-Host $Results.SecuritySummary.Warning -ForegroundColor Yellow
    Write-Host "  Info: " -NoNewline
    Write-Host $Results.SecuritySummary.Info -ForegroundColor Cyan
    
    if ($Results.SecuritySummary.Critical -gt 0) {
        Write-Host "`n🔴 CRITICAL FINDINGS REQUIRE IMMEDIATE ATTENTION" -ForegroundColor Red
    }
    
    Write-Host "`n🔍 TOP SECURITY PATTERNS" -ForegroundColor Yellow
    $topPatterns = $Results.PatternsFound.GetEnumerator() | Sort-Object { $_.Value.Count } -Descending | Select-Object -First 10
    foreach ($pattern in $topPatterns) {
        $severity = switch ($pattern.Value.Severity) {
            'Critical' { 'Red' }
            'Warning' { 'Yellow' }
            'Info' { 'Cyan' }
        }
        Write-Host "  $($pattern.Value.Description): $($pattern.Value.Count) occurrences" -ForegroundColor $severity
    }
    
    if ($Results.Recommendations.Count -gt 0) {
        Write-Host "`n💡 KEY RECOMMENDATIONS" -ForegroundColor Yellow
        foreach ($recommendation in $Results.Recommendations | Select-Object -First 5) {
            Write-Host "  • $recommendation" -ForegroundColor Green
        }
    }
    
    Write-Host "`n" + "="*80 -ForegroundColor Red
}

# Main execution
try {
    # Generate output
    switch ($Format) {
        "HTML" {
            $outputFile = Generate-SecurityHtmlReport -Results $analysisResults -OutputPath $OutputPath
            Write-Host "✅ Security analysis report generated: $outputFile" -ForegroundColor Green
            
            # Open the report in default browser
            if (Get-Command "Start-Process" -ErrorAction SilentlyContinue) {
                Start-Process $outputFile
            }
        }
        "JSON" {
            $outputFile = Generate-SecurityJsonReport -Results $analysisResults -OutputPath $OutputPath
            Write-Host "✅ Security analysis JSON generated: $outputFile" -ForegroundColor Green
        }
        "Console" {
            Show-SecurityConsoleReport -Results $analysisResults
        }
    }
    
    # Security summary
    if ($analysisResults.SecuritySummary.Critical -gt 0) {
        Write-Host "`n⚠️  SECURITY ALERT: $($analysisResults.SecuritySummary.Critical) critical issues found!" -ForegroundColor Red
    } elseif ($analysisResults.SecuritySummary.Warning -gt 0) {
        Write-Host "`n⚠️  Security warnings detected: $($analysisResults.SecuritySummary.Warning) warnings found" -ForegroundColor Yellow
    } else {
        Write-Host "`n✅ No critical security issues detected" -ForegroundColor Green
    }
    
    Write-Host "`n🎉 Security analysis complete!" -ForegroundColor Green
}
catch {
    Write-Error "❌ Error during security analysis: $_"
    exit 1
}
