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

                // Perform string replacements for all template fields
                string pugContent = templateContent
                    .Replace("{{title}}", article.Name ?? string.Empty)
                    .Replace("{{subtitle}}", article.Subtitle ?? string.Empty)
                    .Replace("{{description}}", article.Description ?? string.Empty)
                    .Replace("{{keywords}}", article.Keywords ?? string.Empty)
                    .Replace("{{author}}", article.Author ?? "Mark Hazleton")
                    .Replace("{{slug}}", article.Slug ?? string.Empty)
                    .Replace("{{sectionTitle}}", article.Section ?? string.Empty)
                    .Replace("{{summary}}", article.Summary ?? string.Empty)
                    .Replace("{{content}}", article.ArticleContent ?? string.Empty)
                    .Replace("{{conclusionTitle}}", article.ConclusionTitle ?? string.Empty)
                    .Replace("{{conclusionSummary}}", article.ConclusionSummary ?? string.Empty)
                    .Replace("{{conclusionKeyHeading}}", article.ConclusionKeyHeading ?? string.Empty)
                    .Replace("{{conclusionKeyText}}", article.ConclusionKeyText ?? string.Empty)
                    .Replace("{{conclusionText}}", article.ConclusionText ?? string.Empty);
                return pugContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Pug content for article: {ArticleName}", article.Name);
                return string.Empty;
            }
        }        /// <summary>
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

                    // Calculate source file path if not already set
                    if (string.IsNullOrEmpty(article.Source))
                    {
                        article.Source = CalculateSourceFilePath(article.Slug);
                    }
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
        }    /// <summary>
             /// Gets a list of unique keywords from all articles.
             /// </summary>
             /// <returns>A list of unique keywords.</returns>
        internal List<string> GetKeywords()
        {
            lock (_lock)
            {
                try
                {
                    var allKeywords = _articles
                        .Where(a => !string.IsNullOrWhiteSpace(a.Keywords))
                        .SelectMany(a => a.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(k => k)
                        .ToList();

                    return allKeywords;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting keywords from articles");
                    return [];
                }
            }
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

                    // Calculate source file path
                    newArticle.Source = CalculateSourceFilePath(newArticle.Slug);

                    _articles.Add(newArticle);

                    string pugContent = GeneratePugFileContent(newArticle);
                    if (!string.IsNullOrEmpty(pugContent))
                    {
                        // Save the .pug file
                        string pugFilePath = Path.Combine(_articlesDirectory, $"{newArticle.Slug.Replace(".html", string.Empty).Replace("articles/", string.Empty)}.pug");
                        File.WriteAllText(pugFilePath, pugContent);

                        // Recalculate source path after creating the file
                        newArticle.Source = CalculateSourceFilePath(newArticle.Slug);
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
            return article?.Keywords ?? string.Empty;
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
                string? directoryPath = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    _logger.LogError("Unable to determine directory path for RSS feed generation");
                    return;
                }

                string rssFeedPath = Path.Combine(directoryPath, "rss.xml");
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
                string? directoryPath = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    _logger.LogError("Unable to determine directory path for sitemap generation");
                    return;
                }

                string siteMapPath = Path.Combine(directoryPath, "SiteMap.xml");
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
        }        /// <summary>
                 /// Updates an existing article.
                 /// </summary>
                 /// <param name="updatedArticle">The updated article model.</param>
        public async Task UpdateArticle(ArticleModel updatedArticle)
        {
            updatedArticle.Keywords = await GenerateKeywordsAsync(updatedArticle) ?? updatedArticle.Keywords;

            // Calculate source file path if not already set or if slug has changed
            if (string.IsNullOrEmpty(updatedArticle.Source))
            {
                updatedArticle.Source = CalculateSourceFilePath(updatedArticle.Slug);
            }

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

        /// <summary>
        /// Updates source file paths for all articles that don't have them set.
        /// This method helps populate existing articles with source information.
        /// </summary>
        public void UpdateMissingSourcePaths()
        {
            lock (_lock)
            {
                try
                {
                    bool hasChanges = false;
                    foreach (var article in _articles)
                    {
                        if (string.IsNullOrEmpty(article.Source))
                        {
                            string calculatedSource = CalculateSourceFilePath(article.Slug);
                            if (!string.IsNullOrEmpty(calculatedSource))
                            {
                                article.Source = calculatedSource;
                                hasChanges = true;
                                _logger.LogInformation("Updated source path for article '{ArticleName}': {SourcePath}",
                                    article.Name, article.Source);
                            }
                        }
                    }

                    if (hasChanges)
                    {
                        SaveArticles();
                        _logger.LogInformation("Updated source paths for articles saved successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("No articles required source path updates.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update missing source paths.");
                }
            }
        }        /// <summary>
                 /// Calculates the source file path for a PUG file based on the article slug.
                 /// Follows the same logic as the JavaScript findPugFileFromSlug function.
                 /// </summary>
                 /// <param name="slug">The article slug.</param>
                 /// <returns>The full path to the PUG source file starting with /src/pug/, or empty string if not found.</returns>
        private string CalculateSourceFilePath(string slug)
        {
            try
            {
                // If the slug is empty, return empty string
                if (string.IsNullOrEmpty(slug))
                    return string.Empty;                // If the slug ends with .html, replace it with .pug
                // If the slug ends with /, assume it's an index.html
                string pugFileName;
                if (slug.EndsWith(".html"))
                {
                    pugFileName = slug.Replace(".html", ".pug");
                }
                else if (slug.EndsWith("/"))
                {
                    pugFileName = slug + "index.pug";
                }
                else
                {
                    pugFileName = slug + ".pug";
                }

                // Remove any leading / from slug
                string cleanedSlug = pugFileName.StartsWith("/")
                    ? pugFileName.Substring(1)
                    : pugFileName;

                // Extract path components
                string[] slugParts = cleanedSlug.Split('/');
                string fileNamePart = slugParts.LastOrDefault() ?? string.Empty;

                // If the filename part is still empty after handling directory-based URLs, log and return
                if (string.IsNullOrEmpty(fileNamePart))
                {
                    _logger.LogWarning("Could not extract a valid filename from slug after processing: {Slug}", slug);
                    return string.Empty;
                }

                // Get the base directory path (same as _articlesDirectory but up one level to get to pug folder)
                string pugBaseDir = Path.Combine(_filePath.Replace("articles.json", string.Empty), "pug");

                // Build candidate paths to check
                List<string> candidatePaths = new();

                // Build full path from slug directory structure
                if (slugParts.Length > 1)
                {
                    // Try the exact path from the slug
                    string[] pathParts = slugParts.Take(slugParts.Length - 1).ToArray();
                    candidatePaths.Add(Path.Combine(pugBaseDir, Path.Combine(pathParts), fileNamePart));
                }

                // Try the articles directory
                candidatePaths.Add(Path.Combine(pugBaseDir, "articles", fileNamePart));

                // Try the root pug directory
                candidatePaths.Add(Path.Combine(pugBaseDir, fileNamePart));

                // Check all candidate paths
                foreach (string candidatePath in candidatePaths)
                {
                    if (File.Exists(candidatePath) && !Directory.Exists(candidatePath))
                    {
                        // Return path in the format /src/pug/...
                        string relativePath = Path.GetRelativePath(pugBaseDir, candidatePath).Replace('\\', '/');
                        return $"/src/pug/{relativePath}";
                    }

                    // Also check with .pug extension if not already present
                    if (!candidatePath.EndsWith(".pug"))
                    {
                        string pathWithExt = candidatePath + ".pug";
                        if (File.Exists(pathWithExt) && !Directory.Exists(pathWithExt))
                        {
                            // Return path in the format /src/pug/...
                            string relativePath = Path.GetRelativePath(pugBaseDir, pathWithExt).Replace('\\', '/');
                            return $"/src/pug/{relativePath}";
                        }
                    }
                }

                // If direct path lookup failed, search all pug files to find a match by filename
                string[] searchDirectories = new[]
                {
                    Path.Combine(pugBaseDir, "articles"),
                    pugBaseDir
                };

                foreach (string searchDir in searchDirectories)
                {
                    if (!Directory.Exists(searchDir)) continue;

                    string[] pugFiles = Directory.GetFiles(searchDir, "*.pug");

                    // Find a file that matches the slug's filename part
                    string? matchingFile = pugFiles.FirstOrDefault(file =>
                    {
                        string fileName = Path.GetFileName(file);
                        return fileName == fileNamePart || fileName == fileNamePart + ".pug";
                    });

                    if (!string.IsNullOrEmpty(matchingFile))
                    {
                        // Return path in the format /src/pug/...
                        string relativePath = Path.GetRelativePath(pugBaseDir, matchingFile).Replace('\\', '/');
                        return $"/src/pug/{relativePath}";
                    }
                }

                _logger.LogWarning("Could not find a matching Pug file for slug: {Slug}", slug);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding Pug file for slug {Slug}", slug);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a list of available images from the assets directory.
        /// </summary>
        /// <returns>A list of relative image paths.</returns>
        public List<string> GetAvailableImages()
        {
            try
            {
                var baseDirectory = Path.GetDirectoryName(_filePath);
                if (string.IsNullOrEmpty(baseDirectory))
                {
                    _logger.LogError("Unable to determine base directory for image loading");
                    return [];
                }

                var imagesDirectory = Path.Combine(baseDirectory, "assets", "img");

                if (Directory.Exists(imagesDirectory))
                {
                    var images = Directory.GetFiles(imagesDirectory, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => IsImageFile(file))
                        .Select(imagePath => imagePath.Replace(baseDirectory, string.Empty).Replace("\\", "/").TrimStart('/'))
                        .ToList();

                    return images;
                }
                else
                {
                    _logger.LogWarning("Images directory not found: {ImagesDirectory}", imagesDirectory);
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available images");
                return [];
            }
        }

        /// <summary>
        /// Determines if a file is an image based on its extension.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file is an image; otherwise, false.</returns>
        private static bool IsImageFile(string fileName)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" };
            return imageExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());
        }

        /// <summary>
        /// Tests the slug-to-source-path conversion logic for various slug formats.
        /// This method is useful for debugging and validation.
        /// </summary>
        /// <param name="testSlug">The slug to test</param>
        /// <returns>The calculated source path</returns>
        public string TestSlugToSourcePath(string testSlug)
        {
            _logger.LogInformation("Testing slug conversion: {Slug}", testSlug);
            var result = CalculateSourceFilePath(testSlug);
            _logger.LogInformation("Result for slug '{Slug}': {SourcePath}", testSlug, result);
            return result;
        }
    }
}
