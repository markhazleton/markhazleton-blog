using HtmlAgilityPack;
using System.Globalization;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleService"/> class.
        /// </summary>
        /// <param name="filePath">The file path of the articles JSON file.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public ArticleService(string filePath, ILogger<ArticleService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _filePath = filePath;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _articlesDirectory = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug", "articles");
            LoadArticles();
        }

        /// <summary>
        /// Generates SEO-optimized keywords for the given content using OpenAI's Chat Completions API.
        /// </summary>
        /// <param name="content">The body content of the article.</param>
        /// <returns>A comma-separated string of SEO-optimized keywords.</returns>
        private async Task<string> GenerateKeywordsFromContentAsync(string content)
        {
            try
            {
                var openAiApiKey = _configuration["OPENAI_API_KEY"];
                var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);

                // Prepare the request payload
                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are an SEO expert. Calculate SEO-optimized keywords from the provided content. Ignore the header, footer, and any links off the page. Return a comma separated string of keywords."
                        },
                        new
                        {
                            role = "user",
                            content = content
                        }
                    },
                    max_tokens = 1000,
                    temperature = 0.5
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                // Make the API call
                var response = await httpClient.PostAsync(openAiApiUrl, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonDocument.Parse(responseContent);

                // Extract the response text
                var keywords = responseData.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return keywords?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate keywords using OpenAI API.");
                return string.Empty;
            }
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

        /// <summary>
        /// Loads the articles from the JSON file.
        /// </summary>
        private void LoadArticles()
        {
            try
            {
                string jsonContent = File.ReadAllText(_filePath);
                _articles = JsonSerializer.Deserialize<List<ArticleModel>>(jsonContent) ?? [];
                foreach (var article in _articles.OrderBy(o => o.Id))
                {
                    article.Id = _articles.IndexOf(article);
                }
                _logger.LogInformation("Articles loaded successfully.");
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
                    // calculate days since startdate and then divide by count to get a change frequency
                    var daysSinceStart = (DateTime.Now - startdate).TotalDays;
                    var changeFrequency = (int)(daysSinceStart / count);
                    // set the LastModified and ChangeFrequency properties for each article based on startddate and daysSinceStart to evenely distribute the articles
                    foreach (var article in _articles.OrderBy(o => o.Id))
                    {
                        var newModifiedDate = startdate.AddDays(changeFrequency * article.Id);
                        var daysSinceModified = (DateTime.Now - newModifiedDate).TotalDays;

                        article.LastModified = startdate.AddDays(changeFrequency * article.Id).ToString("yyyy-MM-dd");
                        article.ChangeFrequency = daysSinceModified < 60 ? "daily" : daysSinceModified < 120 ? "weekly" : "monthly";
                    }
                    string jsonContent = JsonSerializer.Serialize(
                        _articles,
                        jsonSerializerOptions);
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
        }

        internal List<string> GetKeywords()
        {
            return [];
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
                    newArticle.Id = _articles.Count != 0 ? _articles.Max(article => article.Id) + 1 : 1; // Assign a new ID
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
        /// Generates the Keywords property for an article by fetching the content from the <section> node with id="post"
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

                    // Parse the HTML to extract the content from <article id='post'>
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    var sectionNode = htmlDoc.DocumentNode.SelectSingleNode("//article[@id='post']");

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
                        _logger.LogWarning("No content found in <article id='post'> for article: {ArticleName}", article.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate keywords for article: {ArticleName}", article.Name);
                }
            }
            return article.Keywords;
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

        /// <summary>
        /// Updates the Keywords property for all articles by fetching meta tag keywords from their respective URLs.
        /// </summary>
        public async Task UpdateKeywordsForAllArticlesAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < _articles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("UpdateKeywordsForAllArticlesAsync operation was cancelled.");
                    break;
                }
                if (string.IsNullOrWhiteSpace(_articles[i].Keywords))
                {
                    // add 1 second delay to avoid hitting the OpenAI API rate limit
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);
                    Console.WriteLine($"Updating keywords for article: {_articles[i].Name}");
                    await UpdateArticle(_articles[i]).ConfigureAwait(true);
                    Console.WriteLine($"UPDATED keywords for article: {_articles[i].Name}");
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);

                }
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

                    // Add the atom namespace for the atom:link
                    writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");

                    writer.WriteStartElement("channel");
                    writer.WriteElementString("title", "Mark Hazleton Articles");
                    writer.WriteElementString("link", "https://markhazleton.com/");
                    writer.WriteElementString("description", "Latest articles from Mark Hazleton.");
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
                string siteMapPath = Path.Combine(Path.GetDirectoryName(_filePath), "SiteMap.xml");
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
        }

        /// <summary>
        /// Updates an existing article.
        /// </summary>
        /// <param name="updatedArticle">The updated article model.</param>
        public async Task UpdateArticle(ArticleModel updatedArticle)
        {
            updatedArticle.Keywords = await GenerateKeywordsAsync(updatedArticle) ?? updatedArticle.Keywords;
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
                    _logger.LogInformation("Article '{ArticleName}' updated successfully.", updatedArticle.Name);
                }
                else
                {
                    _logger.LogWarning("Article with ID {Id} not found. Update failed.", updatedArticle.Id);
                }
            }
            await Task.CompletedTask;
        }
    }
}
