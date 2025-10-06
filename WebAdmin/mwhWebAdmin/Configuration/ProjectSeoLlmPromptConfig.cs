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

        return $@"You are an expert SEO specialist and technical project analyst. Your task is to analyze a GitHub repository and generate comprehensive, SEO-optimized project metadata that will help showcase this project professionally.

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
Generate professional, SEO-optimized project metadata that:

1. **SEO Requirements:**
   - Title: 30-60 characters, keyword-rich, professional
   - Meta Description: 120-160 characters with action words (discover, explore, learn, build)
   - Keywords: 3-8 relevant technical and business keywords
   - Social media optimized titles and descriptions

2. **Technical Analysis:**
   - Identify and categorize the project type (Web App, API, Library, Tool, etc.)
   - Determine the tech stack and framework
   - Suggest appropriate project categorization
   - Analyze CI/CD pipeline patterns from workflows

3. **Professional Presentation:**
   - Write compelling descriptions that highlight business value
   - Use industry-standard terminology
   - Focus on outcomes and benefits
   - Make it appealing to both technical and business audiences

4. **Repository Integration:**
   - Auto-fill repository metadata based on GitHub analysis
   - Suggest deployment environments based on workflow patterns
   - Recommend promotion pipeline stages based on detected CI/CD

## Key Guidelines:
- Write for a professional portfolio audience (potential employers, clients, collaborators)
- Emphasize practical applications and business impact
- Use modern, current technical terminology
- Make descriptions scannable and engaging
- Ensure all content is factual and based on repository evidence
- Focus on what makes this project unique and valuable

## Response Format:
Return a JSON object with all the required fields populated based on your analysis. Be thorough but concise. Every field should add value to the project's presentation.

Remember: This project data will be used for professional portfolio presentation, SEO optimization, and project showcase. Make it compelling and accurate.";
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
                "spring", "asp.net", "blazor", "next.js", "nuxt", "svelte"
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
