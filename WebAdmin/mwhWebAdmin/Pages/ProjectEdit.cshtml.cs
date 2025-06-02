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

    public List<string> AvailableImages { get; set; } = new(); public IActionResult OnGet(int id)
    {
        try
        {
            var project = _projectService.GetProjectById(id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            Project = project;
            LoadAvailableImages();
            return Page();
        }
        catch (Exception)
        {
            // Log the exception here if you have logging
            ModelState.AddModelError(string.Empty, "An error occurred while loading the project.");
            return Page();
        }
    }
    public IActionResult OnPost()
    {
        // Debug: Check if ID is being preserved
        if (Project.Id == 0)
        {
            ModelState.AddModelError(string.Empty, "Project ID was not preserved during form submission. Please try again.");
            LoadAvailableImages();
            return Page();
        }

        if (!ModelState.IsValid)
        {
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
}
