param(
    [int]$Days = 30,
    [string]$OutputPath = ".",
    [ValidateSet("HTML", "JSON", "Console", "All")]
    [string]$OutputFormat = "All"
)

Write-Host "Starting GitHub Git Activity Report..." -ForegroundColor Green

# Test Git availability
try {
    $null = git --version
}
catch {
    Write-Error "Git is not available or not in PATH"
    exit 1
}

# Test Git repository
if (!(Test-Path ".git")) {
    Write-Error "Not in a Git repository"
    exit 1
}

# Get commit data
$sinceDate = (Get-Date).AddDays(-$Days).ToString("yyyy-MM-dd")
Write-Host "Analyzing $Days days of commit history since $sinceDate..." -ForegroundColor Cyan

$gitLogOutput = git log --since="$sinceDate" --pretty=format:"%H|%an|%ae|%ad|%s" --date=iso --numstat

if (-not $gitLogOutput) {
    Write-Warning "No commits found in the last $Days days"
    exit 0
}

# Parse commits
$commits = @()
$currentCommit = $null
$files = @()

foreach ($line in $gitLogOutput) {
    if ($line -match "^[a-f0-9]{40}") {
        # Save previous commit with safe measure
        if ($currentCommit) {
            $currentCommit.Files = $files
            $currentCommit.FilesTouched = $files.Count
            $totalAdded = 0
            $totalDeleted = 0
            foreach ($file in $files) {
                $totalAdded += $file.Added
                $totalDeleted += $file.Deleted
            }
            $currentCommit.LinesAdded = $totalAdded
            $currentCommit.LinesDeleted = $totalDeleted
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
    elseif ($line -match '^\d+\s+\d+\s+' -or $line -match '^-\s+-\s+') {
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

# Add last commit with safe measure
if ($currentCommit) {
    $currentCommit.Files = $files
    $currentCommit.FilesTouched = $files.Count
    $totalAdded = 0
    $totalDeleted = 0
    foreach ($file in $files) {
        $totalAdded += $file.Added
        $totalDeleted += $file.Deleted
    }
    $currentCommit.LinesAdded = $totalAdded
    $currentCommit.LinesDeleted = $totalDeleted
    $commits += $currentCommit
}

Write-Host "Found $($commits.Count) commits to analyze" -ForegroundColor Green

# Basic metrics with safe calculations
$totalCommits = $commits.Count
$totalLinesAdded = 0
$totalLinesDeleted = 0
foreach ($commit in $commits) {
    $totalLinesAdded += $commit.LinesAdded
    $totalLinesDeleted += $commit.LinesDeleted
}
$avgChurn = if ($totalCommits -gt 0) { [math]::Round(($totalLinesAdded + $totalLinesDeleted) / $totalCommits, 1) } else { 0 }

# GitHub integration analysis
$issueReferences = @()
$prMerges = 0
$conventionalCommits = 0

foreach ($commit in $commits) {
    $message = $commit.Message

    # Find issue references
    if ($message -match '#(\d+)') {
        $issueMatches = [regex]::Matches($message, '#(\d+)')
        foreach ($match in $issueMatches) {
            $issueReferences += $match.Groups[1].Value
        }
    }

    # PR merges
    if ($message -match "Merge pull request" -or $message -match "Merged PR") {
        $prMerges++
    }

    # Conventional commits
    if ($message -match "^(feat|fix|docs|style|refactor|test|chore)(\(.+\))?: ") {
        $conventionalCommits++
    }
}

$uniqueIssues = $issueReferences | Sort-Object -Unique

# Timeline analysis
$weeklyData = @{}
$firstCommit = $null
$lastCommit = $null
$totalTimespan = 0
$largestGap = @{ Days = 0; Description = "N/A" }

if ($commits.Count -gt 0) {
    $sortedCommits = $commits | Sort-Object Date

    foreach ($commit in $sortedCommits) {
        $weekStart = $commit.Date.AddDays(-([int]$commit.Date.DayOfWeek)).Date
        $weekKey = $weekStart.ToString("yyyy-MM-dd")

        if (-not $weeklyData.ContainsKey($weekKey)) {
            $weeklyData[$weekKey] = @{
                WeekStart = $weekStart
                CommitCount = 0
                TotalLinesChanged = 0
            }
        }

        $weeklyData[$weekKey].CommitCount++
        $weeklyData[$weekKey].TotalLinesChanged += ($commit.LinesAdded + $commit.LinesDeleted)
    }

    # Timeline statistics
    $commitCounts = $weeklyData.Values | ForEach-Object { $_.CommitCount }
    $averagePerWeek = if ($commitCounts.Count -gt 0) { [math]::Round(($commitCounts | Measure-Object -Average).Average, 1) } else { 0 }

    # Most active week
    $mostActiveWeek = $weeklyData.Values | Sort-Object CommitCount -Descending | Select-Object -First 1
    if (-not $mostActiveWeek) {
        $mostActiveWeek = @{ WeekStart = (Get-Date); CommitCount = 0; TotalLinesChanged = 0 }
    }

    # Consistency score
    $consistencyScore = if ($weeklyData.Values.Count -gt 1) {
        $variance = if ($commitCounts.Count -gt 1) {
            $mean = ($commitCounts | Measure-Object -Average).Average
            $squaredDiffs = $commitCounts | ForEach-Object { [math]::Pow($_ - $mean, 2) }
            ($squaredDiffs | Measure-Object -Average).Average
        } else { 0 }

        $coefficient = if ($averagePerWeek -gt 0) { [math]::Sqrt($variance) / $averagePerWeek } else { 1 }
        [math]::Max(0, [math]::Round((1 - [math]::Min($coefficient, 1)) * 100, 0))
    } else { 0 }

    $consistencyRating = switch ($consistencyScore) {
        {$_ -ge 80} { "Excellent" }
        {$_ -ge 60} { "Good" }
        {$_ -ge 40} { "Fair" }
        default { "Poor" }
    }

    # Pattern analysis with null checks
    $firstCommit = if ($sortedCommits.Count -gt 0) { $sortedCommits[0].Date } else { Get-Date }
    $lastCommit = if ($sortedCommits.Count -gt 0) { $sortedCommits[-1].Date } else { Get-Date }
    $totalTimespan = ($lastCommit - $firstCommit).Days

    # Calculate gaps
    $gaps = @()
    for ($i = 1; $i -lt $sortedCommits.Count; $i++) {
        $gap = ($sortedCommits[$i].Date - $sortedCommits[$i-1].Date).Days
        $gaps += $gap
    }

    $largestGap = if ($gaps.Count -gt 0 -and $sortedCommits.Count -gt 1) {
        $maxGap = ($gaps | Measure-Object -Maximum).Maximum
        $gapIndex = $gaps.IndexOf($maxGap)
        if ($gapIndex -ge 0 -and ($gapIndex + 1) -lt $sortedCommits.Count -and $sortedCommits[$gapIndex] -and $sortedCommits[$gapIndex + 1]) {
            @{
                Days = $maxGap
                Description = "Between $($sortedCommits[$gapIndex].Date.ToString('yyyy-MM-dd')) and $($sortedCommits[$gapIndex + 1].Date.ToString('yyyy-MM-dd'))"
            }
        } else {
            @{ Days = $maxGap; Description = "Gap analysis incomplete" }
        }
    } else {
        @{ Days = 0; Description = "N/A" }
    }
}

# Author analysis
$authors = @{}
foreach ($commit in $commits) {
    $authorKey = $commit.Author

    if (-not $authors.ContainsKey($authorKey)) {
        $authors[$authorKey] = @{
            Name = $commit.Author
            Email = $commit.Email
            TotalCommits = 0
            TotalLinesAdded = 0
            TotalLinesDeleted = 0
        }
    }

    $authors[$authorKey].TotalCommits++
    $authors[$authorKey].TotalLinesAdded += $commit.LinesAdded
    $authors[$authorKey].TotalLinesDeleted += $commit.LinesDeleted
}

# Console output
if ($OutputFormat -eq "Console" -or $OutputFormat -eq "All") {
    Write-Host ""
    Write-Host "="*80 -ForegroundColor Cyan
    Write-Host "  GIT ACTIVITY REPORT - $(Split-Path -Leaf (Get-Location))" -ForegroundColor Cyan
    Write-Host "="*80 -ForegroundColor Cyan

    Write-Host ""
    Write-Host "KEY METRICS" -ForegroundColor Yellow
    Write-Host "  Total Commits: $totalCommits"
    Write-Host "  Active Authors: $($authors.Count)"
    Write-Host "  Lines Added: $totalLinesAdded"
    Write-Host "  Lines Deleted: $totalLinesDeleted"
    Write-Host "  Avg Lines/Commit: $avgChurn"

    Write-Host ""
    Write-Host "GITHUB INTEGRATION" -ForegroundColor Yellow
    Write-Host "  Issue References: $($issueReferences.Count)"
    Write-Host "  Unique Issues: $($uniqueIssues.Count)"
    Write-Host "  PR Merges: $prMerges"
    Write-Host "  Conventional Commits: $conventionalCommits"

    if ($weeklyData.Count -gt 0 -and $firstCommit -and $lastCommit) {
        Write-Host ""
        Write-Host "TIMELINE ANALYSIS" -ForegroundColor Yellow
        Write-Host "  Weekly Average: $averagePerWeek commits"
        Write-Host "  Peak Week: $($mostActiveWeek.CommitCount) commits"
        Write-Host "  Consistency Score: $consistencyScore% ($consistencyRating)"
        $firstCommitStr = if ($firstCommit) { $firstCommit.ToString('yyyy-MM-dd') } else { "N/A" }
        $lastCommitStr = if ($lastCommit) { $lastCommit.ToString('yyyy-MM-dd') } else { "N/A" }
        $largestGapDays = if ($largestGap) { $largestGap.Days } else { 0 }
        Write-Host "  First Commit: $firstCommitStr"
        Write-Host "  Last Commit: $lastCommitStr"
        Write-Host "  Largest Gap: $largestGapDays days"

        Write-Host ""
        Write-Host "WEEKLY TIMELINE" -ForegroundColor Yellow
        $sortedWeeks = $weeklyData.GetEnumerator() | Sort-Object { $_.Value.WeekStart }
        foreach ($week in $sortedWeeks) {
            $weekLabel = $week.Value.WeekStart.ToString("MM/dd")
            $commitCount = $week.Value.CommitCount
            $bar = "#" * [math]::Min($commitCount, 40)
            Write-Host "  $weekLabel : $bar ($commitCount commits)"
        }
    }

    Write-Host ""
    Write-Host "TOP AUTHORS" -ForegroundColor Yellow
    $topAuthors = $authors.GetEnumerator() | Sort-Object { $_.Value.TotalCommits } -Descending | Select-Object -First 5
    foreach ($author in $topAuthors) {
        Write-Host "  $($author.Value.Name): $($author.Value.TotalCommits) commits (+$($author.Value.TotalLinesAdded)/-$($author.Value.TotalLinesDeleted) lines)"
    }

    Write-Host ""
    Write-Host "="*80 -ForegroundColor Cyan
}

# HTML output
if ($OutputFormat -eq "HTML" -or $OutputFormat -eq "All") {
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $htmlFile = Join-Path $OutputPath "git-activity-report-$timestamp.html"

    # Chart data
    $chartLabels = @()
    $chartCommitData = @()
    $chartLinesData = @()

    if ($weeklyData.Count -gt 0) {
        $sortedWeeks = $weeklyData.GetEnumerator() | Sort-Object { $_.Value.WeekStart }
        foreach ($week in $sortedWeeks) {
            $chartLabels += "'$($week.Value.WeekStart.ToString("MM/dd"))'"
            $chartCommitData += $week.Value.CommitCount
            $avgLines = if ($week.Value.CommitCount -gt 0) { [math]::Round($week.Value.TotalLinesChanged / $week.Value.CommitCount, 0) } else { 0 }
            $chartLinesData += $avgLines
        }
    }

    $labelsJs = $chartLabels -join ','
    $commitDataJs = $chartCommitData -join ','
    $linesDataJs = $chartLinesData -join ','

    $repoName = Split-Path -Leaf (Get-Location)

    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Git Activity Report - $repoName</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; background: #f5f5f5; color: #333; }
        .container { max-width: 1200px; margin: 20px auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); }
        .header { text-align: center; border-bottom: 3px solid #007acc; padding-bottom: 20px; margin-bottom: 30px; }
        .header h1 { color: #007acc; margin-bottom: 10px; }
        .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .metric-card { background: linear-gradient(135deg, #f8f9ff, #e3f2fd); padding: 20px; border-radius: 10px; text-align: center; border-left: 5px solid #007acc; box-shadow: 0 2px 10px rgba(0,0,0,0.05); }
        .metric-value { font-size: 2.2em; font-weight: bold; color: #007acc; margin-bottom: 5px; }
        .metric-label { color: #666; font-size: 0.9em; text-transform: uppercase; letter-spacing: 1px; }
        .section { margin-bottom: 40px; }
        .section h2 { color: #333; border-bottom: 2px solid #007acc; padding-bottom: 10px; margin-bottom: 20px; }
        .chart-container { margin: 30px 0; text-align: center; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.05); }
        .pattern-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
        .pattern-card { background: linear-gradient(135deg, #f0f8ff, #e8f5e8); padding: 20px; border-radius: 10px; border-left: 5px solid #28a745; }
        .pattern-card h4 { color: #28a745; margin-top: 0; }
        .stat-row { display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }
        .stat-label { font-weight: bold; }
        .stat-value { font-family: 'Courier New', monospace; color: #666; }
        .authors-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 15px; }
        .author-card { background: #fff; border: 1px solid #ddd; padding: 15px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }
        .author-name { font-weight: bold; color: #007acc; margin-bottom: 5px; }
        .footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Git Activity Report</h1>
            <p><strong>Repository:</strong> $repoName</p>
            <p><strong>Analysis Period:</strong> Last $Days days</p>
            <p><strong>Generated:</strong> $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>
        </div>

        <div class="section">
            <h2>Key Metrics</h2>
            <div class="metrics-grid">
                <div class="metric-card">
                    <div class="metric-value">$totalCommits</div>
                    <div class="metric-label">Total Commits</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($authors.Count)</div>
                    <div class="metric-label">Active Authors</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$totalLinesAdded</div>
                    <div class="metric-label">Lines Added</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($issueReferences.Count)</div>
                    <div class="metric-label">Issue References</div>
                </div>
            </div>
        </div>

        <div class="section">
            <h2>GitHub Integration</h2>
            <div class="metrics-grid">
                <div class="metric-card">
                    <div class="metric-value">$prMerges</div>
                    <div class="metric-label">PR Merges</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$conventionalCommits</div>
                    <div class="metric-label">Conventional Commits</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($uniqueIssues.Count)</div>
                    <div class="metric-label">Unique Issues</div>
                </div>
            </div>
        </div>
"@

    if ($weeklyData.Count -gt 0) {
        $html += @"

        <div class="section">
            <h2>Commit Timeline</h2>
            <div class="chart-container">
                <canvas id="timelineChart" width="800" height="400"></canvas>
            </div>
            <div class="metrics-grid">
                <div class="metric-card">
                    <div class="metric-value">$averagePerWeek</div>
                    <div class="metric-label">Avg Commits/Week</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$consistencyScore%</div>
                    <div class="metric-label">Consistency Score</div>
                </div>
                <div class="metric-card">
                    <div class="metric-value">$($mostActiveWeek.CommitCount)</div>
                    <div class="metric-label">Peak Week Commits</div>
                </div>
            </div>
        </div>

        <div class="section">
            <h2>Commit Patterns</h2>
            <div class="pattern-grid">
                <div class="pattern-card">
                    <h4>Timeline Overview</h4>
                    <div class="stat-row">
                        <span class="stat-label">First Commit:</span>
                        <span class="stat-value">$(if ($firstCommit) { $firstCommit.ToString("yyyy-MM-dd") } else { "N/A" })</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Last Commit:</span>
                        <span class="stat-value">$(if ($lastCommit) { $lastCommit.ToString("yyyy-MM-dd") } else { "N/A" })</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Total Timespan:</span>
                        <span class="stat-value">$totalTimespan days</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Largest Gap:</span>
                        <span class="stat-value">$(if ($largestGap) { $largestGap.Days } else { 0 }) days</span>
                    </div>
                </div>
                <div class="pattern-card">
                    <h4>Activity Statistics</h4>
                    <div class="stat-row">
                        <span class="stat-label">Weekly Average:</span>
                        <span class="stat-value">$averagePerWeek commits</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Peak Week:</span>
                        <span class="stat-value">$($mostActiveWeek.CommitCount) commits</span>
                    </div>
                    <div class="stat-row">
                        <span class="stat-label">Consistency:</span>
                        <span class="stat-value">$consistencyRating</span>
                    </div>
                </div>
            </div>
        </div>

        <script>
            const ctx = document.getElementById('timelineChart').getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [$labelsJs],
                    datasets: [{
                        label: 'Commits per Week',
                        data: [$commitDataJs],
                        borderColor: '#007acc',
                        backgroundColor: 'rgba(0, 122, 204, 0.1)',
                        tension: 0.4,
                        yAxisID: 'y'
                    }, {
                        label: 'Avg Lines per Commit',
                        data: [$linesDataJs],
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
                        title: { display: true, text: 'Weekly Commit Activity and Code Changes' }
                    }
                }
            });
        </script>
"@
    }

    $html += @"

        <div class="section">
            <h2>Author Activity</h2>
            <div class="authors-grid">
"@

    $topAuthors = $authors.GetEnumerator() | Sort-Object { $_.Value.TotalCommits } -Descending | Select-Object -First 10
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

        <div class="footer">
            <p>Generated by Git Activity Report Tool | $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>
        </div>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $htmlFile -Encoding UTF8
    Write-Host "HTML report saved: $htmlFile" -ForegroundColor Green
}

# JSON output
if ($OutputFormat -eq "JSON" -or $OutputFormat -eq "All") {
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $jsonFile = Join-Path $OutputPath "git-activity-report-$timestamp.json"

    $reportData = @{
        GeneratedAt = Get-Date
        AnalysisPeriod = $Days
        Repository = @{ Name = (Split-Path -Leaf (Get-Location)) }
        Metrics = @{
            TotalCommits = $totalCommits
            TotalLinesAdded = $totalLinesAdded
            TotalLinesDeleted = $totalLinesDeleted
            AverageChurnPerCommit = $avgChurn
        }
        GitHubIntegration = @{
            IssueReferences = @{ Total = $issueReferences.Count; UniqueIssues = $uniqueIssues }
            PullRequestMerges = $prMerges
            ConventionalCommits = $conventionalCommits
        }
        TimelineAnalysis = @{
            WeeklyData = $weeklyData
            Statistics = @{
                AverageCommitsPerWeek = $averagePerWeek
                ConsistencyScore = $consistencyScore
                ConsistencyRating = $consistencyRating
            }
        }
        Authors = $authors
    }

    $reportData | ConvertTo-Json -Depth 5 | Out-File -FilePath $jsonFile -Encoding UTF8
    Write-Host "JSON report saved: $jsonFile" -ForegroundColor Green
}

Write-Host ""
Write-Host "Git Activity Report completed successfully!" -ForegroundColor Green
