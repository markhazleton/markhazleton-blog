using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public class ArticleAddModel : BasePageModel
{
    private readonly ArticleService _articleService;

    [BindProperty]
    public ArticleModel NewArticle { get; set; }

    public ArticleAddModel(ArticleService articleService, ProjectService projectService, ILogger<ArticleAddModel> logger)
        : base(articleService, projectService, logger)
    {
        _articleService = articleService;
        NewArticle = new ArticleModel();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _articleService.AddArticle(NewArticle);
        return RedirectToPage("Articles");
    }
}
