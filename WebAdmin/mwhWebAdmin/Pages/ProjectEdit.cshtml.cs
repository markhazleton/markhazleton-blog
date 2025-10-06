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

    /// <summary>
    /// Indicates whether this is a new project (true) or editing existing (false)
    /// </summary>
    public bool IsNewProject => Project.Id == 0;

    public IActionResult OnGet(int? id)
    {
        _baseLogger.LogInformation("[ProjectEdit] OnGet called with ID: {ProjectId}", id);
        
        try
        {
            if (id.HasValue && id.Value > 0)
            {
                // Edit existing project
                _baseLogger.LogInformation("[ProjectEdit] Loading existing project for edit: {ProjectId}", id.Value);
                var project = _projectService.GetProjectById(id.Value);

                if (project == null)
                {
                    _baseLogger.LogWarning("[ProjectEdit] Project not found: {ProjectId}", id.Value);
                    return NotFound($"Project with ID {id.Value} not found.");
                }

                Project = project;
                _baseLogger.LogInformation("[ProjectEdit] Loaded project: {Title}", Project.Title);
            }
            else
            {
                // Add new project
                _baseLogger.LogInformation("[ProjectEdit] Initializing new project form");
                Project = new ProjectModel();
            }

            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "[ProjectEdit] Failed to load project {ProjectId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while loading the project.");
            EnsureProjectAdvancedFields(Project);
            LoadAvailableImages();
            return Page();
        }
    }

    public IActionResult OnPost()
    {
        var operationType = IsNewProject ? "ADD" : "UPDATE";
        _baseLogger.LogInformation("[ProjectEdit] ===== STARTING PROJECT {Operation} PROCESS =====", operationType);
        _baseLogger.LogInformation("[ProjectEdit] OnPost() method called - THIS IS THE REGULAR SAVE, NOT AI GENERATION");
        _baseLogger.LogInformation("[ProjectEdit] Project ID: {ProjectId} (IsNew: {IsNew})", Project.Id, IsNewProject);
        
        try
        {
            if (IsNewProject)
            {
                // For new projects, ensure ID is 0
                Project.Id = 0;
            }
            else if (Project.Id == 0)
            {
                _baseLogger.LogError("[ProjectEdit] Project ID was not preserved during form submission for update");
                ModelState.AddModelError(string.Empty, "Project ID was not preserved during form submission. Please try again.");
                EnsureProjectAdvancedFields(Project);
                LoadAvailableImages();
                return Page();
            }

            _baseLogger.LogInformation("[ProjectEdit] Model validation state: {IsValid}", ModelState.IsValid);
            
            if (!ModelState.IsValid)
            {
                _baseLogger.LogWarning("[ProjectEdit] Model validation failed");
                foreach (var error in ModelState)
                {
                    if (error.Value?.Errors.Any() == true)
                    {
                        _baseLogger.LogWarning("[ProjectEdit] Validation error for {Field}: {Errors}", 
                            error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                
                EnsureProjectAdvancedFields(Project);
                LoadAvailableImages();
                return Page();
            }

            _baseLogger.LogInformation("[ProjectEdit] Model validation passed");
            _baseLogger.LogInformation("[ProjectEdit] Project details - Title: {Title}, Description: {Desc}, Link: {Link}, Slug: {Slug}",
                Project.Title, 
                Project.Description?.Substring(0, Math.Min(50, Project.Description?.Length ?? 0)),
                Project.Link,
                Project.Slug);

            if (IsNewProject)
            {
                _baseLogger.LogInformation("[ProjectEdit] Calling ProjectService.AddProject...");
                _projectService.AddProject(Project);
                _baseLogger.LogInformation("[ProjectEdit] ProjectService.AddProject completed successfully");
                TempData["SuccessMessage"] = "Project added successfully!";
            }
            else
            {
                _baseLogger.LogInformation("[ProjectEdit] Calling ProjectService.UpdateProject...");
                _projectService.UpdateProject(Project);
                _baseLogger.LogInformation("[ProjectEdit] ProjectService.UpdateProject completed successfully");
                TempData["SuccessMessage"] = "Project updated successfully!";
            }
            
            _baseLogger.LogInformation("[ProjectEdit] Operation completed successfully, redirecting to Projects page");
            return RedirectToPage("/Projects");
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "[ProjectEdit] CRITICAL ERROR occurred while {Operation} project. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                operationType.ToLower(), ex.GetType().Name, ex.Message, ex.StackTrace);
            
            ModelState.AddModelError(string.Empty, $"An error occurred while {operationType.ToLower()} the project: {ex.Message}");
            
            try
            {
                EnsureProjectAdvancedFields(Project);
                LoadAvailableImages();
                _baseLogger.LogInformation("[ProjectEdit] Error handling completed, returning to form");
                return Page();
            }
            catch (Exception setupEx)
            {
                _baseLogger.LogError(setupEx, "[ProjectEdit] CRITICAL ERROR in error handling setup");
                throw; // Re-throw if we can't even set up the form
            }
        }
    }

    /// <summary>
    /// Handles AI-powered project data generation for both add and edit modes
    /// </summary>
    public async Task<IActionResult> OnPostGenerateAiContentAsync()
    {
        var operationType = IsNewProject ? "ADD" : "EDIT";
        _baseLogger.LogInformation("[ProjectEdit] ===== STARTING AI GENERATION PROCESS ({Operation}) =====", operationType);
        _baseLogger.LogInformation("[ProjectEdit] OnPostGenerateAiContentAsync() method called - THIS IS AI GENERATION, NOT SAVE");
        _baseLogger.LogInformation("[ProjectEdit] Project ID: {ProjectId} (IsNew: {IsNew})", Project.Id, IsNewProject);
        
        try
        {
            _baseLogger.LogInformation("[ProjectEdit] Starting AI project data generation for project ID: {ProjectId}, Title: {Title}", Project.Id, Project.Title);

            // Ensure project fields are initialized
            EnsureProjectAdvancedFields(Project);

            // Check if we have enough data to work with
            if (string.IsNullOrWhiteSpace(Project.Title) && 
                string.IsNullOrWhiteSpace(Project.Description) && 
                string.IsNullOrWhiteSpace(Project.Repository?.Url))
            {
                _baseLogger.LogWarning("[ProjectEdit] Insufficient data for AI generation");
                TempData["WarningMessage"] = "Please provide at least a project title, description, or repository URL before generating AI content.";
                LoadAvailableImages();
                return Page();
            }

            // Log the data we're starting with
            _baseLogger.LogInformation("[ProjectEdit] Pre-AI Generation - Title: {Title}, Description: {Desc}, RepoUrl: {RepoUrl}", 
                Project.Title, Project.Description?.Substring(0, Math.Min(50, Project.Description?.Length ?? 0)), Project.Repository?.Url);

            // Generate AI content
            _baseLogger.LogInformation("[ProjectEdit] Calling ProjectService.AutoGenerateProjectDataAsync...");
            await _projectService.AutoGenerateProjectDataAsync(Project);
            _baseLogger.LogInformation("[ProjectEdit] AI generation completed");

            // Log the data after AI generation
            _baseLogger.LogInformation("[ProjectEdit] Post-AI Generation - Title: {Title}, SEO Title: {SeoTitle}, OG Title: {OGTitle}, Twitter Title: {TwitterTitle}, Repo Provider: {RepoProvider}, Pipeline: {Pipeline}", 
                Project.Title, 
                Project.Seo?.Title, 
                Project.OpenGraph?.Title, 
                Project.TwitterCard?.Title, 
                Project.Repository?.Provider, 
                Project.Promotion?.Pipeline);

            // Verify that AI actually populated fields
            var fieldsPopulated = new List<string>();
            if (!string.IsNullOrEmpty(Project.Seo?.Title)) fieldsPopulated.Add("SEO Title");
            if (!string.IsNullOrEmpty(Project.Seo?.Description)) fieldsPopulated.Add("SEO Description");
            if (!string.IsNullOrEmpty(Project.OpenGraph?.Title)) fieldsPopulated.Add("Open Graph Title");
            if (!string.IsNullOrEmpty(Project.TwitterCard?.Title)) fieldsPopulated.Add("Twitter Title");
            if (!string.IsNullOrEmpty(Project.Repository?.Provider)) fieldsPopulated.Add("Repository Provider");
            if (!string.IsNullOrEmpty(Project.Promotion?.Pipeline)) fieldsPopulated.Add("Promotion Pipeline");
            if (Project.Promotion?.Environments?.Any(e => !string.IsNullOrEmpty(e.Name)) == true) fieldsPopulated.Add("Environments");

            // For existing projects, we update the project immediately after AI generation
            if (!IsNewProject && Project.Id > 0)
            {
                _baseLogger.LogInformation("[ProjectEdit] Updating existing project {ProjectId} with AI-generated content", Project.Id);
                _projectService.UpdateProject(Project);
                _baseLogger.LogInformation("[ProjectEdit] Project update completed");
            }

            if (fieldsPopulated.Any())
            {
                _baseLogger.LogInformation("[ProjectEdit] AI successfully populated fields: {Fields}", string.Join(", ", fieldsPopulated));
                
                if (IsNewProject)
                {
                    TempData["SuccessMessage"] = $"AI content generated successfully! Updated: {string.Join(", ", fieldsPopulated)}. Review the populated fields and click 'Save Project' to save.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"AI content generated and saved successfully! Updated: {string.Join(", ", fieldsPopulated)}. All changes have been saved to the project.";
                }
            }
            else
            {
                _baseLogger.LogWarning("[ProjectEdit] AI generation completed but no fields were populated");
                TempData["WarningMessage"] = "AI generation completed but no additional fields were populated. Please check your input data or try again.";
            }

            // CRITICAL: Clear ModelState to allow the updated model values to be displayed
            ModelState.Clear();
            _baseLogger.LogInformation("[ProjectEdit] ModelState cleared to display updated values");

            _baseLogger.LogInformation("[ProjectEdit] AI project data generation completed for project ID: {ProjectId}", Project.Id);
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "[ProjectEdit] Error generating AI project data for project ID: {ProjectId}", Project.Id);
            TempData["ErrorMessage"] = "Failed to generate AI content. Please try again. Error: " + ex.Message;
        }

        // Refresh the page data
        EnsureProjectAdvancedFields(Project);
        LoadAvailableImages();
        _baseLogger.LogInformation("[ProjectEdit] ===== AI GENERATION PROCESS COMPLETED =====");
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

































































































