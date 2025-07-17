# Script to update section names in articles.json
$articlesPath = "src\articles.json"
$backupPath = "src\articles.json.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Create backup
Copy-Item $articlesPath $backupPath
Write-Host "Created backup: $backupPath"

# Read the articles.json file
$content = Get-Content $articlesPath -Raw

# Define section mappings (old -> new)
$sectionMappings = @{
    "Current Events" = "Industry Insights"
    "ChatGPT" = "AI & Machine Learning"
    "Blog Management" = "Content Strategy"
    "Personal Philosophy" = "Leadership Philosophy"
    "Project Mechanics" = "Project Management"
    "PromptSpark" = "AI & Machine Learning"
    "ReactSpark" = "AI & Machine Learning"
    # Keep these as-is:
    # "Case Studies" = "Case Studies"
    # "Data Science" = "Data Science"
    # "Development" = "Development"
}

# Apply replacements
foreach ($oldSection in $sectionMappings.Keys) {
    $newSection = $sectionMappings[$oldSection]
    $pattern = "`"Section`": `"$oldSection`""
    $replacement = "`"Section`": `"$newSection`""

    $beforeCount = ([regex]::Matches($content, [regex]::Escape($pattern))).Count
    $content = $content -replace [regex]::Escape($pattern), $replacement

    if ($beforeCount -gt 0) {
        Write-Host "Updated $beforeCount instances of '$oldSection' to '$newSection'"
    }
}

# Write updated content back to file
Set-Content $articlesPath $content -NoNewline

Write-Host "Section updates completed successfully!"
Write-Host "Backup saved as: $backupPath"

# Show summary of new section counts
Write-Host "`nNew section distribution:"
Get-Content $articlesPath | Select-String '"Section"' | ForEach-Object {
    ($_ -split '"Section":\s*"')[1] -split '"' | Select-Object -First 1
} | Group-Object | Sort-Object Count -Descending | Format-Table Name, Count
