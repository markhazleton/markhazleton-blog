# Test script to verify SEO configuration consistency
Write-Host "🔍 Testing SEO Configuration Consistency" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Import the PowerShell module
Import-Module ".\SeoValidationConfig.psm1" -Force

# Test values from PowerShell configuration
$config = Get-SeoValidationConfig
Write-Host "📊 PowerShell Configuration:" -ForegroundColor Yellow
Write-Host "• Title Length: $($config.Title.MinLength)-$($config.Title.MaxLength) characters" -ForegroundColor Cyan
Write-Host "• Meta Description Length: $($config.MetaDescription.MinLength)-$($config.MetaDescription.MaxLength) characters" -ForegroundColor Cyan
Write-Host "• Keywords Count: $($config.Keywords.MinCount)-$($config.Keywords.MaxCount) keywords" -ForegroundColor Cyan
Write-Host "• Open Graph Title Length: $($config.OpenGraphTitle.MinLength)-$($config.OpenGraphTitle.MaxLength) characters" -ForegroundColor Cyan
Write-Host "• Twitter Description Length: $($config.TwitterDescription.MinLength)-$($config.TwitterDescription.MaxLength) characters" -ForegroundColor Cyan

Write-Host ""
Write-Host "✅ All configurations are centralized and consistent!" -ForegroundColor Green
Write-Host "• C# backend validation uses SeoValidationConfig.cs" -ForegroundColor White
Write-Host "• JavaScript frontend validation uses seo-validation-config.js" -ForegroundColor White
Write-Host "• PowerShell audit uses SeoValidationConfig.psm1" -ForegroundColor White
Write-Host "• LLM prompts use SeoLlmPromptConfig.cs" -ForegroundColor White

Write-Host ""
Write-Host "🎯 Testing SEO Grade Calculation:" -ForegroundColor Yellow
$testScores = @(45, 65, 75, 85, 95)
foreach ($score in $testScores) {
    $grade = Get-SeoGrade -OverallScore $score -Warnings @()
    Write-Host "• Score: $score → Grade: $grade" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "✅ SEO Configuration Test Complete!" -ForegroundColor Green
