using mwhWebAdmin.Project;

namespace mwhWebAdmin.Configuration;

/// <summary>
/// Configuration for Project SEO LLM prompts
/// </summary>
public static class ProjectSeoLlmPromptConfig
{
    /// <summary>
    /// Gets the system prompt for project SEO generation based on GitHub repository analysis
    /// </summary>
    /// <param name="projectTitle">Current project title</param>
    /// <param name="repositoryAnalysis">GitHub repository analysis data</param>
    /// <returns>System prompt for LLM</returns>
    public static string GetProjectSeoGenerationPrompt(string projectTitle, GitHubRepositoryAnalysis repositoryAnalysis)
    {
        var techStackInfo = GetTechStackInfo(repositoryAnalysis);
        var workflowInfo = GetWorkflowInfo(repositoryAnalysis);
        var dependencyInfo = GetDependencyInfo(repositoryAnalysis);

        return $@"You are an expert SEO specialist and technical project analyst. Your task is to analyze a GitHub repository and generate comprehensive, SEO-optimized project metadata that will help showcase this project professionally in a developer portfolio.

## Project Context
- Current Title: {projectTitle}
- Repository: {repositoryAnalysis.Description ?? "No description available"}
- Primary Language: {repositoryAnalysis.Language ?? "Not specified"}
- Tech Stack: {techStackInfo}
- Dependencies: {dependencyInfo}
- Workflows/CI-CD: {workflowInfo}
- Topics: {string.Join(", ", repositoryAnalysis.Topics)}
- License: {repositoryAnalysis.License ?? "Not specified"}

## Repository README Analysis
{repositoryAnalysis.ReadmeContent ?? "No README content available"}

## Your Mission
Generate professional, SEO-optimized project metadata that comprehensively covers ALL aspects of project presentation:

### 1. **SEO OPTIMIZATION**
- **Project Title**: Enhance to be professional and keyword-rich (30-60 chars)
- **Meta Description**: Create compelling 120-160 character descriptions with action words
- **Keywords**: Generate 5-8 relevant technical and business keywords
- **SEO Title**: Optimized page title for search engines
- **Canonical URL**: Proper canonical URL structure
- **Robots**: Appropriate robots directive

### 2. **SOCIAL MEDIA OPTIMIZATION**
- **Open Graph**: Complete Facebook/LinkedIn sharing metadata
  - Title: Engaging social media title
  - Description: Compelling social description
  - Image suggestions: Professional project imagery
  - Type: Appropriate content type
- **Twitter Cards**: Complete Twitter sharing metadata
  - Title: Concise Twitter-optimized title (≤50 chars)
  - Description: Twitter-specific description
  - Image suggestions: Twitter card imagery

### 3. **TECHNICAL CLASSIFICATION**
- **Category**: Classify project type (Web App, API, Library, Tool, Mobile App, etc.)
- **Tech Stack**: List primary technologies and frameworks
- **Project Type**: Specific type (SPA, REST API, CLI Tool, npm Package, etc.)

### 4. **REPOSITORY INTEGRATION**
- **Provider**: Auto-detect from URL (GitHub, GitLab, Azure DevOps)
- **Visibility**: Analyze and determine (Public/Private)
- **Branch**: Default branch from analysis
- **Repository Notes**: Key technical insights from README/code analysis

### 5. **DEPLOYMENT PIPELINE**
- **Pipeline Name**: Suggest based on detected CI/CD workflows
- **Current Stage**: Infer deployment stage from workflows
- **Status**: Determine pipeline status
- **Environments**: Suggest appropriate deployment environments:
  - Development environment with dev URLs
  - Staging/Testing environment
  - Production environment
  - Include realistic URLs, statuses, and version suggestions

### 6. **CONTENT STRATEGY**
- Write for professional portfolio audience (employers, clients, collaborators)
- Emphasize practical applications and business impact
- Use modern, current technical terminology
- Make descriptions scannable and engaging
- Ensure all content is factual and based on repository evidence
- Focus on what makes this project unique and valuable

## Key Guidelines:
- **Comprehensive**: Fill ALL fields with meaningful, relevant content
- **Professional**: Portfolio-ready language and presentation
- **SEO-Focused**: Optimize for search engines and social sharing
- **Technical Accuracy**: Base recommendations on actual repository analysis
- **Business Value**: Highlight practical applications and impact
- **Modern Standards**: Use current best practices for SEO and social media

## Quality Standards:
- Titles should be clear, professional, and keyword-optimized
- Descriptions should be engaging and informative
- Keywords should balance technical terms with business language
- Social media content should encourage engagement
- Technical classifications should be accurate and specific
- Environment suggestions should be realistic and practical

Remember: This project data will be used for professional portfolio presentation, search engine optimization, and social media sharing. Every field should add value and present the project in the best possible light while remaining accurate and professional.";
    }

    /// <summary>
    /// Gets formatted tech stack information
    /// </summary>
    private static string GetTechStackInfo(GitHubRepositoryAnalysis analysis)
    {
        var techStack = new List<string>();

        if (!string.IsNullOrEmpty(analysis.Language))
            techStack.Add(analysis.Language);

        if (analysis.PackageInfo?.Framework != null)
            techStack.Add(analysis.PackageInfo.Framework);

        if (analysis.PackageInfo?.Runtime != null)
            techStack.Add(analysis.PackageInfo.Runtime);

        // Add major dependencies that indicate framework/tech choices
        if (analysis.PackageInfo?.Dependencies != null)
        {
            var majorFrameworks = new[]
            {
                "react", "vue", "angular", "express", "django", "flask", "fastapi",
                "spring", "asp.net", "blazor", "next.js", "nuxt", "svelte", "typescript",
                "nodejs", "python", "csharp", "java", "golang", "rust"
            };

            foreach (var dep in analysis.PackageInfo.Dependencies)
            {
                var lowerDep = dep.ToLowerInvariant();
                var framework = majorFrameworks.FirstOrDefault(f => lowerDep.Contains(f.Replace(".", "")));
                if (framework != null && !techStack.Any(t => t.ToLowerInvariant().Contains(framework)))
                {
                    techStack.Add(char.ToUpper(framework[0]) + framework.Substring(1));
                }
            }
        }

        return techStack.Any() ? string.Join(", ", techStack.Distinct()) : "Not specified";
    }

    /// <summary>
    /// Gets formatted workflow information
    /// </summary>
    private static string GetWorkflowInfo(GitHubRepositoryAnalysis analysis)
    {
        if (!analysis.Workflows.Any())
            return "No CI/CD workflows detected";

        var workflowSummary = analysis.Workflows
            .Where(w => w.State == "active")
            .Select(w => w.Name)
            .Take(3)
            .ToList();

        if (!workflowSummary.Any())
            return "CI/CD workflows present but inactive";

        return $"Active workflows: {string.Join(", ", workflowSummary)}";
    }

    /// <summary>
    /// Gets formatted dependency information
    /// </summary>
    private static string GetDependencyInfo(GitHubRepositoryAnalysis analysis)
    {
        if (analysis.PackageInfo?.Dependencies == null || !analysis.PackageInfo.Dependencies.Any())
            return "No dependencies detected";

        var depCount = analysis.PackageInfo.Dependencies.Count;
        var devDepCount = analysis.PackageInfo.DevDependencies?.Count ?? 0;

        return $"{depCount} dependencies" + (devDepCount > 0 ? $", {devDepCount} dev dependencies" : "");
    }
}
