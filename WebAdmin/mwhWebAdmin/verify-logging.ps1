# 🔍 Logging Verification Script

Write-Host "=== Logging Verification ===" -ForegroundColor Cyan
Write-Host ""

$logsPath = "C:\GitHub\MarkHazleton\markhazleton-blog\logs"
$appLog = Join-Path $logsPath "app.log"
$aiLogsPath = Join-Path $logsPath "ai-responses"

# Check if logs directory exists
Write-Host "1. Checking logs directory..." -ForegroundColor Yellow
if (Test-Path $logsPath) {
    Write-Host "   ✓ Logs directory exists: $logsPath" -ForegroundColor Green
} else {
    Write-Host "   ✗ Logs directory NOT found: $logsPath" -ForegroundColor Red
    Write-Host "   Creating directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
    Write-Host "   ✓ Directory created" -ForegroundColor Green
}

# Check if AI logs directory exists
Write-Host "2. Checking AI logs directory..." -ForegroundColor Yellow
if (Test-Path $aiLogsPath) {
    Write-Host "   ✓ AI logs directory exists: $aiLogsPath" -ForegroundColor Green
} else {
    Write-Host "   ✗ AI logs directory NOT found" -ForegroundColor Red
    Write-Host "   Creating directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $aiLogsPath -Force | Out-Null
    Write-Host "   ✓ Directory created" -ForegroundColor Green
}

Write-Host ""
Write-Host "3. Checking for log files..." -ForegroundColor Yellow

# Check for application logs
$appLogs = Get-ChildItem $logsPath -Filter "app*.log" -ErrorAction SilentlyContinue
if ($appLogs) {
    Write-Host "   ✓ Found $($appLogs.Count) application log file(s):" -ForegroundColor Green
    foreach ($log in $appLogs) {
        $size = "{0:N2} KB" -f ($log.Length / 1KB)
        Write-Host "     - $($log.Name) ($size, Modified: $($log.LastWriteTime))" -ForegroundColor Gray
    }
} else {
    Write-Host "   ⚠ No application log files found yet" -ForegroundColor Yellow
    Write-Host "     (Logs will be created when application runs)" -ForegroundColor Gray
}

# Check for AI response logs
$aiLogs = Get-ChildItem $aiLogsPath -Filter "*.json" -ErrorAction SilentlyContinue
if ($aiLogs) {
    Write-Host "   ✓ Found $($aiLogs.Count) AI response log file(s):" -ForegroundColor Green
    $sessionLogs = $aiLogs | Where-Object { $_.Name -like "*_session.json" }
 $stepLogs = $aiLogs | Where-Object { $_.Name -notlike "*_session.json" }
    Write-Host "     - Session logs: $($sessionLogs.Count)" -ForegroundColor Gray
    Write-Host "     - Step logs: $($stepLogs.Count)" -ForegroundColor Gray
} else {
    Write-Host "   ⚠ No AI response logs found yet" -ForegroundColor Yellow
    Write-Host "     (Logs will be created when AI generates articles)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "4. Configuration check..." -ForegroundColor Yellow

# Check appsettings.json
$appSettings = "appsettings.json"
if (Test-Path $appSettings) {
    $config = Get-Content $appSettings | ConvertFrom-Json
    if ($config.Serilog) {
        Write-Host "   ✓ Serilog configuration found in appsettings.json" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Serilog configuration NOT found in appsettings.json" -ForegroundColor Red
    }
    
    if ($config.AiResponseLogs) {
      Write-Host "   ✓ AiResponseLogs configuration found" -ForegroundColor Green
        Write-Host "     - Directory: $($config.AiResponseLogs.Directory)" -ForegroundColor Gray
        Write-Host "  - Retention: $($config.AiResponseLogs.RetentionDays) days" -ForegroundColor Gray
    } else {
   Write-Host "   ✗ AiResponseLogs configuration NOT found" -ForegroundColor Red
    }
} else {
    Write-Host "   ✗ appsettings.json not found in current directory" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To test logging:" -ForegroundColor Yellow
Write-Host "1. Run: dotnet run" -ForegroundColor White
Write-Host "2. Check for app.log creation in: $logsPath" -ForegroundColor White
Write-Host "3. Generate an article with AI" -ForegroundColor White
Write-Host "4. Check for JSON files in: $aiLogsPath" -ForegroundColor White
Write-Host ""
Write-Host "To view logs in real-time:" -ForegroundColor Yellow
Write-Host "Get-Content '$appLog' -Wait -Tail 50" -ForegroundColor White
Write-Host ""
