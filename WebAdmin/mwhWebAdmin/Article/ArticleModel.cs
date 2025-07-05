using System.Text.Json.Serialization;

namespace mwhWebAdmin.Article;

/// <summary>
/// Represents an article model with various properties such as Id, Section, Slug, Name, Content, Description, Image Source, Last Modified date, and Change Frequency.
/// </summary>
public class ArticleModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleModel"/> class with default values for Slug and ImgSrc.
    /// </summary>
    public ArticleModel()
    {
        Slug = "tbd";
        ImgSrc = "assets/img/ArgostoliGreeceBeach.jpg";
    }

    /// <summary>
    /// Gets or sets the unique identifier for the article.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
    ///    /// <summary>
    /// Gets or sets the section to which the article belongs.
    /// </summary>
    [JsonPropertyName("Section")]
    public string Section { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slug (URL-friendly string) for the article.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the article.
    /// </summary>
    [JsonPropertyName("content")]
    public string ArticleContent { get; set; } = string.Empty;
    ///
    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the keywords for the article.
    /// </summary>
    [JsonPropertyName("keywords")]
    public string Keywords { get; set; } = string.Empty;

    ///
    /// <summary>
    /// Gets or sets the image source URL for the article.
    /// </summary>
    [JsonPropertyName("img_src")]
    public string ImgSrc { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the article.
    /// </summary>
    [JsonPropertyName("lastmod")]
    public string LastModified { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    ///
    /// <summary>
    /// Gets or sets the change frequency of the article.
    /// </summary>
    [JsonPropertyName("changefreq")]
    public string ChangeFrequency { get; set; } = "monthly";

    /// <summary>
    /// Gets or sets the source PUG file path for the article.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the source PUG file exists.
    /// </summary>
    [JsonIgnore]
    public bool SourceFileExists => !string.IsNullOrEmpty(Source) && File.Exists(GetFullSourcePath());    /// <summary>
                                                                                                          /// Gets the full path to the source PUG file.
                                                                                                          /// </summary>
                                                                                                          /// <returns>The full path to the source PUG file.</returns>
    private string GetFullSourcePath()
    {
        if (string.IsNullOrEmpty(Source))
            return string.Empty;

        // Source paths start with /src/pug/, we need to convert to absolute path
        // Remove leading slash and convert to Windows path
        string relativePath = Source.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

        // Get the workspace root by going up from the WebAdmin directory
        var assemblyLocation = typeof(ArticleModel).Assembly.Location;
        if (string.IsNullOrEmpty(assemblyLocation))
            return string.Empty;

        var webAdminPath = Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrEmpty(webAdminPath))
            return string.Empty;

        // Navigate up to the workspace root (c:\GitHub\MarkHazleton\markhazleton-blog)
        var workspaceRoot = webAdminPath;
        while (!string.IsNullOrEmpty(workspaceRoot) && !Directory.Exists(Path.Combine(workspaceRoot, "src")))
        {
            var parent = Directory.GetParent(workspaceRoot);
            if (parent == null) break;
            workspaceRoot = parent.FullName;
        }

        if (string.IsNullOrEmpty(workspaceRoot) || !Directory.Exists(Path.Combine(workspaceRoot, "src")))
            return string.Empty;

        return Path.Combine(workspaceRoot, relativePath);
    }

    /// <summary>
    /// Gets or sets the subtitle of the article.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the article.
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = "Solutions Architect";

    /// <summary>
    /// Gets or sets the summary of the article.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion title of the article.
    /// </summary>
    [JsonPropertyName("conclusionTitle")]
    public string ConclusionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion summary of the article.
    /// </summary>
    [JsonPropertyName("conclusionSummary")]
    public string ConclusionSummary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion key heading of the article.
    /// </summary>
    [JsonPropertyName("conclusionKeyHeading")]
    public string ConclusionKeyHeading { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion key text of the article.
    /// </summary>
    [JsonPropertyName("conclusionKeyText")]
    public string ConclusionKeyText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conclusion text of the article.
    /// </summary>
    [JsonPropertyName("conclusionText")]
    public string ConclusionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the enhanced SEO metadata for the article.
    /// </summary>
    [JsonPropertyName("seo")]
    public SeoModel? Seo { get; set; }

    /// <summary>
    /// Gets or sets the Open Graph metadata for the article.
    /// </summary>
    [JsonPropertyName("og")]
    public OpenGraphModel? OpenGraph { get; set; }

    /// <summary>
    /// Gets or sets the Twitter Card metadata for the article.
    /// </summary>
    [JsonPropertyName("twitter")]
    public TwitterCardModel? TwitterCard { get; set; }

    /// <summary>
    /// Gets or sets the YouTube video URL associated with the article.
    /// </summary>
    [JsonPropertyName("youtubeUrl")]
    public string? YouTubeUrl { get; set; }

    /// <summary>
    /// Gets or sets the YouTube video title associated with the article.
    /// </summary>
    [JsonPropertyName("youtubeTitle")]
    public string? YouTubeTitle { get; set; }

    /// <summary>
    /// Gets the effective title for SEO purposes
    /// </summary>
    [JsonIgnore]
    public string EffectiveTitle => Seo?.Title ?? Name;

    /// <summary>
    /// Gets the effective description for SEO purposes
    /// </summary>
    [JsonIgnore]
    public string EffectiveDescription => Seo?.Description ?? Description;

    /// <summary>
    /// Gets the effective keywords for SEO purposes
    /// </summary>
    [JsonIgnore]
    public string EffectiveKeywords => Seo?.Keywords ?? Keywords;
}

