using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Models;

namespace mwhWebAdmin.Pages;

public class ArticleEditModel : PageModel
{
    private readonly ArticleService _articleService;
    public List<string> Sections { get; } = new List<string>
    {
        "Development",
        "Personal Philosophy",
        "Project Mechanics",
        "Project Mechanics Leadership",

    };
    [BindProperty]
    public ArticleModel Article { get; set; }

    public ArticleEditModel(ArticleService articleService)
    {
        _articleService = articleService;
    }

    public IActionResult OnGet(string slug)
    {
        Article = _articleService.GetArticleBySlug(slug);
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
