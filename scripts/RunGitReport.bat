@echo off
setlocal

echo.
echo =====================================================
echo    BSW.EHR Analysis Tools Launcher
echo =====================================================
echo.

cd /d "%~dp0.."

echo Choose analysis type:
echo 1. Git Activity Report (Quick Console - 7 days)
echo 2. Git Activity Report (Weekly HTML - 7 days)
echo 3. Git Activity Report (Bi-Weekly HTML - 14 days)
echo 4. Git Activity Report (Monthly HTML - 30 days)
echo 5. Security Pattern Analysis
echo 6. Dependency Analysis
echo 7. Code Quality Analysis
echo 8. Complete Analysis Suite (All reports)
echo 9. Custom Git Report
echo.

set /p choice="Enter your choice (1-9): "

if "%choice%"=="1" (
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days 7 -Format Console
) else if "%choice%"=="2" (
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days 7 -Format HTML
) else if "%choice%"=="3" (
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days 14 -Format HTML
) else if "%choice%"=="4" (
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days 30 -Format HTML
) else if "%choice%"=="5" (
    echo Running Security Pattern Analysis...
    powershell -ExecutionPolicy Bypass -File "scripts\Analyze-SecurityPatterns.ps1" -Format HTML
) else if "%choice%"=="6" (
    echo Running Dependency Analysis...
    powershell -ExecutionPolicy Bypass -File "scripts\Analyze-Dependencies.ps1" -Format HTML
) else if "%choice%"=="7" (
    echo Running Code Quality Analysis...
    powershell -ExecutionPolicy Bypass -File "scripts\Analyze-CodeQuality.ps1" -Format HTML
) else if "%choice%"=="8" (
    echo Running Complete BSW.EHR Analysis Suite...
    echo This will generate all reports - please wait...
    powershell -ExecutionPolicy Bypass -File "scripts\Quick-GitReport.ps1" -AnalysisType All -Format HTML
) else if "%choice%"=="9" (
    set /p days="Enter number of days for Git analysis: "
    echo Choose format:
    echo 1. HTML
    echo 2. JSON
    echo 3. Console
    set /p format_choice="Enter format choice (1-3): "
    
    if "!format_choice!"=="1" set format=HTML
    if "!format_choice!"=="2" set format=JSON
    if "!format_choice!"=="3" set format=Console
    
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days !days! -Format !format!
) else (
    echo Invalid choice. Running default weekly Git report...
    powershell -ExecutionPolicy Bypass -File "scripts\Generate-GitActivityReport.ps1" -Days 7 -Format HTML
)

echo.
echo Analysis complete!
pause
