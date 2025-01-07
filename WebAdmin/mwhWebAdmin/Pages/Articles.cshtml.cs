using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;

namespace mwhWebAdmin.Pages;

public class ArticlesModel(ArticleService articleService) : PageModel
{
    public List<ArticleModel> Articles { get; private set; } = [];

    public void OnGet()
    {
        Articles = articleService.GetArticles();
        articleService.GenerateSiteMap();
        articleService.GenerateRSSFeed();
    }
}
