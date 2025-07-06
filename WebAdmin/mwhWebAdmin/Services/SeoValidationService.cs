using mwhWebAdmin.Article;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace mwhWebAdmin.Services;

/// <summary>
/// Service for validating and scoring SEO metadata
/// </summary>
public class SeoValidationService
{
    private readonly ILogger<SeoValidationService> _logger;
    private readonly string _srcPath;
    private readonly string _docsPath;

    /// <summary>
    /// Initializes a new instance of the SeoValidationService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration to get file paths</param>
    public SeoValidationService(ILogger<SeoValidationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _srcPath = configuration["SrcPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src");
        _docsPath = configuration["DocsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs");
    }
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
        result.Score.TitleScore = Math.Min(titleResult.score, 100);
        result.Warnings.AddRange(titleResult.warnings);
        result.Errors.AddRange(titleResult.errors);

        // Validate description
        var descResult = ValidateDescription(article.EffectiveDescription);
        result.Score.DescriptionScore = Math.Min(descResult.score, 100);
        result.Warnings.AddRange(descResult.warnings);
        result.Errors.AddRange(descResult.errors);

        // Validate keywords
        var keywordResult = ValidateKeywords(article.EffectiveKeywords);
        result.Score.KeywordsScore = Math.Min(keywordResult.score, 100);
        result.Warnings.AddRange(keywordResult.warnings);
        result.Errors.AddRange(keywordResult.errors);

        // Validate images
        var imageResult = ValidateImages(article);
        result.Score.ImageScore = Math.Min(imageResult.score, 100);
        result.Warnings.AddRange(imageResult.warnings);
        result.Errors.AddRange(imageResult.errors);

        // Validate H1 tags
        var h1Result = ValidateH1Tags(article);
        result.Score.H1Score = Math.Min(h1Result.score, 100);
        result.Warnings.AddRange(h1Result.warnings);
        result.Errors.AddRange(h1Result.errors);

        // Validate content images alt text
        var contentImageResult = ValidateContentImages(article);
        result.Score.ContentImageScore = Math.Min(contentImageResult.score, 100);
        result.Warnings.AddRange(contentImageResult.warnings);
        result.Errors.AddRange(contentImageResult.errors);

        // Validate HTML SEO elements
        var htmlSeoResult = ValidateHtmlSeoElements(article);
        result.Score.HtmlSeoScore = Math.Min(htmlSeoResult.score, 100);
        result.Warnings.AddRange(htmlSeoResult.warnings);
        result.Errors.AddRange(htmlSeoResult.errors);

        // Calculate overall score with weighted importance (max 100)
        // Title, Description, and HTML SEO are most important (weighted 2x)
        // Others are standard weight (1x)
        var weightedScore = (result.Score.TitleScore * 2 + result.Score.DescriptionScore * 2 +
                           result.Score.KeywordsScore + result.Score.ImageScore +
                           result.Score.H1Score + result.Score.ContentImageScore +
                           result.Score.HtmlSeoScore * 2) / 9.0;

        result.Score.OverallScore = Math.Min((int)Math.Round(weightedScore, MidpointRounding.AwayFromZero), 100);

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
    private (int score, List<string> warnings, List<string> errors) ValidateH1Tags(ArticleModel article)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 0;

        try
        {
            // Check HTML file for H1 tags
            var htmlFilePath = GetHtmlFilePath(article);
            if (!string.IsNullOrEmpty(htmlFilePath) && File.Exists(htmlFilePath))
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(htmlFilePath);

                var h1Nodes = htmlDoc.DocumentNode.SelectNodes("//h1");

                if (h1Nodes == null || h1Nodes.Count == 0)
                {
                    errors.Add("No H1 tag found in the HTML file");
                    return (0, warnings, errors);
                }

                if (h1Nodes.Count > 1)
                {
                    warnings.Add($"Multiple H1 tags found ({h1Nodes.Count}). Should have exactly one H1 per page");
                    score = 70;
                }
                else
                {
                    // Check H1 content quality
                    var h1Text = h1Nodes[0].InnerText?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(h1Text))
                    {
                        errors.Add("H1 tag is empty");
                        return (0, warnings, errors);
                    }

                    if (h1Text.Length < 10)
                    {
                        warnings.Add($"H1 text is too short ({h1Text.Length} chars). Recommended: 10-70 characters");
                        score = 60;
                    }
                    else if (h1Text.Length > 70)
                    {
                        warnings.Add($"H1 text is too long ({h1Text.Length} chars). Recommended: 10-70 characters");
                        score = 70;
                    }
                    else
                    {
                        score = 100;
                    }
                }
            }
            else
            {
                // Fallback to using article title as H1 proxy
                var effectiveH1 = article.EffectiveTitle;

                if (string.IsNullOrWhiteSpace(effectiveH1))
                {
                    errors.Add("No H1 content available (HTML file not found and no article title)");
                    return (0, warnings, errors);
                }

                warnings.Add("Could not verify H1 in HTML file - using article title as proxy");

                if (effectiveH1.Length < 10)
                {
                    warnings.Add($"Title/H1 is too short ({effectiveH1.Length} chars). Recommended: 10-70 characters");
                    score = 60;
                }
                else if (effectiveH1.Length > 70)
                {
                    warnings.Add($"Title/H1 is too long ({effectiveH1.Length} chars). Recommended: 10-70 characters");
                    score = 70;
                }
                else
                {
                    score = 90; // Slightly lower since we couldn't verify actual HTML
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating H1 tags for article: {ArticleName}", article.Name);
            warnings.Add("Could not validate H1 tags - file access error");
            score = 50;
        }

        return (score, warnings, errors);
    }
    private (int score, List<string> warnings, List<string> errors) ValidateContentImages(ArticleModel article)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 100;

        try
        {
            // Check both PUG and HTML files for image validation
            var pugImageIssues = ValidateImagesInPugFile(article);
            var htmlImageIssues = ValidateImagesInHtmlFile(article);

            // Combine issues from both sources
            var totalImagesWithoutAlt = pugImageIssues.imagesWithoutAlt + htmlImageIssues.imagesWithoutAlt;
            var totalImages = pugImageIssues.totalImages + htmlImageIssues.totalImages;

            if (totalImages == 0)
            {
                // No images found in content
                score = 100;
            }
            else if (totalImagesWithoutAlt > 0)
            {
                // Some images are missing alt text
                var percentage = (totalImagesWithoutAlt * 100) / totalImages;
                warnings.Add($"Found {totalImagesWithoutAlt} out of {totalImages} images without alt text ({percentage}% missing)");

                if (pugImageIssues.imagesWithoutAlt > 0)
                {
                    warnings.Add($"PUG file: {pugImageIssues.imagesWithoutAlt} images without alt text");
                }

                if (htmlImageIssues.imagesWithoutAlt > 0)
                {
                    warnings.Add($"HTML file: {htmlImageIssues.imagesWithoutAlt} images without alt text");
                }

                // Score decreases based on percentage of images missing alt text
                score = Math.Max(100 - (percentage * 2), 20);
            }
            else
            {
                // All images have alt text
                score = 100;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating content images for article: {ArticleName}", article.Name);
            warnings.Add("Could not validate content images - file access error");
            score = 80;
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

    /// <summary>
    /// Validates images in PUG file for alt text
    /// </summary>
    /// <param name="article">The article to validate</param>
    /// <returns>Tuple containing total images and images without alt text</returns>
    private (int totalImages, int imagesWithoutAlt) ValidateImagesInPugFile(ArticleModel article)
    {
        try
        {
            var pugFilePath = GetPugFilePath(article);
            if (string.IsNullOrEmpty(pugFilePath) || !File.Exists(pugFilePath))
            {
                return (0, 0);
            }

            var pugContent = File.ReadAllText(pugFilePath);

            // PUG image patterns:
            // img(src="path" alt="text")
            // img(src="path")  // without alt
            var imgPatterns = new[]
            {
                @"img\s*\([^)]*src\s*=\s*[""'][^""']*[""'][^)]*\)", // img(src="...")
                @"img\([^)]*\)" // any img() tag
            };

            var totalImages = 0;
            var imagesWithoutAlt = 0;

            foreach (var pattern in imgPatterns)
            {
                var matches = Regex.Matches(pugContent, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    totalImages++;

                    // Check if this image has alt text
                    var imgTag = match.Value;
                    if (!Regex.IsMatch(imgTag, @"alt\s*=\s*[""'][^""']+[""']", RegexOptions.IgnoreCase))
                    {
                        imagesWithoutAlt++;
                    }
                }
                break; // Use first matching pattern to avoid double counting
            }

            return (totalImages, imagesWithoutAlt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PUG images for article: {ArticleName}", article.Name);
            return (0, 0);
        }
    }

    /// <summary>
    /// Validates images in HTML file for alt text
    /// </summary>
    /// <param name="article">The article to validate</param>
    /// <returns>Tuple containing total images and images without alt text</returns>
    private (int totalImages, int imagesWithoutAlt) ValidateImagesInHtmlFile(ArticleModel article)
    {
        try
        {
            var htmlFilePath = GetHtmlFilePath(article);
            if (string.IsNullOrEmpty(htmlFilePath) || !File.Exists(htmlFilePath))
            {
                return (0, 0);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(htmlFilePath);

            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img");
            if (imgNodes == null)
            {
                return (0, 0);
            }

            var totalImages = imgNodes.Count;
            var imagesWithoutAlt = 0;

            foreach (var imgNode in imgNodes)
            {
                var altAttribute = imgNode.GetAttributeValue("alt", string.Empty);
                if (string.IsNullOrWhiteSpace(altAttribute))
                {
                    imagesWithoutAlt++;
                }
            }

            return (totalImages, imagesWithoutAlt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HTML images for article: {ArticleName}", article.Name);
            return (0, 0);
        }
    }

    /// <summary>
    /// Gets the PUG file path for an article
    /// </summary>
    /// <param name="article">The article</param>
    /// <returns>Full path to PUG file or empty string if not found</returns>
    private string GetPugFilePath(ArticleModel article)
    {
        try
        {
            if (!string.IsNullOrEmpty(article.Source))
            {
                // Use the source path if available
                var relativePath = article.Source.Replace("/src/pug/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                return Path.Combine(_srcPath, "pug", relativePath);
            }
            else if (!string.IsNullOrEmpty(article.Slug))
            {
                // Derive from slug
                var fileName = article.Slug.Replace(".html", ".pug").Replace("articles/", "");
                return Path.Combine(_srcPath, "pug", "articles", fileName);
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PUG file path for article: {ArticleName}", article.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the HTML file path for an article
    /// </summary>
    /// <param name="article">The article</param>
    /// <returns>Full path to HTML file or empty string if not found</returns>
    private string GetHtmlFilePath(ArticleModel article)
    {
        try
        {
            if (!string.IsNullOrEmpty(article.Slug))
            {
                // Convert slug to HTML file path
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

                var fullPath = Path.Combine(_docsPath, htmlFileName);

                // If file doesn't exist and slug doesn't end with /, try adding /index.html
                if (!File.Exists(fullPath) && !article.Slug.EndsWith("/") && !article.Slug.EndsWith(".html"))
                {
                    var indexPath = Path.Combine(_docsPath, article.Slug, "index.html");
                    if (File.Exists(indexPath))
                    {
                        return indexPath;
                    }
                }

                return fullPath;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HTML file path for article: {ArticleName}", article.Name);
            return string.Empty;
        }
    }

    /// <summary>
    /// Validates HTML meta tags, title, and other SEO elements in the actual HTML file
    /// </summary>
    /// <param name="article">The article to validate</param>
    /// <returns>Validation result with file-based analysis</returns>
    public (int score, List<string> warnings, List<string> errors) ValidateHtmlSeoElements(ArticleModel article)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int score = 100;

        try
        {
            var htmlFilePath = GetHtmlFilePath(article);
            if (string.IsNullOrEmpty(htmlFilePath) || !File.Exists(htmlFilePath))
            {
                warnings.Add("HTML file not found - using metadata validation only");
                return (80, warnings, errors);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(htmlFilePath);

            // Validate title tag
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            if (titleNode == null)
            {
                errors.Add("Missing <title> tag in HTML");
                score -= 20;
            }
            else
            {
                var titleText = titleNode.InnerText?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(titleText))
                {
                    errors.Add("Empty <title> tag in HTML");
                    score -= 15;
                }
                else if (titleText.Length < 30)
                {
                    warnings.Add($"HTML title too short ({titleText.Length} chars). Recommended: 30-60 characters");
                    score -= 5;
                }
                else if (titleText.Length > 60)
                {
                    warnings.Add($"HTML title too long ({titleText.Length} chars). Recommended: 30-60 characters");
                    score -= 5;
                }
            }

            // Validate meta description
            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (descriptionNode == null)
            {
                errors.Add("Missing meta description in HTML");
                score -= 20;
            }
            else
            {
                var descContent = descriptionNode.GetAttributeValue("content", "").Trim();
                if (string.IsNullOrWhiteSpace(descContent))
                {
                    errors.Add("Empty meta description in HTML");
                    score -= 15;
                }
                else if (descContent.Length < 120)
                {
                    warnings.Add($"HTML meta description too short ({descContent.Length} chars). Recommended: 120-160 characters");
                    score -= 5;
                }
                else if (descContent.Length > 160)
                {
                    warnings.Add($"HTML meta description too long ({descContent.Length} chars). Recommended: 120-160 characters");
                    score -= 5;
                }
            }

            // Validate canonical URL
            var canonicalNode = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
            if (canonicalNode == null)
            {
                warnings.Add("Missing canonical URL in HTML");
                score -= 10;
            }

            // Validate Open Graph tags
            var ogTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            var ogDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            var ogImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            var ogTypeNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:type']");

            if (ogTitleNode == null) { warnings.Add("Missing Open Graph title"); score -= 2; }
            if (ogDescNode == null) { warnings.Add("Missing Open Graph description"); score -= 2; }
            if (ogImageNode == null) { warnings.Add("Missing Open Graph image"); score -= 2; }
            if (ogTypeNode == null) { warnings.Add("Missing Open Graph type"); score -= 2; }

            // Validate Twitter Card tags
            var twitterCardNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:card']");
            var twitterTitleNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']");
            var twitterDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
            var twitterImageNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");

            if (twitterCardNode == null) { warnings.Add("Missing Twitter Card type"); score -= 2; }
            if (twitterTitleNode == null) { warnings.Add("Missing Twitter Card title"); score -= 2; }
            if (twitterDescNode == null) { warnings.Add("Missing Twitter Card description"); score -= 2; }
            if (twitterImageNode == null) { warnings.Add("Missing Twitter Card image"); score -= 2; }

            // Validate keywords
            var keywordsNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            if (keywordsNode != null)
            {
                var keywordsContent = keywordsNode.GetAttributeValue("content", "").Trim();
                if (!string.IsNullOrWhiteSpace(keywordsContent))
                {
                    var keywordList = keywordsContent.Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k)).ToList();
                    if (keywordList.Count < 3)
                    {
                        warnings.Add($"Too few keywords in HTML ({keywordList.Count}). Recommended: 3-8");
                        score -= 3;
                    }
                    else if (keywordList.Count > 8)
                    {
                        warnings.Add($"Too many keywords in HTML ({keywordList.Count}). Recommended: 3-8");
                        score -= 3;
                    }
                }
            }
            else
            {
                warnings.Add("Missing meta keywords in HTML");
                score -= 5;
            }

            return (Math.Max(score, 0), warnings, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HTML SEO elements for article: {ArticleName}", article.Name);
            errors.Add("Could not validate HTML file - file access error");
            return (50, warnings, errors);
        }
    }
}
