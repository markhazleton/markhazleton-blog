using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

/// <summary>
/// Represents the model for editing an article.
/// </summary>
public class ArticleEditModel(ArticleService articleService, ProjectService projectService, IWebHostEnvironment webHostEnvironment, SeoValidationService seoValidationService, IConfiguration configuration, ILogger<ArticleEditModel> logger)
    : BasePageModel(articleService, projectService, logger)
{
    private readonly ArticleService _articleService = articleService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly SeoValidationService _seoValidationService = seoValidationService;
    private readonly IConfiguration _configuration = configuration;

    [BindProperty]
    /// <summary>
    /// Gets or sets the required article.
    /// </summary>
    public required ArticleModel Article { get; set; }

    public List<string> ExistingKeywords { get; set; } = [];
    public List<string> AvailableImages { get; set; } = [];
    public SeoValidationResult? SeoValidation { get; set; }
    public List<string> SeoRecommendations { get; set; } = [];

    /// <summary>
    /// Handles the GET request for editing an article.
    /// </summary>
    /// <param name="Id">The ID of the article to edit.</param>
    /// <returns>The IActionResult representing the response.</returns>
    public IActionResult OnGet(string Id)
    {
        logger.LogInformation("OnGet called for ArticleEdit with Id: {Id}", Id);
        int id = Convert.ToInt32(Id);
        Article = _articleService.GetArticleById(id);
        if (Article == null)
        {
            logger.LogInformation("Not Found ArticleEdit with Id: {Id}", Id);
            return NotFound();
        }

        // Initialize SEO fields if they don't exist
        _articleService.InitializeSeoFields(Article);

        ExistingKeywords = _articleService.GetKeywords();
        AvailableImages = _articleService.GetAvailableImages();

        // Validate SEO
        SeoValidation = _seoValidationService.ValidateArticle(Article);
        SeoRecommendations = _seoValidationService.GetRecommendations(Article);
        logger.LogInformation("PageFound return ArticleEdit with Id: {Id}", Id);

        return Page();
    }


    /// <summary>
    /// Handles the POST request for editing an article.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    public async Task<IActionResult> OnPost(CancellationToken ct = default)
    {
        logger.LogInformation("OnPost called for ArticleEdit with Id: {Id}", Article?.Id);

        if (!ModelState.IsValid)
        {
            logger.LogInformation("ModelState has validation errors:");
            foreach (var modelError in ModelState)
            {
                var key = modelError.Key;
                var errors = modelError.Value?.Errors ?? [];
                foreach (var error in errors)
                {
                    logger.LogInformation("ModelState Error - Key: {Key}, Error: {Error}", key, error.ErrorMessage);
                }
            }

            ExistingKeywords = _articleService.GetKeywords();
            AvailableImages = _articleService.GetAvailableImages();

            // Re-validate SEO for display
            if (Article != null)
            {
                SeoValidation = _seoValidationService.ValidateArticle(Article);
                SeoRecommendations = _seoValidationService.GetRecommendations(Article);
            }
            logger.LogInformation("ModelState is invalid for ArticleEdit with Id: {Id}", Article?.Id);
            return Page();
        }

        // Only perform basic auto-generation without AI calls
        if (Article != null)
        {
            _articleService.AutoGenerateSeoFields(Article);
            await _articleService.UpdateArticle(Article).ConfigureAwait(true);
            logger.LogInformation("Article with Id: {Id} updated successfully", Article?.Id);
        }
        return RedirectToPage("Articles");
    }

    /// <summary>
    /// Handles the POST request for generating AI keywords and SEO data.
    /// </summary>
    /// <returns>The IActionResult representing the response.</returns>
    public async Task<IActionResult> OnPostValidate(CancellationToken ct = default)
    {
        _baseLogger.LogInformation("*** OnPostValidate METHOD CALLED *** - AI Keywords validation requested for article {ArticleId}", Article?.Id);
        Console.WriteLine("*** OnPostValidate METHOD CALLED *** - This should appear in terminal");

        // Debug ModelState issues (for informational purposes)
        if (!ModelState.IsValid)
        {
            _baseLogger.LogInformation("ModelState has validation errors - these should be minimal now that SEO validation attributes are removed:");
            foreach (var modelError in ModelState)
            {
                var key = modelError.Key;
                var errors = modelError.Value?.Errors ?? [];
                foreach (var error in errors)
                {
                    _baseLogger.LogInformation("ModelState Info - Key: {Key}, Error: {Error}", key, error.ErrorMessage);
                }
            }
        }

        if (Article == null)
        {
            _baseLogger.LogWarning("Article is null - returning to page");
            return RefreshPageData();
        }

        if (!ModelState.IsValid)
        {
            _baseLogger.LogWarning("ModelState validation errors found - returning to page");
            return RefreshPageData();
        }

        try
        {
            // Ensure article has content for AI processing
            if (string.IsNullOrEmpty(Article.ArticleContent))
            {
                _baseLogger.LogWarning("No content available for AI validation for article {ArticleId}", Article.Id);
                TempData["WarningMessage"] = "Please add article content before generating AI keywords.";
                return RefreshPageData();
            }

            // Check if OpenAI API key is configured
            var openAiApiKey = _configuration["OPENAI_API_KEY"];
            if (string.IsNullOrEmpty(openAiApiKey))
            {
                _baseLogger.LogError("OpenAI API key is not configured");
                TempData["ErrorMessage"] = "AI service is not configured. Please contact the administrator.";
                return RefreshPageData();
            }

            _baseLogger.LogInformation("Starting AI SEO field generation for article {ArticleId}", Article.Id);

            // Generate AI keywords and SEO data
            _baseLogger.LogInformation("[AI-VALIDATION] About to call AutoGenerateSeoFieldsAsync for article: {ArticleName}", Article.Name);
            _baseLogger.LogInformation("[AI-VALIDATION] Article content length: {ContentLength} characters", Article.ArticleContent?.Length ?? 0);

            await _articleService.AutoGenerateSeoFieldsAsync(Article);

            _baseLogger.LogInformation("[AI-VALIDATION] AutoGenerateSeoFieldsAsync completed successfully for article: {ArticleName}", Article.Name);
            _baseLogger.LogInformation("[AI-VALIDATION] Updated keywords: {Keywords}", Article.Keywords);
            _baseLogger.LogInformation("[AI-VALIDATION] Updated SEO title: {SeoTitle}", Article.Seo?.Title);
            _baseLogger.LogInformation("[AI-VALIDATION] Updated meta description: {MetaDescription}", Article.Seo?.Description);

            _baseLogger.LogInformation("AI SEO field generation completed for article {ArticleId}", Article.Id);
            TempData["SuccessMessage"] = "AI content generated successfully! Updated fields include: Keywords, SEO Title, Meta Description, Open Graph Title/Description, Twitter Title/Description, Article Summary, and Conclusion sections. Check all the form fields for updates.";
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Error generating AI keywords for article {ArticleId}", Article.Id);
            TempData["ErrorMessage"] = "Failed to generate AI content. Please try again. Error: " + ex.Message;
        }

        return RefreshPageData();
    }

    /// <summary>
    /// Refreshes the page data after operations.
    /// </summary>
    private IActionResult RefreshPageData()
    {
        ExistingKeywords = _articleService.GetKeywords();
        AvailableImages = _articleService.GetAvailableImages();
        if (Article != null)
        {
            SeoValidation = _seoValidationService.ValidateArticle(Article);
            SeoRecommendations = _seoValidationService.GetRecommendations(Article);
        }
        return Page();
    }
}
