# Fix rel="noopener" issue in PUG files - Updated version
# This script finds all instances of target="_blank" without rel="noopener" and fixes them

param(
    [string]$rootPath = "c:\GitHub\MarkHazleton\markhazleton-blog\src\pug"
)

# Find all PUG files
$pugFiles = Get-ChildItem -Path $rootPath -Filter "*.pug" -Recurse

Write-Host "Starting to fix rel='noopener' issues in PUG files..." -ForegroundColor Green

foreach ($file in $pugFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # First, handle cases where we have rel="nofollow" but missing noopener
    $content = $content -replace 'target="_blank"\s+rel="nofollow"', 'target="_blank" rel="nofollow noopener noreferrer"'

    # Handle cases where we have rel="nofollow" after target="_blank"
    $content = $content -replace 'rel="nofollow"\s+target="_blank"', 'rel="nofollow noopener noreferrer" target="_blank"'

    # Handle any remaining cases of target="_blank" without any rel attribute
    $content = $content -replace 'target="_blank"(?![^>]*rel=")', 'target="_blank" rel="noopener noreferrer"'

    if ($content -ne $originalContent) {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow

        # Write the fixed content back to file
        Set-Content -Path $file.FullName -Value $content -NoNewline

        Write-Host "Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "Completed fixing rel='noopener' issues!" -ForegroundColor Green
