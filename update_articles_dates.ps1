# PowerShell script to update articles.json with correct lastmod dates

$repoPath = "c:\GitHub\MarkHazleton\markhazleton-blog"
$articlesJsonPath = "$repoPath\src\articles.json"

# File paths and their first appearance dates from git
$fileToDateMap = @{
    "/src/pug/sidetrackedbysizzle.pug" = "2023-07-28"
    "/src/pug/lifelong-learning.pug" = "2023-07-28"
    "/src/pug/projectmechanics/leadership/evolution-over-revolution.pug" = "2023-07-28"
    "/src/pug/projectmechanics/leadership/from-features-to-outcomes.pug" = "2023-07-28"
    "/src/pug/projectmechanics/leadership/index.pug" = "2023-08-18"
    "/src/pug/projectmechanics/leadership/accountability-and-authority.pug" = "2023-07-28"
    "/src/pug/projectmechanics/program-management-office.pug" = "2023-07-28"
    "/src/pug/projectmechanics/index.pug" = "2023-08-18"
    "/src/pug/projectmechanics/project-life-cycle.pug" = "2023-07-28"
    "/src/pug/sample-mvc-crud.pug" = "2023-07-28"
    "/src/pug/decorator-pattern-http-client.pug" = "2023-07-28"
    "/src/pug/articles/nuget-packages-pros-cons.pug" = "2023-07-28"
    "/src/pug/git-organized.pug" = "2023-07-28"
    "/src/pug/web-project-mechanics.pug" = "2023-07-28"
    "/src/pug/data-analysis-demonstration.pug" = "2023-07-28"
    "/src/pug/redis-local-instance.pug" = "2023-08-24"
    "/src/pug/cancellation-token.pug" = "2023-07-28"
    "/src/pug/concurrent-processing.pug" = "2023-08-17"
    "/src/pug/git-flow-rethink.pug" = "2023-07-28"
    "/src/pug/using-chatgpt-for-developers.pug" = "2023-07-28"
    "/src/pug/trivia-spark-development.pug" = "2023-07-28"
    "/src/pug/crafting-chatgpt-prompt.pug" = "2023-07-28"
    "/src/pug/system-cache.pug" = "2023-08-10"
}

# Read the articles.json file
$articlesJson = Get-Content $articlesJsonPath | ConvertFrom-Json

# Track changes
$changesCount = 0

# Update lastmod dates for matching sources
for ($i = 0; $i -lt $articlesJson.Length; $i++) {
    $article = $articlesJson[$i]
    if ($article.source -and $fileToDateMap.ContainsKey($article.source)) {
        $newDate = $fileToDateMap[$article.source]
        $oldDate = $article.lastmod

        if ($oldDate -ne $newDate) {
            Write-Host "Updating article '$($article.name)' (id: $($article.id))"
            Write-Host "  Source: $($article.source)"
            Write-Host "  Old lastmod: $oldDate"
            Write-Host "  New lastmod: $newDate"
            Write-Host ""

            $article.lastmod = $newDate
            $changesCount++
        }
    }
}

if ($changesCount -gt 0) {
    # Convert back to JSON and save
    $updatedJson = $articlesJson | ConvertTo-Json -Depth 10 -Compress:$false
    Set-Content $articlesJsonPath $updatedJson
    Write-Host "Updated $changesCount articles in articles.json"
} else {
    Write-Host "No changes needed - all dates are already correct"
}
