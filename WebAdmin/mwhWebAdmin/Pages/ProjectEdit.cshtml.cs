using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

public class ProjectEditModel : BasePageModel
{
    private readonly ProjectService _projectService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly GitHubIntegrationService? _gitHubService;

    public ProjectEditModel(ProjectService projectService, ArticleService articleService, IWebHostEnvironment webHostEnvironment, ILogger<ProjectEditModel> logger, GitHubIntegrationService? gitHubService = null)
        : base(articleService, projectService, logger)
    {
        _projectService = projectService;
        _webHostEnvironment = webHostEnvironment;
        _gitHubService = gitHubService;
    }

    [BindProperty]
    public ProjectModel Project { get; set; } = new();

    public List<string> AvailableImages { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        try
        {
            var project = _projectService.GetProjectById(id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            Project = project;
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Failed to load project {ProjectId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while loading the project.");
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }
    }

    public IActionResult OnPost()
    {
        if (Project.Id == 0)
        {
            ModelState.AddModelError(string.Empty, "Project ID was not preserved during form submission. Please try again.");
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }

        if (!ModelState.IsValid)
        {
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }

        try
        {
            _projectService.UpdateProject(Project);
            TempData["SuccessMessage"] = "Project updated successfully!";
            return RedirectToPage("/Projects");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred while updating the project: {ex.Message}");
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }
    }

    /// <summary>
    /// Handles AI-powered project data generation for editing
    /// </summary>
    public async Task<IActionResult> OnPostGenerateAiContentAsync()
    {
        try
        {
            _baseLogger.LogInformation("Starting AI project data generation for project ID: {ProjectId}", Project.Id);

            // Ensure project fields are initialized
            EnsureProjectAdvancedFields(Project);

            // Check if we have enough data to work with
            if (string.IsNullOrWhiteSpace(Project.Title) && 
                string.IsNullOrWhiteSpace(Project.Description) && 
                string.IsNullOrWhiteSpace(Project.Repository?.Url))
            {
                TempData["WarningMessage"] = "Please provide at least a project title, description, or repository URL before generating AI content.";
                LoadAvailableImages();
                return Page();
            }

            // Generate AI content
            await _projectService.AutoGenerateProjectDataAsync(Project);

            // For existing projects, we need to save the changes immediately
            if (Project.Id > 0)
            {
                _projectService.UpdateProject(Project);
                _baseLogger.LogInformation("Updated project {ProjectId} with AI-generated content", Project.Id);
            }

            _baseLogger.LogInformation("AI project data generation completed for project ID: {ProjectId}", Project.Id);
            TempData["SuccessMessage"] = "AI content generated and saved successfully! Updated fields include: Title, Description, SEO metadata, Repository details, and Promotion pipeline information. All changes have been saved to the project.";
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Error generating AI project data for project ID: {ProjectId}", Project.Id);
            TempData["ErrorMessage"] = "Failed to generate AI content. Please try again. Error: " + ex.Message;
        }

        // Refresh the page data
        EnsureProjectAdvancedFields(Project);
        LoadAvailableImages();
        return Page();
    }

    private void LoadAvailableImages()
    {
        try
        {
            var imagesDirectory = Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"));
            var rootPath = Path.GetFullPath(Path.Combine("..", "..", "src"));

            if (Directory.Exists(imagesDirectory))
            {
                var images = Directory.GetFiles(imagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => IsImageFile(file))
                    .Select(imagePath => imagePath.Replace(rootPath, string.Empty).Replace("\\", "/").TrimStart('/'))
                    .ToList();

                AvailableImages = images;
            }
            else
            {
                AvailableImages = new List<string>();
            }
        }
        catch (Exception)
        {
            AvailableImages = new List<string>();
        }
    }

    private static bool IsImageFile(string fileName)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
        return imageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
    }

    private static void EnsureProjectAdvancedFields(ProjectModel project)
    {
        if (project == null)
        {
            return;
        }

        project.Repository ??= new ProjectRepository();

        project.Promotion ??= new ProjectPromotion();
        project.Promotion.Environments ??= new List<PromotionEnvironment>();

        while (project.Promotion.Environments.Count < 3)
        {
            project.Promotion.Environments.Add(new PromotionEnvironment());
        }

        project.Seo ??= new SeoModel();
        project.OpenGraph ??= new OpenGraphModel();
        project.TwitterCard ??= new TwitterCardModel();
    }
}





