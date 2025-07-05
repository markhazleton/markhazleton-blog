using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

public class SeoDashboardModel : BasePageModel
{
    private readonly ArticleService _articleService;
    private readonly SeoValidationService _seoValidationService;

    public SeoDashboardModel(ArticleService articleService, ProjectService projectService, SeoValidationService seoValidationService, ILogger<SeoDashboardModel> logger)
        : base(articleService, projectService, logger)
    {
        _articleService = articleService;
        _seoValidationService = seoValidationService;
    }

    public Dictionary<string, object> SeoStats { get; private set; } = new();
    public Dictionary<int, SeoScore> ArticleScores { get; private set; } = new();
    public Dictionary<int, SeoValidationResult> ArticleValidations { get; private set; } = new();
    public List<ArticleModel> ArticlesNeedingAttention { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? FilterGrade { get; set; }

    public List<ArticleModel> FilteredArticles { get; private set; } = new();
    public bool IsFiltered => !string.IsNullOrEmpty(FilterGrade);

    public void OnGet()
    {
        // Get SEO statistics
        SeoStats = _articleService.GetSeoStatistics();

        // Get all articles and validate them
        var articles = _articleService.GetArticles();

        foreach (var article in articles)
        {
            var validation = _seoValidationService.ValidateArticle(article);
            ArticleScores[article.Id] = validation.Score;
            ArticleValidations[article.Id] = validation;
        }

        // Get articles that need attention (score < 80 or have errors)
        ArticlesNeedingAttention = articles
            .Where(a => ArticleScores[a.Id].OverallScore < 80 || ArticleValidations[a.Id].Errors.Any())
            .OrderBy(a => ArticleScores[a.Id].OverallScore)
            .ToList();

        // Filter articles by grade if specified
        if (!string.IsNullOrEmpty(FilterGrade))
        {
            FilteredArticles = articles
                .Where(a => ArticleScores[a.Id].Grade == FilterGrade)
                .OrderByDescending(a => ArticleScores[a.Id].OverallScore)
                .ToList();
        }
    }
}
