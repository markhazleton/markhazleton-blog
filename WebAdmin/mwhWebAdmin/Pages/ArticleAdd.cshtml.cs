using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

public class ArticleAddModel : BasePageModel
{
    private readonly ArticleService _articleService;
    private readonly SeoValidationService _seoValidationService;

    [BindProperty]
    public ArticleModel NewArticle { get; set; }

    public List<string> AvailableImages { get; set; } = [];

    public ArticleAddModel(ArticleService articleService, ProjectService projectService, SeoValidationService seoValidationService, ILogger<ArticleAddModel> logger)
        : base(articleService, projectService, logger)
    {
        _articleService = articleService;
        _seoValidationService = seoValidationService;
        NewArticle = new ArticleModel();

        // Initialize SEO objects
        _articleService.InitializeSeoFields(NewArticle);
    }

    public void OnGet()
    {
        AvailableImages = _articleService.GetAvailableImages();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            // Repopulate available images for form display
            AvailableImages = _articleService.GetAvailableImages();
            return Page();
        }

        // Only perform basic auto-generation without AI calls
        _articleService.AutoGenerateSeoFields(NewArticle);

        _articleService.AddArticle(NewArticle);
        return RedirectToPage("Articles");
    }

    public async Task<IActionResult> OnPostValidateKeywordsAsync()
    {
        _baseLogger.LogInformation("OnPostValidateKeywordsAsync called for new article");

        if (!ModelState.IsValid)
        {
            _baseLogger.LogWarning("ModelState is invalid for new article validation");
            AvailableImages = _articleService.GetAvailableImages();
            return Page();
        }

        try
        {
            // Generate keywords using AI if content is available
            if (!string.IsNullOrEmpty(NewArticle.ArticleContent))
            {
                _baseLogger.LogInformation("Starting AI SEO field generation for new article");
                await _articleService.AutoGenerateSeoFieldsAsync(NewArticle);
                _baseLogger.LogInformation("AI SEO field generation completed for new article");
                TempData["SuccessMessage"] = "Keywords have been validated and updated using AI.";
            }
            else
            {
                _baseLogger.LogWarning("No content available for AI validation for new article");
                TempData["WarningMessage"] = "No content available for AI keyword validation. Please add article content first.";
            }
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Error validating keywords for new article");
            TempData["ErrorMessage"] = "Failed to validate keywords. Please try again.";
        }

        // Refresh the page data
        AvailableImages = _articleService.GetAvailableImages();
        _baseLogger.LogInformation("OnPostValidateKeywordsAsync completed for new article");
        return Page();
    }
}
