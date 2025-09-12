param(
    [int]$Days = 7,
    [string]$OutputFormat = "Console"
)

# Simple test to validate Git and basic functionality
Write-Host "Starting Git Activity Report..." -ForegroundColor Green

try {
    # Test Git availability
    $null = git --version
    Write-Host "Git is available" -ForegroundColor Green
}
catch {
    Write-Error "Git is not available"
    exit 1
}

# Test if we are in a Git repository
if (!(Test-Path ".git")) {
    Write-Error "Not in a Git repository"
    exit 1
}

Write-Host "In Git repository" -ForegroundColor Green

# Get basic commit data
$sinceDate = (Get-Date).AddDays(-$Days).ToString("yyyy-MM-dd")
Write-Host "Getting commits since: $sinceDate" -ForegroundColor Cyan

try {
    $gitLog = git log --since="$sinceDate" --oneline
    $commitCount = ($gitLog | Measure-Object).Count

    Write-Host ""
    Write-Host "RESULTS" -ForegroundColor Yellow
    Write-Host "  Commits found: $commitCount"
    Write-Host "  Analysis period: $Days days"
    Write-Host "  Repository: $(Split-Path -Leaf (Get-Location))"

    if ($commitCount -gt 0) {
        Write-Host ""
        Write-Host "Recent Commits:" -ForegroundColor Yellow
        $gitLog | Select-Object -First 10 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Gray
        }
    }

    Write-Host ""
    Write-Host "Test completed successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Failed to get Git log: $($_.Exception.Message)"
}
