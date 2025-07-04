# PowerShell script to update articles.json with first git appearance dates for .pug files

$repoPath = "c:\GitHub\MarkHazleton\markhazleton-blog"
$articlesJsonPath = "$repoPath\src\articles.json"

# Get list of .pug files from articles.json
$pugFiles = @(
    "/src/pug/sidetrackedbysizzle.pug",
    "/src/pug/lifelong-learning.pug",
    "/src/pug/projectmechanics/leadership/evolution-over-revolution.pug",
    "/src/pug/projectmechanics/leadership/from-features-to-outcomes.pug",
    "/src/pug/projectmechanics/leadership/index.pug",
    "/src/pug/projectmechanics/leadership/accountability-and-authority.pug",
    "/src/pug/projectmechanics/program-management-office.pug",
    "/src/pug/projectmechanics/index.pug",
    "/src/pug/projectmechanics/project-life-cycle.pug",
    "/src/pug/sample-mvc-crud.pug",
    "/src/pug/decorator-pattern-http-client.pug",
    "/src/pug/articles/nuget-packages-pros-cons.pug",
    "/src/pug/git-organized.pug",
    "/src/pug/web-project-mechanics.pug",
    "/src/pug/data-analysis-demonstration.pug",
    "/src/pug/redis-local-instance.pug",
    "/src/pug/cancellation-token.pug",
    "/src/pug/concurrent-processing.pug",
    "/src/pug/git-flow-rethink.pug",
    "/src/pug/using-chatgpt-for-developers.pug",
    "/src/pug/trivia-spark-development.pug",
    "/src/pug/crafting-chatgpt-prompt.pug",
    "/src/pug/system-cache.pug"
)

# Function to get first appearance date of a file
function Get-FirstAppearanceDate {
    param($filePath)

    $cleanPath = $filePath -replace "^/", ""

    try {
        $gitLog = git log --follow --format="%ad" --date=short -- $cleanPath 2>$null
        if ($gitLog -and $gitLog.Count -gt 0) {
            return $gitLog[-1]  # Last entry is the first appearance
        }
    } catch {
        Write-Warning "Could not get git history for $filePath"
    }

    return $null
}

Write-Host "Getting git history for .pug files..."

# Create a hashtable to store file paths and their first appearance dates
$fileInfoTable = @{}

foreach ($pugFile in $pugFiles) {
    $firstDate = Get-FirstAppearanceDate $pugFile
    if ($firstDate) {
        $fileInfoTable[$pugFile] = $firstDate
        Write-Host "$pugFile : $firstDate"
    } else {
        Write-Warning "No git history found for $pugFile"
    }
}

Write-Host "`nFile appearance dates collected. Results:"
$fileInfoTable.GetEnumerator() | Sort-Object Name | ForEach-Object {
    Write-Host "$($_.Key) : $($_.Value)"
}
