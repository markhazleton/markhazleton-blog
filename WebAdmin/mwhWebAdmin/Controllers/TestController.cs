using Microsoft.AspNetCore.Mvc;
using mwhWebAdmin.Article;

namespace mwhWebAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ArticleService _articleService;

        public TestController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpPost("test-seo-generation")]
        public async Task<IActionResult> TestSeoGeneration([FromBody] TestSeoRequest request)
        {
            try
            {
                // Create a test article
                var testArticle = new ArticleModel
                {
                    Name = request.Title,
                    ArticleContent = request.Content,
                    Slug = "test-article"
                };

                // Generate SEO fields using AI
                await _articleService.AutoGenerateSeoFieldsAsync(testArticle);

                var response = new
                {
                    Success = true,
                    GeneratedSeo = new
                    {
                        Keywords = testArticle.Keywords,
                        SeoTitle = testArticle.Seo?.Title,
                        SeoDescription = testArticle.Seo?.Description,
                        OpenGraphTitle = testArticle.OpenGraph?.Title,
                        OpenGraphDescription = testArticle.OpenGraph?.Description,
                        TwitterDescription = testArticle.TwitterCard?.Description,
                        CanonicalUrl = testArticle.Seo?.Canonical,
                        // Article content fields
                        Subtitle = testArticle.Subtitle,
                        Summary = testArticle.Summary,
                        // Conclusion section fields
                        ConclusionTitle = testArticle.ConclusionTitle,
                        ConclusionSummary = testArticle.ConclusionSummary,
                        ConclusionKeyHeading = testArticle.ConclusionKeyHeading,
                        ConclusionKeyText = testArticle.ConclusionKeyText,
                        ConclusionText = testArticle.ConclusionText
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }
    }

    public class TestSeoRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
