using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;

namespace mwhWebAdmin.Pages;

/// <summary>
/// Represents the model for editing an article.
/// </summary>
public class ArticleEditModel(ArticleService articleService) : PageModel
{
    [BindProperty]
    /// <summary>
    /// Gets or sets the required article.
    /// </summary>
    public required ArticleModel Article { get; set; }

    /// <summary>
    /// Handles the GET request for editing an article.
    /// </summary>
    /// <param name="Id">The ID of the article to edit.</param>
    /// <returns>The IActionResult representing the response.</returns>
    public IActionResult OnGet(string Id)
    {
        int id = Convert.ToInt32(Id);
        Article = articleService.GetArticleById(id);
        if (Article == null)
        {
            return NotFound();
        }
        return Page();
    }

    /// <summary>
    /// Handles the POST request for editing an article.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        articleService.UpdateArticle(Article);
        return RedirectToPage("Articles");
    }
}
