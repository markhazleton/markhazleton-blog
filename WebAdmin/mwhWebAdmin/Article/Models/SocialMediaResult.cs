using System.Text.Json.Serialization;

namespace mwhWebAdmin.Article.Models;

/// <summary>
/// Represents the result of social media field generation
/// </summary>
public class SocialMediaResult
{
    /// <summary>
    /// Gets or sets the Open Graph title (60 characters max)
    /// </summary>
    [JsonPropertyName("ogTitle")]
    public string OgTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Open Graph description (200 characters max)
    /// </summary>
    [JsonPropertyName("ogDescription")]
    public string OgDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter title (50 characters max)
    /// </summary>
    [JsonPropertyName("twitterTitle")]
    public string TwitterTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter description (120 characters max)
    /// </summary>
    [JsonPropertyName("twitterDescription")]
    public string TwitterDescription { get; set; } = string.Empty;
}
