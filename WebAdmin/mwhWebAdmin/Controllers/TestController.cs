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
                        // Main article fields
                        ArticleTitle = testArticle.Name,
                        ArticleDescription = testArticle.Description,
                        ArticleContent = testArticle.ArticleContent,
                        // SEO fields
                        Keywords = testArticle.Keywords,
                        SeoTitle = testArticle.Seo?.Title,
                        SeoDescription = testArticle.Seo?.Description,
                        OpenGraphTitle = testArticle.OpenGraph?.Title,
                        OpenGraphDescription = testArticle.OpenGraph?.Description,
                        TwitterTitle = testArticle.TwitterCard?.Title,
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

        [HttpPost("test-seo-validation")]
        public IActionResult TestSeoValidation([FromBody] TestSeoValidationRequest request)
        {
            try
            {
                // Create a test article with potentially problematic SEO data
                var testArticle = new ArticleModel
                {
                    Name = request.Title,
                    Keywords = request.Keywords,
                    Seo = new SeoModel
                    {
                        Title = request.SeoTitle,
                        Description = request.MetaDescription
                    },
                    OpenGraph = new OpenGraphModel
                    {
                        Title = request.OpenGraphTitle,
                        Description = request.OpenGraphDescription
                    },
                    TwitterCard = new TwitterCardModel
                    {
                        Title = request.TwitterTitle,
                        Description = request.TwitterDescription
                    }
                };

                // Test validation logic
                var validationResults = new
                {
                    OriginalData = new
                    {
                        SeoTitle = testArticle.Seo?.Title,
                        SeoTitleLength = testArticle.Seo?.Title?.Length ?? 0,
                        MetaDescription = testArticle.Seo?.Description,
                        MetaDescriptionLength = testArticle.Seo?.Description?.Length ?? 0,
                        Keywords = testArticle.Keywords
                    },
                    ValidationChecks = new
                    {
                        SeoTitleLengthValid = (testArticle.Seo?.Title?.Length ?? 0) >= 30 && (testArticle.Seo?.Title?.Length ?? 0) <= 60,
                        SeoTitleHasBrandSuffix = true, // No longer checking for brand suffix
                        MetaDescriptionLengthValid = (testArticle.Seo?.Description?.Length ?? 0) >= 150 && (testArticle.Seo?.Description?.Length ?? 0) <= 160,
                        KeywordsIncludeBrand = true // No longer checking for brand in keywords
                    }
                };

                return Ok(new { Success = true, ValidationResults = validationResults });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("test-ai-seo-generation")]
        public IActionResult TestAiSeoGeneration([FromBody] TestSeoRequest request)
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

                // Simulate AI SEO generation
                testArticle.Keywords = "AI, SEO, Test";
                testArticle.Seo = new SeoModel
                {
                    Title = "Test Article - AI Generated",
                    Description = "This is a test article with AI-generated SEO fields."
                };
                testArticle.OpenGraph = new OpenGraphModel
                {
                    Title = "Test Article - Open Graph",
                    Description = "Open Graph description for the test article."
                };
                testArticle.TwitterCard = new TwitterCardModel
                {
                    Title = "Test Article - Twitter Card",
                    Description = "Twitter Card description for the test article."
                };

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
                        TwitterTitle = testArticle.TwitterCard?.Title,
                        TwitterDescription = testArticle.TwitterCard?.Description,
                        CanonicalUrl = testArticle.Seo?.Canonical
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

    public class TestSeoValidationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string SeoTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string OpenGraphTitle { get; set; } = string.Empty;
        public string OpenGraphDescription { get; set; } = string.Empty;
        public string TwitterTitle { get; set; } = string.Empty;
        public string TwitterDescription { get; set; } = string.Empty;
    }
}
