using System.Globalization;

namespace mwhWebAdmin.Article;

public class ArticleService
{
    private List<ArticleModel> _articles;
    private readonly string _articlesDirectory;
    private readonly string _filePath;
    private readonly Lock _lock = new();
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(string filePath, ILogger<ArticleService> logger)
    {
        _filePath = filePath;
        _logger = logger;
        _articlesDirectory = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug", "articles");
        LoadArticles();
    }

    private string GeneratePugFileContent(ArticleModel article)
    {
        try
        {
            // Read the article-stub.pug template
            string templateFilePath = Path.Combine(_articlesDirectory, "article-stub.pug");
            string templateContent = File.ReadAllText(templateFilePath);

            // Perform string replacements for template fields
            string pugContent = templateContent
                .Replace("{{name}}", article.Name)
                .Replace("{{section}}", article.Section)
                .Replace("{{slug}}", article.Slug)
                .Replace("{{lastModified}}", article.LastModified)
                .Replace("{{changeFrequency}}", article.ChangeFrequency)
                .Replace("{{description}}", article.Description)
                .Replace("{{content}}", article.ArticleContent);
            return pugContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Pug content for article: {ArticleName}", article.Name);
            return string.Empty;
        }
    }

    private void LoadArticles()
    {
        try
        {
            string jsonContent = File.ReadAllText(_filePath);
            _articles = JsonSerializer.Deserialize<List<ArticleModel>>(jsonContent) ?? new List<ArticleModel>();
            foreach (var article in _articles.OrderBy(o => o.Name))
            {
                article.Id = _articles.IndexOf(article);
            }
            _logger.LogInformation("Articles loaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load articles.");
            _articles = new List<ArticleModel>();
        }
    }

    private void SaveArticles()
    {
        lock (_lock)
        {
            try
            {
                foreach (var article in _articles)
                {
                    article.LastModified = ConvertStringToDate(article.LastModified).ToString("yyyy-MM-dd");
                }
                string jsonContent = JsonSerializer.Serialize(_articles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, jsonContent);
                GenerateSiteMap();
                _logger.LogInformation("Articles saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save articles.");
            }
        }
    }

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
                newArticle.Id = _articles.Any() ? _articles.Max(article => article.Id) + 1 : 1; // Assign a new ID
                _articles.Add(newArticle);

                string pugContent = GeneratePugFileContent(newArticle);
                if (!string.IsNullOrEmpty(pugContent))
                {
                    // Save the .pug file
                    string pugFilePath = Path.Combine(_articlesDirectory, $"{newArticle.Slug.Replace(".html", string.Empty).Replace("articles/", string.Empty)}.pug");
                    File.WriteAllText(pugFilePath, pugContent);
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

    public void GenerateSiteMap()
    {
        try
        {
            string siteMapPath = Path.Combine(Path.GetDirectoryName(_filePath), "SiteMap.xml");
            using (XmlWriter writer = XmlWriter.Create(siteMapPath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                writer.WriteStartElement("url");
                writer.WriteElementString("loc", $"https://markhazleton.com/");
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

    public void GenerateRSSFeed()
    {
        try
        {
            string rssFeedPath = Path.Combine(Path.GetDirectoryName(_filePath), "rss.xml");
            var recentArticles = _articles.OrderByDescending(a => ConvertStringToDate(a.LastModified)).Take(10).ToList();

            using (XmlWriter writer = XmlWriter.Create(rssFeedPath, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("rss");
                writer.WriteAttributeString("version", "2.0");

                writer.WriteStartElement("channel");
                writer.WriteElementString("title", "Mark Hazleton Articles");
                writer.WriteElementString("link", "https://markhazleton.com/");
                writer.WriteElementString("description", "Latest articles from Mark Hazleton.");
                writer.WriteElementString("lastBuildDate", DateTime.Now.ToString("r"));

                foreach (var article in recentArticles)
                {
                    writer.WriteStartElement("item");
                    writer.WriteElementString("title", article.Name);
                    writer.WriteElementString("link", $"https://markhazleton.com/{article.Slug}");
                    writer.WriteElementString("description", article.Description);
                    writer.WriteElementString("pubDate", ConvertStringToDate(article.LastModified).ToString("r"));
                    writer.WriteEndElement();
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

    public static string GenerateSlug(string input)
    {
        string slug = input.ToLower();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    public ArticleModel GetArticleById(int id)
    {
        lock (_lock)
        {
            var article = _articles.FirstOrDefault(w => w.Id == id);
            if (article == null)
            {
                _logger.LogWarning($"Article with ID {id} not found.");
                return new ArticleModel();
            }
            return article;
        }
    }

    public List<ArticleModel> GetArticles()
    {
        lock (_lock)
        {
            return _articles.ToList(); // Return a copy to avoid external modifications
        }
    }

    public void UpdateArticle(ArticleModel updatedArticle)
    {
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
                _logger.LogInformation($"Article '{updatedArticle.Name}' updated successfully.");
            }
            else
            {
                _logger.LogWarning($"Article with ID {updatedArticle.Id} not found. Update failed.");
            }
        }
    }

    private bool ValidateArticle(ArticleModel article)
    {
        return !string.IsNullOrWhiteSpace(article.Name) &&
               !string.IsNullOrWhiteSpace(article.Section) &&
               !string.IsNullOrWhiteSpace(article.Slug);
    }
}
