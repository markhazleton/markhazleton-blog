using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public abstract class BasePageModel : PageModel
{
    protected readonly ArticleService _baseArticleService;
    protected readonly ProjectService _baseProjectService;
    protected readonly ILogger _baseLogger;

    protected BasePageModel(ArticleService articleService, ProjectService projectService, ILogger logger)
    {
        _baseArticleService = articleService;
        _baseProjectService = projectService;
        _baseLogger = logger;
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        base.OnPageHandlerExecuting(context);
        SetNavigationCounts();
    }
    protected virtual void SetNavigationCounts()
    {
        try
        {
            var articles = _baseArticleService.GetArticles();
            var projects = _baseProjectService.GetProjects();

            ViewData["ArticleCount"] = articles?.Count ?? 0;
            ViewData["ProjectCount"] = projects?.Count ?? 0;
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Error setting navigation counts");
            ViewData["ArticleCount"] = 0;
            ViewData["ProjectCount"] = 0;
        }
    }
}
