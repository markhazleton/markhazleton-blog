using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using mwhWebAdmin.Article;
using mwhWebAdmin.Configuration;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Project;

public class ProjectService
{
    private readonly string _jsonFilePath;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private List<ProjectModel> _projects = new();
    private readonly ILogger<ProjectService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly GitHubIntegrationService? _gitHubService;

    public ProjectService(string jsonFilePath, ILogger<ProjectService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, GitHubIntegrationService? gitHubService = null)
    {
        _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _gitHubService = gitHubService;
        LoadProjects();
    }

    /// <summary>
    /// Auto-generates comprehensive project data using AI and GitHub repository analysis
    /// </summary>
    /// <param name="project">The project to enhance with AI-generated content</param>
    public async Task AutoGenerateProjectDataAsync(ProjectModel project)
    {
        _logger.LogInformation("[ProjectService] AutoGenerateProjectDataAsync called for project: {ProjectTitle}", project.Title);

        try
        {
            // Initialize project fields if needed
            InitializeProjectSeoFields(project);

            // Analyze GitHub repository if URL is provided
            GitHubRepositoryAnalysis? repositoryAnalysis = null;
            if (!string.IsNullOrEmpty(project.Repository?.Url))
            {
                _logger.LogInformation("[ProjectService] Analyzing GitHub repository: {RepositoryUrl}", project.Repository.Url);
                
                if (_gitHubService != null)
                {
                    repositoryAnalysis = await _gitHubService.AnalyzeRepositoryAsync(project.Repository.Url);
                    _logger.LogInformation("[ProjectService] GitHub repository analysis completed");
                }
                else
                {
                    _logger.LogWarning("[ProjectService] GitHubIntegrationService not available, skipping repository analysis");
                }
            }

            // Generate AI-powered project data
            if (repositoryAnalysis != null || !string.IsNullOrEmpty(project.Title) || !string.IsNullOrEmpty(project.Description))
            {
                _logger.LogInformation("[ProjectService] Generating AI-powered project data");
                var projectSeoData = await GenerateProjectSeoDataAsync(project, repositoryAnalysis);

                // Update project with AI-generated data
                ApplyGeneratedDataToProject(project, projectSeoData, repositoryAnalysis);
                _logger.LogInformation("[ProjectService] AI-generated project data applied successfully");
            }
            else
            {
                _logger.LogWarning("[ProjectService] Insufficient data for AI generation - no repository URL, title, or description provided");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] Error in AutoGenerateProjectDataAsync for project: {ProjectTitle}", project.Title);
        }
    }

    /// <summary>
    /// Generates SEO-optimized project data using OpenAI
    /// </summary>
    private async Task<ProjectSeoGenerationResult> GenerateProjectSeoDataAsync(ProjectModel project, GitHubRepositoryAnalysis? repositoryAnalysis)
    {
        try
        {
            _logger.LogInformation("[ProjectService] GenerateProjectSeoDataAsync called");

            var openAiApiKey = _configuration["OPENAI_API_KEY"];
            var openAiApiUrl = "https://api.openai.com/v1/chat/completions";

            if (string.IsNullOrEmpty(openAiApiKey))
            {
                _logger.LogError("[ProjectService] OpenAI API key not configured");
                return new ProjectSeoGenerationResult();
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);

            var systemPrompt = ProjectSeoLlmPromptConfig.GetProjectSeoGenerationPrompt(
                project.Title ?? "New Project",
                repositoryAnalysis ?? new GitHubRepositoryAnalysis());

            var userContent = PrepareUserContentForAnalysis(project, repositoryAnalysis);

            // Use simple JSON format for better compatibility
            return await GenerateWithSimpleJson(httpClient, openAiApiUrl, systemPrompt, userContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] Failed to generate project SEO data using OpenAI API");
            return new ProjectSeoGenerationResult();
        }
    }

    /// <summary>
    /// Generate using simple JSON format
    /// </summary>
    private async Task<ProjectSeoGenerationResult> GenerateWithSimpleJson(HttpClient httpClient, string apiUrl, string systemPrompt, string userContent)
    {
        var enhancedPrompt = systemPrompt + @"

IMPORTANT: Return ONLY a valid JSON object with the following structure (no markdown formatting):
{
  ""projectTitle"": ""string"",
  ""projectDescription"": ""string"",
  ""projectSummary"": ""string"",
  ""keywords"": ""string"",
  ""seoTitle"": ""string"",
  ""metaDescription"": ""string"",
  ""ogTitle"": ""string"",
  ""ogDescription"": ""string"",
  ""twitterTitle"": ""string"",
  ""twitterDescription"": ""string"",
  ""repositoryProvider"": ""string"",
  ""repositoryVisibility"": ""string"",
  ""promotionPipeline"": ""string"",
  ""promotionCurrentStage"": ""string"",
  ""promotionStatus"": ""string""
}";

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = enhancedPrompt },
                new { role = "user", content = userContent }
            },
            max_tokens = 2000,
            temperature = 0.3
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(apiUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("[ProjectService] OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new ProjectSeoGenerationResult();
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonDocument.Parse(responseContent);

        var aiResponse = responseData.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(aiResponse))
        {
            return new ProjectSeoGenerationResult();
        }

        // Clean up the response in case it has markdown formatting
        var cleanedResponse = aiResponse.Trim();
        if (cleanedResponse.StartsWith("```json"))
        {
            cleanedResponse = cleanedResponse.Substring(7);
        }
        if (cleanedResponse.StartsWith("```"))
        {
            cleanedResponse = cleanedResponse.Substring(3);
        }
        if (cleanedResponse.EndsWith("```"))
        {
            cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
        }
        cleanedResponse = cleanedResponse.Trim();

        return JsonSerializer.Deserialize<ProjectSeoGenerationResult>(cleanedResponse) ?? new ProjectSeoGenerationResult();
    }

    /// <summary>
    /// Initializes project SEO fields if they don't exist
    /// </summary>
    private void InitializeProjectSeoFields(ProjectModel project)
    {
        project.Seo ??= new SeoModel();
        project.OpenGraph ??= new OpenGraphModel();
        project.TwitterCard ??= new TwitterCardModel();
        project.Repository ??= new ProjectRepository();
        project.Promotion ??= new ProjectPromotion();
        project.Promotion.Environments ??= new List<PromotionEnvironment>();

        // Ensure we have at least 3 environment slots
        while (project.Promotion.Environments.Count < 3)
        {
            project.Promotion.Environments.Add(new PromotionEnvironment());
        }
    }

    /// <summary>
    /// Prepares user content for AI analysis
    /// </summary>
    private string PrepareUserContentForAnalysis(ProjectModel project, GitHubRepositoryAnalysis? repositoryAnalysis)
    {
        var contentParts = new List<string>();

        // Add existing project information
        if (!string.IsNullOrEmpty(project.Title))
            contentParts.Add($"Current Title: {project.Title}");

        if (!string.IsNullOrEmpty(project.Description))
            contentParts.Add($"Current Description: {project.Description}");

        if (!string.IsNullOrEmpty(project.Link))
            contentParts.Add($"Project URL: {project.Link}");

        if (!string.IsNullOrEmpty(project.Keywords))
            contentParts.Add($"Current Keywords: {project.Keywords}");

        // Add repository information if available
        if (repositoryAnalysis != null)
        {
            contentParts.Add("=== REPOSITORY ANALYSIS ===");
            
            if (!string.IsNullOrEmpty(repositoryAnalysis.Description))
                contentParts.Add($"Repository Description: {repositoryAnalysis.Description}");

            if (!string.IsNullOrEmpty(repositoryAnalysis.Language))
                contentParts.Add($"Primary Language: {repositoryAnalysis.Language}");

            if (repositoryAnalysis.Topics.Any())
                contentParts.Add($"Repository Topics: {string.Join(", ", repositoryAnalysis.Topics)}");

            if (repositoryAnalysis.PackageInfo != null)
            {
                contentParts.Add($"Package Manager: {repositoryAnalysis.PackageInfo.PackageManager}");
                contentParts.Add($"Framework: {repositoryAnalysis.PackageInfo.Framework}");
                contentParts.Add($"Runtime: {repositoryAnalysis.PackageInfo.Runtime}");
                
                if (repositoryAnalysis.PackageInfo.Dependencies.Any())
                    contentParts.Add($"Key Dependencies: {string.Join(", ", repositoryAnalysis.PackageInfo.Dependencies.Take(5))}");
            }

            if (repositoryAnalysis.Workflows.Any())
            {
                var activeWorkflows = repositoryAnalysis.Workflows
                    .Where(w => w.State == "active")
                    .Select(w => w.Name)
                    .ToList();
                    
                if (activeWorkflows.Any())
                    contentParts.Add($"CI/CD Workflows: {string.Join(", ", activeWorkflows)}");
            }

            if (!string.IsNullOrEmpty(repositoryAnalysis.ReadmeContent))
            {
                contentParts.Add("=== README CONTENT ===");
                // Limit README content to prevent token overflow
                var readmeContent = repositoryAnalysis.ReadmeContent;
                if (readmeContent.Length > 2000)
                {
                    readmeContent = readmeContent.Substring(0, 2000) + "... [content truncated]";
                }
                contentParts.Add(readmeContent);
            }
        }

        return string.Join("\n\n", contentParts);
    }

    /// <summary>
    /// Applies generated data to the project model
    /// </summary>
    private void ApplyGeneratedDataToProject(ProjectModel project, ProjectSeoGenerationResult seoData, GitHubRepositoryAnalysis? repositoryAnalysis)
    {
        // Update main project fields
        if (!string.IsNullOrEmpty(seoData.ProjectTitle))
            project.Title = seoData.ProjectTitle;

        if (!string.IsNullOrEmpty(seoData.ProjectDescription))
            project.Description = seoData.ProjectDescription;

        if (!string.IsNullOrEmpty(seoData.ProjectSummary))
            project.Summary = seoData.ProjectSummary;

        if (!string.IsNullOrEmpty(seoData.Keywords))
            project.Keywords = seoData.Keywords;

        // Update SEO fields
        if (project.Seo != null)
        {
            if (!string.IsNullOrEmpty(seoData.SeoTitle))
                project.Seo.Title = seoData.SeoTitle;

            if (!string.IsNullOrEmpty(seoData.MetaDescription))
                project.Seo.Description = seoData.MetaDescription;

            if (!string.IsNullOrEmpty(seoData.Keywords))
                project.Seo.Keywords = seoData.Keywords;

            // Auto-generate canonical URL
            if (string.IsNullOrEmpty(project.Seo.Canonical) && !string.IsNullOrEmpty(project.Slug))
                project.Seo.Canonical = $"https://markhazleton.com/projects/{project.Slug}";
        }

        // Update Open Graph fields
        if (project.OpenGraph != null)
        {
            if (!string.IsNullOrEmpty(seoData.OpenGraphTitle))
                project.OpenGraph.Title = seoData.OpenGraphTitle;

            if (!string.IsNullOrEmpty(seoData.OpenGraphDescription))
                project.OpenGraph.Description = seoData.OpenGraphDescription;

            if (string.IsNullOrEmpty(project.OpenGraph.Type))
                project.OpenGraph.Type = "website";
        }

        // Update Twitter Card fields
        if (project.TwitterCard != null)
        {
            if (!string.IsNullOrEmpty(seoData.TwitterTitle))
                project.TwitterCard.Title = seoData.TwitterTitle;

            if (!string.IsNullOrEmpty(seoData.TwitterDescription))
                project.TwitterCard.Description = seoData.TwitterDescription;
        }

        // Update Repository fields from AI and GitHub analysis
        if (project.Repository != null)
        {
            if (!string.IsNullOrEmpty(seoData.RepositoryProvider))
                project.Repository.Provider = seoData.RepositoryProvider;

            if (!string.IsNullOrEmpty(seoData.RepositoryVisibility))
                project.Repository.Visibility = seoData.RepositoryVisibility;

            if (!string.IsNullOrEmpty(seoData.RepositoryNotes))
                project.Repository.Notes = seoData.RepositoryNotes;

            // Apply GitHub analysis data
            if (repositoryAnalysis != null)
            {
                if (string.IsNullOrEmpty(project.Repository.Provider) && !string.IsNullOrEmpty(project.Repository.Url))
                {
                    if (project.Repository.Url.Contains("github.com"))
                        project.Repository.Provider = "GitHub";
                    else if (project.Repository.Url.Contains("gitlab.com"))
                        project.Repository.Provider = "GitLab";
                    else if (project.Repository.Url.Contains("dev.azure.com"))
                        project.Repository.Provider = "Azure DevOps";
                }

                if (string.IsNullOrEmpty(project.Repository.Visibility))
                    project.Repository.Visibility = repositoryAnalysis.IsPrivate ? "Private" : "Public";

                if (string.IsNullOrEmpty(project.Repository.Branch))
                    project.Repository.Branch = repositoryAnalysis.DefaultBranch ?? "main";
            }
        }

        // Update Promotion fields
        if (project.Promotion != null)
        {
            if (!string.IsNullOrEmpty(seoData.PromotionPipeline))
                project.Promotion.Pipeline = seoData.PromotionPipeline;

            if (!string.IsNullOrEmpty(seoData.PromotionCurrentStage))
                project.Promotion.CurrentStage = seoData.PromotionCurrentStage;

            if (!string.IsNullOrEmpty(seoData.PromotionStatus))
                project.Promotion.Status = seoData.PromotionStatus;

            if (!string.IsNullOrEmpty(seoData.PromotionNotes))
                project.Promotion.Notes = seoData.PromotionNotes;

            // Add suggested environments
            if (seoData.Environments != null && seoData.Environments.Any())
            {
                foreach (var envSuggestion in seoData.Environments)
                {
                    var existingEnv = project.Promotion.Environments
                        .FirstOrDefault(e => string.Equals(e.Name, envSuggestion.Name, StringComparison.OrdinalIgnoreCase));

                    if (existingEnv == null && !string.IsNullOrEmpty(envSuggestion.Name))
                    {
                        project.Promotion.Environments.Add(new PromotionEnvironment
                        {
                            Name = envSuggestion.Name,
                            Url = envSuggestion.Url,
                            Status = envSuggestion.Status ?? "Active",
                            Version = envSuggestion.Version,
                            Notes = envSuggestion.Notes
                        });
                    }
                }
            }
        }

        _logger.LogInformation("[ProjectService] Generated data applied to project: {ProjectTitle}", project.Title);
    }

    private void LoadProjects()
    {
        try
        {
            lock (_lock)
            {
                if (!File.Exists(_jsonFilePath))
                {
                    _projects = new List<ProjectModel>();
                    return;
                }

                string json = File.ReadAllText(_jsonFilePath);
                _projects = JsonSerializer.Deserialize<List<ProjectModel>>(json) ?? new List<ProjectModel>();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load projects from {_jsonFilePath}", ex);
        }
    }

    public List<ProjectModel> GetProjects()
    {
        lock (_lock)
        {
            LoadProjects();
            return _projects.Select(CloneProject).ToList();
        }
    }

    public ProjectModel? GetProjectById(int id)
    {
        lock (_lock)
        {
            var existing = _projects.FirstOrDefault(p => p.Id == id);
            return existing is null ? null : CloneProject(existing);
        }
    }

    public void AddProject(ProjectModel project)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        lock (_lock)
        {
            PrepareForPersistence(project);

            if (project.Id == 0)
            {
                project.Id = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1;
            }

            if (_projects.Any(p => p.Id == project.Id))
            {
                throw new InvalidOperationException($"Project with ID {project.Id} already exists.");
            }

            _projects.Add(CloneProject(project));
            SaveProjects();
        }
    }

    public void UpdateProject(ProjectModel updatedProject)
    {
        if (updatedProject == null)
        {
            throw new ArgumentNullException(nameof(updatedProject));
        }

        lock (_lock)
        {
            var existingProject = _projects.FirstOrDefault(p => p.Id == updatedProject.Id);
            if (existingProject == null)
            {
                throw new InvalidOperationException($"Project with ID {updatedProject.Id} not found.");
            }

            PrepareForPersistence(updatedProject);

            existingProject.Title = updatedProject.Title;
            existingProject.Description = updatedProject.Description;
            existingProject.Link = updatedProject.Link;
            existingProject.Image = updatedProject.Image;
            existingProject.Slug = updatedProject.Slug;
            existingProject.Summary = updatedProject.Summary;
            existingProject.Keywords = updatedProject.Keywords;
            existingProject.Seo = CloneSeoModel(updatedProject.Seo);
            existingProject.OpenGraph = CloneOpenGraphModel(updatedProject.OpenGraph);
            existingProject.TwitterCard = CloneTwitterCardModel(updatedProject.TwitterCard);
            existingProject.Repository = updatedProject.Repository is null ? null : updatedProject.Repository.Clone();
            existingProject.Promotion = updatedProject.Promotion is null ? null : updatedProject.Promotion.Clone();

            SaveProjects();
        }
    }

    public void DeleteProject(int id)
    {
        lock (_lock)
        {
            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {id} not found.");
            }

            _projects.Remove(project);
            SaveProjects();
        }
    }

    private void SaveProjects()
    {
        try
        {
            string json = JsonSerializer.Serialize(_projects, _jsonSerializerOptions);
            File.WriteAllText(_jsonFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save projects to {_jsonFilePath}", ex);
        }
    }

    private static ProjectModel CloneProject(ProjectModel source)
    {
        return new ProjectModel
        {
            Id = source.Id,
            Image = source.Image,
            Title = source.Title,
            Description = source.Description,
            Link = source.Link,
            Slug = source.Slug,
            Summary = source.Summary,
            Keywords = source.Keywords,
            Seo = CloneSeoModel(source.Seo),
            OpenGraph = CloneOpenGraphModel(source.OpenGraph),
            TwitterCard = CloneTwitterCardModel(source.TwitterCard),
            Repository = source.Repository?.Clone(),
            Promotion = source.Promotion?.Clone()
        };
    }

    private static void PrepareForPersistence(ProjectModel project)
    {
        project.Title = project.Title?.Trim() ?? string.Empty;
        project.Description = project.Description?.Trim() ?? string.Empty;
        project.Link = project.Link?.Trim() ?? string.Empty;
        project.Image = project.Image?.Trim() ?? string.Empty;
        project.Slug = Slugify(project.Slug);
        project.Summary = string.IsNullOrWhiteSpace(project.Summary) ? string.Empty : project.Summary.Trim();
        project.Keywords = string.IsNullOrWhiteSpace(project.Keywords) ? string.Empty : project.Keywords.Trim();

        if (project.Repository != null && project.Repository.IsEmpty())
        {
            project.Repository = null;
        }

        if (project.Promotion != null)
        {
            project.Promotion.TrimEmptyEnvironments();

            if (project.Promotion.IsEmpty())
            {
                project.Promotion = null;
            }
        }

        if (project.Seo != null)
        {
            NormalizeSeoModel(project.Seo);
            if (IsSeoEmpty(project.Seo))
            {
                project.Seo = null;
            }
        }

        if (project.OpenGraph != null)
        {
            NormalizeOpenGraph(project.OpenGraph);
            if (IsOpenGraphEmpty(project.OpenGraph))
            {
                project.OpenGraph = null;
            }
        }

        if (project.TwitterCard != null)
        {
            NormalizeTwitterCard(project.TwitterCard);
            if (IsTwitterCardEmpty(project.TwitterCard))
            {
                project.TwitterCard = null;
            }
        }
    }

    private static string Slugify(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return string.Empty;
        }

        var normalized = slug.Trim().ToLowerInvariant();
        normalized = Regex.Replace(normalized, "[^a-z0-9-]+", "-");
        normalized = Regex.Replace(normalized, "-+", "-");
        return normalized.Trim('-');
    }

    private static SeoModel? CloneSeoModel(SeoModel? source)
    {
        if (source == null)
        {
            return null;
        }

        return new SeoModel
        {
            Title = source.Title,
            TitleSuffix = source.TitleSuffix,
            Description = source.Description,
            Keywords = source.Keywords,
            Canonical = source.Canonical,
            Robots = source.Robots
        };
    }

    private static OpenGraphModel? CloneOpenGraphModel(OpenGraphModel? source)
    {
        if (source == null)
        {
            return null;
        }

        return new OpenGraphModel
        {
            Title = source.Title,
            Description = source.Description,
            Type = source.Type,
            Image = source.Image,
            ImageAlt = source.ImageAlt
        };
    }

    private static TwitterCardModel? CloneTwitterCardModel(TwitterCardModel? source)
    {
        if (source == null)
        {
            return null;
        }

        return new TwitterCardModel
        {
            Title = source.Title,
            Description = source.Description,
            Image = source.Image,
            ImageAlt = source.ImageAlt
        };
    }

    private static void NormalizeSeoModel(SeoModel seo)
    {
        seo.Title = TrimOrNull(seo.Title);
        seo.TitleSuffix = TrimOrNull(seo.TitleSuffix);
        seo.Description = TrimOrNull(seo.Description);
        seo.Keywords = TrimOrNull(seo.Keywords);
        seo.Canonical = TrimOrNull(seo.Canonical);
        seo.Robots = TrimOrNull(seo.Robots);
    }

    private static void NormalizeOpenGraph(OpenGraphModel og)
    {
        og.Title = TrimOrNull(og.Title);
        og.Description = TrimOrNull(og.Description);
        og.Type = TrimOrNull(og.Type);
        og.Image = TrimOrNull(og.Image);
        og.ImageAlt = TrimOrNull(og.ImageAlt);
    }

    private static void NormalizeTwitterCard(TwitterCardModel twitter)
    {
        twitter.Title = TrimOrNull(twitter.Title);
        twitter.Description = TrimOrNull(twitter.Description);
        twitter.Image = TrimOrNull(twitter.Image);
        twitter.ImageAlt = TrimOrNull(twitter.ImageAlt);
    }

    private static bool IsSeoEmpty(SeoModel seo)
    {
        return string.IsNullOrWhiteSpace(seo.Title)
               && string.IsNullOrWhiteSpace(seo.TitleSuffix)
               && string.IsNullOrWhiteSpace(seo.Description)
               && string.IsNullOrWhiteSpace(seo.Keywords)
               && string.IsNullOrWhiteSpace(seo.Canonical)
               && string.IsNullOrWhiteSpace(seo.Robots);
    }

    private static bool IsOpenGraphEmpty(OpenGraphModel og)
    {
        return string.IsNullOrWhiteSpace(og.Title)
               && string.IsNullOrWhiteSpace(og.Description)
               && string.IsNullOrWhiteSpace(og.Type)
               && string.IsNullOrWhiteSpace(og.Image)
               && string.IsNullOrWhiteSpace(og.ImageAlt);
    }

    private static bool IsTwitterCardEmpty(TwitterCardModel twitter)
    {
        return string.IsNullOrWhiteSpace(twitter.Title)
               && string.IsNullOrWhiteSpace(twitter.Description)
               && string.IsNullOrWhiteSpace(twitter.Image)
               && string.IsNullOrWhiteSpace(twitter.ImageAlt);
    }

    private static string? TrimOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

public class ProjectModel
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("image")]
    [Display(Name = "Image")]
    public string Image { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    [JsonPropertyName("p")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [JsonPropertyName("d")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Link is required")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [JsonPropertyName("h")]
    [Display(Name = "Link")]
    public string Link { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required")]
    [StringLength(120, ErrorMessage = "Slug cannot exceed 120 characters")]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
    [JsonPropertyName("slug")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(320, ErrorMessage = "Summary cannot exceed 320 characters")]
    [JsonPropertyName("summary")]
    [Display(Name = "Summary")]
    public string Summary { get; set; } = string.Empty;

    [StringLength(320, ErrorMessage = "Keywords cannot exceed 320 characters")]
    [JsonPropertyName("keywords")]
    [Display(Name = "Keywords")]
    public string Keywords { get; set; } = string.Empty;

    [JsonPropertyName("seo")]
    [Display(Name = "SEO Metadata")]
    public SeoModel? Seo { get; set; }

    [JsonPropertyName("og")]
    [Display(Name = "Open Graph Metadata")]
    public OpenGraphModel? OpenGraph { get; set; }

    [JsonPropertyName("twitter")]
    [Display(Name = "Twitter Card Metadata")]
    public TwitterCardModel? TwitterCard { get; set; }

    [JsonPropertyName("repository")]
    [Display(Name = "Repository")]
    public ProjectRepository? Repository { get; set; }

    [JsonPropertyName("promotion")]
    [Display(Name = "Promotion")]
    public ProjectPromotion? Promotion { get; set; }
}

public class ProjectRepository
{
    [Display(Name = "Repository Provider")]
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [Display(Name = "Repository Name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [Display(Name = "Repository URL")]
    [Url(ErrorMessage = "Please enter a valid repository URL")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [Display(Name = "Default Branch")]
    [JsonPropertyName("branch")]
    public string? Branch { get; set; }

    [Display(Name = "Visibility")]
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }

    [Display(Name = "Repository Notes")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Provider)
               && string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrWhiteSpace(Url)
               && string.IsNullOrWhiteSpace(Branch)
               && string.IsNullOrWhiteSpace(Visibility)
               && string.IsNullOrWhiteSpace(Notes);
    }

    public ProjectRepository Clone()
    {
        return new ProjectRepository
        {
            Provider = Provider,
            Name = Name,
            Url = Url,
            Branch = Branch,
            Visibility = Visibility,
            Notes = Notes
        };
    }
}

public class ProjectPromotion
{
    [Display(Name = "Pipeline Name")]
    [JsonPropertyName("pipeline")]
    public string? Pipeline { get; set; }

    [Display(Name = "Current Stage")]
    [JsonPropertyName("currentStage")]
    public string? CurrentStage { get; set; }

    [Display(Name = "Promotion Status")]
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [Display(Name = "Last Promoted On")]
    [DataType(DataType.Date)]
    [JsonPropertyName("lastPromotedOn")]
    public DateTime? LastPromotedOn { get; set; }

    [Display(Name = "Promotion Notes")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("environments")]
    public List<PromotionEnvironment> Environments { get; set; } = new();

    public void TrimEmptyEnvironments()
    {
        if (Environments == null || Environments.Count == 0)
        {
            return;
        }

        Environments = Environments
            .Where(env => env != null && !env.IsEmpty())
            .Select(env => env.Clone())
            .ToList();
    }

    public bool IsEmpty()
    {
        bool hasData =
            !string.IsNullOrWhiteSpace(Pipeline) ||
            !string.IsNullOrWhiteSpace(CurrentStage) ||
            !string.IsNullOrWhiteSpace(Status) ||
            LastPromotedOn != null ||
            !string.IsNullOrWhiteSpace(Notes);

        return !hasData && (Environments == null || Environments.Count == 0);
    }

    public ProjectPromotion Clone()
    {
        return new ProjectPromotion
        {
            Pipeline = Pipeline,
            CurrentStage = CurrentStage,
            Status = Status,
            LastPromotedOn = LastPromotedOn,
            Notes = Notes,
            Environments = Environments?.Select(env => env.Clone()).ToList() ?? new List<PromotionEnvironment>()
        };
    }
}

public class PromotionEnvironment
{
    [Display(Name = "Environment Name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [Display(Name = "Environment URL")]
    [Url(ErrorMessage = "Please enter a valid environment URL")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [Display(Name = "Status")]
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [Display(Name = "Version/Build")]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [Display(Name = "Last Promoted On")]
    [DataType(DataType.Date)]
    [JsonPropertyName("lastPromotedOn")]
    public DateTime? LastPromotedOn { get; set; }

    [Display(Name = "Notes")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrWhiteSpace(Url)
               && string.IsNullOrWhiteSpace(Status)
               && string.IsNullOrWhiteSpace(Version)
               && string.IsNullOrWhiteSpace(Notes)
               && LastPromotedOn == null;
    }

    public PromotionEnvironment Clone()
    {
        return new PromotionEnvironment
        {
            Name = Name,
            Url = Url,
            Status = Status,
            Version = Version,
            LastPromotedOn = LastPromotedOn,
            Notes = Notes
        };
    }
}
