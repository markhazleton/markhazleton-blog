using System.Text.Json;
using System.Xml;

namespace mwhWebAdmin.Models;

public class ArticleService
{
    private readonly string _filePath;
    private List<ArticleModel> _articles;

    public ArticleService(string filePath)
    {
        _filePath = filePath;
        LoadArticles();
    }

    private void LoadArticles()
    {
        string jsonContent = File.ReadAllText(_filePath);
        _articles = JsonSerializer.Deserialize<List<ArticleModel>>(jsonContent);

        foreach (var article in _articles.OrderBy(o=>o.Name))
        {
            article.Id = _articles.IndexOf(article);
        }
    }

    private void SaveArticles()
    {
        foreach (var article in _articles)
        {
            article.LastModified = DateTime.Now.ToString("yyyy-MM-dd");
        }
        string jsonContent = JsonSerializer.Serialize(_articles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, jsonContent);
    }

    public List<ArticleModel> GetArticles()
    {
        return _articles;
    }

    public ArticleModel GetArticleById(int id)
    {
        var article = _articles.Where(w => w.Id== id).FirstOrDefault();
        if (article == null)
        {
            return new ArticleModel() { };
        }
        return article;
    }

    public void UpdateArticle(ArticleModel updatedArticle)
    {
        int index = _articles.FindIndex(article => article.Id== updatedArticle.Id);
        if (index != -1)
        {
            _articles[index] = updatedArticle;
            SaveArticles();
            GenerateSiteMap();
        }
    }

    public void GenerateSiteMap()
    {
        string siteMapPath = Path.Combine(Path.GetDirectoryName(_filePath), "SiteMap.xml");
        using (XmlWriter writer = XmlWriter.Create(siteMapPath, new XmlWriterSettings { Indent = true }))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            writer.WriteStartElement("url");
            writer.WriteElementString("loc", $"https://markhazleton.controlorigins.com/");
            writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            writer.WriteElementString("changefreq", "weekly");
            writer.WriteEndElement();

            foreach (var article in _articles)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", $"https://markhazleton.controlorigins.com/{article.Slug}");
                writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                writer.WriteElementString("changefreq", article.ChangeFrequency);
                writer.WriteEndElement(); 
            }
            writer.WriteEndElement(); 
            writer.WriteEndDocument();
        }
    }


}

