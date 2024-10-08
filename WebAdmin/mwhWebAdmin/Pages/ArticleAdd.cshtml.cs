using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace mwhWebAdmin.Pages;

public class ArticleAddModel : PageModel
{
    private readonly ArticleService _articleService;

    [BindProperty]
    public ArticleModel NewArticle { get; set; }

    public ArticleAddModel(ArticleService articleService)
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
