using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace mwhWebAdmin.Pages;

public class ArticleEditModel : PageModel
{
    private readonly ArticleService _articleService;

    [BindProperty]
    public ArticleModel Article { get; set; }

    public ArticleEditModel(ArticleService articleService)
    {
        _articleService = articleService;
    }

    public IActionResult OnGet(string Id)
    {
        int id = Convert.ToInt32(Id);
        Article = _articleService.GetArticleById(id);
        if (Article == null)
        {
            return NotFound();
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _articleService.UpdateArticle(Article);
        return RedirectToPage("Articles");
    }
}
