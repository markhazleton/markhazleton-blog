using System.Text.Json.Serialization;
using mwhWebAdmin.Configuration;

namespace mwhWebAdmin.Article;

/// <summary>
/// Represents SEO-specific metadata for an article
/// </summary>
public class SeoModel
{
    /// <summary>
    /// Gets or sets the SEO-optimized title (30-60 characters recommended)
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the title suffix (e.g., " | Blog")
    /// </summary>
    [JsonPropertyName("titleSuffix")]
    public string? TitleSuffix { get; set; } = "";

    /// <summary>
    /// Gets or sets the SEO-optimized description (120-160 characters recommended)
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the targeted SEO keywords
    /// </summary>
    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets the canonical URL for the article
    /// </summary>
    [JsonPropertyName("canonical")]
    public string? Canonical { get; set; }

    /// <summary>
    /// Gets or sets the robots meta tag content
    /// </summary>
    [JsonPropertyName("robots")]
    public string? Robots { get; set; } = "index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1";
}

/// <summary>
/// Represents Open Graph metadata for an article
/// </summary>
public class OpenGraphModel
{
    /// <summary>
    /// Gets or sets the Open Graph title (30-65 characters recommended)
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the Open Graph description (120-160 characters recommended)
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Open Graph type (usually "article")
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "article";

    /// <summary>
    /// Gets or sets the Open Graph image URL
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the Open Graph image alt text
    /// </summary>
    [JsonPropertyName("imageAlt")]
    public string? ImageAlt { get; set; }
}

/// <summary>
/// Represents Twitter Card metadata for an article
/// </summary>
public class TwitterCardModel
{
    /// <summary>
    /// Gets or sets the Twitter card title (max 50 characters recommended)
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the Twitter card description (120-160 characters recommended)
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Twitter card image URL
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the Twitter card image alt text
    /// </summary>
    [JsonPropertyName("imageAlt")]
    public string? ImageAlt { get; set; }
}

/// <summary>
/// SEO validation result for an article
/// </summary>
public class SeoValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public SeoScore Score { get; set; } = new();
}

/// <summary>
/// SEO scoring metrics
/// </summary>
public class SeoScore
{
    public int TitleScore { get; set; }
    public int DescriptionScore { get; set; }
    public int KeywordsScore { get; set; }
    public int ImageScore { get; set; }
    public int H1Score { get; set; }
    public int ContentImageScore { get; set; }
    public int HtmlSeoScore { get; set; }
    public int OverallScore { get; set; }

    /// <summary>
    /// Gets the grade based on warnings and overall score
    /// A: No warnings about too long or too short attributes (90+ score)
    /// B: Only warnings related to Twitter or Open Graph (80+ score)
    /// C: Warnings related to title and meta descriptions (70+ score)
    /// D: Other warnings (60+ score)
    /// F: Major issues (under 60 score)
    /// </summary>
    public string GetGrade(List<string> warnings)
    {
        return SeoValidationConfig.Grading.GetGrade(OverallScore, warnings);
    }

    /// <summary>
    /// Legacy grade property for backward compatibility
    /// Uses standard grade ranges: A(90+), B(80+), C(70+), D(60+), F(<60)
    /// </summary>
    public string Grade => OverallScore switch
    {
        >= SeoValidationConfig.Grading.GradeAThreshold => "A",
        >= SeoValidationConfig.Grading.GradeBThreshold => "B",
        >= SeoValidationConfig.Grading.GradeCThreshold => "C",
        >= SeoValidationConfig.Grading.GradeDThreshold => "D",
        _ => "F"
    };
}

/// <summary>
/// Represents the result of AI-powered SEO data generation
/// </summary>
public class SeoGenerationResult
{
    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }

    [JsonPropertyName("seoTitle")]
    public string? SeoTitle { get; set; }

    [JsonPropertyName("metaDescription")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("ogTitle")]
    public string? OgTitle { get; set; }

    [JsonPropertyName("ogDescription")]
    public string? OgDescription { get; set; }

    [JsonPropertyName("twitterDescription")]
    public string? TwitterDescription { get; set; }

    [JsonPropertyName("twitterTitle")]
    public string? TwitterTitle { get; set; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("conclusionTitle")]
    public string? ConclusionTitle { get; set; }

    [JsonPropertyName("conclusionSummary")]
    public string? ConclusionSummary { get; set; }

    [JsonPropertyName("conclusionKeyHeading")]
    public string? ConclusionKeyHeading { get; set; }

    [JsonPropertyName("conclusionKeyText")]
    public string? ConclusionKeyText { get; set; }

    [JsonPropertyName("conclusionText")]
    public string? ConclusionText { get; set; }

    // New properties for article content generation
    [JsonPropertyName("articleTitle")]
    public string? ArticleTitle { get; set; }

    [JsonPropertyName("articleDescription")]
    public string? ArticleDescription { get; set; }

    [JsonPropertyName("articleContent")]
    public string? ArticleContent { get; set; }
}
