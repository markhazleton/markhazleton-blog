using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

public class TestAIModel : BasePageModel
{
    private readonly ArticleService _articleService;
    private readonly IConfiguration _configuration;

    public TestAIModel(ArticleService articleService, ProjectService projectService, IConfiguration configuration, ILogger<TestAIModel> logger)
        : base(articleService, projectService, logger)
    {
        _articleService = articleService;
        _configuration = configuration;
    }

    public bool HasApiKey { get; set; }
    public SeoGenerationResult? TestResult { get; set; }

    public void OnGet()
    {
        var openAiApiKey = _configuration["OPENAI_API_KEY"];
        HasApiKey = !string.IsNullOrEmpty(openAiApiKey);
    }

    public async Task<IActionResult> OnPost(string testContent)
    {
        var openAiApiKey = _configuration["OPENAI_API_KEY"];
        HasApiKey = !string.IsNullOrEmpty(openAiApiKey);

        if (!HasApiKey)
        {
            TempData["ErrorMessage"] = "OpenAI API key is not configured. Please set the OPENAI_API_KEY environment variable or configuration setting.";
            return Page();
        }

        if (string.IsNullOrEmpty(testContent))
        {
            TempData["ErrorMessage"] = "Please enter some test content.";
            return Page();
        }

        try
        {
            // Create a test article
            var testArticle = new ArticleModel
            {
                Name = "Test Article",
                ArticleContent = testContent,
                Slug = "test-article"
            };

            // Initialize SEO fields
            _articleService.InitializeSeoFields(testArticle);

            // Generate AI SEO fields
            await _articleService.AutoGenerateSeoFieldsAsync(testArticle);

            // Extract the results
            TestResult = new SeoGenerationResult
            {
                Keywords = testArticle.Keywords,
                SeoTitle = testArticle.Seo?.Title,
                MetaDescription = testArticle.Seo?.Description,
                OgTitle = testArticle.OpenGraph?.Title,
                TwitterDescription = testArticle.TwitterCard?.Description
            };

            TempData["SuccessMessage"] = "AI generation completed successfully!";
        }
        catch (Exception ex)
        {
            _baseLogger.LogError(ex, "Error testing AI generation");
            TempData["ErrorMessage"] = $"Error testing AI generation: {ex.Message}";
        }

        return Page();
    }
}
