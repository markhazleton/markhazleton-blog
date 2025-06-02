using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Pages
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(ArticleService articleService, ProjectService projectService, ILogger<IndexModel> logger)
            : base(articleService, projectService, logger)
        {
        }
        public void OnGet()
        {
            try
            {
                // Load dashboard statistics
                var articles = _baseArticleService.GetArticles();
                var projects = _baseProjectService.GetProjects();

                ViewData["ArticleCount"] = articles?.Count ?? 0;
                ViewData["ProjectCount"] = projects?.Count ?? 0;
                // Count available images from a relative path
                var imageDirectory = Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"));
                var imageCount = 0;

                if (Directory.Exists(imageDirectory))
                {
                    var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
                    imageCount = Directory.GetFiles(imageDirectory)
                        .Count(file => imageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
                }

                ViewData["ImageCount"] = imageCount;

                _baseLogger.LogInformation("Dashboard loaded with {ArticleCount} articles, {ProjectCount} projects, {ImageCount} images",
                    ViewData["ArticleCount"], ViewData["ProjectCount"], ViewData["ImageCount"]);
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "Error loading dashboard data");
                ViewData["ArticleCount"] = 0;
                ViewData["ProjectCount"] = 0;
                ViewData["ImageCount"] = 0;
            }
        }
    }
}
