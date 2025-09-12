#requires -version 5.1

<#
.SYNOPSIS
    Generates comprehensive Git activity reports for GitHub repositories.

.DESCRIPTION
    Analyzes Git repository commit history to generate detailed reports including:
    - Commit metrics and GitHub integration patterns
    - Timeline analysis with visual charts
    - Commit pattern statistics and consistency scoring
    - Developer activity cards with insights
    - Recommendations based on GitHub best practices

.PARAMETER Days
    Number of days to analyze (default: 30)

.PARAMETER OutputPath
    Path to save generated reports (default: current directory)

.PARAMETER OutputFormat
    Output format: HTML, JSON, Console, or All (default: All)

.PARAMETER RepositoryPath
    Path to the Git repository (default: current directory)

.EXAMPLE
    .\Generate-GitActivityReport.ps1 -Days 7 -OutputFormat Console

.EXAMPLE
    .\Generate-GitActivityReport.ps1 -Days 30 -OutputPath "C:\Reports" -OutputFormat All
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$Days = 30,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".",

    [Parameter(Mandatory=$false)]
    [ValidateSet("HTML", "JSON", "Console", "All")]
    [string]$OutputFormat = "All",

    [Parameter(Mandatory=$false)]
    [string]$RepositoryPath = "."
)

# Global variables
$script:ErrorActionPreference = "Stop"
$script:ProgressPreference = "SilentlyContinue"

# Test Git availability
function Test-GitAvailable {
    try {
        $null = git --version
        return $true
    }
    catch {
        Write-Error "Git is not available or not in PATH. Please install Git and ensure it's accessible."
        return $false
    }
}

# Test if current directory is a Git repository
function Test-GitRepository {
    param([string]$Path = ".")

    if (!(Test-Path "$Path\.git" -PathType Container)) {
        Write-Error "Directory '$Path' is not a Git repository."
        return $false
    }
    return $true
}

# Get commit data from Git log
function Get-CommitData {
    param(
        [int]$Days = 30,
        [string]$RepositoryPath = "."
    )

    Write-Host "📊 Analyzing $Days days of commit history..." -ForegroundColor Cyan

    $sinceDate = (Get-Date).AddDays(-$Days).ToString("yyyy-MM-dd")
    $commits = @()

    try {
        Push-Location $RepositoryPath

        # Get commit log with detailed information
        $gitLogOutput = git log --since="$sinceDate" --pretty=format:"%H|%an|%ae|%ad|%s" --date=iso --numstat

        if (-not $gitLogOutput) {
            Write-Warning "No commits found in the last $Days days"
            return @()
        }

        $currentCommit = $null
        $files = @()

        foreach ($line in $gitLogOutput) {
            if ($line -match "^[a-f0-9]{40}\|") {
                # Save previous commit
                if ($currentCommit) {
                    $currentCommit.Files = $files
                    $currentCommit.FilesTouched = $files.Count
                    $currentCommit.LinesAdded = ($files | Measure-Object -Property Added -Sum).Sum
                    $currentCommit.LinesDeleted = ($files | Measure-Object -Property Deleted -Sum).Sum
                    $commits += $currentCommit
                }

                # Parse new commit
                $parts = $line -split '\|', 5
                $currentCommit = @{
                    Hash = $parts[0]
                    Author = $parts[1]
                    Email = $parts[2]
                    Date = [DateTime]::Parse($parts[3])
                    Message = $parts[4]
                    Files = @()
                    FilesTouched = 0
                    LinesAdded = 0
                    LinesDeleted = 0
                }
                $files = @()
            }
            elseif ($line -match "^\d+\s+\d+\s+" -or $line -match "^-\s+-\s+") {
                # Parse file changes
                $fileParts = $line -split '\s+', 3
                if ($fileParts.Count -ge 3) {
                    $files += @{
                        Added = if ($fileParts[0] -eq '-') { 0 } else { [int]$fileParts[0] }
                        Deleted = if ($fileParts[1] -eq '-') { 0 } else { [int]$fileParts[1] }
                        Path = $fileParts[2]
                    }
                }
            }
        }

        # Add last commit
        if ($currentCommit) {
            $currentCommit.Files = $files
            $currentCommit.FilesTouched = $files.Count
            $currentCommit.LinesAdded = ($files | Measure-Object -Property Added -Sum).Sum
            $currentCommit.LinesDeleted = ($files | Measure-Object -Property Deleted -Sum).Sum
            $commits += $currentCommit
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "✅ Found $($commits.Count) commits to analyze" -ForegroundColor Green
    return $commits
}

# Get repository information
function Get-RepositoryInfo {
    param([string]$RepositoryPath = ".")

    try {
        Push-Location $RepositoryPath

        $remotePath = git config --get remote.origin.url
        $repoName = if ($remotePath) {
            if ($remotePath -match "github\.com[:/]([^/]+)/([^/.]+)") {
                "$($matches[1])/$($matches[2])"
            } else {
                Split-Path -Leaf $RepositoryPath
            }
        } else {
            Split-Path -Leaf $RepositoryPath
        }

        return @{
            Name = $repoName
            Path = $RepositoryPath
            RemoteUrl = $remotePath
        }
    }
    finally {
        Pop-Location
    }
}

# Analyze GitHub integration patterns
function Get-GitHubIntegration {
    param([array]$Commits)

    $issueReferences = @()
    $prPatterns = @{
        PullRequestMerges = 0
        MergeCommits = 0
    }
    $commitQuality = @{
        WithConventionalFormat = 0
        Total = $Commits.Count
    }

    foreach ($commit in $Commits) {
        $message = $commit.Message

        # Find issue references (#123)
        if ($message -match '#(\d+)') {
            $issueNumbers = [regex]::Matches($message, '#(\d+)') | ForEach-Object { $_.Groups[1].Value }
            $issueReferences += $issueNumbers
        }

        # Detect PR merges
        if ($message -match "Merge pull request" -or $message -match "Merged PR") {
            $prPatterns.PullRequestMerges++
        }

        # Detect merge commits
        if ($commit.Message -match "^Merge ") {
            $prPatterns.MergeCommits++
        }

        # Check conventional commit format
        if ($message -match "^(feat|fix|docs|style|refactor|test|chore)(\(.+\))?: ") {
            $commitQuality.WithConventionalFormat++
        }
    }

    $uniqueIssues = $issueReferences | Sort-Object -Unique

    return @{
        IssueReferences = @{
            Total = $issueReferences.Count
            UniqueIssues = $uniqueIssues
        }
        PullRequestPatterns = $prPatterns
        CommitMessageQuality = $commitQuality
    }
}

# Get commit timeline analysis
function Get-CommitTimelineAnalysis {
    param([array]$Commits)

    if ($Commits.Count -eq 0) {
        return @{
            WeeklyData = @{}
            Statistics = @{
                AverageCommitsPerWeek = 0
                MedianCommitsPerWeek = 0
                MostActiveWeek = @{ Week = "N/A"; Count = 0; LinesChanged = 0 }
                AverageGapBetweenCommits = 0
                Consistency = @{ Score = 0; Rating = "Poor"; Description = "No commits to analyze" }
            }
            Patterns = @{
                FirstCommit = Get-Date
                LastCommit = Get-Date
                TotalTimespan = 0
                LargestGap = @{ Days = 0; Description = "N/A" }
                ActivityStreak = @{ Longest = 0 }
            }
        }
    }

    # Sort commits by date
    $sortedCommits = $Commits | Sort-Object Date

    # Group commits by week
    $weeklyData = @{}
    foreach ($commit in $sortedCommits) {
        $weekStart = $commit.Date.AddDays(-([int]$commit.Date.DayOfWeek)).Date
        $weekKey = $weekStart.ToString("yyyy-MM-dd")

        if (-not $weeklyData.ContainsKey($weekKey)) {
            $weeklyData[$weekKey] = @{
                WeekStart = $weekStart
                CommitCount = 0
                TotalLinesChanged = 0
                Commits = @()
            }
        }

        $weeklyData[$weekKey].CommitCount++
        $weeklyData[$weekKey].TotalLinesChanged += ($commit.LinesAdded + $commit.LinesDeleted)
        $weeklyData[$weekKey].Commits += $commit
    }

    # Calculate statistics
    $commitCounts = $weeklyData.Values | ForEach-Object { $_.CommitCount }
    $averagePerWeek = if ($commitCounts.Count -gt 0) {
        [math]::Round(($commitCounts | Measure-Object -Average).Average, 1)
    } else { 0 }

    $sortedCounts = $commitCounts | Sort-Object
    $medianPerWeek = if ($sortedCounts.Count -gt 0) {
        if ($sortedCounts.Count % 2 -eq 1) {
            $sortedCounts[[math]::Floor($sortedCounts.Count / 2)]
        } else {
            [math]::Round(($sortedCounts[$sortedCounts.Count / 2 - 1] + $sortedCounts[$sortedCounts.Count / 2]) / 2, 1)
        }
    } else { 0 }

    # Find most active week
    $mostActiveWeek = $weeklyData.Values | Sort-Object CommitCount -Descending | Select-Object -First 1
    if (-not $mostActiveWeek) {
        $mostActiveWeek = @{ Week = "N/A"; Count = 0; LinesChanged = 0 }
    } else {
        $mostActiveWeek = @{
            Week = $mostActiveWeek.WeekStart.ToString("yyyy-MM-dd")
            Count = $mostActiveWeek.CommitCount
            LinesChanged = $mostActiveWeek.TotalLinesChanged
        }
    }

    # Calculate gaps between commits
    $gaps = @()
    for ($i = 1; $i -lt $sortedCommits.Count; $i++) {
        $gap = ($sortedCommits[$i].Date - $sortedCommits[$i-1].Date).Days
        $gaps += $gap
    }

    $averageGap = if ($gaps.Count -gt 0) {
        [math]::Round(($gaps | Measure-Object -Average).Average, 1)
    } else { 0 }

    # Calculate consistency score
    $consistencyScore = if ($weeklyData.Values.Count -gt 1) {
        $variance = if ($commitCounts.Count -gt 1) {
            $mean = ($commitCounts | Measure-Object -Average).Average
            $squaredDiffs = $commitCounts | ForEach-Object { [math]::Pow($_ - $mean, 2) }
            ($squaredDiffs | Measure-Object -Average).Average
        } else { 0 }

        $coefficient = if ($averagePerWeek -gt 0) {
            [math]::Sqrt($variance) / $averagePerWeek
        } else { 1 }

        [math]::Max(0, [math]::Round((1 - [math]::Min($coefficient, 1)) * 100, 0))
    } else { 0 }

    $consistencyRating = switch ($consistencyScore) {
        {$_ -ge 80} { "Excellent" }
        {$_ -ge 60} { "Good" }
        {$_ -ge 40} { "Fair" }
        default { "Poor" }
    }

    $consistencyDescription = switch ($consistencyRating) {
        "Excellent" { "Very consistent commit patterns with minimal variation" }
        "Good" { "Generally consistent with occasional variations" }
        "Fair" { "Moderate consistency with noticeable variations" }
        "Poor" { "Irregular commit patterns with high variation" }
    }

    # Pattern analysis
    $firstCommit = $sortedCommits[0].Date
    $lastCommit = $sortedCommits[-1].Date
    $totalTimespan = ($lastCommit - $firstCommit).Days

    $largestGap = if ($gaps.Count -gt 0) {
        $maxGap = ($gaps | Measure-Object -Maximum).Maximum
        $gapIndex = $gaps.IndexOf($maxGap)
        @{
            Days = $maxGap
            Description = "Between $($sortedCommits[$gapIndex].Date.ToString('yyyy-MM-dd')) and $($sortedCommits[$gapIndex + 1].Date.ToString('yyyy-MM-dd'))"
        }
    } else {
        @{ Days = 0; Description = "N/A" }
    }

    # Activity streak (simplified)
    $activityStreak = @{ Longest = 1 }

    return @{
        WeeklyData = $weeklyData
        Statistics = @{
            AverageCommitsPerWeek = $averagePerWeek
            MedianCommitsPerWeek = $medianPerWeek
            MostActiveWeek = $mostActiveWeek
            AverageGapBetweenCommits = $averageGap
            Consistency = @{
                Score = $consistencyScore
                Rating = $consistencyRating
                Description = $consistencyDescription
            }
        }
        Patterns = @{
            FirstCommit = $firstCommit
            LastCommit = $lastCommit
            TotalTimespan = $totalTimespan
            LargestGap = $largestGap
            ActivityStreak = $activityStreak
        }
    }
}

# Generate basic metrics
function Get-BasicMetrics {
    param([array]$Commits)

    if ($Commits.Count -eq 0) {
        return @{
            TotalCommits = 0
            TotalLinesAdded = 0
            TotalLinesDeleted = 0
            TotalFilesTouched = 0
            AverageChurnPerCommit = 0
        }
    }

    $totalLinesAdded = ($Commits | Measure-Object -Property LinesAdded -Sum).Sum
    $totalLinesDeleted = ($Commits | Measure-Object -Property LinesDeleted -Sum).Sum
    $totalFilesTouched = ($Commits | Measure-Object -Property FilesTouched -Sum).Sum
    $avgChurn = if ($Commits.Count -gt 0) {
        [math]::Round(($totalLinesAdded + $totalLinesDeleted) / $Commits.Count, 1)
    } else { 0 }

    return @{
        TotalCommits = $Commits.Count
        TotalLinesAdded = $totalLinesAdded
        TotalLinesDeleted = $totalLinesDeleted
        TotalFilesTouched = $totalFilesTouched
        AverageChurnPerCommit = $avgChurn
    }
}

# Get author analysis
function Get-AuthorAnalysis {
    param([array]$Commits)

    $authors = @{}

    foreach ($commit in $Commits) {
        $authorKey = "$($commit.Author) <$($commit.Email)>"

        if (-not $authors.ContainsKey($authorKey)) {
            $authors[$authorKey] = @{
                Name = $commit.Author
                Email = $commit.Email
                Commits = @()
                TotalCommits = 0
                TotalLinesAdded = 0
                TotalLinesDeleted = 0
                TotalFilesTouched = 0
            }
        }

        $authors[$authorKey].Commits += $commit
        $authors[$authorKey].TotalCommits++
        $authors[$authorKey].TotalLinesAdded += $commit.LinesAdded
        $authors[$authorKey].TotalLinesDeleted += $commit.LinesDeleted
        $authors[$authorKey].TotalFilesTouched += $commit.FilesTouched
    }

    return $authors
}

# Generate console report
function Show-ConsoleReport {
    param([hashtable]$ReportData)

    Write-Host "`n" + "="*80 -ForegroundColor Cyan
    Write-Host "  📊 GIT ACTIVITY REPORT - $($ReportData.Repository.Name)" -ForegroundColor Cyan
    Write-Host "="*80 -ForegroundColor Cyan

    Write-Host "`n🏛️  REPOSITORY OVERVIEW" -ForegroundColor Yellow
    Write-Host "  Name: $($ReportData.Repository.Name)"
    Write-Host "  Period: Last $($ReportData.AnalysisPeriod) days"
    Write-Host "  Generated: $($ReportData.GeneratedAt.ToString('yyyy-MM-dd HH:mm:ss'))"

    Write-Host "`n📈 KEY METRICS" -ForegroundColor Yellow
    Write-Host "  Total Commits: $($ReportData.Metrics.TotalCommits)"
    Write-Host "  Active Authors: $($ReportData.Authors.Count)"
    Write-Host "  Lines Added: $($ReportData.Metrics.TotalLinesAdded)"
    Write-Host "  Lines Deleted: $($ReportData.Metrics.TotalLinesDeleted)"
    Write-Host "  Avg Lines/Commit: $($ReportData.Metrics.AverageChurnPerCommit)"

    Write-Host "`n🔗 GITHUB INTEGRATION" -ForegroundColor Yellow
    Write-Host "  Issue References: $($ReportData.GitHubIntegration.IssueReferences.Total)"
    Write-Host "  PR Merges: $($ReportData.GitHubIntegration.PullRequestPatterns.PullRequestMerges)"
    Write-Host "  Conventional Commits: $($ReportData.GitHubIntegration.CommitMessageQuality.WithConventionalFormat)"

    if ($ReportData.TimelineAnalysis) {
        Write-Host "`n📊 TIMELINE ANALYSIS" -ForegroundColor Yellow
        Write-Host "  Weekly Average: $($ReportData.TimelineAnalysis.Statistics.AverageCommitsPerWeek) commits"
        Write-Host "  Peak Week: $($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Count) commits"
        Write-Host "  Consistency Score: $($ReportData.TimelineAnalysis.Statistics.Consistency.Score)% ($($ReportData.TimelineAnalysis.Statistics.Consistency.Rating))"
        Write-Host "  First Commit: $($ReportData.TimelineAnalysis.Patterns.FirstCommit.ToString('yyyy-MM-dd'))"
        Write-Host "  Last Commit: $($ReportData.TimelineAnalysis.Patterns.LastCommit.ToString('yyyy-MM-dd'))"
        Write-Host "  Largest Gap: $($ReportData.TimelineAnalysis.Patterns.LargestGap.Days) days"

        # Display simple timeline
        Write-Host "`n📈 WEEKLY TIMELINE" -ForegroundColor Yellow
        $sortedWeeks = $ReportData.TimelineAnalysis.WeeklyData.GetEnumerator() | Sort-Object { $_.Value.WeekStart }
        foreach ($week in $sortedWeeks) {
            $weekLabel = $week.Value.WeekStart.ToString("MM/dd")
            $commitCount = $week.Value.CommitCount
            $bar = "█" * [math]::Min($commitCount, 40)
            Write-Host "  $weekLabel : $bar ($commitCount commits)"
        }
    }

    Write-Host "`n👥 TOP AUTHORS" -ForegroundColor Yellow
    $topAuthors = $ReportData.Authors.GetEnumerator() | Sort-Object { $_.Value.TotalCommits } -Descending | Select-Object -First 5
    foreach ($author in $topAuthors) {
        Write-Host "  $($author.Value.Name): $($author.Value.TotalCommits) commits (+$($author.Value.TotalLinesAdded)/-$($author.Value.TotalLinesDeleted) lines)"
    }

    Write-Host "`n" + "="*80 -ForegroundColor Cyan
    Write-Host ""
}

# Create JSON report
function New-JsonReport {
    param(
        [hashtable]$ReportData,
        [string]$OutputPath
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "git-activity-report-$timestamp.json"
    $filePath = Join-Path $OutputPath $fileName

    $ReportData | ConvertTo-Json -Depth 10 | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Create basic HTML report
function New-HtmlReport {
    param(
        [hashtable]$ReportData,
        [string]$OutputPath
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "git-activity-report-$timestamp.html"
    $filePath = Join-Path $OutputPath $fileName

    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Git Activity Report - $($ReportData.Repository.Name)</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 20px auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; border-bottom: 2px solid #007acc; padding-bottom: 20px; margin-bottom: 30px; }
        .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .metric-card { background: #f8f9ff; padding: 20px; border-radius: 8px; text-align: center; border-left: 4px solid #007acc; }
        .metric-value { font-size: 2em; font-weight: bold; color: #007acc; }
        .metric-label { color: #666; margin-top: 5px; }
        .section { margin-bottom: 40px; }
        .section h2 { color: #333; border-bottom: 1px solid #ddd; padding-bottom: 10px; }
        .chart-container { margin: 20px 0; text-align: center; }
        .timeline-stats { margin-top: 20px; }
        .pattern-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
        .pattern-card { background: #f8f9ff; padding: 20px; border-radius: 8px; border-left: 4px solid #28a745; }
        .stat-row { display: flex; justify-content: space-between; margin: 10px 0; padding: 5px 0; border-bottom: 1px solid #eee; }
        .stat-label { font-weight: bold; }
        .stat-value { font-family: monospace; color: #666; }
        .authors-list { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 15px; }
        .author-card { background: #fff; border: 1px solid #ddd; padding: 15px; border-radius: 8px; }
        .author-name { font-weight: bold; color: #007acc; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📊 Git Activity Report</h1>
            <p><strong>Repository:</strong> $($ReportData.Repository.Name)</p>
            <p><strong>Analysis Period:</strong> Last $($ReportData.AnalysisPeriod) days</p>
            <p><strong>Generated:</strong> $($ReportData.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss"))</p>
        </div>

        <div class="section">
            <h2>📈 Key Metrics</h2>
            <div class="metrics-grid">
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.Metrics.TotalCommits)</div>
                    <div class="metric-label">Total Commits</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.Authors.Count)</div>
                    <div class="metric-label">Active Authors</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.Metrics.TotalLinesAdded)</div>
                    <div class="metric-label">Lines Added</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.GitHubIntegration.IssueReferences.Total)</div>
                    <div class="metric-label">Issue References</div>
                </div>
            </div>
        </div>

        <div class="section">
            <h2>🔗 GitHub Integration</h2>
            <div class="metrics-grid">
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.GitHubIntegration.PullRequestPatterns.PullRequestMerges)</div>
                    <div class="metric-label">PR Merges</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($ReportData.GitHubIntegration.CommitMessageQuality.WithConventionalFormat)</div>
                    <div class="metric-label">Conventional Commits</div>
                </div>
            </div>
        </div>
"@

    # Add timeline section if data exists
    if ($ReportData.TimelineAnalysis -and $ReportData.TimelineAnalysis.WeeklyData.Count -gt 0) {
        # Generate chart data
        $sortedWeeks = $ReportData.TimelineAnalysis.WeeklyData.GetEnumerator() | Sort-Object { $_.Value.WeekStart }
        $labels = @()
        $commitData = @()
        $linesData = @()

        foreach ($week in $sortedWeeks) {
            $labels += "'$($week.Value.WeekStart.ToString("MM/dd"))'"
            $commitData += $week.Value.CommitCount
            $avgLines = if ($week.Value.CommitCount -gt 0) { [math]::Round($week.Value.TotalLinesChanged / $week.Value.CommitCount, 0) } else { 0 }
            $linesData += $avgLines
        }

        $chartLabels = $labels -join ','
        $chartCommitData = $commitData -join ','
        $chartLinesData = $linesData -join ','

        $html += @"

        <div class="section">
            <h2>📊 Commit Timeline</h2>
            <div class="chart-container">
                <canvas id="timelineChart" width="800" height="400"></canvas>
            </div>
            <div class="timeline-stats">
                <div class="metrics-grid">
                    <div class="metric-card">
                        <div class="metric-value">$($ReportData.TimelineAnalysis.Statistics.AverageCommitsPerWeek)</div>
                        <div class="metric-label">Avg Commits/Week</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">$($ReportData.TimelineAnalysis.Statistics.Consistency.Score)%</div>
                        <div class="metric-label">Consistency Score</div>
                    </div>
                </div>
            </div>
        </div>

        <div class="section">
            <h2>🔄 Commit Patterns</h2>
            <div class="pattern-grid">
                <div class="pattern-card">
                    <h4>📅 Timeline Overview</h4>
                    <div class="stat-row">
                        <span class="stat-label">First Commit:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.FirstCommit.ToString("yyyy-MM-dd"))</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Last Commit:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.LastCommit.ToString("yyyy-MM-dd"))</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Largest Gap:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.LargestGap.Days) days</span>
                    </div>
                </div>
                <div class="pattern-card">
                    <h4>📊 Frequency Stats</h4>
                    <div class="stat-row">
                        <span class="stat-label">Weekly Average:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.AverageCommitsPerWeek) commits</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Peak Week:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Count) commits</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Consistency:</span>
                        <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.Consistency.Rating)</span>
                    </div>
                </div>
            </div>
        </div>

        <script>
            const ctx = document.getElementById('timelineChart').getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [$chartLabels],
                    datasets: [{
                        label: 'Commits per Week',
                        data: [$chartCommitData],
                        borderColor: '#007acc',
                        backgroundColor: 'rgba(0, 122, 204, 0.1)',
                        tension: 0.4,
                        yAxisID: 'y'
                    }, {
                        label: 'Avg Lines per Commit',
                        data: [$chartLinesData],
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        tension: 0.4,
                        yAxisID: 'y1'
                    }]
                },
                options: {
                    responsive: true,
                    interaction: { intersect: false },
                    scales: {
                        x: { display: true, title: { display: true, text: 'Week' } },
                        y: { type: 'linear', display: true, position: 'left', title: { display: true, text: 'Commits' } },
                        y1: { type: 'linear', display: true, position: 'right', title: { display: true, text: 'Lines' }, grid: { drawOnChartArea: false } }
                    },
                    plugins: {
                        legend: { display: true },
                        title: { display: true, text: 'Weekly Commit Activity and Code Changes' }
                    }
                }
            });
        </script>
"@
    }

    # Add authors section
    $html += @"

        <div class="section">
            <h2>👥 Author Activity</h2>
            <div class="authors-list">
"@

    $topAuthors = $ReportData.Authors.GetEnumerator() | Sort-Object { $_.Value.TotalCommits } -Descending | Select-Object -First 10
    foreach ($author in $topAuthors) {
        $html += @"
                <div class="author-card">
                    <div class="author-name">$($author.Value.Name)</div>
                    <div class="stat-row">
                        <span class="stat-label">Commits:</span>
                        <span class="stat-value">$($author.Value.TotalCommits)</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Lines Added:</span>
                        <span class="stat-value">$($author.Value.TotalLinesAdded)</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Lines Deleted:</span>
                        <span class="stat-value">$($author.Value.TotalLinesDeleted)</span>
                    </div>
                </div>
"@
    }

    $html += @"
            </div>
        </div>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
}

# Main execution function
function Start-GitActivityReport {
    param(
        [int]$Days,
        [string]$OutputPath,
        [string]$OutputFormat,
        [string]$RepositoryPath
    )

    # Validate prerequisites
    if (-not (Test-GitAvailable)) { return }
    if (-not (Test-GitRepository $RepositoryPath)) { return }

    # Ensure output directory exists
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }

    Write-Host "🚀 Starting Git Activity Report Generation..." -ForegroundColor Green
    Write-Host "   Repository: $RepositoryPath" -ForegroundColor Gray
    Write-Host "   Analysis Period: $Days days" -ForegroundColor Gray
    Write-Host "   Output Format: $OutputFormat" -ForegroundColor Gray
    Write-Host ""

    try {
        # Collect data
        $commits = Get-CommitData -Days $Days -RepositoryPath $RepositoryPath
        $repository = Get-RepositoryInfo -RepositoryPath $RepositoryPath
        $metrics = Get-BasicMetrics -Commits $commits
        $authors = Get-AuthorAnalysis -Commits $commits
        $gitHubIntegration = Get-GitHubIntegration -Commits $commits
        $timelineAnalysis = Get-CommitTimelineAnalysis -Commits $commits

        # Create report data structure
        $reportData = @{
            GeneratedAt = Get-Date
            AnalysisPeriod = $Days
            Repository = $repository
            Metrics = $metrics
            Authors = $authors
            GitHubIntegration = $gitHubIntegration
            TimelineAnalysis = $timelineAnalysis
            RawCommits = $commits
        }

        # Generate reports based on format
        $generatedFiles = @()

        if ($OutputFormat -eq "Console" -or $OutputFormat -eq "All") {
            Show-ConsoleReport -ReportData $reportData
        }

        if ($OutputFormat -eq "JSON" -or $OutputFormat -eq "All") {
            $jsonFile = New-JsonReport -ReportData $reportData -OutputPath $OutputPath
            $generatedFiles += $jsonFile
            Write-Host "✅ JSON report saved: $jsonFile" -ForegroundColor Green
        }

        if ($OutputFormat -eq "HTML" -or $OutputFormat -eq "All") {
            $htmlFile = New-HtmlReport -ReportData $reportData -OutputPath $OutputPath
            $generatedFiles += $htmlFile
            Write-Host "✅ HTML report saved: $htmlFile" -ForegroundColor Green
        }

        if ($generatedFiles.Count -gt 0) {
            Write-Host "`n📁 Reports generated in: $OutputPath" -ForegroundColor Cyan
            foreach ($file in $generatedFiles) {
                Write-Host "   • $(Split-Path -Leaf $file)" -ForegroundColor Gray
            }
        }

        Write-Host "`n🎉 Git Activity Report completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to generate report: $($_.Exception.Message)"
        Write-Host "Stack trace:" -ForegroundColor Red
        Write-Host $_.ScriptStackTrace -ForegroundColor Red
    }
}

# Execute main function
Start-GitActivityReport -Days $Days -OutputPath $OutputPath -OutputFormat $OutputFormat -RepositoryPath $RepositoryPath
