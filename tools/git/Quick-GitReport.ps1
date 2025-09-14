#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Quick launcher for BSW.EHR analysis tools

.DESCRIPTION
    - Git Activity Reports
    - Security Pattern Analysis
    - Dependency Analysis
    - Code Quality Analysis

.PARAMETER AnalysisType
    Type of analysis: Git, Security, Dependencies, Quality, or All

.PARAMETER Preset
    Quick preset options: Daily, Weekly, BiWeekly, Monthly, or Custom (for Git analysis)

.PARAMETER Format
    Output format: HTML, JSON, or Console

.EXAMPLE
    .\Quick-GitReport.ps1
    .\Quick-GitReport.ps1 -AnalysisType Security
    .\Quick-GitReport.ps1 -AnalysisType All -Format HTML

.NOTES
    Convenience wrapper for all BSW.EHR analysis scripts
#>
Provides easy access to all BSW.EHR analysis tools with common presets:
- Git Activity Reports (now with enhanced user stats: earliest, latest, biggest, frequency, average size)
- Security Pattern Analysis
- Dependency Analysis
- Code Quality Analysis

.PARAMETER AnalysisType
Type of analysis: Git, Security, Dependencies, Quality, or All

.PARAMETER Preset
Quick preset options: Daily, Weekly, BiWeekly, Monthly, or Custom (for Git analysis)

.PARAMETER Format
Output format: HTML, JSON, or Console

.EXAMPLE
.\Quick-GitReport.ps1
.\Quick-GitReport.ps1 -AnalysisType Security
.\Quick-GitReport.ps1 -AnalysisType All -Format HTML

.NOTES
Convenience wrapper for all BSW.EHR analysis scripts
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet("Git", "Security", "Dependencies", "Quality", "All")]
    [string]$AnalysisType = "Git",
    
    [Parameter()]
    [ValidateSet("Daily", "Weekly", "BiWeekly", "Monthly", "Custom")]
    [string]$Preset = "Weekly",
    
    [Parameter()]
    [ValidateSet("HTML", "JSON", "Console")]
    [string]$Format = "HTML"
)

# Define presets for Git analysis
$presets = @{
    "Daily"    = 1
    "Weekly"   = 7
    "BiWeekly" = 14
    "Monthly"  = 30
    "Custom"   = 10
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "🚀 BSW.EHR Analysis Launcher" -ForegroundColor Cyan
Write-Host "Analysis Type: $AnalysisType | Format: $Format" -ForegroundColor Yellow

function Start-GitAnalysis {
    param([string]$Days, [string]$Format)
    
    $gitScript = Join-Path $scriptDir "Generate-GitActivityReport.ps1"
    if (-not (Test-Path $gitScript)) {
        Write-Error "Cannot find Generate-GitActivityReport.ps1"
        return
    }
    
    Write-Host "📊 Running Git Activity Analysis..." -ForegroundColor Green
    & $gitScript -Days $Days -Format $Format
}

function Start-SecurityAnalysis {
    param([string]$Format)
    
    $securityScript = Join-Path $scriptDir "Analyze-SecurityPatterns.ps1"
    if (-not (Test-Path $securityScript)) {
        Write-Error "Cannot find Analyze-SecurityPatterns.ps1"
        return
    }
    
    Write-Host "� Running Security Pattern Analysis..." -ForegroundColor Green
    & $securityScript -Format $Format
}

function Start-DependencyAnalysis {
    param([string]$Format)
    
    $dependencyScript = Join-Path $scriptDir "Analyze-Dependencies.ps1"
    if (-not (Test-Path $dependencyScript)) {
        Write-Error "Cannot find Analyze-Dependencies.ps1"
        return
    }
    
    Write-Host "📦 Running Dependency Analysis..." -ForegroundColor Green
    & $dependencyScript -Format $Format
}

function Start-QualityAnalysis {
    param([string]$Format)
    
    $qualityScript = Join-Path $scriptDir "Analyze-CodeQuality.ps1"
    if (-not (Test-Path $qualityScript)) {
        Write-Error "Cannot find Analyze-CodeQuality.ps1"
        return
    }
    
    Write-Host "📊 Running Code Quality Analysis..." -ForegroundColor Green
    & $qualityScript -Format $Format
}

try {
    switch ($AnalysisType) {
        "Git" {
            $days = $presets[$Preset]
            if ($Preset -eq "Custom") {
                $customDays = Read-Host "Enter number of days to analyze"
                if ($customDays -and $customDays -match '^\d+$') {
                    $days = [int]$customDays
                }
            }
            Start-GitAnalysis -Days $days -Format $Format
        }
        "Security" {
            Start-SecurityAnalysis -Format $Format
        }
        "Dependencies" {
            Start-DependencyAnalysis -Format $Format
        }
        "Quality" {
            Start-QualityAnalysis -Format $Format
        }
        "All" {
            Write-Host "🎯 Running Complete BSW.EHR Analysis Suite..." -ForegroundColor Magenta
            
            # Run Git analysis
            $days = $presets[$Preset]
            Start-GitAnalysis -Days $days -Format $Format
            
            # Run Security analysis
            Start-SecurityAnalysis -Format $Format
            
            # Run Dependency analysis
            Start-DependencyAnalysis -Format $Format
            
            # Run Quality analysis
            Start-QualityAnalysis -Format $Format
            
            Write-Host "✅ Complete analysis suite finished!" -ForegroundColor Green
        }
    }
}
catch {
    Write-Error "Failed to run analysis: $_"
    exit 1
}
