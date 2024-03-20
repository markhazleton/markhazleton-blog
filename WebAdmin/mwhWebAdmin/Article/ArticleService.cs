using System.Globalization;

namespace mwhWebAdmin.Models;

public class ArticleService
{
    private List<ArticleModel> _articles;
    private readonly string _articlesDirectory;
    private readonly string _filePath;

    public ArticleService(string filePath)
    {
        _filePath = filePath;
        _articlesDirectory = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug", "articles");
        LoadArticles();
    }

    private string GeneratePugFileContent(ArticleModel article)
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

    private void LoadArticles()
    {
        string jsonContent = File.ReadAllText(_filePath);
        _articles = JsonSerializer.Deserialize<List<ArticleModel>>(jsonContent);

        foreach (var article in _articles.OrderBy(o => o.Name))
        {
            article.Id = _articles.IndexOf(article);
        }
    }

    private void SaveArticles()
    {
        foreach (var article in _articles)
        {
            article.LastModified = ConvertStringToDate(article.LastModified).ToString("yyyy-MM-dd");
        }
        string jsonContent = JsonSerializer.Serialize(_articles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, jsonContent);
        GenerateSiteMap();
    }

    public void AddArticle(ArticleModel newArticle)
    {
        newArticle.Slug = "articles/" + GenerateSlug(newArticle.Name) + ".html";
        newArticle.Id = _articles.Max(article => article.Id) + 1; // Assign a new ID
        _articles.Add(newArticle);
        string pugContent = GeneratePugFileContent(newArticle);
        // Save the .pug file
        string pugFilePath = Path.Combine(_articlesDirectory, $"{newArticle.Slug.Replace(".html", string.Empty).Replace("articles/", string.Empty)}.pug");
        File.WriteAllText(pugFilePath, pugContent);
        SaveArticles();
    }

    public void GenerateSiteMap()
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
    }

    public static DateTime ConvertStringToDate(string dateString)
    {
        DateTime dateValue;
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
        {
            // Conversion succeeded
            return dateValue;
        }
        else
        {
            // Conversion failed
            return DateTime.Now;
        }
    }



    public static string GenerateSlug(string input)
    {
        // Convert to lowercase
        string slug = input.ToLower();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove non-alphanumeric characters and hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);

        // Remove multiple hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }

    public ArticleModel GetArticleById(int id)
    {
        var article = _articles.Where(w => w.Id == id).FirstOrDefault();
        if (article == null)
        {
            return new ArticleModel() { };
        }
        return article;
    }

    public List<ArticleModel> GetArticles()
    {
        return _articles;
    }

    public void UpdateArticle(ArticleModel updatedArticle)
    {
        int index = _articles.FindIndex(article => article.Id == updatedArticle.Id);
        if (index != -1)
        {
            _articles[index] = updatedArticle;
            SaveArticles();
        }
    }
}

