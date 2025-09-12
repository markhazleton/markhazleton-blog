# GitHub Activity Report Script Updates

## Overview

Updated `Generate-GitActivityReport.ps1` to focus on GitHub repository statistics and best practices, removing all Azure DevOps references.

## Key Changes Made

### 1. Updated Script Header and Documentation

- Changed script synopsis from "Azure DevOps projects" to "GitHub repositories"
- Updated description to focus on GitHub-specific metrics
- Changed author from "BSW.EHR Development Team" to "Mark Hazleton"
- Updated version to 3.0 with GitHub focus
- Enhanced examples and parameter descriptions for GitHub context

### 2. GitHub Integration Analysis Function

**Replaced:** `Get-AzureDevOpsIntegration` with `Get-GitHubIntegration`

**New GitHub-Specific Features:**

- **Issue References**: Detects GitHub issue references (#123, GH-123, gh-123)
- **GitHub Keywords**: Recognizes GitHub-specific action keywords (fix, fixes, fixed, close, closes, resolve, resolves)
- **Co-authored Commits**: Detects collaborative commits with `Co-authored-by:`
- **Breaking Changes**: Identifies commits with breaking change indicators
- **Conventional Commits**: Full support for conventional commit format detection
- **Revert Commits**: Tracks revert patterns
- **Squash Merge Detection**: Improved detection of squash merges with PR numbers

### 3. Conventional Commits Support

**New Tracking Categories:**

- `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `build`, `ci`
- Breaking change detection (`!` indicator or `BREAKING CHANGE:`)
- Scope detection in conventional commits

### 4. GitHub Actions Integration

**New Function:** `Get-GitHubActionsMetrics`

- Analyzes `.github/workflows/` directory
- Detects workflow triggers (push, pull_request, schedule, manual, release)
- Identifies workflow types (CI/CD, testing, security, deployment)
- Provides recommendations for missing automation

### 5. Updated Data Structure

**Replaced:**

- `AzureDevOpsIntegration` → `GitHubIntegration`
- `WorkItemReferences` → `IssueReferences`
- `WorkItemIntegration` → `IssueIntegration`

**Added:**

- `GitHubActionsMetrics` for workflow analysis
- `ConventionalCommits` breakdown by type
- Enhanced commit message quality metrics

### 6. Insights and Recommendations

**GitHub-Focused Recommendations:**

- Pull Request workflow implementation
- GitHub branch protection rules
- GitHub code owners usage
- GitHub Discussions for team communication
- Conventional Commits adoption
- GitHub Actions automation
- Security scanning (CodeQL, Dependabot)
- Draft pull requests for WIP

### 7. Report Generation Updates

**HTML Report:**

- Updated title to "GitHub Repository Activity Report"
- Added Conventional Commits metric card
- Updated section headers and descriptions
- GitHub-themed styling and terminology

**Console Report:**

- GitHub integration section instead of Azure DevOps
- Conventional commits usage statistics
- Co-authored commits detection
- GitHub Actions workflow recommendations

**Developer Analysis:**

- Issue references instead of work item references
- Conventional commits usage per developer
- GitHub-specific collaboration patterns

### 8. Function Name Improvements

**Fixed PowerShell Best Practices:**

- `Generate-HtmlReport` → `New-HtmlReport`
- `Generate-JsonReport` → `New-JsonReport`
- Used approved PowerShell verbs

## GitHub Best Practices Implemented

### 1. Issue Management

- Automatic detection of issue references in commit messages
- Support for GitHub's linking keywords (fixes, closes, resolves)
- Tracking of issue integration by author

### 2. Pull Request Workflow

- Enhanced merge commit detection
- Squash merge identification
- Pull request usage metrics and recommendations

### 3. Conventional Commits

- Full conventional commit specification support
- Automatic changelog generation readiness
- Breaking change detection
- Scoped commit tracking

### 4. GitHub Actions Integration

- Workflow file analysis
- CI/CD pipeline detection
- Security automation recommendations
- Testing workflow identification

### 5. Collaboration Features

- Co-authored commit detection
- Branch protection recommendations
- Code owners suggestions
- GitHub Discussions recommendations

## Usage Examples

### Basic Analysis

```powershell
.\Generate-GitActivityReport.ps1
```

### Extended Analysis with JSON Output

```powershell
.\Generate-GitActivityReport.ps1 -Days 30 -Format JSON -OutputPath "C:\Reports"
```

### Console Output for CI/CD

```powershell
.\Generate-GitActivityReport.ps1 -Days 14 -Format Console
```

## Benefits of GitHub Focus

1. **Better Issue Tracking**: Native GitHub issue integration vs Azure DevOps work items
2. **Enhanced Automation**: GitHub Actions analysis and recommendations
3. **Improved Collaboration**: Support for GitHub-specific collaboration features
4. **Modern Workflow**: Conventional commits and automated changelog support
5. **Security Focus**: Built-in security scanning recommendations
6. **Open Source Ready**: Compatible with GitHub's open source workflow patterns

## Next Steps Recommendations

1. **Enable GitHub Actions**: Implement automated workflows for CI/CD
2. **Branch Protection**: Set up branch protection rules
3. **Conventional Commits**: Adopt conventional commit format
4. **Security Scanning**: Enable CodeQL and Dependabot
5. **Code Reviews**: Implement mandatory pull request reviews
6. **Issue Templates**: Create issue and PR templates

The updated script now provides comprehensive GitHub repository analysis with actionable insights for improving development workflow and code quality using GitHub's native features and best practices.
