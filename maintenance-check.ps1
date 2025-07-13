# Site Maintenance Check Script
# This script runs essential maintenance checks for the Mark Hazleton Blog
#
# Usage: .\maintenance-check.ps1 [-Verbose] [-Quick]
#
# Author: Mark Hazleton
# Purpose: Automated site health and SEO validation

param(
    [switch]$Verbose = $false,
    [switch]$Quick = $false
)

# Script configuration
$ErrorActionPreference = "Continue"
$startTime = Get-Date

Write-Host "🚀 Mark Hazleton Blog - Site Maintenance Check" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host "📅 Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host ""

# Track overall results
$checks = @{
    SeoAudit         = @{ Status = "Pending"; Duration = $null; Issues = 0 }
    ContentIntegrity = @{ Status = "Pending"; Duration = $null; Issues = 0 }
    ConfigValidation = @{ Status = "Pending"; Duration = $null; Issues = 0 }
}

# Function to run a check and track results
function Invoke-MaintenanceCheck {
    param(
        [string]$CheckName,
        [string]$ScriptPath,
        [string[]]$Arguments = @(),
        [string]$Description
    )

    Write-Host "🔧 Running $Description..." -ForegroundColor Yellow
    $checkStart = Get-Date

    try {
        if ($Arguments.Count -gt 0) {
            & $ScriptPath @Arguments
        }
        else {
            & $ScriptPath
        }

        $checks[$CheckName].Status = "Completed"
        $checks[$CheckName].Duration = (Get-Date) - $checkStart
        Write-Host "✅ $Description completed successfully" -ForegroundColor Green
    }
    catch {
        $checks[$CheckName].Status = "Failed"
        $checks[$CheckName].Duration = (Get-Date) - $checkStart
        Write-Host "❌ $Description failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
}

# 1. SEO Audit Check
$seoArgs = if ($Verbose) { @("-Verbose") } else { @() }
Invoke-MaintenanceCheck -CheckName "SeoAudit" -ScriptPath ".\seo-audit.ps1" -Arguments $seoArgs -Description "SEO Audit"

# 2. Content Integrity Check
Invoke-MaintenanceCheck -CheckName "ContentIntegrity" -ScriptPath ".\check-missing-html.ps1" -Description "Content Integrity Check"

# 3. Configuration Validation (skip if Quick mode)
if (-not $Quick) {
    Invoke-MaintenanceCheck -CheckName "ConfigValidation" -ScriptPath ".\test-seo-config.ps1" -Description "SEO Configuration Validation"
}

# Summary Report
$totalDuration = (Get-Date) - $startTime
Write-Host "📊 Maintenance Check Summary" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host "🕒 Total Duration: $($totalDuration.ToString('mm\:ss'))" -ForegroundColor Cyan
Write-Host ""

$allPassed = $true
foreach ($checkName in $checks.Keys) {
    $check = $checks[$checkName]
    if ($check.Status -eq "Pending") { continue }  # Skip if not run (Quick mode)

    $statusColor = switch ($check.Status) {
        "Completed" { "Green" }
        "Failed" { "Red" }
        default { "Yellow" }
    }

    $statusIcon = switch ($check.Status) {
        "Completed" { "✅" }
        "Failed" { "❌" }
        default { "⚠️" }
    }

    $duration = if ($check.Duration) { $check.Duration.ToString('ss\.ff') + "s" } else { "N/A" }

    Write-Host "$statusIcon $checkName`: $($check.Status) ($duration)" -ForegroundColor $statusColor

    if ($check.Status -eq "Failed") {
        $allPassed = $false
    }
}

Write-Host ""

# Final status
if ($allPassed) {
    Write-Host "🎉 All maintenance checks passed!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "⚠️ Some maintenance checks failed. Please review the output above." -ForegroundColor Yellow
    exit 1
}
