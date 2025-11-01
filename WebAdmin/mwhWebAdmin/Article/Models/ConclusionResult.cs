using System.Text.Json.Serialization;

namespace mwhWebAdmin.Article.Models;

/// <summary>
/// Represents the result of conclusion section generation
/// </summary>
public class ConclusionResult
{
    /// <summary>
    /// Gets or sets the conclusion section heading
    /// </summary>
[JsonPropertyName("conclusionTitle")]
    public string ConclusionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion summary (2-3 sentences)
    /// </summary>
    [JsonPropertyName("conclusionSummary")]
    public string ConclusionSummary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key takeaway heading
    /// </summary>
    [JsonPropertyName("conclusionKeyHeading")]
    public string ConclusionKeyHeading { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key takeaway text (1-2 sentences)
    /// </summary>
    [JsonPropertyName("conclusionKeyText")]
    public string ConclusionKeyText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the closing text with call-to-action
  /// </summary>
    [JsonPropertyName("conclusionText")]
    public string ConclusionText { get; set; } = string.Empty;
}
