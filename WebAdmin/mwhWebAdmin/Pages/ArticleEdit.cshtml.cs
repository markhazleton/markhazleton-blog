using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

/// <summary>
/// Represents the model for editing an article.
/// </summary>
public class ArticleEditModel(ArticleService articleService, ProjectService projectService, IWebHostEnvironment webHostEnvironment, SeoValidationService seoValidationService, ILogger<ArticleEditModel> logger)
    : BasePageModel(articleService, projectService, logger)
{
    private readonly ArticleService _articleService = articleService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly SeoValidationService _seoValidationService = seoValidationService;

    [BindProperty]
    /// <summary>
    /// Gets or sets the required article.
    /// </summary>
    public required ArticleModel Article { get; set; }

    public List<string> ExistingKeywords { get; set; } = [];
    public List<string> AvailableImages { get; set; } = [];
    public SeoValidationResult? SeoValidation { get; set; }
    public List<string> SeoRecommendations { get; set; } = [];    /// <summary>
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

        // Initialize SEO fields if they don't exist
        _articleService.InitializeSeoFields(Article);

        ExistingKeywords = _articleService.GetKeywords();
        AvailableImages = _articleService.GetAvailableImages();

        // Validate SEO
        SeoValidation = _seoValidationService.ValidateArticle(Article);
        SeoRecommendations = _seoValidationService.GetRecommendations(Article);

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

            // Re-validate SEO for display
            SeoValidation = _seoValidationService.ValidateArticle(Article);
            SeoRecommendations = _seoValidationService.GetRecommendations(Article);

            return Page();
        }

        // Auto-generate SEO fields using AI if content is available
        if (!string.IsNullOrEmpty(Article.ArticleContent))
        {
            await _articleService.AutoGenerateSeoFieldsAsync(Article);
        }
        else
        {
            // Basic auto-generation without AI
            _articleService.AutoGenerateSeoFields(Article);
        }

        await _articleService.UpdateArticle(Article).ConfigureAwait(true);
        await _articleService.UpdateKeywordsForAllArticlesAsync(ct).ConfigureAwait(true);

        return RedirectToPage("Articles");
    }
}
