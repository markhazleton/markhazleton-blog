using System.Text.Json;
using System.Diagnostics;
using mwhWebAdmin.Article.Models;
using mwhWebAdmin.Article.Services;
using mwhWebAdmin.Configuration;

namespace mwhWebAdmin.Article;

/// <summary>
/// Helper methods for ArticleService - Multi-step AI content generation
/// </summary>
public partial class ArticleService
{
    private AiResponseLogger? _aiLogger;

    /// <summary>
    /// Sets the AI response logger (called by dependency injection or manually)
    /// </summary>
    public void SetAiResponseLogger(AiResponseLogger aiLogger)
    {
   _aiLogger = aiLogger;
    _logger.LogInformation("AI Response Logger configured for ArticleService");
    }

    /// <summary>
    /// Step 1: Generates comprehensive article content in markdown format
    /// </summary>
    /// <param name="title">The article title</param>
    /// <param name="description">The article description</param>
    /// <param name="section">The article section/category</param>
    /// <returns>Comprehensive markdown article content (10,000-15,000 characters)</returns>
    private async Task<string> GenerateArticleContentAsync(string title, string description, string section)
    {
        var stopwatch = Stopwatch.StartNew();
    try
        {
   _logger.LogInformation("=== STEP 1: Generating Article Content ===");
      _logger.LogInformation("Title: {Title}, Section: {Section}", title, section);

        var openAiApiKey = _configuration["OPENAI_API_KEY"];
       var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

            using var httpClient = _httpClientFactory.CreateClient();
      httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
    httpClient.Timeout = TimeSpan.FromMinutes(5);

          var systemPrompt = $@"You are an expert technical writer. Generate a comprehensive, well-structured article in MARKDOWN format.

Title: {title}
Description: {description}
Category: {section}

Requirements:
- 10,000-15,000 characters
- Use proper markdown syntax (# headers, ## subheaders, - lists, ```code blocks```, [links](url), **bold**, *italic*)
- Include code examples if relevant to the topic
- Multiple sections with clear headers
- Lists, tables, and examples where appropriate
- Professional, authoritative tone
- SEO-friendly content with natural keyword usage
- Actionable insights and practical advice
- Well-organized with logical flow

Format the entire response as pure markdown without any JSON wrapping.";

      var model = _configuration["OpenAI:Model"] ?? "gpt-5";
    var maxTokens = int.Parse(_configuration["OpenAI:MaxTokensArticle"] ?? "16000");
    bool isGpt5 = model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase);

            object requestBody;
            if (isGpt5)
            {
       requestBody = new
 {
    model = model,
       messages = new[]
   {
         new { role = "system", content = systemPrompt },
           new { role = "user", content = $"Generate a comprehensive article about: {title}" }
           },
        max_completion_tokens = maxTokens
       };
         }
   else
   {
      requestBody = new
      {
  model = model,
         messages = new[]
           {
       new { role = "system", content = systemPrompt },
     new { role = "user", content = $"Generate a comprehensive article about: {title}" }
          },
          max_tokens = maxTokens,
      temperature = 0.3
            };
  }

       var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);

            stopwatch.Stop();

         if (!response.IsSuccessStatusCode)
            {
    var errorContent = await response.Content.ReadAsStringAsync();
   _logger.LogError("OpenAI API error in Step 1: {StatusCode} - {Error}", response.StatusCode, errorContent);
   
                // Log error
       if (_aiLogger != null)
  {
           await _aiLogger.LogErrorAsync(0, title, 1, "ContentGeneration", 
    new Exception($"API Error: {response.StatusCode}"), requestBody);
          }
                
    return string.Empty;
            }

   response.EnsureSuccessStatusCode();
          var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(responseContent);

      var articleContent = responseData.RootElement
     .GetProperty("choices")[0]
              .GetProperty("message")
     .GetProperty("content")
    .GetString() ?? string.Empty;

          _logger.LogInformation("Step 1 Complete: Generated {Length} characters of content", articleContent.Length);
            
         // Log AI interaction
            if (_aiLogger != null)
            {
     await _aiLogger.LogAiInteractionAsync(
    0, title, 1, "ContentGeneration", 
         requestBody, articleContent, stopwatch.ElapsedMilliseconds);
            }
      
return articleContent;
        }
     catch (Exception ex)
        {
            stopwatch.Stop();
         _logger.LogError(ex, "Failed to generate article content in Step 1");
       
    // Log error
    if (_aiLogger != null)
         {
                await _aiLogger.LogErrorAsync(0, title, 1, "ContentGeneration", ex);
}
            
            return string.Empty;
   }
    }

    /// <summary>
    /// Step 2: Extracts SEO metadata from article content
    /// </summary>
    /// <param name="articleContent">The full article content</param>
    /// <param name="title">The original article title</param>
    /// <returns>SEO metadata including title, subtitle, description, and keywords</returns>
    private async Task<SeoMetadataResult> ExtractSeoMetadataAsync(string articleContent, string title)
    {
        var stopwatch = Stopwatch.StartNew();
    try
 {
  _logger.LogInformation("=== STEP 2: Extracting SEO Metadata ===");

var openAiApiKey = _configuration["OPENAI_API_KEY"];
     var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

    using var httpClient = _httpClientFactory.CreateClient();
     httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
 httpClient.Timeout = TimeSpan.FromMinutes(2);

   var systemPrompt = $@"You are an SEO expert. Analyze this article and extract optimized metadata.

Article Content:
{articleContent.Substring(0, Math.Min(8000, articleContent.Length))}

Generate:
1. SEO-optimized title ({SeoValidationConfig.Title.MinLength}-{SeoValidationConfig.Title.MaxLength} chars)
2. Engaging subtitle
3. Meta description ({SeoValidationConfig.MetaDescription.MinLength}-{SeoValidationConfig.MetaDescription.MaxLength} chars with action words like 'discover', 'learn', 'explore')
4. {SeoValidationConfig.Keywords.MinCount}-{SeoValidationConfig.Keywords.MaxCount} relevant keywords (comma-separated)";

            var responseFormat = new
   {
             type = "json_schema",
    json_schema = new
   {
      name = "seo_metadata_result",
  strict = false,  // Changed from true to allow partial results
  schema = new
       {
    type = "object",
  properties = new
   {
       articleTitle = new { type = "string" },
 subtitle = new { type = "string" },
        description = new { type = "string" },
  keywords = new { type = "string" },
         seoTitle = new { type = "string" },
metaDescription = new { type = "string" }
},
     required = new[] { "articleTitle", "subtitle", "description", "keywords", "seoTitle", "metaDescription" },
additionalProperties = false
   }
      }
    };

       var model = _configuration["OpenAI:Model"] ?? "gpt-5";
    bool isGpt5 = model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase);

   object requestBody;
    if (isGpt5)
 {
     requestBody = new
     {
         model = model,
  messages = new[]
     {
     new { role = "system", content = systemPrompt },
   new { role = "user", content = "Extract SEO metadata from the article content provided." }
      },
         max_completion_tokens = 2000,
  response_format = responseFormat
    };
   }
    else
     {
   requestBody = new
   {
   model = model,
  messages = new[]
      {
           new { role = "system", content = systemPrompt },
 new { role = "user", content = "Extract SEO metadata from the article content provided." }
     },
          max_tokens = 2000,
      temperature = 0.3,
      response_format = responseFormat
      };
        }

      var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
     var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);

         stopwatch.Stop();

          if (!response.IsSuccessStatusCode)
          {
           var errorContent = await response.Content.ReadAsStringAsync();
    _logger.LogError("OpenAI API error in Step 2: {StatusCode} - {Error}", response.StatusCode, errorContent);
           
    // Log error
       if (_aiLogger != null)
      {
          await _aiLogger.LogErrorAsync(0, title, 2, "SeoMetadataExtraction", 
        new Exception($"API Error: {response.StatusCode}"), requestBody);
                }
      
 return new SeoMetadataResult();
      }

        response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            
   // Log the raw response for debugging
  _logger.LogDebug("Step 2 Raw API Response: {ResponseContent}", responseContent);
 
 var responseData = JsonDocument.Parse(responseContent);

var aiResponse = responseData.RootElement
    .GetProperty("choices")[0]
   .GetProperty("message")
          .GetProperty("content")
        .GetString();

       // Check if response is empty or null
     if (string.IsNullOrWhiteSpace(aiResponse))
{
      _logger.LogWarning("Step 2: OpenAI returned empty response. Full API response: {Response}", responseContent);
     
       // Log the empty response
                if (_aiLogger != null)
    {
    await _aiLogger.LogErrorAsync(0, title, 2, "SeoMetadataExtraction",
    new Exception("OpenAI returned empty content in response"), requestBody);
           }
        
   return new SeoMetadataResult();
          }

   _logger.LogDebug("Step 2 AI Response: {AiResponse}", aiResponse);

  var seoMetadata = JsonSerializer.Deserialize<SeoMetadataResult>(aiResponse ?? "{}") ?? new SeoMetadataResult();
     
            _logger.LogInformation("Step 2 Complete: Extracted SEO metadata");
 
  // Log AI interaction
  if (_aiLogger != null)
       {
      await _aiLogger.LogAiInteractionAsync(
        0, title, 2, "SeoMetadataExtraction", 
     requestBody, aiResponse ?? string.Empty, stopwatch.ElapsedMilliseconds);
     }
      
 return seoMetadata;
    }
   catch (Exception ex)
        {
        stopwatch.Stop();
  _logger.LogError(ex, "Failed to extract SEO metadata in Step 2");
            
         // Log error
        if (_aiLogger != null)
       {
     await _aiLogger.LogErrorAsync(0, title, 2, "SeoMetadataExtraction", ex);
          }
            
          return new SeoMetadataResult();
      }
    }

    /// <summary>
    /// Step 3: Generates social media fields (Open Graph and Twitter Card)
    /// </summary>
    /// <param name="articleContent">The full article content</param>
    /// <param name="title">The article title</param>
 /// <param name="description">The article description</param>
 /// <returns>Social media metadata for Open Graph and Twitter</returns>
    private async Task<SocialMediaResult> GenerateSocialMediaFieldsAsync(string articleContent, string title, string description)
    {
        var stopwatch = Stopwatch.StartNew();
        try
{
       _logger.LogInformation("=== STEP 3: Generating Social Media Fields ===");

     var openAiApiKey = _configuration["OPENAI_API_KEY"];
  var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

    using var httpClient = _httpClientFactory.CreateClient();
 httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
 httpClient.Timeout = TimeSpan.FromMinutes(2);

   var systemPrompt = $@"You are a social media marketing expert. Create platform-specific sharing content.

Article: {title}
Description: {description}
Content Summary: {articleContent.Substring(0, Math.Min(2000, articleContent.Length))}

Generate:
1. Open Graph title ({SeoValidationConfig.OpenGraphTitle.MaxLength} chars max)
2. Open Graph description ({SeoValidationConfig.OpenGraphDescription.MaxLength} chars max)
3. Twitter title ({SeoValidationConfig.TwitterTitle.MaxLength} chars max)
4. Twitter description ({SeoValidationConfig.TwitterDescription.MaxLength} chars max)

Make titles and descriptions compelling for social media sharing.";

      var responseFormat = new
     {
     type = "json_schema",
      json_schema = new
   {
     name = "social_media_result",
   strict = false,  // Changed from true to allow partial results
     schema = new
        {
  type = "object",
        properties = new
{
     ogTitle = new { type = "string" },
      ogDescription = new { type = "string" },
       twitterTitle = new { type = "string" },
        twitterDescription = new { type = "string" }
},
  required = new[] { "ogTitle", "ogDescription", "twitterTitle", "twitterDescription" },
      additionalProperties = false
     }
       }
};

          var model = _configuration["OpenAI:Model"] ?? "gpt-5";
    bool isGpt5 = model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase);

    object requestBody;
if (isGpt5)
            {
  requestBody = new
                {
   model = model,
       messages = new[]
    {
       new { role = "system", content = systemPrompt },
  new { role = "user", content = "Generate social media sharing content." }
  },
 max_completion_tokens = 1000,
        response_format = responseFormat
    };
 }
            else
     {
     requestBody = new
      {
   model = model,
      messages = new[]
      {
   new { role = "system", content = systemPrompt },
 new { role = "user", content = "Generate social media sharing content." }
     },
   max_tokens = 1000,
    temperature = 0.3,
          response_format = responseFormat
  };
    }

var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
  var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);

            stopwatch.Stop();

  if (!response.IsSuccessStatusCode)
     {
        var errorContent = await response.Content.ReadAsStringAsync();
       _logger.LogError("OpenAI API error in Step 3: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
 // Log error
     if (_aiLogger != null)
{
          await _aiLogger.LogErrorAsync(0, title, 3, "SocialMediaGeneration", 
     new Exception($"API Error: {response.StatusCode}"), requestBody);
      }
     
        return new SocialMediaResult();
          }

 response.EnsureSuccessStatusCode();
    var responseContent = await response.Content.ReadAsStringAsync();
       
   // Log the raw response for debugging
  _logger.LogDebug("Step 3 Raw API Response: {ResponseContent}", responseContent);
    
      var responseData = JsonDocument.Parse(responseContent);

 var aiResponse = responseData.RootElement
 .GetProperty("choices")[0]
     .GetProperty("message")
    .GetProperty("content")
       .GetString();

  // Check if response is empty or null
if (string.IsNullOrWhiteSpace(aiResponse))
        {
  _logger.LogWarning("Step 3: OpenAI returned empty response. Full API response: {Response}", responseContent);
      
 // Log the empty response
      if (_aiLogger != null)
      {
     await _aiLogger.LogErrorAsync(0, title, 3, "SocialMediaGeneration",
    new Exception("OpenAI returned empty content in response"), requestBody);
    }
 
   return new SocialMediaResult();
 }

      _logger.LogDebug("Step 3 AI Response: {AiResponse}", aiResponse);

var socialMedia = JsonSerializer.Deserialize<SocialMediaResult>(aiResponse ?? "{}") ?? new SocialMediaResult();
      
  _logger.LogInformation("Step 3 Complete: Generated social media fields");
  
// Log AI interaction
            if (_aiLogger != null)
   {
       await _aiLogger.LogAiInteractionAsync(
        0, title, 3, "SocialMediaGeneration", 
    requestBody, aiResponse ?? string.Empty, stopwatch.ElapsedMilliseconds);
            }
    
     return socialMedia;
        }
        catch (Exception ex)
  {
            stopwatch.Stop();
      _logger.LogError(ex, "Failed to generate social media fields in Step 3");
        
            // Log error
if (_aiLogger != null)
      {
          await _aiLogger.LogErrorAsync(0, title, 3, "SocialMediaGeneration", ex);
            }
         
        return new SocialMediaResult();
        }
    }

 /// <summary>
    /// Step 4: Generates conclusion section from full article content
    /// </summary>
    /// <param name="articleContent">The full article content</param>
/// <returns>Conclusion section with title, summary, key takeaways, and closing text</returns>
    private async Task<ConclusionResult> GenerateConclusionSectionAsync(string articleContent)
    {
        var stopwatch = Stopwatch.StartNew();
        try
      {
  _logger.LogInformation("=== STEP 4: Creating Conclusion Section ===");

            var openAiApiKey = _configuration["OPENAI_API_KEY"];
   var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

      using var httpClient = _httpClientFactory.CreateClient();
         httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
    httpClient.Timeout = TimeSpan.FromMinutes(2);

      var systemPrompt = $@"You are a content editor. Create an impactful conclusion section.

Article Content:
{articleContent.Substring(0, Math.Min(8000, articleContent.Length))}

Generate:
1. Conclusion heading (compelling, summarizes article)
2. 2-3 sentence summary of main points
3. Key takeaway heading (short, impactful)
4. Key takeaway text (1-2 sentences with key insight)
5. Call-to-action closing (encourage next steps)";

  var responseFormat = new
  {
         type = "json_schema",
     json_schema = new
        {
  name = "conclusion_result",
        strict = true,
        schema = new
     {
         type = "object",
       properties = new
            {
   conclusionTitle = new { type = "string" },
     conclusionSummary = new { type = "string" },
        conclusionKeyHeading = new { type = "string" },
             conclusionKeyText = new { type = "string" },
      conclusionText = new { type = "string" }
 },
 required = new[] { "conclusionTitle", "conclusionSummary", "conclusionKeyHeading", "conclusionKeyText", "conclusionText" },
       additionalProperties = false
   }
           }
  };

            var model = _configuration["OpenAI:Model"] ?? "gpt-5";
 bool isGpt5 = model.Contains("gpt-5", StringComparison.OrdinalIgnoreCase);

  object requestBody;
  if (isGpt5)
      {
   requestBody = new
         {
            model = model,
           messages = new[]
   {
     new { role = "system", content = systemPrompt },
     new { role = "user", content = "Create a conclusion section for this article." }
     },
    max_completion_tokens = 1500,
         response_format = responseFormat
          };
        }
       else
        {
   requestBody = new
          {
model = model,
     messages = new[]
       {
   new { role = "system", content = systemPrompt },
        new { role = "user", content = "Create a conclusion section for this article." }
   },
      max_tokens = 1500,
  temperature = 0.3,
 response_format = responseFormat
   };
      }

var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);

      stopwatch.Stop();

     if (!response.IsSuccessStatusCode)
       {
         var errorContent = await response.Content.ReadAsStringAsync();
         _logger.LogError("OpenAI API error in Step 4: {StatusCode} - {Error}", response.StatusCode, errorContent);
         
            // Log error
             if (_aiLogger != null)
   {
             await _aiLogger.LogErrorAsync(0, "article", 4, "ConclusionGeneration", 
     new Exception($"API Error: {response.StatusCode}"), requestBody);
    }
       
    return new ConclusionResult();
     }

      response.EnsureSuccessStatusCode();
      var responseContent = await response.Content.ReadAsStringAsync();
          var responseData = JsonDocument.Parse(responseContent);

        var aiResponse = responseData.RootElement
      .GetProperty("choices")[0]
    .GetProperty("message")
     .GetProperty("content")
.GetString();

            var conclusion = JsonSerializer.Deserialize<ConclusionResult>(aiResponse ?? "{}") ?? new ConclusionResult();
   
     _logger.LogInformation("Step 4 Complete: Generated conclusion section");
            
         // Log AI interaction
       if (_aiLogger != null)
        {
                await _aiLogger.LogAiInteractionAsync(
  0, "article", 4, "ConclusionGeneration", 
                requestBody, aiResponse ?? string.Empty, stopwatch.ElapsedMilliseconds);
      }
            
        return conclusion;
      }
        catch (Exception ex)
        {
         stopwatch.Stop();
   _logger.LogError(ex, "Failed to generate conclusion section in Step 4");
   
 // Log error
 if (_aiLogger != null)
         {
        await _aiLogger.LogErrorAsync(0, "article", 4, "ConclusionGeneration", ex);
 }
    
  return new ConclusionResult();
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
    /// Auto-generates SEO fields (synchronous wrapper for async method)
    /// </summary>
    /// <param name="article">The article to enhance</param>
    public void AutoGenerateSeoFields(ArticleModel article)
    {
 AutoGenerateSeoFieldsAsync(article).GetAwaiter().GetResult();
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
    ["CompleteSeoArticles"] = articles.Count(a => a.Seo != null && a.OpenGraph != null && a.TwitterCard != null)
        };

        return stats;
    }
}
