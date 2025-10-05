using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public class ProjectEditModel : BasePageModel
{
    private readonly ProjectService _projectService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProjectEditModel(ProjectService projectService, ArticleService articleService, IWebHostEnvironment webHostEnvironment, ILogger<ProjectEditModel> logger)
        : base(articleService, projectService, logger)
    {
        _projectService = projectService;
        _webHostEnvironment = webHostEnvironment;
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

