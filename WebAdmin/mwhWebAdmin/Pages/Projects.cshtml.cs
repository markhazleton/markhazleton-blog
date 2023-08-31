using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;


public class ProjectsModel : PageModel
{
    private readonly ProjectService _projectService;

    public ProjectsModel(ProjectService projectService)
    {
        _projectService = projectService;
    }

    public List<ProjectModel> Projects { get; private set; }

    public void OnGet()
    {
        Projects = _projectService.GetProjects();
    }
}
