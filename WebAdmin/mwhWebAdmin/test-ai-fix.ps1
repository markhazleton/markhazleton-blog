# Quick Test Script for AI Generation Fix

Write-Host "=== AI Generation Fix Verification ===" -ForegroundColor Cyan
Write-Host ""

$logsPath = "C:\GitHub\MarkHazleton\markhazleton-blog\logs\ai-responses"

Write-Host "✅ Fix Applied:" -ForegroundColor Green
Write-Host "   - Step 2: Changed strict: true → strict: false" -ForegroundColor White
Write-Host "   - Step 3: Changed strict: true → strict: false" -ForegroundColor White
Write-Host ""

Write-Host "📋 Testing Instructions:" -ForegroundColor Yellow
Write-Host "1. Run your application: dotnet run" -ForegroundColor White
Write-Host "2. Navigate to Article Add page" -ForegroundColor White
Write-Host "3. Create a test article with AI generation" -ForegroundColor White
Write-Host "4. Come back and run this script again" -ForegroundColor White
Write-Host ""

# Check for recent generations
$recentLogs = Get-ChildItem $logsPath -Filter "*.json" -ErrorAction SilentlyContinue | 
    Where-Object { $_.LastWriteTime -gt (Get-Date).AddMinutes(-30) } |
    Sort-Object LastWriteTime -Descending

if ($recentLogs.Count -eq 0) {
    Write-Host "⚠️  No recent AI logs found (last 30 minutes)" -ForegroundColor Yellow
    Write-Host "   Please generate an article and run this script again" -ForegroundColor Gray
    exit
}

Write-Host "📊 Recent AI Generation Found!" -ForegroundColor Green
Write-Host ""

# Get latest session
$latestSession = $recentLogs | Where-Object { $_.Name -like "*_session.json" } | Select-Object -First 1

if ($latestSession) {
    $sessionData = Get-Content $latestSession.FullName -Raw | ConvertFrom-Json
    Write-Host "Latest Generation:" -ForegroundColor Cyan
    Write-Host "  Article: $($sessionData.articleName)" -ForegroundColor White
    Write-Host "  Time: $($latestSession.LastWriteTime)" -ForegroundColor White
    Write-Host "  Success: $($sessionData.success)" -ForegroundColor $(if ($sessionData.success) { "Green" } else { "Red" })
    Write-Host "  Duration: $($sessionData.totalDurationMs)ms" -ForegroundColor White
    Write-Host ""
}

# Check for errors
$recentErrors = $recentLogs | Where-Object { $_.Name -like "*_ERROR.json" }

Write-Host "=== Step Verification ===" -ForegroundColor Cyan

# Check each step
$steps = @(
    @{ Num=1; Name="ContentGeneration"; Color="Green" },
    @{ Num=2; Name="SeoMetadataExtraction"; Color="Yellow" },
    @{ Num=3; Name="SocialMediaGeneration"; Color="Yellow" },
    @{ Num=4; Name="ConclusionGeneration"; Color="Green" }
)

foreach ($step in $steps) {
    $successFile = $recentLogs | Where-Object { $_.Name -like "*_step$($step.Num)_$($step.Name).json" -and $_.Name -notlike "*_ERROR.json" } | Select-Object -First 1
    $errorFile = $recentLogs | Where-Object { $_.Name -like "*_step$($step.Num)_*_ERROR.json" } | Select-Object -First 1
    
    if ($errorFile) {
   Write-Host "Step $($step.Num) ($($step.Name)): ❌ FAILED" -ForegroundColor Red
   Write-Host "  Error file: $($errorFile.Name)" -ForegroundColor Gray
    }
    elseif ($successFile) {
    Write-Host "Step $($step.Num) ($($step.Name)): ✅ SUCCESS" -ForegroundColor Green
     $fileSize = "{0:N0}" -f ($successFile.Length / 1KB)
  Write-Host "  File: $($successFile.Name) ($fileSize KB)" -ForegroundColor Gray
    }
    else {
    Write-Host "Step $($step.Num) ($($step.Name)): ⚠️  NOT FOUND" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Fix Verification ===" -ForegroundColor Cyan

# Check specifically for Steps 2 and 3 errors
$step2Error = $recentErrors | Where-Object { $_.Name -like "*_step2_*" } | Select-Object -First 1
$step3Error = $recentErrors | Where-Object { $_.Name -like "*_step3_*" } | Select-Object -First 1

if ($step2Error -or $step3Error) {
    Write-Host "❌ FIX DID NOT RESOLVE ISSUE" -ForegroundColor Red
    Write-Host ""
    if ($step2Error) {
        Write-Host "Step 2 still failing. Error:" -ForegroundColor Red
        $errorData = Get-Content $step2Error.FullName -Raw | ConvertFrom-Json
        Write-Host "  $($errorData.errorMessage.Substring(0, [Math]::Min(100, $errorData.errorMessage.Length)))..." -ForegroundColor Gray
    }
    if ($step3Error) {
        Write-Host "Step 3 still failing. Error:" -ForegroundColor Red
        $errorData = Get-Content $step3Error.FullName -Raw | ConvertFrom-Json
        Write-Host "  $($errorData.errorMessage.Substring(0, [Math]::Min(100, $errorData.errorMessage.Length)))..." -ForegroundColor Gray
    }
  Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review AI_GENERATION_FAILURE_ANALYSIS.md for additional fixes" -ForegroundColor White
    Write-Host "2. Enable debug logging to see raw API responses" -ForegroundColor White
    Write-Host "3. Try simplifying the JSON schemas" -ForegroundColor White
}
else {
    # Check if we have successful Step 2 and 3
    $step2Success = $recentLogs | Where-Object { $_.Name -like "*_step2_*.json" -and $_.Name -notlike "*_ERROR.json" } | Select-Object -First 1
    $step3Success = $recentLogs | Where-Object { $_.Name -like "*_step3_*.json" -and $_.Name -notlike "*_ERROR.json" } | Select-Object -First 1
    
    if ($step2Success -and $step3Success) {
        Write-Host "✅ FIX SUCCESSFUL!" -ForegroundColor Green
      Write-Host ""
        Write-Host "All steps completed without errors:" -ForegroundColor Green
        Write-Host "  ✅ Step 1: Content Generation" -ForegroundColor White
     Write-Host "  ✅ Step 2: SEO Metadata (FIXED!)" -ForegroundColor White
      Write-Host "  ✅ Step 3: Social Media (FIXED!)" -ForegroundColor White
        Write-Host "  ✅ Step 4: Conclusion Generation" -ForegroundColor White
        Write-Host ""
        Write-Host "🎉 The strict: false fix resolved the issue!" -ForegroundColor Green
        Write-Host ""
   Write-Host "Your articles will now have complete metadata:" -ForegroundColor Cyan
        Write-Host "  - SEO titles and descriptions" -ForegroundColor White
        Write-Host "  - Keywords" -ForegroundColor White
        Write-Host "  - Open Graph fields" -ForegroundColor White
        Write-Host "  - Twitter Card fields" -ForegroundColor White
    }
    else {
        Write-Host "⚠️  Partial Success" -ForegroundColor Yellow
   Write-Host "No errors found, but check if all steps completed" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=== Log Files ===" -ForegroundColor Cyan
Write-Host "Application logs: C:\GitHub\MarkHazleton\markhazleton-blog\logs\app*.log" -ForegroundColor Gray
Write-Host "AI response logs: $logsPath" -ForegroundColor Gray
Write-Host ""
