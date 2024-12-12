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
    ///
    /// <summary>
    /// Gets or sets the section to which the article belongs.
    /// </summary>
    [JsonPropertyName("Section")]
    public string Section { get; set; }

    /// <summary>
    /// Gets or sets the slug (URL-friendly string) for the article.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the content of the article.
    /// </summary>
    [JsonPropertyName("content")]
    public string ArticleContent { get; set; }
    ///
    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

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
}

