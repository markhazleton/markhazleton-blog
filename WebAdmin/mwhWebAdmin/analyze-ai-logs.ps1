# Analyze AI Response Logs

$logsPath = "C:\GitHub\MarkHazleton\markhazleton-blog\logs\ai-responses"

Write-Host "=== AI Response Log Analysis ===" -ForegroundColor Cyan
Write-Host ""

# Get all logs
$allLogs = Get-ChildItem $logsPath -Filter "*.json"
$errorLogs = $allLogs | Where-Object { $_.Name -like "*_ERROR.json" }
$successLogs = $allLogs | Where-Object { $_.Name -notlike "*_ERROR.json" -and $_.Name -notlike "*_session.json" }
$sessionLogs = $allLogs | Where-Object { $_.Name -like "*_session.json" }

Write-Host "Total Files: $($allLogs.Count)" -ForegroundColor White
Write-Host "  Success: $($successLogs.Count)" -ForegroundColor Green
Write-Host "  Errors: $($errorLogs.Count)" -ForegroundColor Red
Write-Host "  Sessions: $($sessionLogs.Count)" -ForegroundColor Yellow
Write-Host ""

# Analyze recent error
Write-Host "=== Most Recent Error ===" -ForegroundColor Cyan
$latestError = $errorLogs | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latestError) {
    Write-Host "File: $($latestError.Name)" -ForegroundColor Yellow
    $errorContent = Get-Content $latestError.FullName -Raw | ConvertFrom-Json
    Write-Host "Step: $($errorContent.step) - $($errorContent.stepName)" -ForegroundColor White
    Write-Host "Error Type: $($errorContent.errorType)" -ForegroundColor Red
  Write-Host "Error Message: $($errorContent.errorMessage)" -ForegroundColor Red
    Write-Host "Has Request Data: $($null -ne $errorContent.request)" -ForegroundColor White
    Write-Host ""
}

# Analyze a successful step
Write-Host "=== Comparing with Successful Step ===" -ForegroundColor Cyan
$successStep1 = $successLogs | Where-Object { $_.Name -like "*_step1_*" } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($successStep1) {
    Write-Host "File: $($successStep1.Name)" -ForegroundColor Green
    $successContent = Get-Content $successStep1.FullName -Raw | ConvertFrom-Json
    Write-Host "Step: $($successContent.step) - $($successContent.stepName)" -ForegroundColor White
    Write-Host "Duration: $($successContent.durationMs)ms" -ForegroundColor White
    Write-Host "Response Length: $($successContent.responseLength) chars" -ForegroundColor White
    Write-Host "Has Request Data: $($null -ne $successContent.request)" -ForegroundColor White
    
    if ($successContent.request) {
        Write-Host "Request Model: $($successContent.request.model)" -ForegroundColor Gray
        Write-Host "Request Messages Count: $($successContent.request.messages.Count)" -ForegroundColor Gray
    }
    Write-Host ""
}

# Check Step 4 (which works)
Write-Host "=== Step 4 (Conclusion - Works) ===" -ForegroundColor Cyan
$successStep4 = $successLogs | Where-Object { $_.Name -like "*_step4_*" } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($successStep4) {
    Write-Host "File: $($successStep4.Name)" -ForegroundColor Green
    $step4Content = Get-Content $successStep4.FullName -Raw | ConvertFrom-Json
    Write-Host "Duration: $($step4Content.durationMs)ms" -ForegroundColor White
    Write-Host "Response Length: $($step4Content.responseLength) chars" -ForegroundColor White
    if ($step4Content.request) {
        Write-Host "Model: $($step4Content.request.model)" -ForegroundColor Gray
        Write-Host "Has response_format: $($null -ne $step4Content.request.response_format)" -ForegroundColor Gray
    }
    Write-Host ""
}

# Summary
Write-Host "=== Key Findings ===" -ForegroundColor Cyan
Write-Host "- Step 1 (Content): SUCCESS - Generates 20K+ chars" -ForegroundColor Green
Write-Host "- Step 2 (SEO): ERROR - Empty JSON response" -ForegroundColor Red
Write-Host "- Step 3 (Social): ERROR - Empty JSON response" -ForegroundColor Red
Write-Host "- Step 4 (Conclusion): SUCCESS - Generates response" -ForegroundColor Green
Write-Host ""

Write-Host "Pattern: Steps 2 & 3 fail with identical error, Steps 1 & 4 succeed" -ForegroundColor Yellow
Write-Host "This suggests an issue specific to Steps 2 & 3 configuration" -ForegroundColor Yellow
