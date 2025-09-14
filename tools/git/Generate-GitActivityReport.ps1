#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates a comprehensive Git repository activity report with GitHub best practices metrics

.DESCRIPTION
    This script analyzes Git repository activity and generates a comprehensive report for GitHub repositories including:
    - GitHub specific metrics (Pull Request analysis, Issue linking, Code Review patterns)
    - Developer productivity and velocity tracking with DORA metrics
    - Code quality and churn analysis with GitHub Actions integration
    - Commit analysis with detailed summary table for recent commits
    - Author productivity and collaboration patterns
    - Pull Request metrics (review time, approval patterns, merge success rate)
    - Issue linking analysis and traceability metrics
    - File modification patterns and hotspot analysis
    - GitHub best practices assessment and recommendations
    - Team velocity and collaboration health metrics
    - Integration health between GitHub repositories, issues, and actions

.PARAMETER Days
    Number of days to analyze (default: 10)

.PARAMETER OutputPath
    Path where the report will be saved (default: current directory)

.PARAMETER Format
    Output format: HTML, JSON, or Console (default: HTML)

.PARAMETER CommitSummaryCount
    Number of recent commits to include in detailed summary table (default: 50)

.EXAMPLE
    .\Generate-GitActivityReport.ps1
    .\Generate-GitActivityReport.ps1 -Days 14 -Format JSON
    .\Generate-GitActivityReport.ps1 -OutputPath "C:\Reports" -Format HTML

.NOTES
    Author: Mark Hazleton
    Version: 3.0
    Requires: Git, PowerShell 5.1+
    Enhanced with GitHub best practices and DORA metrics
#>

[CmdletBinding()]
param(
    [Parameter()]
    [int]$Days = 10,

    [Parameter()]
    [string]$OutputPath = ".\temp\reports",

    [Parameter()]
    [ValidateSet("HTML", "JSON", "Console")]
    [string]$Format = "HTML",

    [Parameter()]
    [int]$CommitSummaryCount = 50
)

# Ensure we're in a Git repository
if (-not (Test-Path ".git")) {
    Write-Error "This script must be run from within a Git repository."
    exit 1
}

# Initialize variables
$reportData = @{
    GeneratedAt          = Get-Date
    AnalysisPeriod       = $Days
    Repository           = @{}
    Metrics              = @{}
    GitHubIntegration    = @{}
    PullRequestMetrics   = @{}
    IssueIntegration     = @{}
    CodeReviewMetrics    = @{}
    TeamVelocityMetrics  = @{}
    QualityMetrics       = @{}
    Authors              = @{}
    Files                = @{}
    Commits              = @()
    CommitSummary        = @()
    TimelineAnalysis     = @{}
    Insights             = @{}
    Recommendations      = @()
    CollaborationMetrics = @{}
    GitHubActionsMetrics = @{}
}

Write-Host "🔍 Analyzing Git repository activity for the last $Days days..." -ForegroundColor Cyan

# Get repository information
function Get-RepositoryInfo {
    $repoInfo = @{}

    try {
        $repoInfo.Name = (git rev-parse --show-toplevel | Split-Path -Leaf)
        $repoInfo.Branch = git rev-parse --abbrev-ref HEAD
        $repoInfo.RemoteUrl = git config --get remote.origin.url
        $repoInfo.LastCommit = git log -1 --format="%H|%an|%ad|%s" --date=iso
        $repoInfo.TotalCommits = [int](git rev-list --count HEAD)
        $repoInfo.TotalBranches = [int](git branch -a | Measure-Object).Count
        $repoInfo.TotalTags = [int](git tag | Measure-Object).Count
    }
    catch {
        Write-Warning "Could not retrieve some repository information: $_"
    }

    return $repoInfo
}

# Get commit data for analysis period
function Get-CommitData {
    param([int]$Days)

    $sinceDate = (Get-Date).AddDays(-$Days).ToString("yyyy-MM-dd")
    $commits = @()

    try {
        $gitLog = git log --since="$sinceDate" --format="%H|%an|%ae|%ad|%s|%P" --date=iso --numstat

        $currentCommit = $null
        $files = @()

        foreach ($line in $gitLog) {
            if ($line -match "^[a-f0-9]{40}\|") {
                # Save previous commit if exists
                if ($currentCommit) {
                    $currentCommit.Files = $files
                    $commits += $currentCommit
                }

                # Parse new commit
                $parts = $line -split '\|'
                $currentCommit = @{
                    Hash    = $parts[0]
                    Author  = $parts[1]
                    Email   = $parts[2]
                    Date    = [DateTime]::Parse($parts[3])
                    Message = $parts[4]
                    Parents = if ($parts[5]) { $parts[5] -split ' ' } else { @() }
                    IsMerge = ($parts[5] -split ' ').Count -gt 1
                }
                $files = @()
            }
            elseif ($line -match "^\d+\s+\d+\s+.*" -or $line -match "^-\s+-\s+.*") {
                # Parse file changes
                $fileParts = $line -split '\s+', 3
                if ($fileParts.Count -ge 3) {
                    $files += @{
                        Added   = if ($fileParts[0] -eq '-') { 0 } else { [int]$fileParts[0] }
                        Deleted = if ($fileParts[1] -eq '-') { 0 } else { [int]$fileParts[1] }
                        Path    = $fileParts[2]
                    }
                }
            }
        }

        # Add last commit
        if ($currentCommit) {
            $currentCommit.Files = $files
            $commits += $currentCommit
        }
    }
    catch {
        Write-Warning "Could not retrieve commit data: $_"
    }

    return $commits
}

# Get detailed commit summary for recent commits
function Get-CommitSummary {
    param([int]$Count)

    $commits = @()

    try {
        $gitLog = git log -$Count --format="%H|%an|%ae|%ad|%s|%P" --date=iso --numstat

        $currentCommit = $null
        $files = @()

        foreach ($line in $gitLog) {
            if ($line -match "^[a-f0-9]{40}\|") {
                # Save previous commit if exists
                if ($currentCommit) {
                    $totalAdded = ($files | Measure-Object -Property Added -Sum).Sum
                    $totalDeleted = ($files | Measure-Object -Property Deleted -Sum).Sum
                    $currentCommit.LinesAdded = $totalAdded
                    $currentCommit.LinesDeleted = $totalDeleted
                    $currentCommit.FilesTouched = $files.Count
                    $currentCommit.Files = $files
                    $commits += $currentCommit
                }

                # Parse new commit
                $parts = $line -split '\|'
                $currentCommit = @{
                    Hash         = $parts[0]
                    ShortHash    = $parts[0].Substring(0, 8)
                    Author       = $parts[1]
                    Email        = $parts[2]
                    Date         = [DateTime]::Parse($parts[3])
                    Message      = $parts[4]
                    Parents      = if ($parts[5]) { $parts[5] -split ' ' } else { @() }
                    IsMerge      = ($parts[5] -split ' ').Count -gt 1
                    LinesAdded   = 0
                    LinesDeleted = 0
                    FilesTouched = 0
                }
                $files = @()
            }
            elseif ($line -match "^\d+\s+\d+\s+.*" -or $line -match "^-\s+-\s+.*") {
                # Parse file changes
                $fileParts = $line -split '\s+', 3
                if ($fileParts.Count -ge 3) {
                    $files += @{
                        Added   = if ($fileParts[0] -eq '-') { 0 } else { [int]$fileParts[0] }
                        Deleted = if ($fileParts[1] -eq '-') { 0 } else { [int]$fileParts[1] }
                        Path    = $fileParts[2]
                    }
                }
            }
        }

        # Add last commit
        if ($currentCommit) {
            $totalAdded = ($files | Measure-Object -Property Added -Sum).Sum
            $totalDeleted = ($files | Measure-Object -Property Deleted -Sum).Sum
            $currentCommit.LinesAdded = $totalAdded
            $currentCommit.LinesDeleted = $totalDeleted
            $currentCommit.FilesTouched = $files.Count
            $currentCommit.Files = $files
            $commits += $currentCommit
        }
    }
    catch {
        Write-Warning "Could not retrieve commit summary data: $_"
    }

    return $commits
}

# Analyze GitHub integration patterns
function Get-GitHubIntegration {
    param([array]$Commits)

    $integration = @{
        IssueReferences      = @{
            Total        = 0
            UniqueIssues = @{}
            ByAuthor     = @{}
            Patterns     = @{
                Fixes      = 0
                Closes     = 0
                References = 0
                Resolves   = 0
            }
        }
        PullRequestPatterns  = @{
            MergeCommits      = 0
            PullRequestMerges = 0
            SquashMerges      = 0
            DirectCommits     = 0
            RevertCommits     = 0
        }
        BranchPatterns       = @{
            FeatureBranches = 0
            BugfixBranches  = 0
            HotfixBranches  = 0
            ReleaseBranches = 0
            DevelopBranches = 0
            MainBranches    = 0
        }
        CommitMessageQuality = @{
            TotalMessages          = 0
            AverageLength          = 0
            WithIssueReferences    = 0
            WithConventionalFormat = 0
            EmptyOrShort           = 0
            WithCoAuthoredBy       = 0
            WithBreakingChanges    = 0
        }
        ConventionalCommits  = @{
            Feat     = 0
            Fix      = 0
            Docs     = 0
            Style    = 0
            Refactor = 0
            Perf     = 0
            Test     = 0
            Chore    = 0
            Build    = 0
            Ci       = 0
        }
    }

    foreach ($commit in $Commits) {
        $message = $commit.Message
        $author = $commit.Author

        # Initialize author tracking
        if (-not $integration.IssueReferences.ByAuthor[$author]) {
            $integration.IssueReferences.ByAuthor[$author] = @{
                Issues = @{}
                Count  = 0
            }
        }

        # Analyze commit message quality
        $integration.CommitMessageQuality.TotalMessages++
        if ($message.Length -lt 10) {
            $integration.CommitMessageQuality.EmptyOrShort++
        }

        # Look for issue references (GitHub patterns)
        $issueMatches = [regex]::Matches($message, '\b(?:#|GH-|gh-)(\d+)\b', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $issueMatches) {
            $issueId = $match.Groups[1].Value
            $integration.IssueReferences.UniqueIssues[$issueId] = $true
            $integration.IssueReferences.ByAuthor[$author].Issues[$issueId] = $true
            $integration.IssueReferences.ByAuthor[$author].Count++
            $integration.IssueReferences.Total++
        }

        # Check for issue action patterns (GitHub keywords)
        if ($message -match '\b(fix|fixes|fixed)\s+#?\d+\b' -or $message -match '\b(fix|fixes|fixed)\s+(?:GH-|gh-)\d+\b') {
            $integration.IssueReferences.Patterns.Fixes++
            $integration.CommitMessageQuality.WithIssueReferences++
        }
        if ($message -match '\b(close|closes|closed)\s+#?\d+\b' -or $message -match '\b(close|closes|closed)\s+(?:GH-|gh-)\d+\b') {
            $integration.IssueReferences.Patterns.Closes++
            $integration.CommitMessageQuality.WithIssueReferences++
        }
        if ($message -match '\b(resolve|resolves|resolved)\s+#?\d+\b' -or $message -match '\b(resolve|resolves|resolved)\s+(?:GH-|gh-)\d+\b') {
            $integration.IssueReferences.Patterns.Resolves++
            $integration.CommitMessageQuality.WithIssueReferences++
        }
        if ($message -match '\b(?:ref|reference|references|see|relates?\s+to)\s+#?\d+\b') {
            $integration.IssueReferences.Patterns.References++
            $integration.CommitMessageQuality.WithIssueReferences++
        }

        # Check for Co-authored-by (GitHub collaboration pattern)
        if ($message -match 'Co-authored-by:') {
            $integration.CommitMessageQuality.WithCoAuthoredBy++
        }

        # Check for breaking changes
        if ($message -match 'BREAKING CHANGE:?' -or $message -match '!\s*:') {
            $integration.CommitMessageQuality.WithBreakingChanges++
        }

        # Analyze merge patterns
        if ($commit.IsMerge) {
            $integration.PullRequestPatterns.MergeCommits++

            if ($message -match 'Merge pull request #\d+' -or $message -match 'Merged PR #\d+') {
                $integration.PullRequestPatterns.PullRequestMerges++
            }
            if ($message -match '\(\#\d+\)$') {
                $integration.PullRequestPatterns.SquashMerges++
            }
        }
        else {
            $integration.PullRequestPatterns.DirectCommits++
        }

        # Check for revert commits
        if ($message -match '^Revert\s+') {
            $integration.PullRequestPatterns.RevertCommits++
        }

        # Check for conventional commit format
        $conventionalPattern = '^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?\s*!?\s*:\s*.+'
        if ($message -match $conventionalPattern) {
            $integration.CommitMessageQuality.WithConventionalFormat++

            # Count specific conventional commit types
            if ($message -match '^feat') { $integration.ConventionalCommits.Feat++ }
            elseif ($message -match '^fix') { $integration.ConventionalCommits.Fix++ }
            elseif ($message -match '^docs') { $integration.ConventionalCommits.Docs++ }
            elseif ($message -match '^style') { $integration.ConventionalCommits.Style++ }
            elseif ($message -match '^refactor') { $integration.ConventionalCommits.Refactor++ }
            elseif ($message -match '^perf') { $integration.ConventionalCommits.Perf++ }
            elseif ($message -match '^test') { $integration.ConventionalCommits.Test++ }
            elseif ($message -match '^chore') { $integration.ConventionalCommits.Chore++ }
            elseif ($message -match '^build') { $integration.ConventionalCommits.Build++ }
            elseif ($message -match '^ci') { $integration.ConventionalCommits.Ci++ }
        }
    }

    # Calculate averages
    if ($integration.CommitMessageQuality.TotalMessages -gt 0) {
        $totalLength = ($Commits | ForEach-Object { $_.Message.Length } | Measure-Object -Sum).Sum
        $integration.CommitMessageQuality.AverageLength = [math]::Round($totalLength / $integration.CommitMessageQuality.TotalMessages, 1)
    }

    return $integration
}

# Analyze code review and collaboration metrics
function Get-CodeReviewMetrics {
    param([array]$Commits, [hashtable]$Authors)

    $codeReview = @{
        MergeCommitRatio     = 0
        AverageCommitSize    = 0
        LargeCommitThreshold = 500
        LargeCommits         = 0
        SmallCommits         = 0
        ReviewableCommits    = 0
        CollaborationScore   = 0
        AuthorDistribution   = @{
            SingleAuthor           = 0
            MultipleAuthors        = 0
            PrimaryContributors    = 0
            OccasionalContributors = 0
        }
        TimePatterns         = @{
            BusinessHours = 0
            AfterHours    = 0
            Weekends      = 0
            EarlyMorning  = 0
            LateNight     = 0
        }
    }

    $totalCommits = $Commits.Count
    $totalChurn = 0

    foreach ($commit in $Commits) {
        $commitSize = 0
        foreach ($file in $commit.Files) {
            $commitSize += $file.Added + $file.Deleted
        }
        $totalChurn += $commitSize

        # Categorize commit sizes
        if ($commitSize -gt $codeReview.LargeCommitThreshold) {
            $codeReview.LargeCommits++
        }
        elseif ($commitSize -lt 50) {
            $codeReview.SmallCommits++
        }
        else {
            $codeReview.ReviewableCommits++
        }

        # Analyze time patterns
        $hour = $commit.Date.Hour
        $dayOfWeek = $commit.Date.DayOfWeek

        if ($hour -ge 9 -and $hour -le 17 -and $dayOfWeek -ne [System.DayOfWeek]::Saturday -and $dayOfWeek -ne [System.DayOfWeek]::Sunday) {
            $codeReview.TimePatterns.BusinessHours++
        }
        else {
            $codeReview.TimePatterns.AfterHours++
        }

        if ($dayOfWeek -eq [System.DayOfWeek]::Saturday -or $dayOfWeek -eq [System.DayOfWeek]::Sunday) {
            $codeReview.TimePatterns.Weekends++
        }

        if ($hour -ge 5 -and $hour -le 8) {
            $codeReview.TimePatterns.EarlyMorning++
        }
        elseif ($hour -ge 22 -or $hour -le 4) {
            $codeReview.TimePatterns.LateNight++
        }
    }

    # Calculate metrics
    if ($totalCommits -gt 0) {
        $mergeCommits = ($Commits | Where-Object { $_.IsMerge }).Count
        $codeReview.MergeCommitRatio = [math]::Round($mergeCommits / $totalCommits, 3)
        $codeReview.AverageCommitSize = [math]::Round($totalChurn / $totalCommits, 1)
    }

    # Analyze author distribution
    $authorCommitCounts = $Authors.GetEnumerator() | ForEach-Object { $_.Value.Commits }
    $totalAuthorCommits = ($authorCommitCounts | Measure-Object -Sum).Sum

    foreach ($authorData in $Authors.GetEnumerator()) {
        $commitPercentage = if ($totalAuthorCommits -gt 0) { $authorData.Value.Commits / $totalAuthorCommits } else { 0 }

        if ($commitPercentage -ge 0.4) {
            $codeReview.AuthorDistribution.PrimaryContributors++
        }
        elseif ($commitPercentage -ge 0.1) {
            $codeReview.AuthorDistribution.SingleAuthor++
        }
        else {
            $codeReview.AuthorDistribution.OccasionalContributors++
        }
    }

    # Calculate collaboration score (0-100)
    $collaborationFactors = @(
        [math]::Min($codeReview.MergeCommitRatio * 100, 40)  # Max 40 points for merge commits
        [math]::Min($Authors.Count * 10, 30)                 # Max 30 points for multiple authors
        if ($codeReview.ReviewableCommits -gt 0) { [math]::Min(($codeReview.ReviewableCommits / $totalCommits) * 30, 30) } else { 0 }  # Max 30 points for reviewable commit sizes
    )
    $codeReview.CollaborationScore = [math]::Round(($collaborationFactors | Measure-Object -Sum).Sum, 1)

    return $codeReview
}

# Calculate code churn metrics
function Get-CodeChurnMetrics {
    param([array]$Commits)

    $metrics = @{
        TotalCommits          = $Commits.Count
        TotalLinesAdded       = 0
        TotalLinesDeleted     = 0
        TotalLinesChanged     = 0
        TotalFilesChanged     = 0
        AverageChurnPerCommit = 0
        ChurnRate             = 0
        FileChurnDistribution = @{}
        DailyChurn            = @{}
    }

    $uniqueFiles = @{}
    $dailyStats = @{}

    foreach ($commit in $Commits) {
        $commitAdded = 0
        $commitDeleted = 0
        $commitDate = $commit.Date.ToString("yyyy-MM-dd")

        if (-not $dailyStats[$commitDate]) {
            $dailyStats[$commitDate] = @{
                Commits      = 0
                LinesAdded   = 0
                LinesDeleted = 0
                FilesChanged = 0
            }
        }

        $dailyStats[$commitDate].Commits++

        foreach ($file in $commit.Files) {
            $commitAdded += $file.Added
            $commitDeleted += $file.Deleted
            $uniqueFiles[$file.Path] = $true

            if (-not $metrics.FileChurnDistribution[$file.Path]) {
                $metrics.FileChurnDistribution[$file.Path] = @{
                    Added   = 0
                    Deleted = 0
                    Commits = 0
                }
            }

            $metrics.FileChurnDistribution[$file.Path].Added += $file.Added
            $metrics.FileChurnDistribution[$file.Path].Deleted += $file.Deleted
            $metrics.FileChurnDistribution[$file.Path].Commits++
        }

        $dailyStats[$commitDate].LinesAdded += $commitAdded
        $dailyStats[$commitDate].LinesDeleted += $commitDeleted
        $dailyStats[$commitDate].FilesChanged += $commit.Files.Count

        $metrics.TotalLinesAdded += $commitAdded
        $metrics.TotalLinesDeleted += $commitDeleted
    }

    $metrics.TotalLinesChanged = $metrics.TotalLinesAdded + $metrics.TotalLinesDeleted
    $metrics.TotalFilesChanged = $uniqueFiles.Count
    $metrics.AverageChurnPerCommit = if ($metrics.TotalCommits -gt 0) {
        [math]::Round($metrics.TotalLinesChanged / $metrics.TotalCommits, 2)
    }
    else { 0 }
    $metrics.ChurnRate = $metrics.TotalLinesDeleted / [math]::Max($metrics.TotalLinesAdded, 1)
    $metrics.DailyChurn = $dailyStats

    return $metrics
}

# Analyze author productivity
function Get-AuthorAnalysis {
    param([array]$Commits)

    $authors = @{}

    foreach ($commit in $Commits) {
        $author = $commit.Author
        if (-not $authors[$author]) {
            $authors[$author] = @{
                Email          = $commit.Email
                Commits        = 0
                LinesAdded     = 0
                LinesDeleted   = 0
                FilesChanged   = @{}
                FirstCommit    = $commit.Date
                LastCommit     = $commit.Date
                MergeCommits   = 0
                CommitMessages = @()
                Productivity   = @{
                    AverageCommitSize = 0
                    FilesDiversity    = 0
                    CommitFrequency   = 0
                }
                BiggestCommit  = @{
                    Size    = 0
                    Hash    = ""
                    Date    = $commit.Date
                    Message = ""
                }
            }
        }
        $authorData = $authors[$author]
        $authorData.Commits++
        if ($commit.IsMerge) {
            $authorData.MergeCommits++
        }
        if ($commit.Date -lt $authorData.FirstCommit) {
            $authorData.FirstCommit = $commit.Date
        }
        if ($commit.Date -gt $authorData.LastCommit) {
            $authorData.LastCommit = $commit.Date
        }
        $authorData.CommitMessages += $commit.Message
        $commitSize = 0
        foreach ($file in $commit.Files) {
            $authorData.LinesAdded += $file.Added
            $authorData.LinesDeleted += $file.Deleted
            $authorData.FilesChanged[$file.Path] = $true
            $commitSize += $file.Added + $file.Deleted
        }
        if ($commitSize -gt $authorData.BiggestCommit.Size) {
            $authorData.BiggestCommit.Size = $commitSize
            $authorData.BiggestCommit.Hash = $commit.Hash
            $authorData.BiggestCommit.Date = $commit.Date
            $authorData.BiggestCommit.Message = $commit.Message
        }
    }
    # Calculate productivity metrics
    foreach ($author in $authors.Keys) {
        $authorData = $authors[$author]
        $totalLines = $authorData.LinesAdded + $authorData.LinesDeleted
        $authorData.Productivity.AverageCommitSize = if ($authorData.Commits -gt 0) {
            [math]::Round($totalLines / $authorData.Commits, 2)
        }
        else { 0 }
        $authorData.Productivity.FilesDiversity = $authorData.FilesChanged.Count
        $daysDiff = ($authorData.LastCommit - $authorData.FirstCommit).Days
        $authorData.Productivity.CommitFrequency = if ($daysDiff -gt 0) {
            [math]::Round($authorData.Commits / $daysDiff, 2)
        }
        else { $authorData.Commits }
    }
    return $authors
}

# Analyze commit timeline and patterns
function Get-CommitTimelineAnalysis {
    param([array]$Commits)

    $timeline = @{
        WeeklyData = @{}
        Patterns   = @{
            FirstCommit       = $null
            LastCommit        = $null
            TotalTimespan     = $null
            LargestGap        = @{
                Days        = 0
                StartDate   = $null
                EndDate     = $null
                Description = ""
            }
            CommitFrequency   = @{
                Daily     = @{}
                Weekly    = @{}
                Monthly   = @{}
                Hourly    = @(0) * 24
                DayOfWeek = @(0) * 7
            }
            ActivityStreak    = @{
                Current      = 0
                Longest      = 0
                LongestStart = $null
                LongestEnd   = $null
            }
            InactivityPeriods = @()
        }
        Statistics = @{
            AverageCommitsPerWeek    = 0
            MedianCommitsPerWeek     = 0
            AverageGapBetweenCommits = 0
            MostActiveWeek           = @{
                Week         = ""
                Count        = 0
                LinesChanged = 0
            }
            LeastActiveWeek          = @{
                Week  = ""
                Count = 999999
            }
            Consistency              = @{
                Score       = 0
                Rating      = ""
                Description = ""
            }
        }
    }

    if ($Commits.Count -eq 0) {
        return $timeline
    }

    # Sort commits by date
    $sortedCommits = $Commits | Sort-Object Date

    # Set first and last commit dates
    $timeline.Patterns.FirstCommit = $sortedCommits[0].Date
    $timeline.Patterns.LastCommit = $sortedCommits[-1].Date
    $timeline.Patterns.TotalTimespan = ($timeline.Patterns.LastCommit - $timeline.Patterns.FirstCommit).Days

    # Build weekly timeline data
    foreach ($commit in $sortedCommits) {
        $weekStart = $commit.Date.AddDays( - [int]$commit.Date.DayOfWeek).Date
        $weekKey = $weekStart.ToString("yyyy-MM-dd")

        if (-not $timeline.WeeklyData[$weekKey]) {
            $timeline.WeeklyData[$weekKey] = @{
                WeekStart             = $weekStart
                WeekEnd               = $weekStart.AddDays(6)
                CommitCount           = 0
                TotalLinesAdded       = 0
                TotalLinesDeleted     = 0
                TotalLinesChanged     = 0
                UniqueAuthors         = @{}
                CommitHashes          = @()
                AverageLinesPerCommit = 0
            }
        }

        $weekData = $timeline.WeeklyData[$weekKey]
        $weekData.CommitCount++

        # Calculate lines changed for this commit
        $linesAdded = ($commit.Files | Measure-Object -Property Added -Sum).Sum
        $linesDeleted = ($commit.Files | Measure-Object -Property Deleted -Sum).Sum
        $linesChanged = $linesAdded + $linesDeleted

        $weekData.TotalLinesAdded += $linesAdded
        $weekData.TotalLinesDeleted += $linesDeleted
        $weekData.TotalLinesChanged += $linesChanged
        $weekData.UniqueAuthors[$commit.Author] = $true
        $weekData.CommitHashes += $commit.Hash

        # Update hourly and day-of-week patterns
        $timeline.Patterns.CommitFrequency.Hourly[$commit.Date.Hour]++
        $timeline.Patterns.CommitFrequency.DayOfWeek[[int]$commit.Date.DayOfWeek]++

        # Update daily data
        $dayKey = $commit.Date.Date.ToString("yyyy-MM-dd")
        if (-not $timeline.Patterns.CommitFrequency.Daily[$dayKey]) {
            $timeline.Patterns.CommitFrequency.Daily[$dayKey] = 0
        }
        $timeline.Patterns.CommitFrequency.Daily[$dayKey]++
    }

    # Calculate average lines per commit for each week
    foreach ($weekKey in $timeline.WeeklyData.Keys) {
        $weekData = $timeline.WeeklyData[$weekKey]
        if ($weekData.CommitCount -gt 0) {
            $weekData.AverageLinesPerCommit = [math]::Round($weekData.TotalLinesChanged / $weekData.CommitCount, 1)
        }
    }

    # Find largest gap between commits
    for ($i = 1; $i -lt $sortedCommits.Count; $i++) {
        $gap = ($sortedCommits[$i].Date - $sortedCommits[$i - 1].Date).Days
        if ($gap -gt $timeline.Patterns.LargestGap.Days) {
            $timeline.Patterns.LargestGap.Days = $gap
            $timeline.Patterns.LargestGap.StartDate = $sortedCommits[$i - 1].Date
            $timeline.Patterns.LargestGap.EndDate = $sortedCommits[$i].Date
            $timeline.Patterns.LargestGap.Description = "From $($sortedCommits[$i-1].Date.ToString('yyyy-MM-dd')) to $($sortedCommits[$i].Date.ToString('yyyy-MM-dd'))"
        }
    }

    # Calculate activity streaks and gaps
    $activeDays = $timeline.Patterns.CommitFrequency.Daily.Keys | Sort-Object
    $currentStreak = 1
    $longestStreak = 1
    $streakStart = $null
    $longestStart = $null

    if ($activeDays.Count -gt 0) {
        $streakStart = [DateTime]::Parse($activeDays[0])
        $longestStart = $streakStart

        for ($i = 1; $i -lt $activeDays.Count; $i++) {
            $currentDay = [DateTime]::Parse($activeDays[$i])
            $previousDay = [DateTime]::Parse($activeDays[$i - 1])

            if (($currentDay - $previousDay).Days -le 7) {
                # Allow up to 7-day gaps in streak
                $currentStreak++
            }
            else {
                if ($currentStreak -gt $longestStreak) {
                    $longestStreak = $currentStreak
                    $longestStart = $streakStart
                    $timeline.Patterns.ActivityStreak.LongestEnd = $previousDay
                }
                $currentStreak = 1
                $streakStart = $currentDay
            }
        }

        # Check final streak
        if ($currentStreak -gt $longestStreak) {
            $longestStreak = $currentStreak
            $longestStart = $streakStart
            $timeline.Patterns.ActivityStreak.LongestEnd = [DateTime]::Parse($activeDays[-1])
        }

        $timeline.Patterns.ActivityStreak.Longest = $longestStreak
        $timeline.Patterns.ActivityStreak.LongestStart = $longestStart
    }

    # Calculate weekly statistics
    $weeklyCommitCounts = $timeline.WeeklyData.Values | ForEach-Object { $_.CommitCount }
    if ($weeklyCommitCounts.Count -gt 0) {
        $timeline.Statistics.AverageCommitsPerWeek = [math]::Round(($weeklyCommitCounts | Measure-Object -Average).Average, 2)
        $sortedWeeklyCounts = $weeklyCommitCounts | Sort-Object
        $middle = [math]::Floor($sortedWeeklyCounts.Count / 2)
        $timeline.Statistics.MedianCommitsPerWeek = if ($sortedWeeklyCounts.Count % 2 -eq 0) {
            [math]::Round(($sortedWeeklyCounts[$middle - 1] + $sortedWeeklyCounts[$middle]) / 2, 2)
        }
        else {
            $sortedWeeklyCounts[$middle]
        }
    }

    # Find most and least active weeks
    $mostActiveWeek = $timeline.WeeklyData.GetEnumerator() | Sort-Object { $_.Value.CommitCount } -Descending | Select-Object -First 1
    if ($mostActiveWeek) {
        $timeline.Statistics.MostActiveWeek.Week = $mostActiveWeek.Key
        $timeline.Statistics.MostActiveWeek.Count = $mostActiveWeek.Value.CommitCount
        $timeline.Statistics.MostActiveWeek.LinesChanged = $mostActiveWeek.Value.TotalLinesChanged
    }

    # Calculate average gap between commits
    if ($sortedCommits.Count -gt 1) {
        $totalGaps = 0
        for ($i = 1; $i -lt $sortedCommits.Count; $i++) {
            $totalGaps += ($sortedCommits[$i].Date - $sortedCommits[$i - 1].Date).Days
        }
        $timeline.Statistics.AverageGapBetweenCommits = [math]::Round($totalGaps / ($sortedCommits.Count - 1), 2)
    }

    # Calculate consistency score (0-100)
    # Factors: regularity of commits, streak length, gap size
    $consistencyFactors = @()

    # Factor 1: Average gap score (lower gaps = higher score)
    if ($timeline.Statistics.AverageGapBetweenCommits -gt 0) {
        $gapScore = [math]::Min(100 / [math]::Max($timeline.Statistics.AverageGapBetweenCommits / 7, 1), 40)
        $consistencyFactors += $gapScore
    }

    # Factor 2: Streak score
    $streakScore = [math]::Min($timeline.Patterns.ActivityStreak.Longest * 2, 30)
    $consistencyFactors += $streakScore

    # Factor 3: Weekly activity score
    $weeklyActivityScore = [math]::Min($timeline.Statistics.AverageCommitsPerWeek * 10, 30)
    $consistencyFactors += $weeklyActivityScore

    $timeline.Statistics.Consistency.Score = [math]::Round(($consistencyFactors | Measure-Object -Sum).Sum, 1)

    # Consistency rating
    if ($timeline.Statistics.Consistency.Score -ge 80) {
        $timeline.Statistics.Consistency.Rating = "Excellent"
        $timeline.Statistics.Consistency.Description = "Very consistent commit patterns with regular activity"
    }
    elseif ($timeline.Statistics.Consistency.Score -ge 60) {
        $timeline.Statistics.Consistency.Rating = "Good"
        $timeline.Statistics.Consistency.Description = "Good commit consistency with some regular patterns"
    }
    elseif ($timeline.Statistics.Consistency.Score -ge 40) {
        $timeline.Statistics.Consistency.Rating = "Fair"
        $timeline.Statistics.Consistency.Description = "Moderate consistency with some gaps in activity"
    }
    else {
        $timeline.Statistics.Consistency.Rating = "Poor"
        $timeline.Statistics.Consistency.Description = "Irregular commit patterns with significant gaps"
    }

    return $timeline
}

# Analyze GitHub Actions integration (if workflow files exist)
function Get-GitHubActionsMetrics {
    $metrics = @{
        WorkflowFiles    = @()
        HasCICD          = $false
        HasTesting       = $false
        HasSecurity      = $false
        HasDeployment    = $false
        WorkflowTriggers = @{
            Push        = 0
            PullRequest = 0
            Schedule    = 0
            Manual      = 0
            Release     = 0
        }
    }

    # Check for GitHub Actions workflow files
    if (Test-Path ".github/workflows") {
        $workflowFiles = Get-ChildItem ".github/workflows" -Filter "*.yml" -ErrorAction SilentlyContinue
        if (-not $workflowFiles) {
            $workflowFiles = Get-ChildItem ".github/workflows" -Filter "*.yaml" -ErrorAction SilentlyContinue
        }

        foreach ($file in $workflowFiles) {
            try {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content) {
                    $metrics.WorkflowFiles += @{
                        Name    = $file.Name
                        Path    = $file.FullName
                        Content = $content
                    }

                    # Analyze workflow content
                    if ($content -match 'on:\s*push' -or $content -match 'on:\s*\[.*push.*\]') {
                        $metrics.WorkflowTriggers.Push++
                    }
                    if ($content -match 'on:\s*pull_request' -or $content -match 'on:\s*\[.*pull_request.*\]') {
                        $metrics.WorkflowTriggers.PullRequest++
                    }
                    if ($content -match 'schedule:' -or $content -match 'cron:') {
                        $metrics.WorkflowTriggers.Schedule++
                    }
                    if ($content -match 'workflow_dispatch') {
                        $metrics.WorkflowTriggers.Manual++
                    }
                    if ($content -match 'on:\s*release' -or $content -match 'on:\s*\[.*release.*\]') {
                        $metrics.WorkflowTriggers.Release++
                    }

                    # Detect workflow types
                    if ($content -match 'npm test|yarn test|pytest|dotnet test|go test|mvn test') {
                        $metrics.HasTesting = $true
                    }
                    if ($content -match 'build|compile|docker build') {
                        $metrics.HasCICD = $true
                    }
                    if ($content -match 'snyk|security|codeql|semgrep|trivy') {
                        $metrics.HasSecurity = $true
                    }
                    if ($content -match 'deploy|azure|aws|gcp|heroku|vercel|netlify') {
                        $metrics.HasDeployment = $true
                    }
                }
            }
            catch {
                Write-Warning "Could not analyze workflow file: $($file.Name)"
            }
        }
    }

    return $metrics
}

# Generate insights and recommendations with GitHub focus
function Get-Insights {
    param(
        [hashtable]$Metrics,
        [hashtable]$Authors,
        [array]$Commits
    )

    $insights = @{
        OverallHealth     = "Good"
        KeyFindings       = @()
        TrendAnalysis     = @{}
        QualityIndicators = @{}
        GitHubHealth      = @{}
    }

    $recommendations = @()

    # Analyze commit frequency
    if ($Metrics.TotalCommits -eq 0) {
        $insights.OverallHealth = "Poor"
        $insights.KeyFindings += "No commits in the analysis period"
        $recommendations += "Increase development activity and commit frequency"
    }
    elseif ($Metrics.TotalCommits -lt 5) {
        $insights.OverallHealth = "Fair"
        $insights.KeyFindings += "Low commit frequency ($($Metrics.TotalCommits) commits in $Days days)"
        $recommendations += "Consider more frequent, smaller commits for better tracking and collaboration"
    }

    # Analyze code churn
    if ($Metrics.AverageChurnPerCommit -gt 500) {
        $insights.KeyFindings += "High average churn per commit ($($Metrics.AverageChurnPerCommit) lines)"
        $recommendations += "Consider breaking large changes into smaller, focused commits for better code review"
        $recommendations += "Use GitHub's draft pull requests for work-in-progress changes"
    }

    if ($Metrics.ChurnRate -gt 0.5) {
        $insights.KeyFindings += "High deletion rate - significant code refactoring detected"
        $recommendations += "High churn rate detected - ensure adequate testing and code review for refactored code"
        $recommendations += "Consider using GitHub's code owners feature to ensure proper review of critical changes"
    }

    # Analyze author distribution
    $authorCount = $Authors.Count
    if ($authorCount -eq 1) {
        $insights.KeyFindings += "Single author development - consider implementing Pull Request workflow"
        $recommendations += "Implement Pull Request workflow even for single-developer projects using GitHub"
        $recommendations += "Use GitHub branch protection rules to enforce code review and status checks"
    }
    elseif ($authorCount -gt 5) {
        $insights.KeyFindings += "High number of contributors ($authorCount) - excellent team collaboration"
        $recommendations += "Consider using GitHub branch protection rules to maintain code quality with multiple contributors"
        $recommendations += "Implement GitHub code owners for critical path reviews"
    }

    # Analyze file concentration
    $topFiles = $Metrics.FileChurnDistribution.GetEnumerator() |
    Sort-Object { $_.Value.Added + $_.Value.Deleted } -Descending |
    Select-Object -First 5

    if ($topFiles) {
        $topFile = $topFiles[0]
        $totalChurn = $Metrics.TotalLinesChanged
        $topFileChurn = $topFile.Value.Added + $topFile.Value.Deleted

        if ($totalChurn -gt 0 -and ($topFileChurn / $totalChurn) -gt 0.4) {
            $insights.KeyFindings += "High concentration of changes in '$($topFile.Key)' ($([math]::Round(($topFileChurn / $totalChurn) * 100, 1))%)"
            $recommendations += "Consider refactoring heavily modified files to improve maintainability"
            $recommendations += "Create GitHub issues to track refactoring efforts and technical debt"
        }
    }

    # GitHub specific quality indicators
    $mergeCommits = ($Commits | Where-Object { $_.IsMerge }).Count
    $insights.QualityIndicators.MergeCommitRatio = if ($Metrics.TotalCommits -gt 0) {
        [math]::Round($mergeCommits / $Metrics.TotalCommits, 2)
    }
    else { 0 }

    $insights.QualityIndicators.AverageCommitMessage = if ($Commits.Count -gt 0) {
        [math]::Round(($Commits | ForEach-Object { $_.Message.Length } | Measure-Object -Average).Average, 1)
    }
    else { 0 }

    # GitHub integration health assessment
    if ($insights.QualityIndicators.MergeCommitRatio -gt 0.3) {
        $insights.GitHubHealth.PullRequestUsage = "Good"
        $insights.KeyFindings += "Good use of Pull Request workflow ($(($insights.QualityIndicators.MergeCommitRatio * 100).ToString('F1'))% merge commits)"
    }
    elseif ($insights.QualityIndicators.MergeCommitRatio -gt 0.1) {
        $insights.GitHubHealth.PullRequestUsage = "Fair"
        $recommendations += "Increase use of Pull Requests for better code review and collaboration"
        $recommendations += "Consider enabling GitHub's merge queue for better integration"
    }
    else {
        $insights.GitHubHealth.PullRequestUsage = "Poor"
        $recommendations += "Implement Pull Request workflow for all code changes"
        $recommendations += "Set up GitHub branch protection rules to require Pull Request reviews"
    }

    # Commit message quality assessment
    if ($insights.QualityIndicators.AverageCommitMessage -lt 20) {
        $insights.KeyFindings += "Short commit messages detected - consider more descriptive messages"
        $recommendations += "Use descriptive commit messages that explain the 'why' not just the 'what'"
        $recommendations += "Link commit messages to GitHub issues using #IssueNumber format"
        $recommendations += "Consider adopting Conventional Commits specification for consistency"
    }

    # Team collaboration assessment
    if ($authorCount -gt 1 -and $insights.QualityIndicators.MergeCommitRatio -lt 0.2) {
        $insights.KeyFindings += "Multi-author project with low Pull Request usage - collaboration risk detected"
        $recommendations += "Implement mandatory Pull Request reviews for multi-author projects"
        $recommendations += "Use GitHub's CODEOWNERS file to automatically request reviews"
        $recommendations += "Enable GitHub Discussions for team communication and decision tracking"
    }

    # Check for conventional commits usage
    $conventionalCommits = ($Commits | Where-Object { $_.Message -match '^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?\s*!?\s*:\s*.+' }).Count
    $conventionalRatio = if ($Metrics.TotalCommits -gt 0) { $conventionalCommits / $Metrics.TotalCommits } else { 0 }

    if ($conventionalRatio -gt 0.7) {
        $insights.KeyFindings += "Excellent use of Conventional Commits ($(($conventionalRatio * 100).ToString('F1'))%)"
    }
    elseif ($conventionalRatio -gt 0.3) {
        $insights.KeyFindings += "Good adoption of Conventional Commits ($(($conventionalRatio * 100).ToString('F1'))%)"
        $recommendations += "Continue improving Conventional Commits adoption for better changelog generation"
    }
    else {
        $recommendations += "Consider adopting Conventional Commits for better automation and changelog generation"
        $recommendations += "Use tools like commitizen or conventional-changelog for GitHub releases"
    }

    # GitHub Actions recommendations
    if ($reportData.GitHubActionsMetrics.WorkflowFiles.Count -eq 0) {
        $recommendations += "Consider implementing GitHub Actions workflows for CI/CD automation"
        $recommendations += "Add automated testing workflows to improve code quality"
    }
    else {
        if (-not $reportData.GitHubActionsMetrics.HasTesting) {
            $recommendations += "Add automated testing to your GitHub Actions workflows"
        }
        if (-not $reportData.GitHubActionsMetrics.HasSecurity) {
            $recommendations += "Consider adding security scanning (CodeQL, Dependabot) to your workflows"
        }
        if ($reportData.GitHubActionsMetrics.WorkflowTriggers.PullRequest -eq 0) {
            $recommendations += "Add pull request triggers to workflows for better CI integration"
        }
    }

    return @{
        Insights        = $insights
        Recommendations = $recommendations
    }
}

# Create HTML report
function New-HtmlReport {
    param(
        [hashtable]$ReportData,
        [string]$OutputPath,
        [array]$DeveloperCards
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $fileName = "git-activity-report-$timestamp.html"
    $filePath = Join-Path $OutputPath $fileName
    $html = @"
<html>
    <head>
    <meta charset='UTF-8'>
    <title>Git Activity Report</title>
    <style>
    body { font-family: 'Segoe UI', Arial, sans-serif; background: #f4f6fb; color: #222; margin: 0; }
        .container { max-width: 1200px; margin: 40px auto; background: #fff; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow: hidden; }
            .header { background: linear-gradient(90deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 30px; text-align: center; }
                    .header h1 { font-size: 2.5em; margin-bottom: 10px; }
                    .header p { font-size: 1.2em; opacity: 0.9; }
                    .content { padding: 30px; }
                    .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin-bottom: 30px; }
                    .metric-card { background: #f8f9ff; border: 1px solid #e1e5fe; border-radius: 10px; padding: 20px; text-align: center; transition: transform 0.3s ease; }
                        .metric-card:hover { transform: translateY(-5px); }
                        .metric-value { font-size: 2em; font-weight: bold; color: #4facfe; margin-bottom: 5px; }
                            .metric-label { color: #666; font-size: 0.9em; text-transform: uppercase; letter-spacing: 1px; }
                                .section { margin-bottom: 40px; }
                                .section h2 { color: #333; border-bottom: 3px solid #4facfe; padding-bottom: 10px; margin-bottom: 20px; font-size: 1.8em; }
                                    .author-list { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 15px; }
                                    .author-card { background: #ffffff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 15px; box-shadow: 0 2px 10px rgba(0,0,0,0.05); }
                                        .author-name { font-weight: bold; color: #333; margin-bottom: 5px; }
                                            .author-stats { font-size: 0.9em; color: #666; }
                                                .recommendations { background: #fff3cd; border: 1px solid #ffeaa7; border-radius: 8px; padding: 20px; }
                                                    .recommendations ul { margin-left: 20px; }
                                                    .recommendations li { margin-bottom: 8px; }
                                                    .insights { background: #d1ecf1; border: 1px solid #bee5eb; border-radius: 8px; padding: 20px; }
                                                        .file-list { max-height: 300px; overflow-y: auto; }
                                                        .file-item { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }
                                                            .health-indicator { display: inline-block; padding: 5px 15px; border-radius: 20px; color: white; font-weight: bold; margin-left: 10px; }
                                                            .health-good { background: #28a745; }
                                                                .health-fair { background: #ffc107; }
                                                                    .health-poor { background: #dc3545; }
                                                                        .footer { text-align: center; padding: 20px; background: #f8f9fa; color: #666; border-top: 1px solid #eee; }
                                                                            table { width: 100%; border-collapse: collapse; margin-top: 15px; }
                                                                            th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
                                                                                th { background: #f8f9ff; font-weight: 600; }
                                                                                    tr:hover { background: #f5f5f5; }
                                                                                    .developer-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 20px; margin-top: 20px; }
                                                                                    .developer-card { background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; border-left: 4px solid #007acc; }
                                                                                    .developer-card-header { background: linear-gradient(45deg, #007acc, #0084d4); color: white; padding: 20px; }
                                                                                    .developer-card-body { padding: 20px; }
                                                                                    .developer-name { font-size: 1.3em; font-weight: bold; margin: 0; }
                                                                                    .developer-email { opacity: 0.9; margin: 5px 0 0 0; font-size: 0.9em; }
                                                                                    .stat-row { display: flex; justify-content: space-between; margin: 8px 0; padding: 8px 0; border-bottom: 1px solid #f0f0f0; }
                                                                                    .stat-label { font-weight: 600; color: #333; }
                                                                                    .stat-value { color: #666; font-family: monospace; }
                                                                                    .biggest-commit { background: #f8f9fa; padding: 15px; border-radius: 6px; margin: 15px 0; border-left: 3px solid #28a745; }
                                                                                    .commit-message { font-weight: bold; color: #333; margin-bottom: 5px; }
                                                                                    .commit-stats { font-family: monospace; font-size: 0.9em; color: #666; }
                                                                                    .insights-section { background: #e3f2fd; padding: 15px; border-radius: 6px; margin-top: 15px; border-left: 3px solid #2196f3; }
                                                                                    .insights-title { font-weight: bold; color: #1976d2; margin-bottom: 10px; }
                                                                                    .insight-item { margin: 5px 0; color: #333; }

                                                                                    /* Timeline and Pattern Analysis Styles */
                                                                                    .timeline-chart { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; margin-bottom: 20px; text-align: center; }
                                                                                    .timeline-stats { margin-top: 15px; }
                                                                                    .pattern-analysis { margin-top: 20px; }
                                                                                    .pattern-card { background: #f8f9ff; border: 1px solid #e1e5fe; border-radius: 8px; padding: 20px; height: 100%; }
                                                                                    .pattern-card h4 { color: #4facfe; margin-top: 0; margin-bottom: 15px; }
                                                                                    .gap-details, .consistency-details { margin-top: 10px; color: #666; font-style: italic; }
                                                                                    .consistency-excellent { color: #28a745; font-weight: bold; }
                                                                                    .consistency-good { color: #17a2b8; font-weight: bold; }
                                                                                    .consistency-fair { color: #ffc107; font-weight: bold; }
                                                                                    .consistency-poor { color: #dc3545; font-weight: bold; }
                                                                                    .row { display: flex; flex-wrap: wrap; margin: -10px; }
                                                                                    .col-md-6 { flex: 0 0 50%; max-width: 50%; padding: 10px; }
                                                                                    .mt-4 { margin-top: 1.5rem; }
                                                                                    @media (max-width: 768px) {
                                                                                        .col-md-6 { flex: 0 0 100%; max-width: 100%; }
                                                                                    }
                                                                                        </style>
                                                                                        <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
                                                                                        </style>
                                                                                        </head>
                                                                                        <body>
                                                                                        <div class="container">
                                                                                        <div class="header">
                                                                                        <h1>📊 GitHub Repository Activity Report</h1>
                                                                                        <p>Repository: <strong>$($ReportData.Repository.Name)</strong> | Period: Last $($ReportData.AnalysisPeriod) days</p>
                                                                                        <p>Generated on: $($ReportData.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss"))</p>
                                                                                        </div>
                                                                                        <div class="content">

                                                                                        <!-- Key Metrics Grid -->
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

                                                                                        <!-- GitHub Integration Section -->
                                                                                        <div class="section">
                                                                                            <h2>🔗 GitHub Integration Health</h2>
                                                                                            <div class="metrics-grid">
                                                                                                <div class="metric-card">
                                                                                                    <div class="metric-value">$($ReportData.GitHubIntegration.PullRequestPatterns.PullRequestMerges)</div>
                                                                                                    <div class="metric-label">Pull Request Merges</div>
                                                                                                </div>
                                                                                                <div class="metric-card">
                                                                                                    <div class="metric-value">$($ReportData.GitHubIntegration.IssueReferences.UniqueIssues.Count)</div>
                                                                                                    <div class="metric-label">Unique Issues</div>
                                                                                                </div>
                                                                                                <div class="metric-card">
                                                                                                    <div class="metric-value">$($ReportData.CodeReviewMetrics.CollaborationScore)%</div>
                                                                                                    <div class="metric-label">Collaboration Score</div>
                                                                                                </div>
                                                                                                <div class="metric-card">
                                                                                                    <div class="metric-value">$($ReportData.GitHubIntegration.CommitMessageQuality.WithConventionalFormat)</div>
                                                                                                    <div class="metric-label">Conventional Commits</div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </div>

                                                                                        <!-- Timeline Chart Section -->
                                                                                        <div class="section">
                                                                                            <h2>📈 Commit Timeline (Weekly)</h2>
                                                                                            <div class="timeline-chart">
                                                                                                <canvas id="commitTimeline" width="800" height="400"></canvas>
                                                                                            </div>
                                                                                            <div class="timeline-stats">
                                                                                                <div class="metrics-grid">
                                                                                                    <div class="metric-card">
                                                                                                        <div class="metric-value">$($ReportData.TimelineAnalysis.Statistics.AverageCommitsPerWeek)</div>
                                                                                                        <div class="metric-label">Avg Commits/Week</div>
                                                                                                    </div>
                                                                                                    <div class="metric-card">
                                                                                                        <div class="metric-value">$($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Count)</div>
                                                                                                        <div class="metric-label">Peak Week Commits</div>
                                                                                                    </div>
                                                                                                    <div class="metric-card">
                                                                                                        <div class="metric-value">$([math]::Round($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.LinesChanged / [math]::Max($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Count, 1), 0))</div>
                                                                                                        <div class="metric-label">Peak Week Avg Lines</div>
                                                                                                    </div>
                                                                                                    <div class="metric-card">
                                                                                                        <div class="metric-value">$($ReportData.TimelineAnalysis.Statistics.Consistency.Score)%</div>
                                                                                                        <div class="metric-label">Consistency Score</div>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </div>

                                                                                        <!-- Commit Patterns Section -->
                                                                                        <div class="section">
                                                                                            <h2>🔄 Commit Pattern Analysis</h2>
                                                                                            <div class="pattern-analysis">
                                                                                                <div class="row">
                                                                                                    <div class="col-md-6">
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
                                                                                                                <span class="stat-label">Total Timespan:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.TotalTimespan) days</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Largest Gap:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.LargestGap.Days) days</span>
                                                                                                            </div>
                                                                                                            <div class="gap-details">
                                                                                                                <small>$($ReportData.TimelineAnalysis.Patterns.LargestGap.Description)</small>
                                                                                                            </div>
                                                                                                        </div>
                                                                                                    </div>
                                                                                                    <div class="col-md-6">
                                                                                                        <div class="pattern-card">
                                                                                                            <h4>📊 Frequency Statistics</h4>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Average Gap Between Commits:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.AverageGapBetweenCommits) days</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Weekly Average:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.AverageCommitsPerWeek) commits</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Weekly Median:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.MedianCommitsPerWeek) commits</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Consistency Rating:</span>
                                                                                                                <span class="stat-value consistency-$($ReportData.TimelineAnalysis.Statistics.Consistency.Rating.ToLower())">$($ReportData.TimelineAnalysis.Statistics.Consistency.Rating)</span>
                                                                                                            </div>
                                                                                                            <div class="consistency-details">
                                                                                                                <small>$($ReportData.TimelineAnalysis.Statistics.Consistency.Description)</small>
                                                                                                            </div>
                                                                                                        </div>
                                                                                                    </div>
                                                                                                </div>

                                                                                                <!-- Activity Patterns -->
                                                                                                <div class="row mt-4">
                                                                                                    <div class="col-md-6">
                                                                                                        <div class="pattern-card">
                                                                                                            <h4>🏃‍♂️ Activity Streaks</h4>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Longest Streak:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.ActivityStreak.Longest) active periods</span>
                                                                                                            </div>
"@

    # Add streak details if available
    if ($ReportData.TimelineAnalysis.Patterns.ActivityStreak.LongestStart) {
        $html += @"
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Streak Period:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Patterns.ActivityStreak.LongestStart.ToString("yyyy-MM-dd")) to $($ReportData.TimelineAnalysis.Patterns.ActivityStreak.LongestEnd.ToString("yyyy-MM-dd"))</span>
                                                                                                            </div>
"@
    }

    $html += @"
                                                                                                        </div>
                                                                                                    </div>
                                                                                                    <div class="col-md-6">
                                                                                                        <div class="pattern-card">
                                                                                                            <h4>📈 Peak Activity</h4>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Most Active Week:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Week)</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Peak Week Commits:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.Count) commits</span>
                                                                                                            </div>
                                                                                                            <div class="stat-row">
                                                                                                                <span class="stat-label">Peak Week Changes:</span>
                                                                                                                <span class="stat-value">$($ReportData.TimelineAnalysis.Statistics.MostActiveWeek.LinesChanged) lines</span>
                                                                                                            </div>
                                                                                                        </div>
                                                                                                    </div>
                                                                                                </div>
                                                                                            </div>
                                                                                        </div>

                                                                                        <!-- Recent Commits Summary Table -->
                                                                                        <div class="section">
                                                                                            <h2>📋 Recent Commits Summary</h2>
                                                                                            <table>
                                                                                                <thead>
                                                                                                    <tr>
                                                                                                        <th>Hash</th>
                                                                                                        <th>Author</th>
                                                                                                        <th>Date</th>
                                                                                                        <th>Message</th>
                                                                                                        <th>+Lines</th>
                                                                                                        <th>-Lines</th>
                                                                                                        <th>Files</th>
                                                                                                    </tr>
                                                                                                </thead>
                                                                                                <tbody>
"@

    # Add commit summary rows
    $ReportData.CommitSummary | ForEach-Object {
        $html += @"
                                                                                                    <tr>
                                                                                                        <td><code>$($_.ShortHash)</code></td>
                                                                                                        <td>$($_.Author)</td>
                                                                                                        <td>$($_.Date.ToString("MM-dd HH:mm"))</td>
                                                                                                        <td>$([System.Web.HttpUtility]::HtmlEncode($_.Message))</td>
                                                                                                        <td style="color: green;">+$($_.LinesAdded)</td>
                                                                                                        <td style="color: red;">-$($_.LinesDeleted)</td>
                                                                                                        <td>$($_.FilesTouched)</td>
                                                                                                    </tr>
"@
    }

    $healthClass = switch ($ReportData.Insights.OverallHealth) {
        "Good" { "health-good" }
        "Fair" { "health-fair" }
        "Poor" { "health-poor" }
        default { "health-fair" }
    }

    $html += @"
                                                                                                </tbody>
                                                                                            </table>
                                                                                        </div>

                                                                                        <!-- Insights Section -->
                                                                                        <div class="section">
                                                                                            <h2>🔍 Insights & Health Assessment</h2>
                                                                                            <div class="insights">
                                                                                                <p><strong>Overall Health:</strong> <span class="health-indicator $healthClass">$($ReportData.Insights.OverallHealth)</span></p>
                                                                                                <h4>Key Findings:</h4>
                                                                                                <ul>
"@

    foreach ($finding in $ReportData.Insights.KeyFindings) {
        $html += "<li>$([System.Web.HttpUtility]::HtmlEncode($finding))</li>"
    }

    $html += @"
                                                                                                </ul>
                                                                                            </div>
                                                                                        </div>

                                                                                        <!-- Recommendations Section -->
                                                                                        <div class="section">
                                                                                            <h2>💡 GitHub Best Practice Recommendations</h2>
                                                                                            <div class="recommendations">
                                                                                                <ul>
"@

    foreach ($recommendation in $ReportData.Recommendations) {
        $html += "<li>$([System.Web.HttpUtility]::HtmlEncode($recommendation))</li>"
    }

    $html += @"
                                                                                                </ul>
                                                                                            </div>
                                                                                        </div>

                                                                                        <!-- Developer Cards -->
                                                                                        <div class="section">
                                                                                            <h2>👥 Developer Analysis Cards</h2>
                                                                                            <div class="developer-cards">
"@

    # Add developer cards
    foreach ($author in $DeveloperCards) {
        $html += @"
                                                                                                <div class="developer-card">
                                                                                                    <div class="developer-card-header">
                                                                                                        <h3 class="developer-name">$([System.Web.HttpUtility]::HtmlEncode($author.Name))</h3>
                                                                                                        <p class="developer-email">$([System.Web.HttpUtility]::HtmlEncode($author.Email))</p>
                                                                                                    </div>
                                                                                                    <div class="developer-card-body">
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Active Period:</span>
                                                                                                            <span class="stat-value">$($author.FirstCommit) to $($author.LastCommit)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Total Commits:</span>
                                                                                                            <span class="stat-value">$($author.TotalCommits)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Code Changes:</span>
                                                                                                            <span class="stat-value">+$($author.TotalAdditions) / -$($author.TotalDeletions)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Files Touched:</span>
                                                                                                            <span class="stat-value">$($author.TotalFiles)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">File Diversity:</span>
                                                                                                            <span class="stat-value">$($author.FileTypes) file types</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Avg Commit Size:</span>
                                                                                                            <span class="stat-value">$($author.AvgCommitSize) changes</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Commits per Day:</span>
                                                                                                            <span class="stat-value">$($author.CommitsPerDay)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">Issue Links:</span>
                                                                                                            <span class="stat-value">$($author.IssueLinks)</span>
                                                                                                        </div>
                                                                                                        <div class="stat-row">
                                                                                                            <span class="stat-label">PR Integration:</span>
                                                                                                            <span class="stat-value">$($author.PRIntegrationScore)%</span>
                                                                                                        </div>

                                                                                                        <div class="biggest-commit">
                                                                                                            <div class="commit-message">🎯 Biggest Commit:</div>
                                                                                                            <div>$([System.Web.HttpUtility]::HtmlEncode($author.BiggestCommitMessage))</div>
                                                                                                            <div class="commit-stats">$($author.BiggestCommitStats)</div>
                                                                                                        </div>

                                                                                                        <div class="insights-section">
                                                                                                            <div class="insights-title">💡 Analysis Insights</div>
"@
        foreach ($insight in $author.Insights) {
            $html += "                                                                                                            <div class=""insight-item"">$([System.Web.HttpUtility]::HtmlEncode($insight))</div>`n"
        }
        $html += @"
                                                                                                        </div>
                                                                                                    </div>
                                                                                                </div>
"@
    }

    $html += @"
    $html += @"
                                                                                            </div>
                                                                                        </div>

                                                                                        </div>
                                                                                        <div class="footer">
                                                                                            <p>Generated by GitHub Git Activity Analyzer v3.0 | Enhanced with GitHub best practices and DORA metrics</p>
                                                                                        </div>
                                                                                        </div>

                                                                                        <script>
                                                                                        // Generate Timeline Chart
                                                                                        document.addEventListener('DOMContentLoaded', function() {
                                                                                            const ctx = document.getElementById('commitTimeline').getContext('2d');

                                                                                            // Prepare timeline data
"@

    # Generate timeline chart data
    $timelineDataJs = @()
    $linesDataJs = @()
    $labelsJs = @()

    $sortedWeeks = $ReportData.TimelineAnalysis.WeeklyData.GetEnumerator() | Sort-Object { $_.Value.WeekStart }
    foreach ($week in $sortedWeeks) {
        $weekLabel = $week.Value.WeekStart.ToString("MM/dd")
        $labelsJs += "'$weekLabel'"
        $timelineDataJs += $week.Value.CommitCount
        $avgLines = if ($week.Value.CommitCount -gt 0) { [math]::Round($week.Value.TotalLinesChanged / $week.Value.CommitCount, 0) } else { 0 }
        $linesDataJs += $avgLines
    }

    $timelineLabels = $labelsJs -join ','
    $timelineCommitData = $timelineDataJs -join ','
    $timelineLinesData = $linesDataJs -join ','

    $html += @"
                                                                                            const timelineChart = new Chart(ctx, {
                                                                                                type: 'line',
                                                                                                data: {
                                                                                                    labels: [$timelineLabels],
                                                                                                    datasets: [{
                                                                                                        label: 'Commits per Week',
                                                                                                        data: [$timelineCommitData],
                                                                                                        borderColor: '#4facfe',
                                                                                                        backgroundColor: 'rgba(79, 172, 254, 0.1)',
                                                                                                        borderWidth: 3,
                                                                                                        fill: true,
                                                                                                        tension: 0.4,
                                                                                                        yAxisID: 'y'
                                                                                                    }, {
                                                                                                        label: 'Avg Lines per Commit',
                                                                                                        data: [$timelineLinesData],
                                                                                                        borderColor: '#ff6b6b',
                                                                                                        backgroundColor: 'rgba(255, 107, 107, 0.1)',
                                                                                                        borderWidth: 2,
                                                                                                        fill: false,
                                                                                                        tension: 0.4,
                                                                                                        yAxisID: 'y1'
                                                                                                    }]
                                                                                                },
                                                                                                options: {
                                                                                                    responsive: true,
                                                                                                    interaction: {
                                                                                                        mode: 'index',
                                                                                                        intersect: false,
                                                                                                    },
                                                                                                    plugins: {
                                                                                                        title: {
                                                                                                            display: true,
                                                                                                            text: 'Weekly Commit Activity Timeline'
                                                                                                        },
                                                                                                        legend: {
                                                                                                            display: true,
                                                                                                            position: 'top'
                                                                                                        }
                                                                                                    },
                                                                                                    scales: {
                                                                                                        x: {
                                                                                                            display: true,
                                                                                                            title: {
                                                                                                                display: true,
                                                                                                                text: 'Week'
                                                                                                            }
                                                                                                        },
                                                                                                        y: {
                                                                                                            type: 'linear',
                                                                                                            display: true,
                                                                                                            position: 'left',
                                                                                                            title: {
                                                                                                                display: true,
                                                                                                                text: 'Commits per Week'
                                                                                                            },
                                                                                                            beginAtZero: true
                                                                                                        },
                                                                                                        y1: {
                                                                                                            type: 'linear',
                                                                                                            display: true,
                                                                                                            position: 'right',
                                                                                                            title: {
                                                                                                                display: true,
                                                                                                                text: 'Avg Lines per Commit'
                                                                                                            },
                                                                                                            grid: {
                                                                                                                drawOnChartArea: false,
                                                                                                            },
                                                                                                            beginAtZero: true
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            });
                                                                                        });
                                                                                        </script>
                                                                                        </body>
                                                                                        </html>
"@
    </body>
    </html>
    "@

    # Add System.Web assembly for HTML encoding
    Add-Type -AssemblyName System.Web

    $html | Out-File -FilePath $filePath -Encoding UTF8
    return $filePath
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

# Generate console report
function Show-ConsoleReport {
    param([hashtable]$ReportData)

    Write-Host "`n" + "="*80 -ForegroundColor Cyan
    Write-Host "  📊 GIT ACTIVITY REPORT - $($ReportData.Repository.Name)" -ForegroundColor Cyan
    Write-Host "="*80 -ForegroundColor Cyan

    Write-Host "`n🏛️  REPOSITORY OVERVIEW" -ForegroundColor Yellow
    Write-Host "  Name: $($ReportData.Repository.Name)"
    Write-Host "  Branch: $($ReportData.Repository.Branch)"
    Write-Host "  Analysis Period: Last $($ReportData.AnalysisPeriod) days"
    Write-Host "  Generated: $($ReportData.GeneratedAt)"

    Write-Host "`n📈 KEY METRICS" -ForegroundColor Yellow
    Write-Host "  Total Commits: $($ReportData.Metrics.TotalCommits)"
    Write-Host "  Lines Added: $($ReportData.Metrics.TotalLinesAdded)"
    Write-Host "  Lines Deleted: $($ReportData.Metrics.TotalLinesDeleted)"
    Write-Host "  Files Changed: $($ReportData.Metrics.TotalFilesChanged)"
    Write-Host "  Average Churn per Commit: $($ReportData.Metrics.AverageChurnPerCommit) lines"
    Write-Host "  Churn Rate: $([math]::Round($ReportData.Metrics.ChurnRate, 2))"

    Write-Host "`n👥 TOP AUTHORS" -ForegroundColor Yellow
    $topAuthors = $ReportData.Authors.GetEnumerator() | Sort-Object { $_.Value.Commits } -Descending | Select-Object -First 5
    foreach ($author in $topAuthors) {
        $authorData = $author.Value
        Write-Host "  $($author.Key): $($authorData.Commits) commits, +$($authorData.LinesAdded)/-$($authorData.LinesDeleted) lines"
        Write-Host "    First: $($authorData.FirstCommit.ToString('yyyy-MM-dd')), Last: $($authorData.LastCommit.ToString('yyyy-MM-dd'))"
        Write-Host "    Biggest: $($authorData.BiggestCommit.Size) lines ($($authorData.BiggestCommit.Date.ToString('yyyy-MM-dd'))), Avg Size: $($authorData.Productivity.AverageCommitSize), Freq: $($authorData.Productivity.CommitFrequency)/day"
    }

    Write-Host "`n🔗 GITHUB INTEGRATION" -ForegroundColor Yellow
    $githubIntegration = $ReportData.GitHubIntegration
    Write-Host "  Issue References: $($githubIntegration.IssueReferences.Total) references to $($githubIntegration.IssueReferences.UniqueIssues.Count) unique issues"
    Write-Host "  Pull Request Merges: $($githubIntegration.PullRequestPatterns.PullRequestMerges) PR merges, $($githubIntegration.PullRequestPatterns.DirectCommits) direct commits"
    Write-Host "  Commit Message Quality: $($githubIntegration.CommitMessageQuality.WithIssueReferences)/$($githubIntegration.CommitMessageQuality.TotalMessages) with issues, Avg Length: $($githubIntegration.CommitMessageQuality.AverageLength) chars"
    Write-Host "  Conventional Commits: $($githubIntegration.CommitMessageQuality.WithConventionalFormat)/$($githubIntegration.CommitMessageQuality.TotalMessages) using conventional format"
    Write-Host "  Issue Actions: $($githubIntegration.IssueReferences.Patterns.Fixes) fixes, $($githubIntegration.IssueReferences.Patterns.Closes) closes, $($githubIntegration.IssueReferences.Patterns.Resolves) resolves"
    if ($githubIntegration.CommitMessageQuality.WithCoAuthoredBy -gt 0) {
        Write-Host "  Co-authored commits: $($githubIntegration.CommitMessageQuality.WithCoAuthoredBy) (excellent collaboration pattern)"
    }

    Write-Host "`n📋 RECENT COMMITS SUMMARY ($($ReportData.CommitSummary.Count) latest)" -ForegroundColor Yellow
    $ReportData.CommitSummary | Select-Object -First 15 | ForEach-Object {
        Write-Host "  $($_.ShortHash) | $($_.Author) | $($_.Date.ToString('MM-dd HH:mm')) | + $($_.LinesAdded) / - $($_.LinesDeleted) | $($_.FilesTouched) files" -ForegroundColor Gray
        Write-Host "    $($_.Message)" -ForegroundColor DarkGray
    }
    if ($ReportData.CommitSummary.Count -gt 15) {
        Write-Host "  ... and $($ReportData.CommitSummary.Count - 15) more commits" -ForegroundColor DarkGray
    }

    Write-Host "`n👤 DETAILED DEVELOPER ANALYSIS" -ForegroundColor Yellow
    Write-Host "=" * 80 -ForegroundColor Yellow

    $sortedAuthors = $ReportData.Authors.GetEnumerator() | Sort-Object { $_.Value.Commits } -Descending
    foreach ($authorEntry in $sortedAuthors) {
        $author = $authorEntry.Key
        $data = $authorEntry.Value
        $issueRefs = if ($ReportData.GitHubIntegration.IssueReferences.ByAuthor[$author]) {
            $ReportData.GitHubIntegration.IssueReferences.ByAuthor[$author].Count
        }
        else { 0 }

        Write-Host "`n👤 $author" -ForegroundColor Cyan
        Write-Host "   Email: $($data.Email)" -ForegroundColor Gray
        Write-Host "   ├─ Activity Period: $($data.FirstCommit.ToString('yyyy-MM-dd')) → $($data.LastCommit.ToString('yyyy-MM-dd')) ($((($data.LastCommit - $data.FirstCommit).Days + 1)) days)"
        Write-Host "   ├─ Commits: $($data.Commits) total ($($data.MergeCommits) merges, $($data.Commits - $data.MergeCommits) direct)"
        Write-Host "   ├─ Code Changes: +$($data.LinesAdded) lines added, -$($data.LinesDeleted) lines deleted"
        Write-Host "   ├─ Files Touched: $($data.FilesChanged.Count) unique files"
        Write-Host "   ├─ Productivity Metrics:"
        Write-Host "   │  ├─ Average Commit Size: $($data.Productivity.AverageCommitSize) lines"
        Write-Host "   │  ├─ Commit Frequency: $($data.Productivity.CommitFrequency) commits/day"
        Write-Host "   │  └─ File Diversity: $($data.Productivity.FilesDiversity) different files"
        Write-Host "   ├─ Biggest Commit: $($data.BiggestCommit.Size) lines on $($data.BiggestCommit.Date.ToString('yyyy-MM-dd'))"
        Write-Host "   │  └─ Message: $($data.BiggestCommit.Message)" -ForegroundColor DarkGray
        Write-Host "   ├─ GitHub Integration:"
        Write-Host "   │  ├─ Issue References: $issueRefs"
        Write-Host "   │  └─ Merge Commit Ratio: $([math]::Round($data.MergeCommits / [math]::Max($data.Commits, 1) * 100, 1))%"

        # Calculate developer-specific insights
        $insights = @()
        if ($data.Productivity.AverageCommitSize -gt 300) {
            $insights += "Large commits - consider smaller, focused changes"
        }
        if ($data.MergeCommits / [math]::Max($data.Commits, 1) -gt 0.8) {
            $insights += "Excellent Pull Request workflow usage"
        }
        elseif ($data.MergeCommits / [math]::Max($data.Commits, 1) -lt 0.2 -and $data.Commits -gt 2) {
            $insights += "Consider using more Pull Requests"
        }
        if ($issueRefs -eq 0 -and $data.Commits -gt 2) {
            $insights += "No issue references - improve traceability with GitHub issues"
        }
        elseif ($issueRefs / [math]::Max($data.Commits, 1) -gt 0.5) {
            $insights += "Good GitHub issue integration"
        }
        if ($data.FilesChanged.Count -gt 20 -and $data.Commits -gt 5) {
            $insights += "Broad file coverage - good system knowledge"
        }

        # Check for conventional commits usage by this author
        $authorCommits = $ReportData.Commits | Where-Object { $_.Author -eq $author }
        $conventionalCount = ($authorCommits | Where-Object { $_.Message -match '^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?\s*!?\s*:\s*.+' }).Count
        if ($conventionalCount -gt 0) {
            $conventionalRatio = [math]::Round($conventionalCount / $authorCommits.Count * 100, 1)
            $insights += "Using Conventional Commits ($conventionalRatio % of commits)"
        }

        if ($insights.Count -gt 0) {
            Write-Host "   └─ Developer Insights:" -ForegroundColor Green
            foreach ($insight in $insights) {
                Write-Host "      • $insight" -ForegroundColor DarkGreen
            }
        }

        Write-Host "   " + "-" * 78 -ForegroundColor DarkGray
    }

    Write-Host "`n📁 TOP MODIFIED FILES" -ForegroundColor Yellow
    $topFiles = $ReportData.Metrics.FileChurnDistribution.GetEnumerator() |
    Sort-Object { $_.Value.Added + $_.Value.Deleted } -Descending |
    Select-Object -First 5
    foreach ($file in $topFiles) {
        $totalChurn = $file.Value.Added + $file.Value.Deleted
        Write-Host "  $($file.Key): $totalChurn lines changed"
    }

    $healthColor = switch ($ReportData.Insights.OverallHealth) {
        "Good" { "Green" }
        "Fair" { "Yellow" }
        "Poor" { "Red" }
        default { "Yellow" }
    }

    Write-Host "`n🔍 INSIGHTS" -ForegroundColor Yellow
    Write-Host "  Overall Health: " -NoNewline
    Write-Host $ReportData.Insights.OverallHealth -ForegroundColor $healthColor

    if ($ReportData.Insights.KeyFindings.Count -gt 0) {
        Write-Host "  Key Findings:"
        foreach ($finding in $ReportData.Insights.KeyFindings) {
            Write-Host "    • $finding" -ForegroundColor Cyan
        }
    }

    if ($ReportData.Recommendations.Count -gt 0) {
        Write-Host "`n💡 RECOMMENDATIONS" -ForegroundColor Yellow
        foreach ($recommendation in $ReportData.Recommendations) {
            Write-Host "  • $recommendation" -ForegroundColor Green
        }
    }

    Write-Host "`n" + "="*80 -ForegroundColor Cyan
}

# Main execution
try {
    Write-Host "🚀 Starting Git Activity Analysis..." -ForegroundColor Green

    # Gather data
    $reportData.Repository = Get-RepositoryInfo
    $commits = Get-CommitData -Days $Days
    $reportData.Commits = $commits
    $reportData.CommitSummary = Get-CommitSummary -Count $CommitSummaryCount
    $reportData.Metrics = Get-CodeChurnMetrics -Commits $commits
    $reportData.Authors = Get-AuthorAnalysis -Commits $commits

    # GitHub specific analysis
    $reportData.GitHubIntegration = Get-GitHubIntegration -Commits $commits
    $reportData.GitHubActionsMetrics = Get-GitHubActionsMetrics
    $reportData.TimelineAnalysis = Get-CommitTimelineAnalysis -Commits $commits
    $reportData.CodeReviewMetrics = Get-CodeReviewMetrics -Commits $commits -Authors $reportData.Authors

    $insightResults = Get-Insights -Metrics $reportData.Metrics -Authors $reportData.Authors -Commits $commits
    $reportData.Insights = $insightResults.Insights
    $reportData.Recommendations = $insightResults.Recommendations

    # Create developer cards for HTML/JSON output
    $DeveloperCards = @()
    $sortedAuthors = $reportData.Authors.GetEnumerator() | Sort-Object { $_.Value.Commits } -Descending
    foreach ($authorEntry in $sortedAuthors) {
        $author = $authorEntry.Key
        $data = $authorEntry.Value
        $issueRefs = if ($reportData.GitHubIntegration.IssueReferences.ByAuthor[$author]) {
            $reportData.GitHubIntegration.IssueReferences.ByAuthor[$author].Count
        }
        else { 0 }

        # Calculate PR integration score
        $prScore = if ($data.Commits -gt 0) {
            [math]::Round((($data.MergeCommits / $data.Commits) * 100), 0)
        }
        else { 0 }

        # Generate insights for this developer
        $insights = @()
        if ($data.Productivity.AverageCommitSize -gt 500) {
            $insights += "Large commits - consider smaller, focused changes"
        }
        if ($prScore -eq 100) {
            $insights += "Excellent Pull Request workflow usage"
        }
        elseif ($prScore -eq 0) {
            $insights += "Consider using more Pull Requests"
        }
        if ($issueRefs -eq 0) {
            $insights += "No issue references - improve traceability with GitHub issues"
        }
        if ($data.Productivity.FilesDiversity -gt 20) {
            $insights += "High file diversity - excellent system knowledge"
        }

        # Check for conventional commits usage by this author
        $authorCommits = $reportData.Commits | Where-Object { $_.Author -eq $author }
        $conventionalCount = ($authorCommits | Where-Object { $_.Message -match '^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+\))?\s*!?\s*:\s*.+' }).Count
        if ($conventionalCount -gt 0) {
            $conventionalRatio = [math]::Round($conventionalCount / $authorCommits.Count * 100, 1)
            $insights += "Using Conventional Commits ($conventionalRatio % of commits)"
        }

        if ($insights.Count -eq 0) {
            $insights += "Solid development practices"
        }

        $DeveloperCards += @{
            Name                 = $author
            Email                = $data.Email
            FirstCommit          = $data.FirstCommit.ToString('yyyy-MM-dd')
            LastCommit           = $data.LastCommit.ToString('yyyy-MM-dd')
            TotalCommits         = $data.Commits
            TotalAdditions       = $data.LinesAdded
            TotalDeletions       = $data.LinesDeleted
            TotalFiles           = $data.FilesChanged.Count
            FileTypes            = $data.Productivity.FilesDiversity
            AvgCommitSize        = $data.Productivity.AverageCommitSize
            CommitsPerDay        = $data.Productivity.CommitFrequency
            IssueLinks           = $issueRefs
            PRIntegrationScore   = $prScore
            BiggestCommitMessage = $data.BiggestCommit.Message
            BiggestCommitStats   = "$($data.BiggestCommit.Size) lines on $($data.BiggestCommit.Date.ToString('yyyy-MM-dd'))"
            Insights             = $insights
        }
    }

    # Generate output
    switch ($Format) {
        "HTML" {
            $outputFile = New-HtmlReport -ReportData $reportData -OutputPath $OutputPath -DeveloperCards $DeveloperCards
            Write-Host "✅ HTML report generated: $outputFile" -ForegroundColor Green

            # Open the report in default browser
            if (Get-Command "Start-Process" -ErrorAction SilentlyContinue) {
                Start-Process $outputFile
            }
        }
        "JSON" {
            $outputFile = New-JsonReport -ReportData $reportData -OutputPath $OutputPath
            Write-Host "✅ JSON report generated: $outputFile" -ForegroundColor Green
        }
        "Console" {
            Show-ConsoleReport -ReportData $reportData
        }
    }

    Write-Host "`n🎉 GitHub Git Analysis complete!" -ForegroundColor Green
    Write-Host "Summary: $($reportData.Metrics.TotalCommits) commits, $($reportData.Authors.Count) authors, $($reportData.Metrics.TotalFilesChanged) files changed" -ForegroundColor Cyan
    Write-Host "GitHub Integration: $($reportData.GitHubIntegration.IssueReferences.Total) issue references, $($reportData.GitHubIntegration.PullRequestPatterns.PullRequestMerges) PR merges" -ForegroundColor Cyan
}
catch {
    Write-Error "❌ Error during analysis: $_"
    exit 1
}
