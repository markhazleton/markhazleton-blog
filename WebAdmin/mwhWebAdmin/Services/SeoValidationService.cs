using HtmlAgilityPack;
using mwhWebAdmin.Article;
using mwhWebAdmin.Configuration;
using System.Text.RegularExpressions;

namespace mwhWebAdmin.Services;

/// <summary>
/// Validation result for individual SEO components
/// </summary>
public record ValidationResult(int Score, List<string> Warnings, List<string> Errors)
{
    public static ValidationResult Success(int score = 100) => new(score, new(), new());
    public static ValidationResult Warning(int score, params string[] warnings) => new(score, warnings.ToList(), new());
    public static ValidationResult Error(params string[] errors) => new(0, new(), errors.ToList());
    public static ValidationResult WithWarningsAndErrors(int score, IEnumerable<string> warnings, IEnumerable<string> errors) =>
        new(score, warnings.ToList(), errors.ToList());
}

/// <summary>
/// Generic validator interface for SEO components
/// </summary>
public interface ISeoValidator<T>
{
    ValidationResult Validate(T input);
    string ComponentName { get; }
}

/// <summary>
/// Base validator with common functionality
/// </summary>
public abstract class BaseSeoValidator<T> : ISeoValidator<T>
{
    public abstract string ComponentName { get; }
    public abstract ValidationResult Validate(T input);

    protected ValidationResult ValidateRequired(T input, Func<T, bool> isValid, string errorMessage)
    {
        return isValid(input) ? ValidationResult.Success() : ValidationResult.Error(errorMessage);
    }

    protected ValidationResult ValidateLength<TConfig>(string text, TConfig config, string componentName)
        where TConfig : ILengthValidationConfig
    {
        if (string.IsNullOrWhiteSpace(text))
            return ValidationResult.Error($"{componentName} is required");

        var validationMessage = config.GetValidationMessage(text.Length);
        var score = config.GetScore(text.Length);

        return string.IsNullOrEmpty(validationMessage)
            ? ValidationResult.Success(score)
            : ValidationResult.Warning(score, validationMessage);
    }
}

/// <summary>
/// Configuration wrapper for static validation classes
/// </summary>
public class LengthValidationConfig : ILengthValidationConfig
{
    private readonly Func<int, string> _getValidationMessage;
    private readonly Func<int, int> _getScore;

    public LengthValidationConfig(Func<int, string> getValidationMessage, Func<int, int> getScore)
    {
        _getValidationMessage = getValidationMessage;
        _getScore = getScore;
    }

    public string GetValidationMessage(int length) => _getValidationMessage(length);
    public int GetScore(int length) => _getScore(length);

    public static LengthValidationConfig ForTitle() =>
        new(SeoValidationConfig.Title.GetValidationMessage, SeoValidationConfig.Title.GetScore);

    public static LengthValidationConfig ForMetaDescription() =>
        new(SeoValidationConfig.MetaDescription.GetValidationMessage, SeoValidationConfig.MetaDescription.GetScore);

    public static LengthValidationConfig ForH1() =>
        new(SeoValidationConfig.H1Tag.GetValidationMessage, SeoValidationConfig.H1Tag.GetScore);
}

/// <summary>
/// Configuration interface for length-based validation
/// </summary>
public interface ILengthValidationConfig
{
    string GetValidationMessage(int length);
    int GetScore(int length);
}

/// <summary>
/// Title validator
/// </summary>
public class TitleValidator : BaseSeoValidator<string>
{
    public override string ComponentName => "Title";

    public override ValidationResult Validate(string title)
    {
        return ValidateLength(title, LengthValidationConfig.ForTitle(), ComponentName);
    }
}

/// <summary>
/// Description validator with call-to-action checking
/// </summary>
public class DescriptionValidator : BaseSeoValidator<string>
{
    public override string ComponentName => "Description";

    public override ValidationResult Validate(string description)
    {
        var lengthResult = ValidateLength(description, LengthValidationConfig.ForMetaDescription(), ComponentName);

        if (lengthResult.Errors.Any()) return lengthResult;

        var warnings = lengthResult.Warnings.ToList();
        var score = lengthResult.Score;

        // Check for call-to-action words
        if (!SeoValidationConfig.CallToAction.HasCallToAction(description))
        {
            warnings.Add("Consider adding action words like 'discover', 'learn', or 'explore' to encourage clicks");
            score = Math.Max(score - 5, 0);
        }

        return ValidationResult.WithWarningsAndErrors(score, warnings, lengthResult.Errors);
    }
}

/// <summary>
/// Keywords validator
/// </summary>
public class KeywordsValidator : BaseSeoValidator<string>
{
    public override string ComponentName => "Keywords";

    public override ValidationResult Validate(string keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            return ValidationResult.Warning(
                SeoValidationConfig.Keywords.GetScore(0),
                "Keywords are recommended for better SEO"
            );
        }

        var keywordList = keywords.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToList();

        var validationMessage = SeoValidationConfig.Keywords.GetValidationMessage(keywordList.Count);
        var score = SeoValidationConfig.Keywords.GetScore(keywordList.Count);

        return string.IsNullOrEmpty(validationMessage)
            ? ValidationResult.Success(score)
            : ValidationResult.Warning(score, validationMessage);
    }
}

/// <summary>
/// File-based validator for components that require file system access
/// </summary>
public abstract class FileBasedValidator<T> : BaseSeoValidator<T>
{
    protected readonly ILogger Logger;
    protected readonly string SrcPath;
    protected readonly string DocsPath;

    protected FileBasedValidator(ILogger logger, string srcPath, string docsPath)
    {
        Logger = logger;
        SrcPath = srcPath;
        DocsPath = docsPath;
    }

    protected string GetPugFilePath(ArticleModel article)
    {
        try
        {
            if (!string.IsNullOrEmpty(article.Source))
            {
                var relativePath = article.Source.Replace("/src/pug/", "").Replace("/", Path.DirectorySeparatorChar.ToString());
                return Path.Combine(SrcPath, "pug", relativePath);
            }
            else if (!string.IsNullOrEmpty(article.Slug))
            {
                var fileName = article.Slug.Replace(".html", ".pug").Replace("articles/", "");
                return Path.Combine(SrcPath, "pug", "articles", fileName);
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting PUG file path for article: {ArticleName}", article.Name);
            return string.Empty;
        }
    }

    protected string GetHtmlFilePath(ArticleModel article)
    {
        try
        {
            if (string.IsNullOrEmpty(article.Slug)) return string.Empty;

            var htmlFileName = article.Slug;
            if (htmlFileName.EndsWith("/"))
                htmlFileName += "index.html";
            else if (!htmlFileName.EndsWith(".html"))
                htmlFileName += ".html";

            var fullPath = Path.Combine(DocsPath, htmlFileName);

            if (!File.Exists(fullPath) && !article.Slug.EndsWith("/") && !article.Slug.EndsWith(".html"))
            {
                var indexPath = Path.Combine(DocsPath, article.Slug, "index.html");
                if (File.Exists(indexPath)) return indexPath;
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting HTML file path for article: {ArticleName}", article.Name);
            return string.Empty;
        }
    }
}

/// <summary>
/// H1 tags validator
/// </summary>
public class H1Validator : FileBasedValidator<ArticleModel>
{
    public override string ComponentName => "H1 Tags";

    public H1Validator(ILogger logger, string srcPath, string docsPath)
        : base(logger, srcPath, docsPath) { }

    public override ValidationResult Validate(ArticleModel article)
    {
        try
        {
            var htmlFilePath = GetHtmlFilePath(article);

            if (!string.IsNullOrEmpty(htmlFilePath) && File.Exists(htmlFilePath))
            {
                return ValidateH1InHtmlFile(htmlFilePath);
            }

            return ValidateH1Fallback(article);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating H1 tags for article: {ArticleName}", article.Name);
            return ValidationResult.Warning(50, "Could not validate H1 tags - file access error");
        }
    }

    private ValidationResult ValidateH1InHtmlFile(string htmlFilePath)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.Load(htmlFilePath);

        var h1Nodes = htmlDoc.DocumentNode.SelectNodes("//h1");

        if (h1Nodes == null || h1Nodes.Count == 0)
            return ValidationResult.Error("No H1 tag found in the HTML file");

        if (h1Nodes.Count > 1)
            return ValidationResult.Warning(70, $"Multiple H1 tags found ({h1Nodes.Count}). Should have exactly one H1 per page");

        var h1Text = h1Nodes[0].InnerText?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(h1Text))
            return ValidationResult.Error("H1 tag is empty");

        var validationMessage = LengthValidationConfig.ForH1().GetValidationMessage(h1Text.Length);
        var score = LengthValidationConfig.ForH1().GetScore(h1Text.Length);

        return string.IsNullOrEmpty(validationMessage)
            ? ValidationResult.Success(score)
            : ValidationResult.Warning(score, validationMessage);
    }

    private ValidationResult ValidateH1Fallback(ArticleModel article)
    {
        var effectiveH1 = article.EffectiveTitle;

        if (string.IsNullOrWhiteSpace(effectiveH1))
            return ValidationResult.Error("No H1 content available (HTML file not found and no article title)");

        var warnings = new List<string> { "Could not verify H1 in HTML file - using article title as proxy" };

        var validationMessage = LengthValidationConfig.ForH1().GetValidationMessage(effectiveH1.Length);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            warnings.Add($"Title/H1 {validationMessage.Replace("H1 text", "").ToLowerInvariant()}");
        }

        var score = Math.Max(LengthValidationConfig.ForH1().GetScore(effectiveH1.Length) - 10, 0);
        return ValidationResult.WithWarningsAndErrors(score, warnings, new List<string>());
    }
}

/// <summary>
/// Images validator
/// </summary>
public class ImagesValidator : BaseSeoValidator<ArticleModel>
{
    public override string ComponentName => "Images";

    public override ValidationResult Validate(ArticleModel article)
    {
        var warnings = new List<string>();
        int score = 0;

        if (string.IsNullOrWhiteSpace(article.ImgSrc))
        {
            return ValidationResult.Warning(70, "Featured image is recommended for social media sharing");
        }

        score = 90;

        // Validate OpenGraph description if provided
        if (!string.IsNullOrWhiteSpace(article.OpenGraph?.Description))
        {
            var ogDescLength = article.OpenGraph.Description.Length;
            if (ogDescLength < SeoValidationConfig.OpenGraphDescription.MinLength)
            {
                warnings.Add($"Open Graph description too short ({ogDescLength} chars). Recommended: {SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} characters");
                score = Math.Max(score - 5, 0);
            }
            else if (ogDescLength > SeoValidationConfig.OpenGraphDescription.MaxLength)
            {
                warnings.Add($"Open Graph description too long ({ogDescLength} chars). Recommended: {SeoValidationConfig.OpenGraphDescription.MinLength}-{SeoValidationConfig.OpenGraphDescription.MaxLength} characters");
                score = Math.Max(score - 5, 0);
            }
        }

        // Validate Twitter description if provided
        if (!string.IsNullOrWhiteSpace(article.TwitterCard?.Description))
        {
            var twitterDescLength = article.TwitterCard.Description.Length;
            if (twitterDescLength < SeoValidationConfig.TwitterDescription.MinLength)
            {
                warnings.Add($"Twitter description too short ({twitterDescLength} chars). Recommended: {SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} characters");
                score = Math.Max(score - 5, 0);
            }
            else if (twitterDescLength > SeoValidationConfig.TwitterDescription.MaxLength)
            {
                warnings.Add($"Twitter description too long ({twitterDescLength} chars). Recommended: {SeoValidationConfig.TwitterDescription.MinLength}-{SeoValidationConfig.TwitterDescription.MaxLength} characters");
                score = Math.Max(score - 5, 0);
            }
        }

        if (string.IsNullOrWhiteSpace(article.OpenGraph?.ImageAlt) &&
            string.IsNullOrWhiteSpace(article.TwitterCard?.ImageAlt))
        {
            warnings.Add("Image alt text is missing - important for accessibility");
            score = Math.Max(score - 10, 0);
        }

        return ValidationResult.WithWarningsAndErrors(score, warnings, new List<string>());
    }
}

/// <summary>
/// Content images validator
/// </summary>
public class ContentImagesValidator : FileBasedValidator<ArticleModel>
{
    public override string ComponentName => "Content Images";

    public ContentImagesValidator(ILogger logger, string srcPath, string docsPath)
        : base(logger, srcPath, docsPath) { }

    public override ValidationResult Validate(ArticleModel article)
    {
        try
        {
            var pugImageIssues = ValidateImagesInPugFile(article);
            var htmlImageIssues = ValidateImagesInHtmlFile(article);

            var totalImagesWithoutAlt = pugImageIssues.imagesWithoutAlt + htmlImageIssues.imagesWithoutAlt;
            var totalImages = pugImageIssues.totalImages + htmlImageIssues.totalImages;

            if (totalImages == 0) return ValidationResult.Success();
            if (totalImagesWithoutAlt == 0) return ValidationResult.Success();

            var warnings = new List<string>();
            var percentage = (totalImagesWithoutAlt * 100) / totalImages;
            warnings.Add($"Found {totalImagesWithoutAlt} out of {totalImages} images without alt text ({percentage}% missing)");

            if (pugImageIssues.imagesWithoutAlt > 0)
                warnings.Add($"PUG file: {pugImageIssues.imagesWithoutAlt} images without alt text");

            if (htmlImageIssues.imagesWithoutAlt > 0)
                warnings.Add($"HTML file: {htmlImageIssues.imagesWithoutAlt} images without alt text");

            var score = SeoValidationConfig.Images.GetScore(totalImages, totalImagesWithoutAlt);
            return ValidationResult.WithWarningsAndErrors(score, warnings, new List<string>());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating content images for article: {ArticleName}", article.Name);
            return ValidationResult.Warning(80, "Could not validate content images - file access error");
        }
    }

    private (int totalImages, int imagesWithoutAlt) ValidateImagesInPugFile(ArticleModel article)
    {
        try
        {
            var pugFilePath = GetPugFilePath(article);
            if (string.IsNullOrEmpty(pugFilePath) || !File.Exists(pugFilePath))
                return (0, 0);

            var pugContent = File.ReadAllText(pugFilePath);
            var imgPatterns = new[]
            {
                @"img\s*\([^)]*src\s*=\s*[""'][^""']*[""'][^)]*\)",
                @"img\([^)]*\)"
            };

            var totalImages = 0;
            var imagesWithoutAlt = 0;

            foreach (var pattern in imgPatterns)
            {
                var matches = Regex.Matches(pugContent, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    totalImages++;
                    var imgTag = match.Value;
                    if (!Regex.IsMatch(imgTag, @"alt\s*=\s*[""'][^""']+[""']", RegexOptions.IgnoreCase))
                    {
                        imagesWithoutAlt++;
                    }
                }
                break;
            }

            return (totalImages, imagesWithoutAlt);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating PUG images for article: {ArticleName}", article.Name);
            return (0, 0);
        }
    }

    private (int totalImages, int imagesWithoutAlt) ValidateImagesInHtmlFile(ArticleModel article)
    {
        try
        {
            var htmlFilePath = GetHtmlFilePath(article);
            if (string.IsNullOrEmpty(htmlFilePath) || !File.Exists(htmlFilePath))
                return (0, 0);

            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(htmlFilePath);

            var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img");
            if (imgNodes == null) return (0, 0);

            var totalImages = imgNodes.Count;
            var imagesWithoutAlt = imgNodes.Count(imgNode =>
                string.IsNullOrWhiteSpace(imgNode.GetAttributeValue("alt", string.Empty)));

            return (totalImages, imagesWithoutAlt);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating HTML images for article: {ArticleName}", article.Name);
            return (0, 0);
        }
    }
}

/// <summary>
/// HTML SEO elements validator
/// </summary>
public class HtmlSeoValidator : FileBasedValidator<ArticleModel>
{
    public override string ComponentName => "HTML SEO Elements";

    public HtmlSeoValidator(ILogger logger, string srcPath, string docsPath)
        : base(logger, srcPath, docsPath) { }

    public override ValidationResult Validate(ArticleModel article)
    {
        try
        {
            var htmlFilePath = GetHtmlFilePath(article);
            if (string.IsNullOrEmpty(htmlFilePath) || !File.Exists(htmlFilePath))
            {
                return ValidationResult.Warning(80, "HTML file not found - using metadata validation only");
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(htmlFilePath);

            var warnings = new List<string>();
            var errors = new List<string>();
            int score = 100;

            // Validate using a collection of validation rules
            var validators = new List<Func<HtmlDocument, (int scoreDeduction, List<string> warnings, List<string> errors)>>
            {
                ValidateTitle,
                ValidateMetaDescription,
                ValidateCanonical,
                ValidateOpenGraph,
                ValidateTwitterCard,
                ValidateKeywords
            };

            foreach (var validator in validators)
            {
                var (scoreDeduction, validatorWarnings, validatorErrors) = validator(htmlDoc);
                score -= scoreDeduction;
                warnings.AddRange(validatorWarnings);
                errors.AddRange(validatorErrors);
            }

            return ValidationResult.WithWarningsAndErrors(Math.Max(score, 0), warnings, errors);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating HTML SEO elements for article: {ArticleName}", article.Name);
            return ValidationResult.WithWarningsAndErrors(50, new List<string>(), new List<string> { "Could not validate HTML file - file access error" });
        }
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateTitle(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
        if (titleNode == null)
        {
            errors.Add("Missing <title> tag in HTML");
            return (20, warnings, errors);
        }

        var titleText = titleNode.InnerText?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(titleText))
        {
            errors.Add("Empty <title> tag in HTML");
            return (15, warnings, errors);
        }

        if (titleText.Length < 30)
        {
            warnings.Add($"HTML title too short ({titleText.Length} chars). Recommended: 30-60 characters");
            return (5, warnings, errors);
        }

        if (titleText.Length > 60)
        {
            warnings.Add($"HTML title too long ({titleText.Length} chars). Recommended: 30-60 characters");
            return (5, warnings, errors);
        }

        return (0, warnings, errors);
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateMetaDescription(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
        if (descriptionNode == null)
        {
            errors.Add("Missing meta description in HTML");
            return (20, warnings, errors);
        }

        var descContent = descriptionNode.GetAttributeValue("content", "").Trim();
        if (string.IsNullOrWhiteSpace(descContent))
        {
            errors.Add("Empty meta description in HTML");
            return (15, warnings, errors);
        }

        if (descContent.Length < 120)
        {
            warnings.Add($"HTML meta description too short ({descContent.Length} chars). Recommended: 120-160 characters");
            return (5, warnings, errors);
        }

        if (descContent.Length > 160)
        {
            warnings.Add($"HTML meta description too long ({descContent.Length} chars). Recommended: 120-160 characters");
            return (5, warnings, errors);
        }

        return (0, warnings, errors);
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateCanonical(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        var canonicalNode = htmlDoc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
        if (canonicalNode == null)
        {
            warnings.Add("Missing canonical URL in HTML");
            return (10, warnings, errors);
        }

        return (0, warnings, errors);
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateOpenGraph(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int scoreDeduction = 0;

        var requiredOgTags = new[]
        {
            ("og:title", "Missing Open Graph title", 2),
            ("og:type", "Missing Open Graph type", 2)
        };

        foreach (var (property, message, penalty) in requiredOgTags)
        {
            var node = htmlDoc.DocumentNode.SelectSingleNode($"//meta[@property='{property}']");
            if (node == null)
            {
                warnings.Add(message);
                scoreDeduction += penalty;
            }
        }

        // Special handling for og:description with length validation
        var ogDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
        if (ogDescNode == null)
        {
            warnings.Add("Missing Open Graph description");
            scoreDeduction += 2;
        }
        else
        {
            var ogDescContent = ogDescNode.GetAttributeValue("content", "").Trim();
            if (string.IsNullOrWhiteSpace(ogDescContent))
            {
                errors.Add("Empty Open Graph description");
                scoreDeduction += 10;
            }
            else if (ogDescContent.Length < 120)
            {
                warnings.Add($"Open Graph description too short ({ogDescContent.Length} chars). Recommended: 120-160 characters");
                scoreDeduction += 3;
            }
            else if (ogDescContent.Length > 160)
            {
                warnings.Add($"Open Graph description too long ({ogDescContent.Length} chars). Recommended: 120-160 characters");
                scoreDeduction += 3;
            }
        }

        return (scoreDeduction, warnings, errors);
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateTwitterCard(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();
        int scoreDeduction = 0;

        var requiredTwitterTags = new[]
        {
            ("twitter:card", "Missing Twitter Card type", 2),
            ("twitter:title", "Missing Twitter Card title", 2)
        };

        foreach (var (name, message, penalty) in requiredTwitterTags)
        {
            var node = htmlDoc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']");
            if (node == null)
            {
                warnings.Add(message);
                scoreDeduction += penalty;
            }
        }

        // Special handling for twitter:description with length validation
        var twitterDescNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
        if (twitterDescNode == null)
        {
            warnings.Add("Missing Twitter Card description");
            scoreDeduction += 2;
        }
        else
        {
            var twitterDescContent = twitterDescNode.GetAttributeValue("content", "").Trim();
            if (string.IsNullOrWhiteSpace(twitterDescContent))
            {
                errors.Add("Empty Twitter Card description");
                scoreDeduction += 10;
            }
            else if (twitterDescContent.Length < 120)
            {
                warnings.Add($"Twitter Card description too short ({twitterDescContent.Length} chars). Recommended: 120-160 characters");
                scoreDeduction += 3;
            }
            else if (twitterDescContent.Length > 160)
            {
                warnings.Add($"Twitter Card description too long ({twitterDescContent.Length} chars). Recommended: 120-160 characters");
                scoreDeduction += 3;
            }
        }

        return (scoreDeduction, warnings, errors);
    }

    private static (int scoreDeduction, List<string> warnings, List<string> errors) ValidateKeywords(HtmlDocument htmlDoc)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        var keywordsNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
        if (keywordsNode == null)
        {
            warnings.Add("Missing meta keywords in HTML");
            return (5, warnings, errors);
        }

        var keywordsContent = keywordsNode.GetAttributeValue("content", "").Trim();
        if (string.IsNullOrWhiteSpace(keywordsContent)) return (0, warnings, errors);

        var keywordList = keywordsContent.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToList();

        if (keywordList.Count < 3)
        {
            warnings.Add($"Too few keywords in HTML ({keywordList.Count}). Recommended: 3-8");
            return (3, warnings, errors);
        }

        if (keywordList.Count > 8)
        {
            warnings.Add($"Too many keywords in HTML ({keywordList.Count}). Recommended: 3-8");
            return (3, warnings, errors);
        }

        return (0, warnings, errors);
    }
}

/// <summary>
/// Main SEO validation service using composition of validators
/// </summary>
public class SeoValidationService
{
    private readonly ILogger<SeoValidationService> _logger;
    private readonly Dictionary<string, ISeoValidator<string>> _stringValidators;
    private readonly Dictionary<string, ISeoValidator<ArticleModel>> _articleValidators;

    public SeoValidationService(ILogger<SeoValidationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var srcPath = configuration["SrcPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src");
        var docsPath = configuration["DocsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs");

        _stringValidators = new Dictionary<string, ISeoValidator<string>>
        {
            ["Title"] = new TitleValidator(),
            ["Description"] = new DescriptionValidator(),
            ["Keywords"] = new KeywordsValidator()
        };

        _articleValidators = new Dictionary<string, ISeoValidator<ArticleModel>>
        {
            ["Images"] = new ImagesValidator(),
            ["H1"] = new H1Validator(logger, srcPath, docsPath),
            ["ContentImages"] = new ContentImagesValidator(logger, srcPath, docsPath),
            ["HtmlSeo"] = new HtmlSeoValidator(logger, srcPath, docsPath)
        };
    }

    public SeoValidationResult ValidateArticle(ArticleModel article)
    {
        var result = new SeoValidationResult();

        // Validate string properties
        var titleResult = _stringValidators["Title"].Validate(article.EffectiveTitle);
        result.Score.TitleScore = Math.Min(titleResult.Score, 100);
        result.Warnings.AddRange(titleResult.Warnings);
        result.Errors.AddRange(titleResult.Errors);

        var descResult = _stringValidators["Description"].Validate(article.EffectiveDescription);
        result.Score.DescriptionScore = Math.Min(descResult.Score, 100);
        result.Warnings.AddRange(descResult.Warnings);
        result.Errors.AddRange(descResult.Errors);

        var keywordResult = _stringValidators["Keywords"].Validate(article.EffectiveKeywords);
        result.Score.KeywordsScore = Math.Min(keywordResult.Score, 100);
        result.Warnings.AddRange(keywordResult.Warnings);
        result.Errors.AddRange(keywordResult.Errors);

        // Validate article properties
        var imageResult = _articleValidators["Images"].Validate(article);
        result.Score.ImageScore = Math.Min(imageResult.Score, 100);
        result.Warnings.AddRange(imageResult.Warnings);
        result.Errors.AddRange(imageResult.Errors);

        var h1Result = _articleValidators["H1"].Validate(article);
        result.Score.H1Score = Math.Min(h1Result.Score, 100);
        result.Warnings.AddRange(h1Result.Warnings);
        result.Errors.AddRange(h1Result.Errors);

        var contentImageResult = _articleValidators["ContentImages"].Validate(article);
        result.Score.ContentImageScore = Math.Min(contentImageResult.Score, 100);
        result.Warnings.AddRange(contentImageResult.Warnings);
        result.Errors.AddRange(contentImageResult.Errors);

        var htmlSeoResult = _articleValidators["HtmlSeo"].Validate(article);
        result.Score.HtmlSeoScore = Math.Min(htmlSeoResult.Score, 100);
        result.Warnings.AddRange(htmlSeoResult.Warnings);
        result.Errors.AddRange(htmlSeoResult.Errors);

        // Calculate overall score
        result.Score.OverallScore = SeoValidationConfig.Scoring.CalculateOverallScore(
            result.Score.TitleScore,
            result.Score.DescriptionScore,
            result.Score.KeywordsScore,
            result.Score.ImageScore,
            result.Score.H1Score,
            result.Score.ContentImageScore,
            result.Score.HtmlSeoScore
        );

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public List<string> GetRecommendations(ArticleModel article)
    {
        var recommendations = new List<string>();
        var validation = ValidateArticle(article);

        var recommendationRules = new[]
        {
            (validation.Score.OverallScore < 80, "Overall SEO score could be improved. Focus on the areas with warnings below."),
            (string.IsNullOrEmpty(article.Seo?.Title), "Add an SEO-optimized title in the 'SEO' section for better search engine visibility."),
            (string.IsNullOrEmpty(article.Seo?.Description), "Add an SEO-optimized description (150-160 chars) for better search results."),
            (article.OpenGraph == null, "Add Open Graph metadata for better social media sharing on Facebook and LinkedIn."),
            (article.TwitterCard == null, "Add Twitter Card metadata for better appearance when shared on Twitter/X."),
            (string.IsNullOrEmpty(article.Seo?.Canonical), "Set a canonical URL to prevent duplicate content issues.")
        };

        recommendations.AddRange(recommendationRules
            .Where(rule => rule.Item1)
            .Select(rule => rule.Item2));

        return recommendations;
    }

    /// <summary>
    /// Validates a specific component by name
    /// </summary>
    /// <typeparam name="T">The input type for validation</typeparam>
    /// <param name="componentName">Name of the component to validate</param>
    /// <param name="input">Input to validate</param>
    /// <returns>Validation result</returns>
    public ValidationResult ValidateComponent<T>(string componentName, T input)
    {
        if (typeof(T) == typeof(string) && _stringValidators.TryGetValue(componentName, out var stringValidator))
        {
            return stringValidator.Validate(input as string);
        }

        if (typeof(T) == typeof(ArticleModel) && _articleValidators.TryGetValue(componentName, out var articleValidator))
        {
            return articleValidator.Validate(input as ArticleModel);
        }

        throw new ArgumentException($"No validator found for component '{componentName}' with input type '{typeof(T).Name}'");
    }

    /// <summary>
    /// Gets all available validator component names
    /// </summary>
    /// <returns>List of component names that can be validated</returns>
    public IEnumerable<string> GetAvailableComponents()
    {
        return _stringValidators.Keys.Concat(_articleValidators.Keys);
    }
}
