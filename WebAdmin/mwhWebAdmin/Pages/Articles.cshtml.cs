using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;

namespace mwhWebAdmin.Pages;

public class ArticlesModel : PageModel
{
    private readonly ArticleService _articleService;

    public List<ArticleModel> Articles { get; private set; }

    public ArticlesModel(ArticleService articleService)
    {
        _articleService = articleService;
    }

    public void OnGet()
    {
        Articles = _articleService.GetArticles();
        _articleService.GenerateSiteMap();
        _articleService.GenerateRSSFeed();
    }
}
