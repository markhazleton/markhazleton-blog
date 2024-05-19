using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public class ProjectEditModel : PageModel
{
    private readonly ProjectService _projectService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProjectEditModel(ProjectService projectService, IWebHostEnvironment webHostEnvironment)
    {
        _projectService = projectService;
        _webHostEnvironment = webHostEnvironment;
    }

    [BindProperty]
    public ProjectModel Project { get; set; }

    public List<string> AvailableImages { get; set; }

    public IActionResult OnGet(int id)
    {
        Project = _projectService.GetProjectById(id);

        if (Project == null)
        {
            return NotFound();
        }

        // Get the list of available images from the virtual directory
        var imagesDirectory = Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"));
        var rootPath = Path.GetFullPath(Path.Combine("..", "..", "src"));
        var images = Directory.GetFiles(imagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Select(imagePath => imagePath.Replace(rootPath, string.Empty).Replace("\\", "/").TrimStart('/'))
            .ToList();


        AvailableImages = images;

        return Page();
    }

    public IActionResult OnPost()
    {
        // Update the project details
        _projectService.UpdateProject(Project);

        // Redirect to a page displaying the updated project or a list of projects
        return RedirectToPage("/Projects");
    }
}
