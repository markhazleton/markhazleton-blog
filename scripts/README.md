# BSW.EHR Scripts Directory

This directory contains powerful automation and analysis scripts for the BSW.EHR project - Baylor Scott & White Electronic Health Record system.

BSW.EHR is a comprehensive healthcare technology platform providing electronic health record capabilities, patient management, and healthcare workflow optimization for Baylor Scott & White Health.

## 📊 Azure DevOps Git Activity Report Generator

### Overview

The `Generate-GitActivityReport.ps1` script is a comprehensive Git repository analyzer specifically designed for Azure DevOps projects. It generates world-class development activity reports with advanced metrics, Azure DevOps integration analysis, and healthcare industry best practices.

#### 🚀 New in v2.0: Azure DevOps Integration Focus

Enhanced with Azure DevOps-specific metrics and analysis:

- **Azure DevOps Integration Health**: Work Item linking analysis, Pull Request workflow assessment
- **Commit Summary Table**: Detailed table of recent commits with author, datetime, message, lines changed, and files touched
- **Work Item Traceability**: Analysis of #WorkItemID references and Azure Boards integration
- **Pull Request Analysis**: PR merge patterns, code review workflow assessment
- **Azure DevOps Best Practices**: Recommendations specific to Azure DevOps workflows and governance
- **Team Collaboration Metrics**: Multi-author project analysis and collaboration scoring
- **Branch Policy Compliance**: Assessment of merge vs. direct commit patterns

## 🔒 Security Pattern Analyzer

### Overview

The `Analyze-SecurityPatterns.ps1` script performs comprehensive security analysis of the BSW.EHR codebase, focusing on healthcare-specific security patterns and HIPAA compliance requirements.

### Features

#### 🏥 **Healthcare Security Focus**

- **HIPAA Compliance**: PHI handling and patient identifier protection
- **Healthcare Standards**: HL7/FHIR implementation patterns
- **Audit Requirements**: Logging and audit trail analysis
- **Minimum Necessary**: Access control pattern verification

#### 🔐 **Security Pattern Detection**

- **Encryption Analysis**: AES, RSA, and hashing algorithm usage
- **Authentication Patterns**: JWT, OAuth, Identity framework usage
- **Authorization Controls**: Role-based and claims-based access
- **Input Validation**: SQL injection and XSS protection

#### 🚨 **Vulnerability Assessment**

- **Hardcoded Secrets**: Configuration security analysis
- **Weak Cryptography**: Random number generation assessment
- **Protocol Security**: HTTP vs HTTPS usage patterns

### Usage

```powershell
# Generate security analysis report
.\scripts\Analyze-SecurityPatterns.ps1

# Analyze specific project
.\scripts\Analyze-SecurityPatterns.ps1 -ScanPath ".\BSW.EHR.Api"

# Generate JSON for CI/CD integration
.\scripts\Analyze-SecurityPatterns.ps1 -Format JSON
```

## 📦 Dependency Analyzer

### Overview

The `Analyze-Dependencies.ps1` script analyzes .NET dependencies for security vulnerabilities, compliance issues, and outdated packages with a focus on healthcare application requirements.

### Features

#### 🔍 **Comprehensive Dependency Analysis**

- **Vulnerability Detection**: Known security issues in packages
- **Update Recommendations**: Outdated package identification
- **License Compliance**: Healthcare-appropriate licensing
- **Security Package Assessment**: Healthcare-specific security libraries

#### 🏥 **Healthcare Package Guidelines**

- **Security Libraries**: Authentication and encryption packages
- **Logging Frameworks**: Audit trail and compliance logging
- **Validation Tools**: Input validation and data integrity
- **Healthcare Standards**: HL7/FHIR support libraries

### Usage

```powershell
# Analyze solution dependencies
.\scripts\Analyze-Dependencies.ps1

# Analyze specific solution file
.\scripts\Analyze-Dependencies.ps1 -SolutionPath ".\BSW.EHR.sln"

# Check for vulnerabilities
.\scripts\Analyze-Dependencies.ps1 -CheckVulnerabilities $true
```

## 📊 Code Quality Analyzer

### Overview

The `Analyze-CodeQuality.ps1` script performs comprehensive code quality analysis with metrics, complexity assessment, and healthcare-specific quality checks.

### Features

#### 📈 **Quality Metrics**

- **Complexity Analysis**: McCabe complexity calculations
- **Code Metrics**: Lines of code, method counts, class counts
- **Quality Scoring**: 0-100 quality score per file
- **Technical Debt**: TODO, FIXME, and HACK detection

#### 🏥 **Healthcare Quality Focus**

- **Patient Safety**: Error handling pattern analysis
- **Clinical Algorithms**: Documentation and testing requirements
- **Compliance**: Code structure for regulatory requirements
- **Reliability**: Async pattern and exception handling

### Usage

```powershell
# Generate code quality report
.\scripts\Analyze-CodeQuality.ps1

# Analyze specific directory
.\scripts\Analyze-CodeQuality.ps1 -ScanPath ".\BSW.EHR.Api"

# Console output for quick checks
.\scripts\Analyze-CodeQuality.ps1 -Format Console
```

### Azure DevOps Integration Features

#### 🔍 **Azure DevOps Analysis**

- **Work Item Integration**: Analysis of #WorkItemID references and Azure Boards connectivity
- **Pull Request Workflow**: PR merge patterns, code review effectiveness, and branch policy compliance
- **Azure DevOps Best Practices**: Governance recommendations specific to Azure DevOps workflows
- **Team Collaboration Assessment**: Multi-author project analysis with collaboration scoring
- **Commit Message Quality**: Analysis of Azure DevOps commit message standards and work item linking
- **Integration Health Scoring**: Overall assessment of Azure DevOps tools integration

#### � **Enhanced Commit Summary**

- **Detailed Commit Table**: Last N commits with author, datetime, message, lines added/deleted, files touched
- **Work Item Traceability**: Identification of commits linked to Azure Boards work items
- **Merge Pattern Analysis**: Detection of Pull Request merges vs. direct commits
- **Commit Size Analysis**: Classification of commits as small, reviewable, or large changes

#### 📈 **Comprehensive Analytics**

- **Code Churn Analysis**: Lines added, deleted, and modified with trend analysis
- **Author Productivity**: Individual contributor metrics, velocity, and collaboration patterns
- **File Modification Patterns**: Hotspot analysis and change distribution across codebase
- **Quality Assessment**: Commit frequency, review patterns, and development workflow health
- **Time Pattern Analysis**: Business hours vs. after-hours development patterns

#### 🎨 **Multiple Output Formats**

- **HTML**: Beautiful, interactive web report with charts and visualizations
- **JSON**: Machine-readable data for integration with other tools
- **Console**: Quick overview directly in the terminal

### Azure DevOps Project Usage

#### Basic Usage

```powershell
# Generate HTML report for last 10 days with 50 commit summary (default)
.\scripts\Generate-GitActivityReport.ps1

# Generate report for last 14 days with Azure DevOps focus
.\scripts\Generate-GitActivityReport.ps1 -Days 14

# Generate JSON report for Azure DevOps integration with CI/CD
.\scripts\Generate-GitActivityReport.ps1 -Format JSON -CommitSummaryCount 100

# Console output for daily standups and sprint reviews
.\scripts\Generate-GitActivityReport.ps1 -Format Console -Days 7

# Custom output location for team reports
.\scripts\Generate-GitActivityReport.ps1 -OutputPath "C:\AzureDevOpsReports"
```

#### Advanced Azure DevOps Examples

```powershell
# Comprehensive sprint analysis for Azure DevOps sprint reviews (2 weeks)
.\scripts\Generate-GitActivityReport.ps1 -Days 14 -Format HTML -OutputPath ".\sprint-reports" -CommitSummaryCount 75

# Quick console overview for daily Azure DevOps team standups  
.\scripts\Generate-GitActivityReport.ps1 -Days 7 -Format Console -CommitSummaryCount 20

# Generate JSON for Azure DevOps CI/CD integration and quality gates
.\scripts\Generate-GitActivityReport.ps1 -Days 30 -Format JSON -OutputPath ".\azure-devops-reports" -CommitSummaryCount 200

# Monthly team retrospective with comprehensive Azure DevOps analysis
.\scripts\Generate-GitActivityReport.ps1 -Days 30 -Format HTML -CommitSummaryCount 100

# Weekly development velocity tracking for Azure DevOps Analytics integration
.\scripts\Generate-GitActivityReport.ps1 -Days 7 -Format JSON -CommitSummaryCount 50
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `-Days` | Integer | 10 | Number of days to analyze for activity metrics |
| `-OutputPath` | String | Current directory | Directory where the report will be saved |
| `-Format` | String | HTML | Output format: HTML, JSON, or Console |
| `-CommitSummaryCount` | Integer | 50 | Number of recent commits to include in detailed summary table |

### Azure DevOps Report Sections

#### 📊 **Key Metrics Dashboard**

- Visual overview of repository activity with Azure DevOps integration health
- Code churn statistics and development velocity
- Work Item integration and Pull Request workflow metrics

#### 🔗 **Azure DevOps Integration Analysis**

- Work Item reference tracking and traceability metrics
- Pull Request workflow assessment and merge pattern analysis
- Branch policy compliance and code review effectiveness
- Team collaboration scoring and multi-author project health

#### 📋 **Recent Commits Summary Table**

- Detailed tabular view of recent commits (configurable count)
- Author, datetime, commit message, lines changed, and files touched
- Work Item linking identification and merge commit detection
- Commit size categorization for code review effectiveness

#### 👥 **Author & Team Analysis**

- Individual contributor statistics with Azure DevOps workflow integration
- Productivity metrics per author with collaboration patterns
- Work Item ownership and contribution distribution
- Team velocity and development consistency patterns

#### 📁 **File Modification Analysis**

- Most frequently modified files with change concentration analysis
- Hotspot identification for refactoring opportunities
- Code ownership patterns and knowledge distribution risks

#### 🔍 **Azure DevOps Health Assessment**

- Overall repository and team health scoring
- Azure DevOps best practices compliance assessment
- Integration effectiveness between Repos, Boards, and Pipelines
- Quality indicators specific to Azure DevOps workflows

#### 💡 **Azure DevOps Best Practice Recommendations**

- Workflow optimization suggestions for Azure DevOps
- Branch policy and Pull Request workflow improvements
- Work Item integration and traceability enhancements
- Team collaboration and code review process recommendations

### Sample Insights

The script provides intelligent analysis including:

- **High Churn Detection**: Identifies files with excessive modifications
- **Commit Pattern Analysis**: Evaluates commit frequency and size
- **Collaboration Assessment**: Analyzes multi-author contributions
- **Quality Metrics**: Commit message quality and merge patterns
- **Trend Analysis**: Development velocity and consistency

### Requirements

- **PowerShell 5.1+**
- **Git** (accessible from command line)
- Must be run from within a Git repository

### Best Practices

1. **Regular Analysis**: Run weekly or bi-weekly for ongoing insights
2. **Team Reviews**: Share HTML reports in team meetings and sprint retrospectives
3. **CI/CD Integration**: Use JSON format for automated quality gates
4. **Historical Tracking**: Archive reports to track improvements over time
5. **Healthcare Compliance**: Monitor code quality for healthcare applications and HIPAA compliance
6. **EHR Development**: Track changes to patient data handling, clinical workflows, and integration components

### Output Examples

#### HTML Report Features

- 🎨 Beautiful, responsive design
- 📊 Interactive metrics dashboard
- 📈 Visual trend analysis
- 🔍 Detailed author breakdowns
- 💡 Actionable recommendations

#### JSON Output Structure

```json
{
  "GeneratedAt": "2025-08-20T...",
  "AnalysisPeriod": 10,
  "Repository": { ... },
  "Metrics": { ... },
  "Authors": { ... },
  "Insights": { ... },
  "Recommendations": [ ... ]
}
```

### Troubleshooting

#### Common Issues

1. **Not in Git Repository**

   ```
   Error: This script must be run from within a Git repository.
   ```

   Solution: Navigate to your Git repository root directory

2. **Git Not Found**

   ```
   Error: Git command not found
   ```

   Solution: Ensure Git is installed and accessible in PATH

3. **Permission Issues**

   ```
   Error: Cannot write to output directory
   ```

   Solution: Check write permissions for the output directory

### Integration Examples

#### CI/CD Pipeline Integration

```yaml
# GitHub Actions example
- name: Generate Git Activity Report
  run: |
    .\scripts\Generate-GitActivityReport.ps1 -Format JSON -OutputPath "./reports"
    # Process JSON for quality gates
```

#### Automated Team Reports

```powershell
# Weekly team report automation
$report = .\scripts\Generate-GitActivityReport.ps1 -Days 7 -Format HTML
# Email or post to team chat
```

### Contributing

When adding new scripts to this directory:

1. Follow PowerShell best practices
2. Include comprehensive help documentation
3. Add parameter validation
4. Implement proper error handling
5. Update this README with new script documentation

### Version History

- **v2.0**: Azure DevOps Integration Focus
  - Enhanced Azure DevOps work item integration analysis
  - Pull Request workflow assessment and best practices
  - Detailed commit summary table with configurable count
  - Azure DevOps specific recommendations and health scoring  
  - Work item traceability and linking analysis
  - Team collaboration metrics and scoring
  - Branch policy compliance assessment

- **v1.0**: Initial release with comprehensive Git activity analysis
  - Code churn metrics and author productivity analysis
  - HTML/JSON/Console output formats
  - Basic best practices recommendations
  - File modification patterns and insights

---

*This script is part of the BSW.EHR development toolkit, designed to promote high-quality development practices and team collaboration for healthcare technology solutions. BSW.EHR is a comprehensive .NET healthcare platform supporting electronic health records, patient management, and clinical workflow optimization for Baylor Scott & White Health.*
