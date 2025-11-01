using System.Text.Json.Serialization;

namespace mwhWebAdmin.Article.Models;

/// <summary>
/// Represents the result of SEO metadata extraction from article content
/// </summary>
public class SeoMetadataResult
{
    /// <summary>
    /// Gets or sets the SEO-optimized article title (30-60 characters)
    /// </summary>
    [JsonPropertyName("articleTitle")]
    public string ArticleTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the article subtitle
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the article description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the comma-separated keywords (5-8 keywords)
    /// </summary>
    [JsonPropertyName("keywords")]
    public string Keywords { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SEO title for meta tags (30-60 characters)
    /// </summary>
    [JsonPropertyName("seoTitle")]
  public string SeoTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the meta description (120-160 characters with action words)
    /// </summary>
    [JsonPropertyName("metaDescription")]
    public string MetaDescription { get; set; } = string.Empty;
}
