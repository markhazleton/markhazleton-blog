using mwhWebAdmin.Article;

namespace mwhWebAdmin.Services;

/// <summary>
/// Service for validating and scoring SEO metadata
/// </summary>
public class SeoValidationService
{
    /// <summary>
    /// Validates the SEO metadata for an article
    /// </summary>
    /// <param name="article">The article to validate</param>
    /// <returns>SEO validation result with score and recommendations</returns>
    public SeoValidationResult ValidateArticle(ArticleModel article)
    {
        var result = new SeoValidationResult();

        // Validate title
        var titleResult = ValidateTitle(article.EffectiveTitle);
        result.Score.TitleScore = titleResult.score;
        result.Warnings.AddRange(titleResult.warnings);
        result.Errors.AddRange(titleResult.errors);

        // Validate description
        var descResult = ValidateDescription(article.EffectiveDescription);
        result.Score.DescriptionScore = descResult.score;
        result.Warnings.AddRange(descResult.warnings);
        result.Errors.AddRange(descResult.errors);

        // Validate keywords
        var keywordResult = ValidateKeywords(article.EffectiveKeywords);
        result.Score.KeywordsScore = keywordResult.score;
        result.Warnings.AddRange(keywordResult.warnings);
        result.Errors.AddRange(keywordResult.errors);

        // Validate images
        var imageResult = ValidateImages(article);
        result.Score.ImageScore = imageResult.score;
        result.Warnings.AddRange(imageResult.warnings);
        result.Errors.AddRange(imageResult.errors);

        // Calculate overall score
        result.Score.OverallScore = (result.Score.TitleScore + result.Score.DescriptionScore +
                                   result.Score.KeywordsScore + result.Score.ImageScore) / 4;

        result.IsValid = result.Errors.Count == 0;

        return result;
    }

    private (int score, List<string> warnings, List<string> errors) ValidateTitle(string title)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 0;

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required");
            return (0, warnings, errors);
        }

        // Length validation
        if (title.Length < 30)
        {
            warnings.Add($"Title is too short ({title.Length} chars). Recommended: 30-60 characters");
            score = 60;
        }
        else if (title.Length > 60)
        {
            warnings.Add($"Title is too long ({title.Length} chars). Recommended: 30-60 characters");
            score = 70;
        }
        else
        {
            score = 100;
        }

        // Check for brand suffix
        if (!title.Contains("Mark Hazleton"))
        {
            warnings.Add("Consider adding '| Mark Hazleton' suffix for brand consistency");
            score = Math.Max(score - 10, 0);
        }

        return (score, warnings, errors);
    }

    private (int score, List<string> warnings, List<string> errors) ValidateDescription(string description)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 0;

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add("Description is required");
            return (0, warnings, errors);
        }

        // Length validation
        if (description.Length < 120)
        {
            warnings.Add($"Description is too short ({description.Length} chars). Recommended: 120-160 characters");
            score = 60;
        }
        else if (description.Length > 160)
        {
            warnings.Add($"Description is too long ({description.Length} chars). Recommended: 120-160 characters");
            score = 70;
        }
        else
        {
            score = 100;
        }

        // Check for call-to-action words
        var ctaWords = new[] { "discover", "learn", "explore", "understand", "master", "guide" };
        if (!ctaWords.Any(word => description.ToLower().Contains(word)))
        {
            warnings.Add("Consider adding action words like 'discover', 'learn', or 'explore' to encourage clicks");
            score = Math.Max(score - 5, 0);
        }

        return (score, warnings, errors);
    }

    private (int score, List<string> warnings, List<string> errors) ValidateKeywords(string keywords)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 0;

        if (string.IsNullOrWhiteSpace(keywords))
        {
            warnings.Add("Keywords are recommended for better SEO");
            return (80, warnings, errors);
        }

        var keywordList = keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();

        if (keywordList.Count < 3)
        {
            warnings.Add($"Consider adding more keywords. Current: {keywordList.Count}, Recommended: 3-8");
            score = 70;
        }
        else if (keywordList.Count > 8)
        {
            warnings.Add($"Too many keywords may dilute SEO value. Current: {keywordList.Count}, Recommended: 3-8");
            score = 80;
        }
        else
        {
            score = 100;
        }

        // Check for Mark Hazleton in keywords
        if (!keywordList.Any(k => k.ToLower().Contains("mark hazleton")))
        {
            warnings.Add("Consider including 'Mark Hazleton' in keywords for brand visibility");
            score = Math.Max(score - 5, 0);
        }

        return (score, warnings, errors);
    }

    private (int score, List<string> warnings, List<string> errors) ValidateImages(ArticleModel article)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 0;

        if (string.IsNullOrWhiteSpace(article.ImgSrc))
        {
            warnings.Add("Featured image is recommended for social media sharing");
            return (70, warnings, errors);
        }

        score = 90;

        // Check Open Graph image
        if (article.OpenGraph?.Image == null)
        {
            warnings.Add("Open Graph image not specified - will use featured image");
            score = Math.Max(score - 5, 0);
        }

        // Check Twitter image
        if (article.TwitterCard?.Image == null)
        {
            warnings.Add("Twitter Card image not specified - will use featured image");
            score = Math.Max(score - 5, 0);
        }

        // Check alt text
        if (string.IsNullOrWhiteSpace(article.OpenGraph?.ImageAlt) &&
            string.IsNullOrWhiteSpace(article.TwitterCard?.ImageAlt))
        {
            warnings.Add("Image alt text is missing - important for accessibility");
            score = Math.Max(score - 10, 0);
        }

        return (score, warnings, errors);
    }

    /// <summary>
    /// Generates SEO recommendations for improving an article
    /// </summary>
    /// <param name="article">The article to analyze</param>
    /// <returns>List of actionable recommendations</returns>
    public List<string> GetRecommendations(ArticleModel article)
    {
        var recommendations = new List<string>();
        var validation = ValidateArticle(article);

        if (validation.Score.OverallScore < 80)
        {
            recommendations.Add("Overall SEO score could be improved. Focus on the areas with warnings below.");
        }

        if (string.IsNullOrEmpty(article.Seo?.Title))
        {
            recommendations.Add("Add an SEO-optimized title in the 'SEO' section for better search engine visibility.");
        }

        if (string.IsNullOrEmpty(article.Seo?.Description))
        {
            recommendations.Add("Add an SEO-optimized description (120-160 chars) for better search results.");
        }

        if (article.OpenGraph == null)
        {
            recommendations.Add("Add Open Graph metadata for better social media sharing on Facebook and LinkedIn.");
        }

        if (article.TwitterCard == null)
        {
            recommendations.Add("Add Twitter Card metadata for better appearance when shared on Twitter/X.");
        }

        if (string.IsNullOrEmpty(article.Seo?.Canonical))
        {
            recommendations.Add("Set a canonical URL to prevent duplicate content issues.");
        }

        return recommendations;
    }
}
