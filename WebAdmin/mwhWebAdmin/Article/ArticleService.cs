using System.Net;
using System.Text.Json;


namespace mwhWebAdmin.Models
{
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

            // URL decode slugs
            foreach (var article in _articles)
            {
                article.Slug = WebUtility.UrlEncode(article.Slug);
            }
        }

        private void SaveArticles()
        {
            foreach (var article in _articles)
            {
                article.Slug = WebUtility.UrlDecode(article.Slug);
            }
            string jsonContent = JsonSerializer.Serialize(_articles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, jsonContent);
        }

        public List<ArticleModel> GetArticles()
        {
            return _articles;
        }

        public ArticleModel GetArticleBySlug(string slug)
        {
            return _articles.Find(article => article.Slug == slug) ?? new ArticleModel() { Slug = slug };
        }

        public void UpdateArticle(ArticleModel updatedArticle)
        {
            int index = _articles.FindIndex(article => article.Slug == updatedArticle.Slug);
            if (index != -1)
            {
                _articles[index] = updatedArticle;
                SaveArticles();
            }
        }
    }
}

