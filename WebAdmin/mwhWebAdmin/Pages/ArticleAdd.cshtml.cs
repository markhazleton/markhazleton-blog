using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace mwhWebAdmin.Pages;

public class ArticleAddModel : PageModel
{
    private readonly ArticleService _articleService;
    public List<string> Sections { get; } = new List<string>
    {
        "Case Studies",
        "ChatGPT",
        "Development",
        "Data Science",
        "Personal Philosophy",
        "Project Mechanics",
        "Project Mechanics Leadership",
    };
    public List<string> ChangeFrequency { get; } = new List<string>
    {
        "always",
        "hourly",
        "daily",
        "weekly",
        "monthly",
        "yearly",
        "never"
    };


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
