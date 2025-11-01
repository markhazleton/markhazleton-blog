using HtmlAgilityPack;
using System.Globalization;
using mwhWebAdmin.Configuration;
using mwhWebAdmin.Services;

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
        private readonly ArticleContentService _contentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleService"/> class.
        /// </summary>
        /// <param name="filePath">The file path of the articles JSON file.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="contentService">The article content service for managing external content files.</param>
        public ArticleService(string filePath, ILogger<ArticleService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ArticleContentService contentService)
        {
            _filePath = filePath;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _contentService = contentService;
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
                _logger.LogInformation("[ArticleService] GenerateSeoDataFromContentAsync called with content length: {ContentLength} characters", content?.Length);
                _logger.LogInformation("[ArticleService] Current title: {CurrentTitle}", currentTitle ?? "None");

                var openAiApiKey = _configuration["OPENAI_API_KEY"];
                var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

                Console.WriteLine($"[ArticleService] OpenAI API Key present: {!string.IsNullOrEmpty(openAiApiKey)}");
                Console.WriteLine($"[ArticleService] OpenAI API URL: {openAiApiUrl}");

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);

                Console.WriteLine($"[ArticleService] HTTP client created and authorization header set");

                var systemPrompt = SeoLlmPromptConfig.GetContentGenerationPrompt(
                    currentTitle ?? "Article",
                    content ?? "");

                Console.WriteLine($"[ArticleService] System prompt generated, length: {systemPrompt.Length} characters");

                var userContent = string.IsNullOrEmpty(currentTitle)
                    ? content ?? ""
                    : $"Current Title: {currentTitle}\n\nContent: {content ?? ""}";

                Console.WriteLine($"[ArticleService] User content prepared, length: {userContent.Length} characters");

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
                                keywords = new { type = "string", description = $"Comma-separated list of {SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} SEO keywords" },
                                seoTitle = new { type = "string", description = $"SEO-optimized title (ABSOLUTE MINIMUM {SeoValidationConfig.Title.MinLength} characters, MAXIMUM {SeoValidationConfig.Title.MaxLength} characters)" },
                                metaDescription = new { type = "string", description = $"Meta description (ABSOLUTE MINIMUM {SeoValidationConfig.MetaDescription.MinLength} characters, MAXIMUM {SeoValidationConfig.MetaDescription.MaxLength} characters), MUST include action words like 'discover', 'learn', 'explore'. Count characters carefully before submitting." },
                                ogTitle = new { type = "string", description = $"Open Graph title - use articleTitle or slight variation ({SeoValidationConfig.OpenGraphTitle.MinLength}-{SeoValidationConfig.OpenGraphTitle.MaxLength} characters)" },
                                ogDescription = new { type = "string", description = $"Open Graph description ({SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} characters)" },
                                twitterTitle = new { type = "string", description = $"Twitter title - shortened version of articleTitle (up to {SeoValidationConfig.TwitterTitle.MaxLength} characters)" },
                                twitterDescription = new { type = "string", description = $"Twitter description ({SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} characters)" },
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

                _logger.LogInformation("[ArticleService] Request body prepared, making API call to OpenAI...");
                _logger.LogInformation("[ArticleService] *** STARTING LLM API CALL ***");

                // Make the API call
                var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);

                _logger.LogInformation("[ArticleService] *** LLM API CALL COMPLETED ***");
                _logger.LogInformation("[ArticleService] OpenAI API response received. Status: {StatusCode}", response.StatusCode);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ArticleService] Response content received, length: {responseContent.Length} characters");

                var responseData = JsonDocument.Parse(responseContent);

                // Extract the response text
                var aiResponse = responseData.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                Console.WriteLine($"[ArticleService] AI response extracted, length: {aiResponse?.Length} characters");

                if (string.IsNullOrEmpty(aiResponse))
                {
                    Console.WriteLine($"[ArticleService] AI response is empty, returning empty result");
                    return new SeoGenerationResult();
                }

                // With structured outputs, the response should be valid JSON without markdown formatting
                try
                {
                    Console.WriteLine($"[ArticleService] Attempting to parse JSON response...");
                    var seoData = JsonSerializer.Deserialize<SeoGenerationResult>(aiResponse);
                    Console.WriteLine($"[ArticleService] JSON parsed successfully. SeoData is null: {seoData == null}");

                    if (seoData != null)
                    {
                        Console.WriteLine($"[ArticleService] Processing and validating SEO data...");
                        // Post-process and validate the SEO data to ensure it meets requirements
                        seoData = ValidateAndCorrectSeoData(seoData, currentTitle);
                        Console.WriteLine($"[ArticleService] SEO data validated and corrected");
                    }
                    return seoData ?? new SeoGenerationResult();
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[ArticleService] JSON parsing failed: {ex.Message}");
                    _logger.LogWarning(ex, "Failed to parse structured AI SEO response as JSON. Raw response: {Response}", aiResponse);

                    // Fallback: try to extract keywords from the response
                    Console.WriteLine($"[ArticleService] Using fallback - extracting keywords from response");
                    return new SeoGenerationResult
                    {
                        Keywords = aiResponse.Trim()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ArticleService] Exception in GenerateSeoDataFromContentAsync: {ex.Message}");
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
        /// Ensures the SEO title meets the {SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} character requirement and includes the brand suffix.
        /// </summary>
        /// <param name="title">The original title.</param>
        /// <param name="fallbackTitle">Fallback title to use if original is too short.</param>
        /// <returns>A corrected title that meets all requirements.</returns>
        private string EnsureSeoTitleMeetsRequirements(string title, string? fallbackTitle = null)
        {
            const string brandSuffix = "";
            var minLength = SeoValidationConfig.Title.MinLength;
            var maxLength = SeoValidationConfig.Title.MaxLength;

            // Use the title as-is since we're not adding any suffix
            var cleanTitle = title.Trim();

            // If the title is too short, try to use fallback or expand it
            if (cleanTitle.Length < minLength)
            {
                if (!string.IsNullOrEmpty(fallbackTitle))
                {
                    var cleanFallback = fallbackTitle.Trim();
                    if (cleanFallback.Length >= minLength)
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
                if (expandedTitle.Length >= minLength && expandedTitle.Length <= 60)
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
        /// Ensures the meta description meets the {SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} character requirement.
        /// </summary>
        /// <param name="description">The original description.</param>
        /// <returns>A corrected description that meets length requirements.</returns>
        private string EnsureMetaDescriptionMeetsRequirements(string description)
        {
            var minLength = SeoValidationConfig.MetaDescription.MinLength;
            var maxLength = SeoValidationConfig.MetaDescription.MaxLength;

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
        /// Ensures keywords meet the requirement of being {SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} keywords.
        /// </summary>
        /// <param name="keywords">The original keywords.</param>
        /// <returns>Corrected keywords that meet all requirements.</returns>
        private string EnsureKeywordsMeetRequirements(string keywords)
        {
            if (string.IsNullOrEmpty(keywords))
                return "solutions architect, project management, web development";

            var keywordList = keywords.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();

            // Ensure we have at least minimum keywords
            if (keywordList.Count < SeoValidationConfig.Keywords.MinCount)
            {
                var additionalKeywords = new[] { "solutions architect", "project management", "web development", "IT consulting", "technical leadership" };
                foreach (var keyword in additionalKeywords)
                {
                    if (keywordList.Count >= SeoValidationConfig.Keywords.MinCount) break;
                    if (!keywordList.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
                    {
                        keywordList.Add(keyword);
                    }
                }
            }

            // Ensure we don't exceed maximum keywords
            if (keywordList.Count > SeoValidationConfig.Keywords.MaxCount)
            {
                // Keep the first maximum allowed keywords
                keywordList = keywordList.Take(SeoValidationConfig.Keywords.MaxCount).ToList();
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
    
                // Load external content for articles that use it
                foreach (var article in _articles.OrderBy(o => o.Id))
                {
                    article.Id = _articles.IndexOf(article);

                    // Load content from external file if configured
                    if (!string.IsNullOrEmpty(article.ContentFile))
                    {
                        try
                        {
                            article.ArticleContent = _contentService.LoadContentAsync(article.ContentFile).GetAwaiter().GetResult();
                            _logger.LogDebug("Loaded external content for article: {ArticleName} from {ContentFile}", 
                                            article.Name, article.ContentFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to load external content for article: {ArticleName} from {ContentFile}", 
                                             article.Name, article.ContentFile);
                        }
                    }

                    // Calculate source file path if not already set
                    if (string.IsNullOrEmpty(article.Source))
                    {
                        article.Source = CalculateSourceFilePath(article.Slug);
                    }
                }
                _logger.LogInformation("Articles loaded successfully. Total: {Count}", _articles.Count);
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
                    var daysSinceStart = (DateTime.Now - startdate).TotalDays;
                    var changeFrequency = (int)(daysSinceStart / count);
                    
                    // Process each article
                    foreach (var article in _articles.OrderBy(o => o.Id))
                    {
                        var newModifiedDate = startdate.AddDays(changeFrequency * article.Id);
                        var daysSinceModified = (DateTime.Now - newModifiedDate).TotalDays;

                        article.LastModified = startdate.AddDays(changeFrequency * article.Id).ToString("yyyy-MM-dd");
                        article.ChangeFrequency = daysSinceModified < 60 ? "daily" : daysSinceModified < 120 ? "weekly" : "monthly";
                      
                      // Save content to external file if configured
                      if (!string.IsNullOrEmpty(article.ContentFile) && !string.IsNullOrEmpty(article.ArticleContent))
                      {
                          try
                          {
                               _contentService.SaveContentAsync(article.ContentFile, article.ArticleContent).GetAwaiter().GetResult();
                               _logger.LogDebug("Saved external content for article: {ArticleName} to {ContentFile}", 
                                               article.Name, article.ContentFile);
                          }
                          catch (Exception ex)
                          {
                             _logger.LogError(ex, "Failed to save external content for article: {ArticleName} to {ContentFile}", 
                                               article.Name, article.ContentFile);
                          }
                      }
                    }
          
 // Create a copy of articles for serialization with content removed for external files
                    var articlesToSerialize = _articles.Select(a => 
   {
          var clone = new ArticleModel
   {
         Id = a.Id,
      Section = a.Section,
      Slug = a.Slug,
      Name = a.Name,
          Description = a.Description,
     Keywords = a.Keywords,
    ImgSrc = a.ImgSrc,
       LastModified = a.LastModified,
   PublishedDate = a.PublishedDate,
                EstimatedReadTime = a.EstimatedReadTime,
        ChangeFrequency = a.ChangeFrequency,
      Source = a.Source,
        Subtitle = a.Subtitle,
              Author = a.Author,
     Summary = a.Summary,
        ConclusionTitle = a.ConclusionTitle,
     ConclusionSummary = a.ConclusionSummary,
   ConclusionKeyHeading = a.ConclusionKeyHeading,
            ConclusionKeyText = a.ConclusionKeyText,
              ConclusionText = a.ConclusionText,
    Seo = a.Seo,
      OpenGraph = a.OpenGraph,
                   TwitterCard = a.TwitterCard,
    YouTubeUrl = a.YouTubeUrl,
 YouTubeTitle = a.YouTubeTitle,
         ContentFile = a.ContentFile,
        // Do NOT include ArticleContent - it's in external file
         ArticleContent = string.IsNullOrEmpty(a.ContentFile) ? a.ArticleContent : null
                 };
        return clone;
        }).ToList();
      
             string jsonContent = JsonSerializer.Serialize(articlesToSerialize, jsonSerializerOptions);
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
      newArticle.Id = _articles.Count != 0 ? _articles.Max(article => article.Id) + 1 : 1;

       // Set up external content file
                newArticle.ContentFile = _contentService.GenerateContentFileName(newArticle.Slug);
     _logger.LogInformation("Generated content filename for article: {ContentFile}", newArticle.ContentFile);

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
            _logger.LogInformation("Article '{ArticleName}' added successfully with content file: {ContentFile}", 
             newArticle.Name, newArticle.ContentFile);
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
        /// Generates the Keywords property for an article by fetching the content from the <article> element
        /// (supports both id="post" for legacy layout and id="main-article" for modern layout)
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

                    // Parse the HTML to extract the content from <article> element (supports both id='post' and id='main-article')
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    // Try to find article with id='post' first (legacy layout), then id='main-article' (modern layout)
                    var sectionNode = htmlDoc.DocumentNode.SelectSingleNode("//article[@id='post']")
                                   ?? htmlDoc.DocumentNode.SelectSingleNode("//article[@id='main-article']");

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
                        _logger.LogWarning("No content found in <article> element (tried both id='post' and id='main-article') for article: {ArticleName}", article.Name);
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
                    writer.WriteElementString("title", "Solutions Architect Articles");
                    writer.WriteElementString("link", "https://markhazleton.com/");
                    writer.WriteElementString("description", "Latest articles from Solutions Architect blog.");
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
            // Calculate source file path if not already set or if slug has changed
            if (string.IsNullOrEmpty(updatedArticle.Source))
            {
                updatedArticle.Source = CalculateSourceFilePath(updatedArticle.Slug);
            }

       // Ensure content file is set
        if (string.IsNullOrEmpty(updatedArticle.ContentFile))
       {
     updatedArticle.ContentFile = _contentService.GenerateContentFileName(updatedArticle.Slug);
      _logger.LogInformation("Generated content filename for updated article: {ContentFile}", updatedArticle.ContentFile);
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
       _logger.LogInformation("Article '{ArticleName}' updated successfully with content file: {ContentFile}", 
         updatedArticle.Name, updatedArticle.ContentFile);
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
                ImageAlt = $"{article.Name} - Solutions Architect"
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
                article.OpenGraph.ImageAlt = $"{article.Name} - Solutions Architect";
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
            _logger.LogInformation("[ArticleService] AutoGenerateSeoFieldsAsync called for article: {ArticleName}", article.Name);

            // Initialize if needed
            InitializeSeoFields(article);
            _logger.LogInformation("[ArticleService] SEO fields initialized");

            // Prepare content for AI analysis
            string contentForAnalysis = await PrepareContentForSeoAnalysis(article);
            _logger.LogInformation("[ArticleService] Content prepared for analysis, length: {ContentLength} characters", contentForAnalysis?.Length);

            // Generate AI-powered SEO data if content is available
            if (!string.IsNullOrEmpty(contentForAnalysis))
            {
                _logger.LogInformation("[ArticleService] Calling GenerateSeoDataFromContentAsync...");
                _logger.LogInformation("[ArticleService] About to make LLM API call to OpenAI...");

                var seoData = await GenerateSeoDataFromContentAsync(contentForAnalysis, article.Name);

                _logger.LogInformation("[ArticleService] LLM API call completed successfully!");
                _logger.LogInformation("[ArticleService] SEO data generated successfully");

                _logger.LogInformation("AI generated SEO data for article '{ArticleName}': Title='{SeoTitle}' ({TitleLength} chars), Description='{MetaDescription}' ({DescriptionLength} chars)",
                    article.Name, seoData.SeoTitle, seoData.SeoTitle?.Length ?? 0, seoData.MetaDescription, seoData.MetaDescription?.Length ?? 0);

                // Always update main article fields with AI-generated data
                if (!string.IsNullOrEmpty(seoData.ArticleTitle))
                {
                    _logger.LogInformation("[ArticleService] Updating article title: {ArticleTitle}", seoData.ArticleTitle);
                    article.Name = seoData.ArticleTitle;
                    _logger.LogInformation("Updated article title for article '{OriginalName}': '{NewTitle}'", article.Name, seoData.ArticleTitle);
                }

                if (!string.IsNullOrEmpty(seoData.ArticleDescription))
                {
                    _logger.LogInformation("[ArticleService] Updating article description");
                    article.Description = seoData.ArticleDescription;
                    _logger.LogInformation("Updated article description for article '{ArticleName}'", article.Name);
                }

                if (!string.IsNullOrEmpty(seoData.ArticleContent))
                {
                    Console.WriteLine($"[ArticleService] Updating article content, length: {seoData.ArticleContent.Length} characters");
                    article.ArticleContent = seoData.ArticleContent;
                    _logger.LogInformation("Updated article content for article '{ArticleName}'", article.Name);
                }

                // Always update with AI-generated SEO data
                if (!string.IsNullOrEmpty(seoData.Keywords))
                {
                    Console.WriteLine($"[ArticleService] Updating keywords: {seoData.Keywords}");
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

                // Update article content fields (always update with AI data)
                if (!string.IsNullOrEmpty(seoData.Subtitle))
                {
                    Console.WriteLine($"[ArticleService] Updating subtitle: {seoData.Subtitle}");
                    article.Subtitle = seoData.Subtitle;
                }

                if (!string.IsNullOrEmpty(seoData.Summary))
                {
                    Console.WriteLine($"[ArticleService] Updating summary");
                    article.Summary = seoData.Summary;
                }

                // Update conclusion section fields (always update with AI data)
                if (!string.IsNullOrEmpty(seoData.ConclusionTitle))
                {
                    Console.WriteLine($"[ArticleService] Updating conclusion title: {seoData.ConclusionTitle}");
                    article.ConclusionTitle = seoData.ConclusionTitle;
                }

                if (!string.IsNullOrEmpty(seoData.ConclusionSummary))
                {
                    Console.WriteLine($"[ArticleService] Updating conclusion summary");
                    article.ConclusionSummary = seoData.ConclusionSummary;
                }

                if (!string.IsNullOrEmpty(seoData.ConclusionKeyHeading))
                {
                    Console.WriteLine($"[ArticleService] Updating conclusion key heading: {seoData.ConclusionKeyHeading}");
                    article.ConclusionKeyHeading = seoData.ConclusionKeyHeading;
                }

                if (!string.IsNullOrEmpty(seoData.ConclusionKeyText))
                {
                    Console.WriteLine($"[ArticleService] Updating conclusion key text");
                    article.ConclusionKeyText = seoData.ConclusionKeyText;
                }

                if (!string.IsNullOrEmpty(seoData.ConclusionText))
                {
                    Console.WriteLine($"[ArticleService] Updating conclusion text");
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
                article.OpenGraph.ImageAlt = $"{article.Name} - Solutions Architect";
            }

            // Auto-generate Twitter image alt text
            if (string.IsNullOrEmpty(article.TwitterCard!.ImageAlt))
            {
                article.TwitterCard.ImageAlt = article.OpenGraph.ImageAlt;
            }

            Console.WriteLine($"[ArticleService] AutoGenerateSeoFieldsAsync completed for article: {article.Name}");
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
                                                     a.EffectiveTitle.Length < SeoValidationConfig.Title.MinLength ||
                                                     a.EffectiveTitle.Length > SeoValidationConfig.Title.MaxLength),
                ["DescriptionIssues"] = articles.Count(a => string.IsNullOrEmpty(a.EffectiveDescription) ||
                                                            a.EffectiveDescription.Length < SeoValidationConfig.MetaDescription.MinLength ||
                                                            a.EffectiveDescription.Length > SeoValidationConfig.MetaDescription.MaxLength),
                ["MissingImages"] = articles.Count(a => string.IsNullOrEmpty(a.ImgSrc)),
                ["CompleteSeoArticles"] = articles.Count(a => a.Seo != null && a.OpenGraph != null && a.TwitterCard != null),
                // New file-based validation statistics
                ["PugFilesFound"] = articles.Count(a => GetPugFilePathForStats(a) != null),
                ["HtmlFilesFound"] = articles.Count(a => GetHtmlFilePathForStats(a) != null),
                ["FilesWithValidation"] = articles.Count(a => GetPugFilePathForStats(a) != null || GetHtmlFilePathForStats(a) != null)
            };

            return stats;
        }

        /// <summary>
        /// Gets the PUG file path for an article - helper method for statistics
        /// </summary>
        /// <param name="article">The article</param>
        /// <returns>PUG file path or null if not found</returns>
        private string? GetPugFilePathForStats(ArticleModel article)
        {
            try
            {
                var srcPath = _configuration["SrcPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src");

                if (!string.IsNullOrEmpty(article.Source))
                {
                    var relativePath = article.Source.Replace("/src/pug/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                    var fullPath = Path.Combine(srcPath, "pug", relativePath);
                    return File.Exists(fullPath) ? fullPath : null;
                }
                else if (!string.IsNullOrEmpty(article.Slug))
                {
                    var fileName = article.Slug.Replace(".html", ".pug").Replace("articles/", "");
                    var fullPath = Path.Combine(srcPath, "pug", "articles", fileName);
                    return File.Exists(fullPath) ? fullPath : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PUG file path for article: {ArticleName}", article.Name);
                return null;
            }
        }

        /// <summary>
        /// Gets the HTML file path for an article - helper method for statistics
        /// </summary>
        /// <param name="article">The article</param>
        /// <returns>HTML file path or null if not found</returns>
        private string? GetHtmlFilePathForStats(ArticleModel article)
        {
            try
            {
                var docsPath = _configuration["DocsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs");

                if (!string.IsNullOrEmpty(article.Slug))
                {
                    var htmlFileName = article.Slug;

                    // If the slug ends with /, check for index.html
                    if (htmlFileName.EndsWith("/"))
                    {
                        htmlFileName += "index.html";
                    }
                    else if (!htmlFileName.EndsWith(".html"))
                    {
                        htmlFileName += ".html";
                    }

                    var fullPath = Path.Combine(docsPath, htmlFileName);

                    // If file doesn't exist and slug doesn't end with /, try adding /index.html
                    if (!File.Exists(fullPath) && !article.Slug.EndsWith("/") && !article.Slug.EndsWith(".html"))
                    {
                        var indexPath = Path.Combine(docsPath, article.Slug, "index.html");
                        if (File.Exists(indexPath))
                        {
                            return indexPath;
                        }
                    }

                    return File.Exists(fullPath) ? fullPath : null;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting HTML file path for article: {ArticleName}", article.Name);
                return null;
            }
        }

        /// <summary>
        /// Extracts YouTube video ID from URL
        /// </summary>
        /// <param name="youtubeUrl">YouTube URL</param>
        /// <returns>Video ID or empty string if not found</returns>
        private string ExtractYouTubeVideoId(string youtubeUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(youtubeUrl))
                    return string.Empty;

                var uri = new Uri(youtubeUrl);
                var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);

                // Handle different YouTube URL formats
                if (uri.Host.Contains("youtube.com"))
                {
                    return queryParameters["v"] ?? string.Empty;
                }
                else if (uri.Host.Contains("youtu.be"))
                {
                    return uri.Segments.LastOrDefault()?.TrimStart('/') ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting YouTube video ID from URL: {YoutubeUrl}", youtubeUrl);
                return string.Empty;
            }
        }

        /// <summary>
        /// Prepares content for SEO analysis by combining article metadata with PUG file content
        /// </summary>
        /// <param name="article">The article to prepare content for</param>
        /// <returns>Combined content including PUG file structure and article metadata</returns>
        private async Task<string> PrepareContentForSeoAnalysis(ArticleModel article)
        {
            Console.WriteLine($"[ArticleService] PrepareContentForSeoAnalysis called for article: {article.Name}");

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

            Console.WriteLine($"[ArticleService] Added basic metadata to content parts");

            // Try to read PUG file content for additional context
            try
            {
                Console.WriteLine($"[ArticleService] Attempting to read PUG file content...");
                string pugContent = await ReadPugFileContent(article);
                if (!string.IsNullOrEmpty(pugContent))
                {
                    Console.WriteLine($"[ArticleService] PUG content read successfully, length: {pugContent.Length} characters");
                    contentParts.Add("PUG File Content Structure:");
                    contentParts.Add(pugContent);
                }
                else
                {
                    Console.WriteLine($"[ArticleService] PUG content is empty");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ArticleService] Error reading PUG file: {ex.Message}");
                _logger.LogWarning(ex, "Could not read PUG file for article: {ArticleName}", article.Name);
            }

            // Add existing article content if available
            if (!string.IsNullOrEmpty(article.ArticleContent))
            {
                Console.WriteLine($"[ArticleService] Adding existing article content, length: {article.ArticleContent.Length} characters");
                contentParts.Add("Existing Article Content:");
                contentParts.Add(article.ArticleContent);
            }

            var result = string.Join("\n\n", contentParts);
            Console.WriteLine($"[ArticleService] PrepareContentForSeoAnalysis completed, final content length: {result.Length} characters");
            return result;
        }

        /// <summary>
        /// Reads PUG file content for an article
        /// </summary>
        /// <param name="article">The article</param>
        /// <returns>PUG file content or empty string if not found</returns>
        private async Task<string> ReadPugFileContent(ArticleModel article)
        {
            try
            {
                Console.WriteLine($"[ArticleService] ReadPugFileContent called for article: {article.Name}");

                var srcPath = _configuration["SrcPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src");
                Console.WriteLine($"[ArticleService] Source path: {srcPath}");

                if (!string.IsNullOrEmpty(article.Source))
                {
                    Console.WriteLine($"[ArticleService] Using article.Source: {article.Source}");
                    var relativePath = article.Source.Replace("/src/pug/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                    var fullPath = Path.Combine(srcPath, "pug", relativePath);
                    Console.WriteLine($"[ArticleService] Full path from Source: {fullPath}");

                    if (File.Exists(fullPath))
                    {
                        Console.WriteLine($"[ArticleService] File exists, reading content...");
                        return await File.ReadAllTextAsync(fullPath);
                    }
                    else
                    {
                        Console.WriteLine($"[ArticleService] File does not exist: {fullPath}");
                    }
                }
                else if (!string.IsNullOrEmpty(article.Slug))
                {
                    Console.WriteLine($"[ArticleService] Using article.Slug: {article.Slug}");
                    var fileName = article.Slug.Replace(".html", ".pug").Replace("articles/", "");
                    var fullPath = Path.Combine(srcPath, "pug", "articles", fileName);
                    Console.WriteLine($"[ArticleService] Full path from Slug: {fullPath}");

                    if (File.Exists(fullPath))
                    {
                        Console.WriteLine($"[ArticleService] File exists, reading content...");
                        return await File.ReadAllTextAsync(fullPath);
                    }
                    else
                    {
                        Console.WriteLine($"[ArticleService] File does not exist: {fullPath}");
                    }
                }

                Console.WriteLine($"[ArticleService] No PUG file found, returning empty string");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ArticleService] Exception in ReadPugFileContent: {ex.Message}");
                _logger.LogError(ex, "Error reading PUG file for article: {ArticleName}", article.Name);
                return string.Empty;
            }
        }
    }
}
