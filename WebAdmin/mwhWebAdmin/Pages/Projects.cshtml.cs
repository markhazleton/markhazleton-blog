using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public class ProjectsModel : BasePageModel
{
    private readonly ProjectService _projectService;

    public ProjectsModel(ProjectService projectService, ArticleService articleService, ILogger<ProjectsModel> logger)
        : base(articleService, projectService, logger)
    {
        _projectService = projectService;
    }

    public List<ProjectModel> Projects { get; private set; } = new();

    public void OnGet()
    {
        try
        {
            Projects = _projectService.GetProjects();
        }
        catch (Exception)
        {
            Projects = new List<ProjectModel>();
            TempData["ErrorMessage"] = "An error occurred while loading projects.";
        }
    }

    public IActionResult OnPostDelete(int id)
    {
        try
        {
            _projectService.DeleteProject(id);
            TempData["SuccessMessage"] = "Project deleted successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting the project: {ex.Message}";
        }

        return RedirectToPage();
    }
}
