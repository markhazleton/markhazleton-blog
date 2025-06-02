using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages;

public class ArticlesModel : BasePageModel
{
    private readonly ArticleService _articleService;

    public ArticlesModel(ArticleService articleService, ProjectService projectService, ILogger<ArticlesModel> logger)
        : base(articleService, projectService, logger)
    {
        _articleService = articleService;
    }

    public List<ArticleModel> Articles { get; private set; } = [];

    public void OnGet()
    {
        Articles = _articleService.GetArticles();
    }

    public void OnPostUpdateSourcePaths()
    {
        _articleService.UpdateMissingSourcePaths();
        Articles = _articleService.GetArticles();
    }

    public void OnPostDebugSourcePath()
    {
        // Debug the first article (Sidetracked by Sizzle)
        Articles = _articleService.GetArticles();
        if (Articles.Count > 0)
        {
            var firstArticle = Articles[0];
            Console.WriteLine($"Debug Article: {firstArticle.Name}");
            Console.WriteLine($"Debug Slug: {firstArticle.Slug}");
            Console.WriteLine($"Debug Source: {firstArticle.Source}");
            Console.WriteLine($"Debug SourceFileExists: {firstArticle.SourceFileExists}");
        }
    }

    public void OnPostTestDirectorySlugProcessing()
    {
        // Test directory-based URLs ending with /
        string[] testSlugs = {
            "projectmechanics/leadership/",
            "projectmechanics/change-management/",
            "projectmechanics/conflict-management/",
            "projectmechanics/",
            "projectmechanics/leadership",  // without trailing slash
            "nonexistent/directory/",
            "projectmechanics/leadership.html"
        };

        Console.WriteLine("=== Testing Directory-Based Slug Processing ===");
        foreach (var slug in testSlugs)
        {
            var result = _articleService.TestSlugToSourcePath(slug);
            Console.WriteLine($"Slug: '{slug}' -> Source: '{result}'");
        }
        Console.WriteLine("=== End Test ===");

        Articles = _articleService.GetArticles();
    }
}
