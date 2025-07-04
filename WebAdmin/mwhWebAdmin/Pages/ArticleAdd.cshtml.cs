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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Repopulate available images for form display
            AvailableImages = _articleService.GetAvailableImages();
            return Page();
        }

        // Auto-generate SEO fields using AI if content is available
        if (!string.IsNullOrEmpty(NewArticle.ArticleContent))
        {
            await _articleService.AutoGenerateSeoFieldsAsync(NewArticle);
        }
        else
        {
            // Basic auto-generation without AI
            _articleService.AutoGenerateSeoFields(NewArticle);
        }

        _articleService.AddArticle(NewArticle);
        return RedirectToPage("Articles");
    }
}
