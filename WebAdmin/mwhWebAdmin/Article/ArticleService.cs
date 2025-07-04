using HtmlAgilityPack;
using System.Globalization;

namespace mwhWebAdmin.Article
{
    /// <summary>
    /// Represents a service for managing articles.
    /// </summary>
    public class ArticleService
    {
        private List<ArticleModel> _articles = [];
        private readonly string _articlesDirectory;
        private readonly IConfiguration _configuration;
        private readonly string _filePath;
        private readonly object _lock = new();
        private readonly ILogger<ArticleService> _logger;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleService"/> class.
        /// </summary>
        /// <param name="filePath">The file path of the articles JSON file.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public ArticleService(string filePath, ILogger<ArticleService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _filePath = filePath;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _articlesDirectory = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug", "articles");
            LoadArticles();
        }

        /// <summary>
        /// Generates comprehensive SEO data for the given content using OpenAI's Chat Completions API.
        /// </summary>
        /// <param name="content">The body content of the article.</param>
        /// <param name="currentTitle">The current article title for context.</param>
        /// <returns>A structured SEO data object with keywords, title, description, etc.</returns>
        private async Task<SeoGenerationResult> GenerateSeoDataFromContentAsync(string content, string? currentTitle = null)
        {
            try
            {
                var openAiApiKey = _configuration["OPENAI_API_KEY"];
                var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);

                var systemPrompt = @"You are an SEO expert specializing in technical and business content optimization.
Analyze the provided content which may include:
- Article title and metadata
- PUG template structure indicators (marked with [Structure: ...])
- Raw article content
- Current descriptions

Generate comprehensive SEO metadata, social media previews, article content, and conclusion content based on this analysis.
Focus on the main article content and structural elements to understand the topic.

TITLE CONSISTENCY STRATEGY:
- Generate ONE primary optimized title and use variations for different platforms
- SEO Title: Full optimized title with '| Mark Hazleton' suffix
- Article Title: Clean version without the suffix (will be the same as SEO title minus '| Mark Hazleton')
- Open Graph Title: Same as Article Title or slight variation for social media
- Twitter Title: Shortened version of Article Title (max 50 characters)

CRITICAL SEO REQUIREMENTS (MANDATORY - THESE MUST BE FOLLOWED):

For Article Content:
- Article Title: Clean, compelling title without brand suffix (this will be used as the main article title)
- Article Description: Single paragraph summary (1-2 sentences) of what the article covers
- Article Content: Full article content in MARKDOWN format - create comprehensive, well-structured content using proper markdown syntax (headers, lists, code blocks, links, etc.)

MARKDOWN FORMATTING REQUIREMENTS:
- Use proper heading hierarchy: # Main Title, ## Section Headings, ### Subsections
- Format lists with - or * for unordered, 1. 2. 3. for ordered
- Use **bold** and *italic* for emphasis
- Create code blocks with ```language for multi-line code, `inline` for single words
- Format links as [text](url) and ensure they open in new tabs where appropriate
- Use > for blockquotes when including quotes or important callouts
- Structure content with clear sections, proper spacing, and logical flow

For SEO Optimization:
- Keywords: 3-8 relevant keywords (comma-separated), MUST include 'Mark Hazleton' for brand visibility
- SEO Title: ABSOLUTE REQUIREMENT - MINIMUM 30 characters, MAXIMUM 60 characters, compelling and keyword-rich, MUST include '| Mark Hazleton' suffix for brand consistency. NEVER submit a title shorter than 30 characters total.
- Meta Description: ABSOLUTE REQUIREMENT - MINIMUM 120 characters, MAXIMUM 160 characters, engaging summary with primary keywords, MUST include action words like 'discover', 'learn', 'explore', 'understand', 'master', or 'guide'. NEVER submit a description shorter than 120 characters.

For Social Media Preview (USE CONSISTENT TITLES):
- Open Graph Title: Use the Article Title (same as SEO title without suffix) or slight variation (30-65 characters)
- Open Graph Description: Up to 200 characters, more engaging for social media
- Twitter Title: Shortened version of Article Title (up to 50 characters)
- Twitter Description: Concise version, up to 120 characters

For Article Content:
- Subtitle: Complementary subtitle that provides additional context
- Summary: 2-3 sentence introduction that hooks the reader

For Conclusion Section:
- Conclusion Title: Compelling heading (e.g., Key Takeaways, Final Thoughts)
- Conclusion Summary: 2-3 sentences summarizing main points
- Conclusion Key Heading: Short, impactful heading (e.g., Bottom Line, Key Insight)
- Conclusion Key Text: 1-2 sentences with the key insight
- Conclusion Text: Final thoughts, call to action, or next steps

VALIDATION REQUIREMENTS:
- SEO Title: Must be 30-60 characters INCLUDING the '| Mark Hazleton' suffix. Count characters carefully.
- Article Title: Should be the SEO title WITHOUT the '| Mark Hazleton' suffix
- Meta Description: Must be 120-160 characters. Count characters carefully.
- Keywords: Must include 'Mark Hazleton' and be 3-8 total keywords.
- Article Content: Should be comprehensive and well-structured
- Title Consistency: Open Graph and Twitter titles should be based on the Article Title

EXAMPLE TITLE CONSTRUCTION:
- Article Title: 'Complete Guide to SampleMvcCRUD Project' (41 chars)
- SEO Title: 'Complete Guide to SampleMvcCRUD Project | Mark Hazleton' (55 chars)
- Open Graph Title: 'Complete Guide to SampleMvcCRUD Project' (same as Article Title)
- Twitter Title: 'SampleMvcCRUD Guide' (19 chars, shortened version)

IMPORTANT: The SEO title and meta description will be validated for exact character count requirements.
Any response that does not meet these requirements will be rejected.
The SEO title MUST be at least 30 characters and include '| Mark Hazleton'.
The meta description MUST be between 120-160 characters and include action words.
Keywords MUST include 'Mark Hazleton' and be between 3-8 total keywords.
Article content should be comprehensive and informative.

Consider the article structure from PUG templates to understand content organization.
Ignore navigation elements, headers, footers, and technical markup.";

                var userContent = string.IsNullOrEmpty(currentTitle)
                    ? content
                    : $"Current Title: {currentTitle}\n\nContent: {content}";

                // Define the JSON schema for structured outputs
                var responseFormat = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "seo_generation_result",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                articleTitle = new { type = "string", description = "Clean article title without brand suffix (will be used as main article title)" },
                                articleDescription = new { type = "string", description = "Single paragraph summary (1-2 sentences) of what the article covers" },
                                articleContent = new { type = "string", description = "Full article content in MARKDOWN format - create comprehensive, well-structured content using proper markdown syntax (# headers, - lists, ```code blocks```, [links](url), **bold**, *italic*, etc.)" },
                                keywords = new { type = "string", description = "Comma-separated list of 3-8 SEO keywords, MUST include 'Mark Hazleton'" },
                                seoTitle = new { type = "string", description = "SEO-optimized title (ABSOLUTE MINIMUM 30 characters, MAXIMUM 60 characters), MUST include '| Mark Hazleton' suffix. Should be articleTitle + '| Mark Hazleton'" },
                                metaDescription = new { type = "string", description = "Meta description (ABSOLUTE MINIMUM 120 characters, MAXIMUM 160 characters), MUST include action words like 'discover', 'learn', 'explore'. Count characters carefully before submitting." },
                                ogTitle = new { type = "string", description = "Open Graph title - use articleTitle or slight variation (30-65 characters)" },
                                ogDescription = new { type = "string", description = "Open Graph description (up to 200 characters)" },
                                twitterTitle = new { type = "string", description = "Twitter title - shortened version of articleTitle (up to 50 characters)" },
                                twitterDescription = new { type = "string", description = "Twitter description (up to 120 characters)" },
                                subtitle = new { type = "string", description = "Article subtitle providing additional context" },
                                summary = new { type = "string", description = "2-3 sentence article introduction" },
                                conclusionTitle = new { type = "string", description = "Conclusion section heading" },
                                conclusionSummary = new { type = "string", description = "2-3 sentences summarizing main points" },
                                conclusionKeyHeading = new { type = "string", description = "Short, impactful key takeaway heading" },
                                conclusionKeyText = new { type = "string", description = "1-2 sentences with key insight" },
                                conclusionText = new { type = "string", description = "Final thoughts and call to action" }
                            },
                            required = new[] {
                                "articleTitle", "articleDescription", "articleContent", "keywords", "seoTitle", "metaDescription",
                                "ogTitle", "ogDescription", "twitterTitle", "twitterDescription", "subtitle", "summary",
                                "conclusionTitle", "conclusionSummary", "conclusionKeyHeading", "conclusionKeyText", "conclusionText"
                            },
                            additionalProperties = false
                        }
                    }
                };

                // Prepare the request payload with structured outputs
                var requestBody = new
                {
                    model = "gpt-4o-2024-08-06", // Use the model that supports structured outputs
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = systemPrompt
                        },
                        new
                        {
                            role = "user",
                            content = userContent
                        }
                    },
                    max_tokens = 3000,
                    temperature = 0.3,
                    response_format = responseFormat
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                // Make the API call
                var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonDocument.Parse(responseContent);

                // Extract the response text
                var aiResponse = responseData.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrEmpty(aiResponse))
                {
                    return new SeoGenerationResult();
                }

                // With structured outputs, the response should be valid JSON without markdown formatting
                try
                {
                    var seoData = JsonSerializer.Deserialize<SeoGenerationResult>(aiResponse);
                    if (seoData != null)
                    {
                        // Post-process and validate the SEO data to ensure it meets requirements
                        seoData = ValidateAndCorrectSeoData(seoData, currentTitle);
                    }
                    return seoData ?? new SeoGenerationResult();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse structured AI SEO response as JSON. Raw response: {Response}", aiResponse);

                    // Fallback: try to extract keywords from the response
                    return new SeoGenerationResult
                    {
                        Keywords = aiResponse.Trim()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SEO data using OpenAI API with structured outputs.");
                return new SeoGenerationResult();
            }
        }

        /// <summary>
        /// Validates and corrects AI-generated SEO data to ensure it meets all requirements.
        /// </summary>
        /// <param name="seoData">The SEO data to validate and correct.</param>
        /// <param name="fallbackTitle">Fallback title to use if correction is needed.</param>
        /// <returns>Corrected SEO data that meets all validation requirements.</returns>
        private SeoGenerationResult ValidateAndCorrectSeoData(SeoGenerationResult seoData, string? fallbackTitle = null)
        {
            try
            {
                // Validate and correct SEO Title
                if (!string.IsNullOrEmpty(seoData.SeoTitle))
                {
                    var correctedTitle = EnsureSeoTitleMeetsRequirements(seoData.SeoTitle, fallbackTitle);
                    if (correctedTitle != seoData.SeoTitle)
                    {
                        _logger.LogInformation("Corrected SEO title from '{Original}' to '{Corrected}'", seoData.SeoTitle, correctedTitle);
                        seoData.SeoTitle = correctedTitle;
                    }
                }

                // Validate and correct Meta Description
                if (!string.IsNullOrEmpty(seoData.MetaDescription))
                {
                    var correctedDescription = EnsureMetaDescriptionMeetsRequirements(seoData.MetaDescription);
                    if (correctedDescription != seoData.MetaDescription)
                    {
                        _logger.LogInformation("Corrected meta description length from {Original} to {Corrected} characters",
                            seoData.MetaDescription.Length, correctedDescription.Length);
                        seoData.MetaDescription = correctedDescription;
                    }
                }

                // Validate and correct Keywords
                if (!string.IsNullOrEmpty(seoData.Keywords))
                {
                    var correctedKeywords = EnsureKeywordsMeetRequirements(seoData.Keywords);
                    if (correctedKeywords != seoData.Keywords)
                    {
                        _logger.LogInformation("Corrected keywords from '{Original}' to '{Corrected}'", seoData.Keywords, correctedKeywords);
                        seoData.Keywords = correctedKeywords;
                    }
                }

                return seoData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating and correcting SEO data");
                return seoData;
            }
        }

        /// <summary>
        /// Ensures the SEO title meets the 30-60 character requirement and includes the brand suffix.
        /// </summary>
        /// <param name="title">The original title.</param>
        /// <param name="fallbackTitle">Fallback title to use if original is too short.</param>
        /// <returns>A corrected title that meets all requirements.</returns>
        private string EnsureSeoTitleMeetsRequirements(string title, string? fallbackTitle = null)
        {
            const string brandSuffix = " | Mark Hazleton";
            const int minLength = 30;
            const int maxLength = 60;

            // Remove existing brand suffix if present to avoid duplication
            var cleanTitle = title.Replace(" | Mark Hazleton", "").Replace("|Mark Hazleton", "").Trim();

            // If the clean title is too short, try to use fallback or expand it
            if (cleanTitle.Length < (minLength - brandSuffix.Length))
            {
                if (!string.IsNullOrEmpty(fallbackTitle))
                {
                    var cleanFallback = fallbackTitle.Replace(" | Mark Hazleton", "").Replace("|Mark Hazleton", "").Trim();
                    if (cleanFallback.Length >= (minLength - brandSuffix.Length))
                    {
                        cleanTitle = cleanFallback;
                    }
                    else
                    {
                        // Expand the title to meet minimum requirements
                        cleanTitle = ExpandTitleToMinimumLength(cleanTitle, minLength - brandSuffix.Length);
                    }
                }
                else
                {
                    // Expand the title to meet minimum requirements
                    cleanTitle = ExpandTitleToMinimumLength(cleanTitle, minLength - brandSuffix.Length);
                }
            }

            // Ensure the title with brand suffix doesn't exceed maximum length
            var titleWithBrand = cleanTitle + brandSuffix;
            if (titleWithBrand.Length > maxLength)
            {
                // Truncate the clean title to fit within the limit
                var maxCleanLength = maxLength - brandSuffix.Length;
                cleanTitle = cleanTitle.Substring(0, maxCleanLength).Trim();
                titleWithBrand = cleanTitle + brandSuffix;
            }

            return titleWithBrand;
        }

        /// <summary>
        /// Expands a title to meet minimum length requirements by adding descriptive words.
        /// </summary>
        /// <param name="title">The original title.</param>
        /// <param name="minLength">The minimum required length.</param>
        /// <returns>An expanded title that meets the minimum length.</returns>
        private string ExpandTitleToMinimumLength(string title, int minLength)
        {
            if (title.Length >= minLength)
                return title;

            // Common descriptive additions for technical and business content
            var expansions = new[]
            {
                "Complete Guide",
                "Step-by-Step Guide",
                "Comprehensive Guide",
                "Professional Guide",
                "Expert Guide",
                "Implementation Guide",
                "Development Guide",
                "Best Practices",
                "Expert Tips",
                "Professional Tips",
                "Development Tips",
                "Implementation Tips"
            };

            // Try to find an expansion that fits
            foreach (var expansion in expansions)
            {
                var expandedTitle = $"{expansion}: {title}";
                if (expandedTitle.Length >= minLength && expandedTitle.Length <= 60 - " | Mark Hazleton".Length)
                {
                    return expandedTitle;
                }
            }

            // If no expansion works, add generic descriptive text
            var genericExpansion = $"Professional Guide to {title}";
            if (genericExpansion.Length >= minLength)
            {
                return genericExpansion;
            }

            // Last resort: add "Complete" prefix
            return $"Complete {title} Guide";
        }

        /// <summary>
        /// Ensures the meta description meets the 120-160 character requirement.
        /// </summary>
        /// <param name="description">The original description.</param>
        /// <returns>A corrected description that meets length requirements.</returns>
        private string EnsureMetaDescriptionMeetsRequirements(string description)
        {
            const int minLength = 120;
            const int maxLength = 160;

            if (description.Length >= minLength && description.Length <= maxLength)
                return description;

            if (description.Length < minLength)
            {
                // Expand the description to meet minimum requirements
                var expansions = new[]
                {
                    " Learn practical tips and expert insights.",
                    " Discover proven techniques and best practices.",
                    " Explore comprehensive strategies and solutions.",
                    " Master essential skills and techniques.",
                    " Understand key concepts and implementation details."
                };

                foreach (var expansion in expansions)
                {
                    var expandedDesc = description + expansion;
                    if (expandedDesc.Length >= minLength && expandedDesc.Length <= maxLength)
                    {
                        return expandedDesc;
                    }
                }

                // If still too short, add generic ending
                var genericExpansion = " Learn more about this topic and discover practical solutions.";
                return (description + genericExpansion).Substring(0, Math.Min(description.Length + genericExpansion.Length, maxLength));
            }

            if (description.Length > maxLength)
            {
                // Truncate to maximum length, preserving word boundaries
                var truncated = description.Substring(0, maxLength);
                var lastSpace = truncated.LastIndexOf(' ');
                if (lastSpace > maxLength - 20) // Only truncate at word boundary if it's not too short
                {
                    truncated = truncated.Substring(0, lastSpace);
                }
                return truncated.Trim();
            }

            return description;
        }

        /// <summary>
        /// Ensures keywords meet the requirement of including "Mark Hazleton" and being 3-8 keywords.
        /// </summary>
        /// <param name="keywords">The original keywords.</param>
        /// <returns>Corrected keywords that meet all requirements.</returns>
        private string EnsureKeywordsMeetRequirements(string keywords)
        {
            if (string.IsNullOrEmpty(keywords))
                return "Mark Hazleton";

            var keywordList = keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();

            // Ensure "Mark Hazleton" is included
            if (!keywordList.Any(k => k.Equals("Mark Hazleton", StringComparison.OrdinalIgnoreCase)))
            {
                keywordList.Add("Mark Hazleton");
            }

            // Ensure we have at least 3 keywords
            if (keywordList.Count < 3)
            {
                var additionalKeywords = new[] { "solutions architect", "project management", "web development", "IT consulting", "technical leadership" };
                foreach (var keyword in additionalKeywords)
                {
                    if (keywordList.Count >= 3) break;
                    if (!keywordList.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
                    {
                        keywordList.Add(keyword);
                    }
                }
            }

            // Ensure we don't exceed 8 keywords
            if (keywordList.Count > 8)
            {
                // Keep "Mark Hazleton" and the first 7 other keywords
                var markHazeltonKeyword = keywordList.FirstOrDefault(k => k.Equals("Mark Hazleton", StringComparison.OrdinalIgnoreCase));
                var otherKeywords = keywordList.Where(k => !k.Equals("Mark Hazleton", StringComparison.OrdinalIgnoreCase)).Take(7).ToList();

                keywordList = new List<string>();
                if (markHazeltonKeyword != null)
                {
                    keywordList.Add(markHazeltonKeyword);
                }
                keywordList.AddRange(otherKeywords);
            }

            return string.Join(", ", keywordList);
        }

        /// <summary>
        /// Generates SEO-optimized keywords for the given content using OpenAI's Chat Completions API.
        /// This method is maintained for backward compatibility.
        /// </summary>
        /// <param name="content">The body content of the article.</param>
        /// <returns>A comma-separated string of SEO-optimized keywords.</returns>
        private async Task<string> GenerateKeywordsFromContentAsync(string content)
        {
            var seoData = await GenerateSeoDataFromContentAsync(content);
            return seoData.Keywords ?? string.Empty;
        }

        /// <summary>
        /// Generates the Pug file content for an article.
        /// </summary>
        /// <param name="article">The article model.</param>
        /// <returns>The Pug file content.</returns>
        private string GeneratePugFileContent(ArticleModel article)
        {
            try
            {
                // Read the article-stub.pug template
                string templateFilePath = Path.Combine(_articlesDirectory, "article-stub.pug");
                string templateContent = File.ReadAllText(templateFilePath);

                // Replace placeholder content with actual article data
                string pugContent = templateContent
                    .Replace("Article Title Here", article.Name ?? "New Article Title")
                    .Replace("Article Subtitle Here", article.Subtitle ?? "Article Subtitle")
                    .Replace("Brief article summary or introduction paragraph goes here.",
                            article.Summary ?? article.Description ?? "Brief article summary or introduction paragraph.")
                    .Replace("Main Section Title", article.Section ?? "Main Content")
                    .Replace("Main article content goes here. This is where you'll write your detailed content.",
                            article.ArticleContent ?? "Main article content goes here. This is where you'll write your detailed content.")
                    .Replace("Summary of key points from the article.",
                            article.ConclusionSummary ?? "Summary of key points from the article.")
                    .Replace("Main insight or actionable takeaway from the article.",
                            article.ConclusionKeyText ?? "Main insight or actionable takeaway from the article.")
                    .Replace("Final thoughts and call to action or next steps.",
                            article.ConclusionText ?? "Final thoughts and call to action or next steps.");

                // Add YouTube video section if URL is provided
                if (!string.IsNullOrEmpty(article.YouTubeUrl))
                {
                    string videoId = ExtractYouTubeVideoId(article.YouTubeUrl);
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        string videoTitle = article.YouTubeTitle ?? "Featured Video";
                        string videoSection = $@"
          // Featured Video
          .card.mb-4.shadow-sm
            .card-header.bg-dark.text-white
              h5.card-title.mb-0
                i.bi.bi-play-circle.me-2
                | Watch: {videoTitle}
            .card-body.p-0
              .ratio.ratio-16x9
                iframe(
                  src=""https://www.youtube.com/embed/{videoId}""
                  title=""{videoTitle}""
                  allow=""accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share""
                  referrerpolicy=""strict-origin-when-cross-origin""
                  allowfullscreen
                )
            .card-footer.text-muted
              small
                i.bi.bi-info-circle.me-1
                | {videoTitle}
";
                        // Insert the video section after the article introduction
                        pugContent = pugContent.Replace("p.lead.mb-5", $"p.lead.mb-5{videoSection}");
                    }
                }

                return pugContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Pug content for article: {ArticleName}", article.Name);
                return string.Empty;
            }
        }        /// <summary>
                 /// Loads the articles from the JSON file.
                 /// </summary>
        private void LoadArticles()
        {
            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                _articles = JsonSerializer.Deserialize<List<ArticleModel>>(jsonContent) ?? [];
                foreach (var article in _articles.OrderBy(o => o.Id))
                {
                    article.Id = _articles.IndexOf(article);

                    // Calculate source file path if not already set
                    if (string.IsNullOrEmpty(article.Source))
                    {
                        article.Source = CalculateSourceFilePath(article.Slug);
                    }
                }
                _logger.LogInformation("Articles loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load articles.");
                _articles = [];
            }
        }

        /// <summary>
        /// Saves the articles to the JSON file.
        /// </summary>
        private void SaveArticles()
        {
            lock (_lock)
            {
                try
                {
                    var startdate = ConvertStringToDate("2023-01-01");
                    var count = _articles.Count;
                    // calculate days since startdate and then divide by count to get a change frequency
                    var daysSinceStart = (DateTime.Now - startdate).TotalDays;
                    var changeFrequency = (int)(daysSinceStart / count);
                    // set the LastModified and ChangeFrequency properties for each article based on startddate and daysSinceStart to evenely distribute the articles
                    foreach (var article in _articles.OrderBy(o => o.Id))
                    {
                        var newModifiedDate = startdate.AddDays(changeFrequency * article.Id);
                        var daysSinceModified = (DateTime.Now - newModifiedDate).TotalDays;

                        article.LastModified = startdate.AddDays(changeFrequency * article.Id).ToString("yyyy-MM-dd");
                        article.ChangeFrequency = daysSinceModified < 60 ? "daily" : daysSinceModified < 120 ? "weekly" : "monthly";
                    }
                    string jsonContent = JsonSerializer.Serialize(
                        _articles,
                        jsonSerializerOptions);
                    File.WriteAllText(_filePath, jsonContent);
                    _logger.LogInformation("Articles saved successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save articles.");
                }
            }
        }

        /// <summary>
        /// Validates an article model.
        /// </summary>
        /// <param name="article">The article model to validate.</param>
        /// <returns>True if the article is valid; otherwise, false.</returns>
        private static bool ValidateArticle(ArticleModel article)
        {
            return !string.IsNullOrWhiteSpace(article.Name) &&
                   !string.IsNullOrWhiteSpace(article.Section) &&
                   !string.IsNullOrWhiteSpace(article.Slug);
        }    /// <summary>
             /// Gets a list of unique keywords from all articles.
             /// </summary>
             /// <returns>A list of unique keywords.</returns>
        internal List<string> GetKeywords()
        {
            lock (_lock)
            {
                try
                {
                    var allKeywords = _articles
                        .Where(a => !string.IsNullOrWhiteSpace(a.Keywords))
                        .SelectMany(a => a.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(k => k)
                        .ToList();

                    return allKeywords;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting keywords from articles");
                    return [];
                }
            }
        }

        /// <summary>
        /// Adds a new article.
        /// </summary>
        /// <param name="newArticle">The new article model.</param>
        public void AddArticle(ArticleModel newArticle)
        {
            lock (_lock)
            {
                if (!ValidateArticle(newArticle))
                {
                    _logger.LogWarning("Invalid article data. Article not added.");
                    return;
                }
                try
                {
                    newArticle.Slug = "articles/" + GenerateSlug(newArticle.Name) + ".html";
                    newArticle.Id = _articles.Count != 0 ? _articles.Max(article => article.Id) + 1 : 1; // Assign a new ID

                    // Calculate source file path
                    newArticle.Source = CalculateSourceFilePath(newArticle.Slug);

                    _articles.Add(newArticle);

                    string pugContent = GeneratePugFileContent(newArticle);
                    if (!string.IsNullOrEmpty(pugContent))
                    {
                        // Save the .pug file
                        string pugFilePath = Path.Combine(_articlesDirectory, $"{newArticle.Slug.Replace(".html", string.Empty).Replace("articles/", string.Empty)}.pug");
                        File.WriteAllText(pugFilePath, pugContent);

                        // Recalculate source path after creating the file
                        newArticle.Source = CalculateSourceFilePath(newArticle.Slug);
                    }

                    SaveArticles();
                    _logger.LogInformation("Article '{ArticleName}' added successfully.", newArticle.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add article: {ArticleName}", newArticle.Name);
                }
            }
        }

        /// <summary>
        /// Converts a string to a DateTime object.
        /// </summary>
        /// <param name="dateString">The string representation of the date.</param>
        /// <returns>The DateTime object.</returns>
        public static DateTime ConvertStringToDate(string dateString)
        {
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
            {
                return dateValue;
            }
            else
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Generates the Keywords property for an article by fetching the content from the <section> node with id="post"
        /// and calculating keywords using the GenerateKeywordsFromContentAsync function.
        /// </summary>
        /// <param name="targetSlug">Optional slug to target a specific article.</param>
        /// <returns>The generated keywords.</returns>
        public async Task<string> GenerateKeywordsAsync(ArticleModel article)
        {
            if (article != null)
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    string url = $"https://markhazleton.com/{article.Slug}";
                    _logger.LogInformation("Fetching content for article: {ArticleName} from {Url}", article.Name, url);

                    // Fetch the HTML content
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    // Parse the HTML to extract the content from <article id='post'>
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    var sectionNode = htmlDoc.DocumentNode.SelectSingleNode("//article[@id='post']");

                    var content = sectionNode?.InnerText;

                    if (!string.IsNullOrEmpty(content))
                    {
                        // Generate keywords using the content from the section
                        article.Keywords = await GenerateKeywordsFromContentAsync(content);
                        _logger.LogInformation("Keywords updated for article: {ArticleName}", article.Name);
                        return article.Keywords;
                    }
                    else
                    {
                        _logger.LogWarning("No content found in <article id='post'> for article: {ArticleName}", article.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate keywords for article: {ArticleName}", article.Name);
                }
            }
            return article?.Keywords ?? string.Empty;
        }

        /// <summary>
        /// Updates the Keywords property for a specific article by fetching meta tag keywords from its URL.
        /// </summary>
        /// <param name="targetSlug">The slug of the article to update.</param>
        /// <returns>The updated keywords.</returns>
        public async Task<string> UpdateKeywordsAsync(string targetSlug)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var article = _articles.FirstOrDefault(a => a.Slug == targetSlug);
            if (article == null)
            {
                _logger.LogWarning("Article with slug {TargetSlug} not found.", targetSlug);
                return string.Empty;
            }
            try
            {
                string url = $"https://markhazleton.com/{article.Slug}";
                _logger.LogInformation("Fetching keywords for article: {ArticleName} from {Url}", article.Name, url);

                // Fetch the HTML content
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string htmlContent = await response.Content.ReadAsStringAsync();

                // Parse the HTML to extract meta keywords
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var metaKeywords = htmlDoc.DocumentNode
                    .SelectSingleNode("//meta[@name='keywords']")
                    ?.GetAttributeValue("content", string.Empty);

                if (!string.IsNullOrEmpty(metaKeywords))
                {
                    article.Keywords = string.Join(", ", metaKeywords.Split(',').Select(k => k.Trim()));
                    _logger.LogInformation("Keywords updated for article: {ArticleName}", article.Name);
                }
                else
                {
                    _logger.LogWarning("No keywords found for article: {ArticleName}", article.Name);
                }
                return article.Keywords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update keywords for article: {ArticleName}", article.Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Updates the Keywords property for all articles by fetching meta tag keywords from their respective URLs.
        /// </summary>
        public async Task UpdateKeywordsForAllArticlesAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < _articles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("UpdateKeywordsForAllArticlesAsync operation was cancelled.");
                    break;
                }
                if (string.IsNullOrWhiteSpace(_articles[i].Keywords))
                {
                    // add 1 second delay to avoid hitting the OpenAI API rate limit
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);
                    Console.WriteLine($"Updating keywords for article: {_articles[i].Name}");
                    await UpdateArticle(_articles[i]).ConfigureAwait(true);
                    Console.WriteLine($"UPDATED keywords for article: {_articles[i].Name}");
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);

                }
            }
        }

        public void GenerateRSSFeed()
        {
            try
            {
                string? directoryPath = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    _logger.LogError("Unable to determine directory path for RSS feed generation");
                    return;
                }

                string rssFeedPath = Path.Combine(directoryPath, "rss.xml");
                var recentArticles = _articles.OrderByDescending(a => ConvertStringToDate(a.LastModified)).Take(10).ToList();

                using (XmlWriter writer = XmlWriter.Create(rssFeedPath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("rss");
                    writer.WriteAttributeString("version", "2.0");

                    // Add the atom namespace for the atom:link
                    writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");

                    writer.WriteStartElement("channel");
                    writer.WriteElementString("title", "Mark Hazleton Articles");
                    writer.WriteElementString("link", "https://markhazleton.com/");
                    writer.WriteElementString("description", "Latest articles from Mark Hazleton.");
                    writer.WriteElementString("lastBuildDate", DateTime.Now.ToString("r"));

                    // Add the atom:link element with rel="self"
                    writer.WriteStartElement("link", "atom", "http://www.w3.org/2005/Atom");
                    writer.WriteAttributeString("href", "https://markhazleton.com/rss.xml");
                    writer.WriteAttributeString("rel", "self");
                    writer.WriteAttributeString("type", "application/rss+xml");
                    writer.WriteEndElement(); // atom:link

                    foreach (var article in recentArticles)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteElementString("title", article.Name);
                        writer.WriteElementString("link", $"https://markhazleton.com/{article.Slug}");
                        writer.WriteElementString("description", article.Description);
                        writer.WriteElementString("pubDate", ConvertStringToDate(article.LastModified).ToString("r"));

                        // Add a unique GUID for the item (use the article's link as the GUID)
                        writer.WriteElementString("guid", $"https://markhazleton.com/{article.Slug}");

                        writer.WriteEndElement(); // item
                    }

                    writer.WriteEndElement(); // channel
                    writer.WriteEndElement(); // rss
                    writer.WriteEndDocument();
                }
                _logger.LogInformation("RSS feed generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate RSS feed.");
            }
        }

        /// <summary>
        /// Generates the site map XML file.
        /// </summary>
        public void GenerateSiteMap()
        {
            try
            {
                string? directoryPath = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    _logger.LogError("Unable to determine directory path for sitemap generation");
                    return;
                }

                string siteMapPath = Path.Combine(directoryPath, "SiteMap.xml");
                using (XmlWriter writer = XmlWriter.Create(siteMapPath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", "https://markhazleton.com/");
                    writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                    writer.WriteElementString("changefreq", "weekly");
                    writer.WriteEndElement();

                    foreach (var article in _articles)
                    {
                        writer.WriteStartElement("url");
                        writer.WriteElementString("loc", $"https://markhazleton.com/{article.Slug}");
                        writer.WriteElementString("lastmod", ConvertStringToDate(article.LastModified).ToString("yyyy-MM-ddTHH:mm:sszzz"));
                        writer.WriteElementString("changefreq", article.ChangeFrequency);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                _logger.LogInformation("Sitemap generated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate sitemap.");
            }
        }

        /// <summary>
        /// Generates a URL-friendly slug from the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The URL-friendly slug.</returns>
        public static string GenerateSlug(string input)
        {
            string slug = input.ToLower();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');
            return slug;
        }

        /// <summary>
        /// Gets an article by its ID.
        /// </summary>
        /// <param name="id">The ID of the article.</param>
        /// <returns>The article model.</returns>
        public ArticleModel GetArticleById(int id)
        {
            lock (_lock)
            {
                var article = _articles.FirstOrDefault(w => w.Id == id);
                if (article == null)
                {
                    _logger.LogWarning("Article with ID {Id} not found.", id);
                    return new ArticleModel();
                }
                return article;
            }
        }

        /// <summary>
        /// Gets a list of all articles.
        /// </summary>
        /// <returns>The list of articles.</returns>
        public List<ArticleModel> GetArticles()
        {
            lock (_lock)
            {
                return [.. _articles]; // Return a copy to avoid external modifications
            }
        }        /// <summary>
                 /// Updates an existing article.
                 /// </summary>
                 /// <param name="updatedArticle">The updated article model.</param>
        public async Task UpdateArticle(ArticleModel updatedArticle)
        {
            updatedArticle.Keywords = await GenerateKeywordsAsync(updatedArticle) ?? updatedArticle.Keywords;

            // Calculate source file path if not already set or if slug has changed
            if (string.IsNullOrEmpty(updatedArticle.Source))
            {
                updatedArticle.Source = CalculateSourceFilePath(updatedArticle.Slug);
            }

            lock (_lock)
            {
                if (!ValidateArticle(updatedArticle))
                {
                    _logger.LogWarning("Invalid article data. Article not updated.");
                    return;
                }

                int index = _articles.FindIndex(article => article.Id == updatedArticle.Id);
                if (index != -1)
                {
                    _articles[index] = updatedArticle;
                    SaveArticles();
                    _logger.LogInformation("Article '{ArticleName}' updated successfully.", updatedArticle.Name);
                }
                else
                {
                    _logger.LogWarning("Article with ID {Id} not found. Update failed.", updatedArticle.Id);
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Updates source file paths for all articles that don't have them set.
        /// This method helps populate existing articles with source information.
        /// </summary>
        public void UpdateMissingSourcePaths()
        {
            lock (_lock)
            {
                try
                {
                    bool hasChanges = false;
                    foreach (var article in _articles)
                    {
                        if (string.IsNullOrEmpty(article.Source))
                        {
                            string calculatedSource = CalculateSourceFilePath(article.Slug);
                            if (!string.IsNullOrEmpty(calculatedSource))
                            {
                                article.Source = calculatedSource;
                                hasChanges = true;
                                _logger.LogInformation("Updated source path for article '{ArticleName}': {SourcePath}",
                                    article.Name, article.Source);
                            }
                        }
                    }

                    if (hasChanges)
                    {
                        SaveArticles();
                        _logger.LogInformation("Updated source paths for articles saved successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("No articles required source path updates.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update missing source paths.");
                }
            }
        }        /// <summary>
                 /// Calculates the source file path for a PUG file based on the article slug.
                 /// Follows the same logic as the JavaScript findPugFileFromSlug function.
                 /// </summary>
                 /// <param name="slug">The article slug.</param>
                 /// <returns>The full path to the PUG source file starting with /src/pug/, or empty string if not found.</returns>
        private string CalculateSourceFilePath(string slug)
        {
            try
            {
                // If the slug is empty, return empty string
                if (string.IsNullOrEmpty(slug))
                    return string.Empty;                // If the slug ends with .html, replace it with .pug
                // If the slug ends with /, assume it's an index.html
                string pugFileName;
                if (slug.EndsWith(".html"))
                {
                    pugFileName = slug.Replace(".html", ".pug");
                }
                else if (slug.EndsWith("/"))
                {
                    pugFileName = slug + "index.pug";
                }
                else
                {
                    pugFileName = slug + ".pug";
                }

                // Remove any leading / from slug
                string cleanedSlug = pugFileName.StartsWith("/")
                    ? pugFileName.Substring(1)
                    : pugFileName;

                // Extract path components
                string[] slugParts = cleanedSlug.Split('/');
                string fileNamePart = slugParts.LastOrDefault() ?? string.Empty;

                // If the filename part is still empty after handling directory-based URLs, log and return
                if (string.IsNullOrEmpty(fileNamePart))
                {
                    _logger.LogWarning("Could not extract a valid filename from slug after processing: {Slug}", slug);
                    return string.Empty;
                }

                // Get the base directory path (same as _articlesDirectory but up one level to get to pug folder)
                string pugBaseDir = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug");

                // Build candidate paths to check
                List<string> candidatePaths = new();

                // Build full path from slug directory structure
                if (slugParts.Length > 1)
                {
                    // Try the exact path from the slug
                    string[] pathParts = slugParts.Take(slugParts.Length - 1).ToArray();
                    candidatePaths.Add(Path.Combine(pugBaseDir, Path.Combine(pathParts), fileNamePart));
                }

                // Try the articles directory
                candidatePaths.Add(Path.Combine(pugBaseDir, "articles", fileNamePart));

                // Try the root pug directory
                candidatePaths.Add(Path.Combine(pugBaseDir, fileNamePart));

                // Check all candidate paths
                foreach (string candidatePath in candidatePaths)
                {
                    if (File.Exists(candidatePath) && !Directory.Exists(candidatePath))
                    {
                        // Return path in the format /src/pug/...
                        string relativePath = Path.GetRelativePath(pugBaseDir, candidatePath).Replace('\\', '/');
                        return $"/src/pug/{relativePath}";
                    }

                    // Also check with .pug extension if not already present
                    if (!candidatePath.EndsWith(".pug"))
                    {
                        string pathWithExt = candidatePath + ".pug";
                        if (File.Exists(pathWithExt) && !Directory.Exists(pathWithExt))
                        {
                            // Return path in the format /src/pug/...
                            string relativePath = Path.GetRelativePath(pugBaseDir, pathWithExt).Replace('\\', '/');
                            return $"/src/pug/{relativePath}";
                        }
                    }
                }

                // If direct path lookup failed, search all pug files to find a match by filename
                string[] searchDirectories = new[]
                {
                    Path.Combine(pugBaseDir, "articles"),
                    pugBaseDir
                };

                foreach (string searchDir in searchDirectories)
                {
                    if (!Directory.Exists(searchDir)) continue;

                    string[] pugFiles = Directory.GetFiles(searchDir, "*.pug");

                    // Find a file that matches the slug's filename part
                    string? matchingFile = pugFiles.FirstOrDefault(file =>
                    {
                        string fileName = Path.GetFileName(file);
                        return fileName == fileNamePart || fileName == fileNamePart + ".pug";
                    });

                    if (!string.IsNullOrEmpty(matchingFile))
                    {
                        // Return path in the format /src/pug/...
                        string relativePath = Path.GetRelativePath(pugBaseDir, matchingFile).Replace('\\', '/');
                        return $"/src/pug/{relativePath}";
                    }
                }

                _logger.LogWarning("Could not find a matching Pug file for slug: {Slug}", slug);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding Pug file for slug {Slug}", slug);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a list of available images from the assets directory.
        /// </summary>
        /// <returns>A list of relative image paths.</returns>
        public List<string> GetAvailableImages()
        {
            try
            {
                var baseDirectory = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(baseDirectory))
                {
                    _logger.LogError("Unable to determine base directory for image loading");
                    return [];
                }

                var imagesDirectory = Path.Combine(baseDirectory, "assets", "img");

                if (Directory.Exists(imagesDirectory))
                {
                    var images = Directory.GetFiles(imagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => IsImageFile(file))
                        .Select(imagePath => imagePath.Replace(baseDirectory, string.Empty).Replace("\\", "/").TrimStart('/'))
                        .ToList();

                    return images;
                }
                else
                {
                    _logger.LogWarning("Images directory not found: {ImagesDirectory}", imagesDirectory);
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available images");
                return [];
            }
        }

        /// <summary>
        /// Determines if a file is an image based on its extension.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file is an image; otherwise, false.</returns>
        private static bool IsImageFile(string fileName)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
            return imageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
        }

        /// <summary>
        /// Tests the slug-to-source-path conversion logic for various slug formats.
        /// This method is useful for debugging and validation.
        /// </summary>
        /// <param name="testSlug">The slug to test</param>
        /// <returns>The calculated source path</returns>
        public string TestSlugToSourcePath(string testSlug)
        {
            _logger.LogInformation("Testing slug conversion: {Slug}", testSlug);
            var result = CalculateSourceFilePath(testSlug);
            _logger.LogInformation("Result for slug '{Slug}': {SourcePath}", testSlug, result);
            return result;
        }

        /// <summary>
        /// Initializes empty SEO objects for an article if they don't exist
        /// </summary>
        /// <param name="article">The article to initialize SEO for</param>
        public void InitializeSeoFields(ArticleModel article)
        {
            article.Seo ??= new SeoModel
            {
                Title = article.Name,
                Description = article.Description,
                Keywords = article.Keywords,
                Canonical = $"https://markhazleton.com/{article.Slug}"
            };

            article.OpenGraph ??= new OpenGraphModel
            {
                Title = article.Seo.Title,
                Description = article.Seo.Description,
                Image = !string.IsNullOrEmpty(article.ImgSrc) ? $"https://markhazleton.com/{article.ImgSrc}" : null,
                ImageAlt = $"{article.Name} - Mark Hazleton"
            };

            article.TwitterCard ??= new TwitterCardModel
            {
                Title = article.Seo.Title?.Length > 50 ? article.Seo.Title[..47] + "..." : article.Seo.Title,
                Description = article.Seo.Description?.Length > 120 ? article.Seo.Description[..117] + "..." : article.Seo.Description,
                Image = article.OpenGraph.Image,
                ImageAlt = article.OpenGraph.ImageAlt
            };
        }

        /// <summary>
        /// Auto-generates SEO fields based on article content
        /// </summary>
        /// <param name="article">The article to enhance</param>
        public void AutoGenerateSeoFields(ArticleModel article)
        {
            // Initialize if needed
            InitializeSeoFields(article);

            // Auto-generate canonical URL
            if (string.IsNullOrEmpty(article.Seo!.Canonical))
            {
                article.Seo.Canonical = $"https://markhazleton.com/{article.Slug}";
            }

            // Auto-generate Open Graph image alt text
            if (string.IsNullOrEmpty(article.OpenGraph!.ImageAlt))
            {
                article.OpenGraph.ImageAlt = $"{article.Name} - Mark Hazleton";
            }

            // Auto-generate Twitter image alt text
            if (string.IsNullOrEmpty(article.TwitterCard!.ImageAlt))
            {
                article.TwitterCard.ImageAlt = article.OpenGraph.ImageAlt;
            }
        }

        /// <summary>
        /// Auto-generates SEO fields based on article content using AI
        /// </summary>
        /// <param name="article">The article to enhance</param>
        public async Task AutoGenerateSeoFieldsAsync(ArticleModel article)
        {
            // Initialize if needed
            InitializeSeoFields(article);

            // Prepare content for AI analysis
            string contentForAnalysis = await PrepareContentForSeoAnalysis(article);

            // Generate AI-powered SEO data if content is available
            if (!string.IsNullOrEmpty(contentForAnalysis))
            {
                var seoData = await GenerateSeoDataFromContentAsync(contentForAnalysis, article.Name);

                _logger.LogInformation("AI generated SEO data for article '{ArticleName}': Title='{SeoTitle}' ({TitleLength} chars), Description='{MetaDescription}' ({DescriptionLength} chars)",
                    article.Name, seoData.SeoTitle, seoData.SeoTitle?.Length ?? 0, seoData.MetaDescription, seoData.MetaDescription?.Length ?? 0);

                // Always update main article fields with AI-generated data
                if (!string.IsNullOrEmpty(seoData.ArticleTitle))
                {
                    article.Name = seoData.ArticleTitle;
                    _logger.LogInformation("Updated article title for article '{OriginalName}': '{NewTitle}'", article.Name, seoData.ArticleTitle);
                }

                if (!string.IsNullOrEmpty(seoData.ArticleDescription))
                {
                    article.Description = seoData.ArticleDescription;
                    _logger.LogInformation("Updated article description for article '{ArticleName}'", article.Name);
                }

                if (!string.IsNullOrEmpty(seoData.ArticleContent))
                {
                    article.ArticleContent = seoData.ArticleContent;
                    _logger.LogInformation("Updated article content for article '{ArticleName}'", article.Name);
                }

                // Always update with AI-generated SEO data
                if (!string.IsNullOrEmpty(seoData.Keywords))
                {
                    article.Keywords = seoData.Keywords;
                    _logger.LogInformation("Updated keywords for article '{ArticleName}': {Keywords}", article.Name, seoData.Keywords);
                }

                // Always update SEO title with AI-generated data
                if (!string.IsNullOrEmpty(seoData.SeoTitle))
                {
                    article.Seo!.Title = seoData.SeoTitle;
                    _logger.LogInformation("Updated SEO title for article '{ArticleName}': '{SeoTitle}' ({Length} chars)", article.Name, seoData.SeoTitle, seoData.SeoTitle.Length);
                }

                // Always update meta description with AI-generated data
                if (!string.IsNullOrEmpty(seoData.MetaDescription))
                {
                    article.Seo!.Description = seoData.MetaDescription;
                }

                // Always update Open Graph title with AI-generated data
                if (!string.IsNullOrEmpty(seoData.OgTitle))
                {
                    article.OpenGraph!.Title = seoData.OgTitle;
                }

                // Always update Open Graph description with AI-generated data
                if (!string.IsNullOrEmpty(seoData.OgDescription))
                {
                    article.OpenGraph!.Description = seoData.OgDescription;
                }

                // Always update Twitter description with AI-generated data
                if (!string.IsNullOrEmpty(seoData.TwitterDescription))
                {
                    article.TwitterCard!.Description = seoData.TwitterDescription;
                }

                // Always update Twitter title with AI-generated data
                if (!string.IsNullOrEmpty(seoData.TwitterTitle))
                {
                    article.TwitterCard!.Title = seoData.TwitterTitle;
                }

                // Update article content fields
                if (string.IsNullOrEmpty(article.Subtitle) && !string.IsNullOrEmpty(seoData.Subtitle))
                {
                    article.Subtitle = seoData.Subtitle;
                }

                if (string.IsNullOrEmpty(article.Summary) && !string.IsNullOrEmpty(seoData.Summary))
                {
                    article.Summary = seoData.Summary;
                }

                // Update conclusion section fields
                if (string.IsNullOrEmpty(article.ConclusionTitle) && !string.IsNullOrEmpty(seoData.ConclusionTitle))
                {
                    article.ConclusionTitle = seoData.ConclusionTitle;
                }

                if (string.IsNullOrEmpty(article.ConclusionSummary) && !string.IsNullOrEmpty(seoData.ConclusionSummary))
                {
                    article.ConclusionSummary = seoData.ConclusionSummary;
                }

                if (string.IsNullOrEmpty(article.ConclusionKeyHeading) && !string.IsNullOrEmpty(seoData.ConclusionKeyHeading))
                {
                    article.ConclusionKeyHeading = seoData.ConclusionKeyHeading;
                }

                if (string.IsNullOrEmpty(article.ConclusionKeyText) && !string.IsNullOrEmpty(seoData.ConclusionKeyText))
                {
                    article.ConclusionKeyText = seoData.ConclusionKeyText;
                }

                if (string.IsNullOrEmpty(article.ConclusionText) && !string.IsNullOrEmpty(seoData.ConclusionText))
                {
                    article.ConclusionText = seoData.ConclusionText;
                }
            }

            // Auto-generate canonical URL
            if (string.IsNullOrEmpty(article.Seo!.Canonical))
            {
                article.Seo.Canonical = $"https://markhazleton.com/{article.Slug}";
            }

            // Auto-generate Open Graph image alt text
            if (string.IsNullOrEmpty(article.OpenGraph!.ImageAlt))
            {
                article.OpenGraph.ImageAlt = $"{article.Name} - Mark Hazleton";
            }

            // Auto-generate Twitter image alt text
            if (string.IsNullOrEmpty(article.TwitterCard!.ImageAlt))
            {
                article.TwitterCard.ImageAlt = article.OpenGraph.ImageAlt;
            }
        }

        /// <summary>
        /// Gets SEO statistics for all articles
        /// </summary>
        /// <returns>Dictionary with SEO statistics</returns>
        public Dictionary<string, object> GetSeoStatistics()
        {
            var articles = GetArticles();
            var stats = new Dictionary<string, object>
            {
                ["TotalArticles"] = articles.Count,
                ["ArticlesWithSeo"] = articles.Count(a => a.Seo != null),
                ["ArticlesWithOpenGraph"] = articles.Count(a => a.OpenGraph != null),
                ["ArticlesWithTwitterCard"] = articles.Count(a => a.TwitterCard != null),
                ["TitleIssues"] = articles.Count(a => string.IsNullOrEmpty(a.EffectiveTitle) ||
                                                     a.EffectiveTitle.Length < 30 ||
                                                     a.EffectiveTitle.Length > 60),
                ["DescriptionIssues"] = articles.Count(a => string.IsNullOrEmpty(a.EffectiveDescription) ||
                                                            a.EffectiveDescription.Length < 120 ||
                                                            a.EffectiveDescription.Length > 160),
                ["MissingImages"] = articles.Count(a => string.IsNullOrEmpty(a.ImgSrc)),
                ["CompleteSeoArticles"] = articles.Count(a => a.Seo != null && a.OpenGraph != null && a.TwitterCard != null)
            };

            return stats;
        }

        /// <summary>
        /// Prepares content for SEO analysis by combining article metadata with PUG file content
        /// </summary>
        /// <param name="article">The article to prepare content for</param>
        /// <returns>Combined content including PUG file structure and article metadata</returns>
        private async Task<string> PrepareContentForSeoAnalysis(ArticleModel article)
        {
            var contentParts = new List<string>();

            // Add basic article metadata
            if (!string.IsNullOrEmpty(article.Name))
            {
                contentParts.Add($"Article Title: {article.Name}");
            }

            if (!string.IsNullOrEmpty(article.Description))
            {
                contentParts.Add($"Current Description: {article.Description}");
            }

            if (!string.IsNullOrEmpty(article.Section))
            {
                contentParts.Add($"Category/Section: {article.Section}");
            }

            // Try to read PUG file content for additional context
            try
            {
                string pugContent = await ReadPugFileContent(article);
                if (!string.IsNullOrEmpty(pugContent))
                {
                    contentParts.Add("PUG File Content Structure:");
                    contentParts.Add(pugContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read PUG file for article: {ArticleName}", article.Name);
            }

            // Add existing article content if available
            if (!string.IsNullOrEmpty(article.ArticleContent))
            {
                contentParts.Add("Article Content:");
                contentParts.Add(article.ArticleContent);
            }

            return string.Join("\n\n", contentParts);
        }

        /// <summary>
        /// Reads and extracts meaningful content from a PUG file for SEO analysis
        /// </summary>
        /// <param name="article">The article whose PUG file to read</param>
        /// <returns>Extracted and cleaned content from the PUG file</returns>
        private async Task<string> ReadPugFileContent(ArticleModel article)
        {
            try
            {
                // Calculate the PUG file path
                string pugFilePath = string.Empty;

                if (!string.IsNullOrEmpty(article.Source))
                {
                    // Use the source path if available (remove /src/pug/ prefix and convert to actual file path)
                    string relativePath = article.Source.Replace("/src/pug/", "").Replace("/", "\\");
                    pugFilePath = Path.Combine(_filePath.Replace("articles.json", ""), "pug", relativePath);
                }
                else if (!string.IsNullOrEmpty(article.Slug))
                {
                    // Fallback: derive from slug
                    string fileName = article.Slug.Replace(".html", ".pug").Replace("articles/", "");
                    pugFilePath = Path.Combine(_articlesDirectory, fileName);
                }

                if (string.IsNullOrEmpty(pugFilePath) || !File.Exists(pugFilePath))
                {
                    _logger.LogDebug("PUG file not found for article: {ArticleName} at path: {PugFilePath}", article.Name, pugFilePath);
                    return string.Empty;
                }

                // Read the PUG file
                string pugContent = await File.ReadAllTextAsync(pugFilePath);

                // Extract meaningful content from PUG file
                return ExtractContentFromPug(pugContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading PUG file for article: {ArticleName}", article.Name);
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts meaningful text content from PUG markup for SEO analysis
        /// </summary>
        /// <param name="pugContent">Raw PUG file content</param>
        /// <returns>Cleaned text content suitable for AI analysis</returns>
        private static string ExtractContentFromPug(string pugContent)
        {
            if (string.IsNullOrEmpty(pugContent))
                return string.Empty;

            var lines = pugContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var contentLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comments, extends, block declarations, and other PUG syntax
                if (trimmedLine.StartsWith("//") ||
                    trimmedLine.StartsWith("extends ") ||
                    trimmedLine.StartsWith("block ") ||
                    trimmedLine.StartsWith("include ") ||
                    trimmedLine.StartsWith("mixin ") ||
                    trimmedLine.StartsWith("if ") ||
                    trimmedLine.StartsWith("each ") ||
                    trimmedLine.StartsWith("case ") ||
                    trimmedLine.StartsWith("when ") ||
                    trimmedLine.StartsWith("unless ") ||
                    string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                // Extract text content (lines starting with | or plain text)
                if (trimmedLine.StartsWith("| "))
                {
                    contentLines.Add(trimmedLine.Substring(2).Trim());
                }
                // Extract text that might be after HTML elements (simple heuristic)
                else if (trimmedLine.Contains(" ") && !trimmedLine.StartsWith(".") && !trimmedLine.StartsWith("#"))
                {
                    // Look for text after the last space that might be content
                    var lastSpaceIndex = trimmedLine.LastIndexOf(' ');
                    if (lastSpaceIndex > 0 && lastSpaceIndex < trimmedLine.Length - 1)
                    {
                        var potentialContent = trimmedLine.Substring(lastSpaceIndex + 1).Trim();
                        if (potentialContent.Length > 3 && !potentialContent.StartsWith(".") && !potentialContent.StartsWith("#"))
                        {
                            contentLines.Add(potentialContent);
                        }
                    }
                }
                // Extract headings and other meaningful structure indicators
                else if (trimmedLine.Contains("h1.") || trimmedLine.Contains("h2.") || trimmedLine.Contains("h3.") ||
                         trimmedLine.Contains("p.") || trimmedLine.Contains("article") || trimmedLine.Contains("section"))
                {
                    contentLines.Add($"[Structure: {trimmedLine}]");
                }
            }

            return string.Join("\n", contentLines);
        }

        /// <summary>
        /// Extracts the YouTube video ID from various YouTube URL formats.
        /// </summary>
        /// <param name="url">The YouTube URL.</param>
        /// <returns>The video ID if found, otherwise empty string.</returns>
        private string ExtractYouTubeVideoId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                // Handle different YouTube URL formats
                var uri = new Uri(url);

                // Standard YouTube URL: https://www.youtube.com/watch?v=VIDEO_ID
                if (uri.Host.Contains("youtube.com"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    return query["v"] ?? string.Empty;
                }

                // Short YouTube URL: https://youtu.be/VIDEO_ID
                if (uri.Host.Contains("youtu.be"))
                {
                    return uri.Segments.LastOrDefault()?.TrimStart('/') ?? string.Empty;
                }

                // Embed URL: https://www.youtube.com/embed/VIDEO_ID
                if (uri.AbsolutePath.StartsWith("/embed/"))
                {
                    return uri.Segments.LastOrDefault()?.TrimStart('/') ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract YouTube video ID from URL: {Url}", url);
                return string.Empty;
            }
        }
    }
}
