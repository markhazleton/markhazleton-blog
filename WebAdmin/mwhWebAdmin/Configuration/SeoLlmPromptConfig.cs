using mwhWebAdmin.Configuration;

namespace mwhWebAdmin.Services;

/// <summary>
/// Centralized LLM prompt configuration for SEO content generation
/// Ensures consistent SEO requirements across AI-generated content
/// </summary>
public static class SeoLlmPromptConfig
{
    /// <summary>
    /// Gets the base SEO requirements prompt that should be included in all LLM prompts
    /// </summary>
    public static string GetBaseSeoRequirementsPrompt()
    {
        return $@"
**SEO Requirements (MUST be strictly followed):**

1. **Title**: {SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} characters, compelling and keyword-rich
2. **Meta Description**: {SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} characters, engaging summary with call-to-action words
3. **Keywords**: {SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} relevant keywords, comma-separated, no repetition
4. **Open Graph Title**: {SeoValidationConfig.OpenGraphTitle.MinLength}-{SeoValidationConfig.OpenGraphTitle.MaxLength} characters, social media optimized
5. **Open Graph Description**: {SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} characters, compelling for social sharing
6. **Twitter Title**: {SeoValidationConfig.TwitterTitle.MinLength}-{SeoValidationConfig.TwitterTitle.MaxLength} characters, Twitter-optimized
7. **Twitter Description**: {SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} characters, engaging for Twitter audience

**Call-to-Action Words to Include**: {string.Join(", ", SeoValidationConfig.CallToAction.Words)}

**VALIDATION REQUIREMENTS:**
- ALL character counts must be within specified ranges
- Keywords must be relevant and specific to the content
- Descriptions must include compelling call-to-action language
- Content must be engaging and encourage clicks
- NO keyword stuffing or repetitive phrases";
    }

    /// <summary>
    /// Gets the content generation prompt with SEO requirements
    /// </summary>
    public static string GetContentGenerationPrompt(string title, string existingContent = "")
    {
        var basePrompt = GetBaseSeoRequirementsPrompt();

        var contentPrompt = $@"
Generate comprehensive SEO metadata and content for an article titled: ""{title}""

{(string.IsNullOrEmpty(existingContent) ? "" : $"Existing content context: {existingContent}")}

{basePrompt}

**Response Format (JSON):**
```json
{{
    ""seoTitle"": ""[{SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} chars]"",
    ""metaDescription"": ""[{SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} chars]"",
    ""keywords"": ""[{SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} keywords, comma-separated]"",
    ""ogTitle"": ""[{SeoValidationConfig.OpenGraphTitle.MinLength}-{SeoValidationConfig.OpenGraphTitle.MaxLength} chars]"",
    ""ogDescription"": ""[{SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} chars]"",
    ""twitterTitle"": ""[{SeoValidationConfig.TwitterTitle.MinLength}-{SeoValidationConfig.TwitterTitle.MaxLength} chars]"",
    ""twitterDescription"": ""[{SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} chars]"",
    ""articleTitle"": ""[Main article title]"",
    ""articleDescription"": ""[Brief article description]"",
    ""articleContent"": ""[Full article content in markdown]""
}}
```

**CRITICAL:** Verify all character counts are within the specified ranges before responding.";

        return contentPrompt;
    }

    /// <summary>
    /// Gets the SEO optimization prompt for existing content
    /// </summary>
    public static string GetSeoOptimizationPrompt(string title, string description, string content)
    {
        var basePrompt = GetBaseSeoRequirementsPrompt();

        var optimizationPrompt = $@"
Optimize the SEO metadata for existing content:

**Title:** {title}
**Description:** {description}
**Content:** {content}

{basePrompt}

**Task:** Generate optimized SEO metadata that follows all requirements above.

**Response Format (JSON):**
```json
{{
    ""seoTitle"": ""[{SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} chars]"",
    ""metaDescription"": ""[{SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} chars]"",
    ""keywords"": ""[{SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} keywords, comma-separated]"",
    ""ogTitle"": ""[{SeoValidationConfig.OpenGraphTitle.MinLength}-{SeoValidationConfig.OpenGraphTitle.MaxLength} chars]"",
    ""ogDescription"": ""[{SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} chars]"",
    ""twitterTitle"": ""[{SeoValidationConfig.TwitterTitle.MinLength}-{SeoValidationConfig.TwitterTitle.MaxLength} chars]"",
    ""twitterDescription"": ""[{SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} chars]""
}}
```

**CRITICAL:** Verify all character counts are within the specified ranges before responding.";

        return optimizationPrompt;
    }

    /// <summary>
    /// Gets the validation prompt to check generated content
    /// </summary>
    public static string GetValidationPrompt(string generatedContent)
    {
        return $@"
Validate the following SEO content against the requirements:

{generatedContent}

**Validation Checklist:**
1. Title: {SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} characters ✓/✗
2. Meta Description: {SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} characters ✓/✗
3. Keywords: {SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} keywords ✓/✗
4. Open Graph Title: {SeoValidationConfig.OpenGraphTitle.MinLength}-{SeoValidationConfig.OpenGraphTitle.MaxLength} characters ✓/✗
5. Open Graph Description: {SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} characters ✓/✗
6. Twitter Title: {SeoValidationConfig.TwitterTitle.MinLength}-{SeoValidationConfig.TwitterTitle.MaxLength} characters ✓/✗
7. Twitter Description: {SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} characters ✓/✗
8. Call-to-action words included ✓/✗
9. Keywords relevant and non-repetitive ✓/✗
10. Content engaging and clickable ✓/✗

**Response:** List any validation failures and provide corrected versions.";
    }

    /// <summary>
    /// Gets the character count validation message for a specific field
    /// </summary>
    public static string GetCharacterCountValidation(string fieldName, int actualLength, int minLength, int maxLength)
    {
        if (actualLength < minLength)
            return $"{fieldName} is too short ({actualLength} chars). Required: {minLength}-{maxLength} characters.";
        if (actualLength > maxLength)
            return $"{fieldName} is too long ({actualLength} chars). Required: {minLength}-{maxLength} characters.";
        return string.Empty;
    }
}
