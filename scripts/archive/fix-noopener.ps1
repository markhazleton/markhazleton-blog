# Fix rel="noopener" issue in PUG files
# This script finds all instances of target="_blank" without rel="noopener" and fixes them

param(
    [string]$rootPath = "c:\GitHub\MarkHazleton\markhazleton-blog\src\pug"
)

# Find all PUG files that need fixing
$pugFiles = Get-ChildItem -Path $rootPath -Filter "*.pug" -Recurse

Write-Host "Starting to fix rel='noopener' issues in PUG files..." -ForegroundColor Green

foreach ($file in $pugFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Pattern to find target="_blank" without rel="noopener"
    # This regex looks for target="_blank" and checks if there's no rel="noopener" or rel="noreferrer" after it
    $pattern = 'target="_blank"(?![^>]*rel="[^"]*noopener)'

    if ($content -match $pattern) {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Yellow

        # Replace target="_blank" with target="_blank" rel="noopener noreferrer"
        # But only if there's no existing rel attribute
        $content = $content -replace 'target="_blank"(?![^>]*rel=")', 'target="_blank" rel="noopener noreferrer"'

        # Write the fixed content back to file
        Set-Content -Path $file.FullName -Value $content -NoNewline

        Write-Host "Fixed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "Completed fixing rel='noopener' issues!" -ForegroundColor Green
