using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

/// <summary>
/// Represents the model for editing an article.
/// </summary>
public class ArticleEditModel(ArticleService articleService, ProjectService projectService, IWebHostEnvironment webHostEnvironment, ILogger<ArticleEditModel> logger)
    : BasePageModel(articleService, projectService, logger)
{
    private readonly ArticleService _articleService = articleService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    [BindProperty]
    /// <summary>
    /// Gets or sets the required article.
    /// </summary>
    public required ArticleModel Article { get; set; }

    public List<string> ExistingKeywords { get; set; } = [];
    public List<string> AvailableImages { get; set; } = [];    /// <summary>
                                                               /// Handles the GET request for editing an article.
                                                               /// </summary>
                                                               /// <param name="Id">The ID of the article to edit.</param>
                                                               /// <returns>The IActionResult representing the response.</returns>
    public IActionResult OnGet(string Id)
    {
        int id = Convert.ToInt32(Id);
        Article = _articleService.GetArticleById(id);
        if (Article == null)
        {
            return NotFound();
        }
        ExistingKeywords = _articleService.GetKeywords();
        AvailableImages = _articleService.GetAvailableImages();
        return Page();
    }    /// <summary>
         /// Handles the POST request for editing an article.
         /// </summary>
         /// <returns>The IActionResult representing the response.</returns>
    public async Task<IActionResult> OnPost(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ExistingKeywords = _articleService.GetKeywords();
            AvailableImages = _articleService.GetAvailableImages();
            return Page();
        }
        await _articleService.UpdateArticle(Article).ConfigureAwait(true);

        await _articleService.UpdateKeywordsForAllArticlesAsync(ct).ConfigureAwait(true); return RedirectToPage("Articles");
    }
}
