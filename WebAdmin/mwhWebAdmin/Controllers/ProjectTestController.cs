using Microsoft.AspNetCore.Mvc;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Controllers;

/// <summary>
/// Test controller for project AI generation functionality
/// </summary>
[ApiController]
[Route("api/test")]
public class ProjectTestController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly GitHubIntegrationService _gitHubService;
    private readonly ILogger<ProjectTestController> _logger;

    public ProjectTestController(
        ProjectService projectService, 
        GitHubIntegrationService gitHubService, 
        ILogger<ProjectTestController> logger)
    {
        _projectService = projectService;
        _gitHubService = gitHubService;
        _logger = logger;
    }

    /// <summary>
    /// Tests project SEO generation with repository analysis
    /// </summary>
    /// <param name="request">Test request with project data</param>
    /// <returns>Generated project SEO data</returns>
    [HttpPost("test-project-generation")]
    public async Task<IActionResult> TestProjectGeneration([FromBody] TestProjectGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Testing project AI generation for: {Title}", request.Title);

            // Create a test project
            var testProject = new ProjectModel
            {
                Title = request.Title ?? "Test Project",
                Description = request.Description,
                Link = request.ProjectUrl,
                Repository = new ProjectRepository
                {
                    Url = request.RepositoryUrl
                }
            };

            // Generate AI content
            await _projectService.AutoGenerateProjectDataAsync(testProject);

            var result = new
            {
                success = true,
                message = "Project AI generation completed successfully",
                generatedProject = new
                {
                    title = testProject.Title,
                    description = testProject.Description,
                    summary = testProject.Summary,
                    keywords = testProject.Keywords,
                    seoTitle = testProject.Seo?.Title,
                    seoDescription = testProject.Seo?.Description,
                    seoKeywords = testProject.Seo?.Keywords,
                    canonical = testProject.Seo?.Canonical,
                    openGraphTitle = testProject.OpenGraph?.Title,
                    openGraphDescription = testProject.OpenGraph?.Description,
                    twitterTitle = testProject.TwitterCard?.Title,
                    twitterDescription = testProject.TwitterCard?.Description,
                    repositoryProvider = testProject.Repository?.Provider,
                    repositoryVisibility = testProject.Repository?.Visibility,
                    repositoryBranch = testProject.Repository?.Branch,
                    repositoryNotes = testProject.Repository?.Notes,
                    promotionPipeline = testProject.Promotion?.Pipeline,
                    promotionCurrentStage = testProject.Promotion?.CurrentStage,
                    promotionStatus = testProject.Promotion?.Status,
                    promotionNotes = testProject.Promotion?.Notes,
                    environments = testProject.Promotion?.Environments?.Where(e => !string.IsNullOrEmpty(e.Name)).Select(e => new
                    {
                        name = e.Name,
                        url = e.Url,
                        status = e.Status,
                        version = e.Version,
                        notes = e.Notes
                    })
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing project AI generation");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                message = "Failed to generate project AI content"
            });
        }
    }

    /// <summary>
    /// Tests GitHub repository analysis
    /// </summary>
    /// <param name="request">Repository analysis request</param>
    /// <returns>Repository analysis results</returns>
    [HttpPost("test-repository-analysis")]
    public async Task<IActionResult> TestRepositoryAnalysis([FromBody] TestRepositoryAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Testing repository analysis for: {RepositoryUrl}", request.RepositoryUrl);

            var analysis = await _gitHubService.AnalyzeRepositoryAsync(request.RepositoryUrl);

            var result = new
            {
                success = true,
                message = "Repository analysis completed successfully",
                analysis = new
                {
                    description = analysis.Description,
                    language = analysis.Language,
                    topics = analysis.Topics,
                    license = analysis.License,
                    isPrivate = analysis.IsPrivate,
                    defaultBranch = analysis.DefaultBranch,
                    lastUpdated = analysis.LastUpdated,
                    readmeLength = analysis.ReadmeContent?.Length ?? 0,
                    readmePreview = analysis.ReadmeContent?.Substring(0, Math.Min(analysis.ReadmeContent.Length, 200)) + "...",
                    workflows = analysis.Workflows.Select(w => new
                    {
                        name = w.Name,
                        path = w.Path,
                        state = w.State,
                        createdAt = w.CreatedAt,
                        updatedAt = w.UpdatedAt
                    }),
                    packageInfo = analysis.PackageInfo != null ? new
                    {
                        packageManager = analysis.PackageInfo.PackageManager,
                        framework = analysis.PackageInfo.Framework,
                        runtime = analysis.PackageInfo.Runtime,
                        dependencyCount = analysis.PackageInfo.Dependencies.Count,
                        devDependencyCount = analysis.PackageInfo.DevDependencies.Count,
                        keyDependencies = analysis.PackageInfo.Dependencies.Take(10)
                    } : null
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing repository analysis");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                message = "Failed to analyze repository"
            });
        }
    }
}

/// <summary>
/// Request model for testing project generation
/// </summary>
public class TestProjectGenerationRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public string? RepositoryUrl { get; set; }
}

/// <summary>
/// Request model for testing repository analysis
/// </summary>
public class TestRepositoryAnalysisRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
}
