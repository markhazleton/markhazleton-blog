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
        
        _logger.LogInformation("[ProjectService] Initialized with file path: {FilePath}", _jsonFilePath);
        
        // Don't load projects in constructor - force fresh reads on every request
        // This ensures we always have the most current data from the file
        _projects = new List<ProjectModel>();
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
                _logger.LogWarning("[ProjectService] OpenAI API key not configured, using fallback generation");
                return GenerateFallbackProjectData(project, repositoryAnalysis);
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);

            var systemPrompt = ProjectSeoLlmPromptConfig.GetProjectSeoGenerationPrompt(
                project.Title ?? "New Project",
                repositoryAnalysis ?? new GitHubRepositoryAnalysis());

            var userContent = PrepareUserContentForAnalysis(project, repositoryAnalysis);

            // Use simple JSON format for better compatibility
            var result = await GenerateWithSimpleJson(httpClient, openAiApiUrl, systemPrompt, userContent);
            
            // Validate that we got meaningful data
            if (string.IsNullOrEmpty(result.ProjectTitle) && string.IsNullOrEmpty(result.SeoTitle) && string.IsNullOrEmpty(result.OpenGraphTitle))
            {
                _logger.LogWarning("[ProjectService] AI response was empty or incomplete, using fallback generation");
                return GenerateFallbackProjectData(project, repositoryAnalysis);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] Failed to generate project SEO data using OpenAI API, using fallback");
            return GenerateFallbackProjectData(project, repositoryAnalysis);
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
  ""projectTitle"": ""Enhanced project title"",
  ""projectDescription"": ""Comprehensive project description"",
  ""projectSummary"": ""Concise project summary for meta descriptions"",
  ""keywords"": ""comma, separated, project, keywords"",
  ""seoTitle"": ""SEO optimized page title"",
  ""seoTitleSuffix"": "" | Mark Hazleton Projects"",
  ""metaDescription"": ""SEO meta description 120-160 characters"",
  ""seoKeywords"": ""seo, specific, keywords"",
  ""canonical"": ""https://markhazleton.com/projects/project-slug"",
  ""robots"": ""index, follow"",
  ""ogTitle"": ""Open Graph title for social sharing"",
  ""ogDescription"": ""Open Graph description for social media"",
  ""ogType"": ""website"",
  ""ogImage"": ""/assets/img/project-card.png"",
  ""ogImageAlt"": ""Project screenshot or logo"",
  ""twitterTitle"": ""Twitter card title (up to 50 chars)"",
  ""twitterDescription"": ""Twitter description for cards"",
  ""twitterImage"": ""/assets/img/twitter-card.png"",
  ""twitterImageAlt"": ""Twitter image alt text"",
  ""repositoryProvider"": ""GitHub"",
  ""repositoryName"": ""owner/repository-name"",
  ""repositoryVisibility"": ""Public or Private"",
  ""repositoryBranch"": ""main"",
  ""repositoryNotes"": ""Key technical notes about the repository"",
  ""promotionPipeline"": ""CI/CD pipeline name"",
  ""promotionCurrentStage"": ""Development, Testing, or Production"",
  ""promotionStatus"": ""Active, In Progress, or Scheduled"",
  ""promotionNotes"": ""Deployment and pipeline notes"",
  ""category"": ""Web Application, API, Library, Tool, etc."",
  ""techStack"": ""Primary technologies used"",
  ""projectType"": ""SPA, REST API, npm Package, etc."",
  ""environments"": [
    {
      ""name"": ""Development"",
      ""url"": ""https://dev.example.com"",
      ""status"": ""Active"",
      ""version"": ""1.0.0-dev"",
      ""notes"": ""Development environment""
    },
    {
      ""name"": ""Production"",
      ""url"": ""https://example.com"",
      ""status"": ""Active"",
      ""version"": ""1.0.0"",
      ""notes"": ""Production environment""
    }
  ]
}";

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = enhancedPrompt },
                new { role = "user", content = userContent }
            },
            max_tokens = 3000,
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
    /// Generates comprehensive fallback project data when AI is not available
    /// </summary>
    private ProjectSeoGenerationResult GenerateFallbackProjectData(ProjectModel project, GitHubRepositoryAnalysis? repositoryAnalysis)
    {
        _logger.LogInformation("[ProjectService] Generating fallback project data");

        var projectTitle = project.Title ?? "Professional Project";
        var projectDescription = project.Description ?? "A comprehensive project showcasing modern development practices.";
        var slug = Slugify(projectTitle);

        return new ProjectSeoGenerationResult
        {
            // Main project fields
            ProjectTitle = $"{projectTitle} - Enhanced",
            ProjectDescription = $"{projectDescription} This project demonstrates professional development practices with modern technologies and comprehensive implementation.",
            ProjectSummary = "A professional project featuring modern technologies, comprehensive documentation, and industry best practices.",
            Keywords = "professional development, modern technologies, web application, portfolio project",

            // SEO fields
            SeoTitle = $"{projectTitle} | Professional Development Portfolio",
            SeoTitleSuffix = " | Mark Hazleton Projects",
            MetaDescription = $"Explore {projectTitle}, a professional development project showcasing modern technologies and best practices. Comprehensive implementation with detailed documentation.",
            SeoKeywords = "professional development, portfolio, web development, modern technologies",
            Canonical = $"https://markhazleton.com/projects/{slug}",
            Robots = "index, follow",

            // Open Graph
            OpenGraphTitle = $"{projectTitle} - Professional Portfolio",
            OpenGraphDescription = $"Discover {projectTitle}, a comprehensive project demonstrating professional development practices and modern technology implementation.",
            OpenGraphType = "website",
            OpenGraphImage = "/assets/img/project-card.png",
            OpenGraphImageAlt = $"Screenshot of {projectTitle} showing professional interface and functionality",

            // Twitter Card
            TwitterTitle = $"{projectTitle} Portfolio",
            TwitterDescription = $"Professional project: {projectTitle}. Modern development with comprehensive features.",
            TwitterImage = "/assets/img/twitter-card.png",
            TwitterImageAlt = $"{projectTitle} preview for social sharing",

            // Repository information
            RepositoryProvider = repositoryAnalysis?.Language switch
            {
                not null when repositoryAnalysis.Language.Contains("C#") => "GitHub",
                not null when repositoryAnalysis.Language.Contains("JavaScript") => "GitHub",
                not null when repositoryAnalysis.Language.Contains("TypeScript") => "GitHub",
                _ => "GitHub"
            },
            RepositoryName = ExtractRepositoryNameFromUrl(project.Repository?.Url) ?? "professional/project-repo",
            RepositoryVisibility = repositoryAnalysis?.IsPrivate == true ? "Private" : "Public",
            RepositoryBranch = repositoryAnalysis?.DefaultBranch ?? "main",
            RepositoryNotes = repositoryAnalysis != null 
                ? $"Professional {repositoryAnalysis.Language} implementation with comprehensive documentation and modern development practices."
                : "Professional implementation with modern development practices and comprehensive documentation.",

            // Promotion pipeline
            PromotionPipeline = repositoryAnalysis?.Workflows.Any() == true ? "GitHub Actions CI/CD" : "Professional Deployment Pipeline",
            PromotionCurrentStage = "Production",
            PromotionStatus = "Active",
            PromotionNotes = "Automated deployment pipeline with quality gates, comprehensive testing, and professional monitoring.",

            // Environment suggestions
            Environments = new List<ProjectEnvironmentSuggestion>
            {
                new()
                {
                    Name = "Development",
                    Url = $"https://dev-{slug}.example.com",
                    Status = "Active",
                    Version = "1.0.0-dev",
                    Notes = "Development environment for feature development and testing"
                },
                new()
                {
                    Name = "Staging",
                    Url = $"https://staging-{slug}.example.com",
                    Status = "Active",
                    Version = "1.0.0-rc",
                    Notes = "Staging environment for final testing and validation"
                },
                new()
                {
                    Name = "Production",
                    Url = $"https://{slug}.example.com",
                    Status = "Active",
                    Version = "1.0.0",
                    Notes = "Live production environment with full monitoring and backup"
                }
            }
        };
    }

    /// <summary>
    /// Extracts repository name from GitHub URL
    /// </summary>
    private string? ExtractRepositoryNameFromUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        var match = System.Text.RegularExpressions.Regex.Match(url, @"github\.com[\/:]([^\/]+)\/([^\/\s]+?)(?:\.git|\/.*)?$");
        return match.Success ? $"{match.Groups[1].Value}/{match.Groups[2].Value}" : null;
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
    /// Applies generated data to the project model with comprehensive field mapping
    /// </summary>
    private void ApplyGeneratedDataToProject(ProjectModel project, ProjectSeoGenerationResult seoData, GitHubRepositoryAnalysis? repositoryAnalysis)
    {
        _logger.LogInformation("[ProjectService] Starting to apply AI-generated data to project");

        // Update main project fields
        if (!string.IsNullOrEmpty(seoData.ProjectTitle))
        {
            project.Title = seoData.ProjectTitle;
            _logger.LogDebug("Applied AI title: {Title}", seoData.ProjectTitle);
        }

        if (!string.IsNullOrEmpty(seoData.ProjectDescription))
        {
            project.Description = seoData.ProjectDescription;
            _logger.LogDebug("Applied AI description");
        }

        if (!string.IsNullOrEmpty(seoData.ProjectSummary))
        {
            project.Summary = seoData.ProjectSummary;
        }

        if (!string.IsNullOrEmpty(seoData.Keywords))
        {
            project.Keywords = seoData.Keywords;
        }

        // ENSURE REQUIRED FIELDS ARE POPULATED
        // If Link is not provided and we have a repository URL, use that as the project link
        if (string.IsNullOrEmpty(project.Link) && !string.IsNullOrEmpty(project.Repository?.Url))
        {
            project.Link = project.Repository.Url;
            _logger.LogDebug("Applied repository URL as project link: {Link}", project.Link);
        }
        // If we still don't have a link, provide a placeholder that meets validation requirements
        else if (string.IsNullOrEmpty(project.Link))
        {
            var slug = Slugify(project.Title ?? "project");
            project.Link = $"https://demo-{slug}.example.com";
            _logger.LogDebug("Applied fallback project link: {Link}", project.Link);
        }

        // Update SEO fields - ENSURE these are applied
        if (project.Seo != null)
        {
            if (!string.IsNullOrEmpty(seoData.SeoTitle))
                project.Seo.Title = seoData.SeoTitle;

            if (!string.IsNullOrEmpty(seoData.SeoTitleSuffix))
                project.Seo.TitleSuffix = seoData.SeoTitleSuffix;

            if (!string.IsNullOrEmpty(seoData.MetaDescription))
                project.Seo.Description = seoData.MetaDescription;

            if (!string.IsNullOrEmpty(seoData.SeoKeywords ?? seoData.Keywords))
                project.Seo.Keywords = seoData.SeoKeywords ?? seoData.Keywords;

            if (!string.IsNullOrEmpty(seoData.Canonical))
                project.Seo.Canonical = seoData.Canonical;

            if (!string.IsNullOrEmpty(seoData.Robots))
                project.Seo.Robots = seoData.Robots;
        }

        // Update Open Graph fields - ENSURE these are applied
        if (project.OpenGraph != null)
        {
            if (!string.IsNullOrEmpty(seoData.OpenGraphTitle))
                project.OpenGraph.Title = seoData.OpenGraphTitle;

            if (!string.IsNullOrEmpty(seoData.OpenGraphDescription))
                project.OpenGraph.Description = seoData.OpenGraphDescription;

            if (!string.IsNullOrEmpty(seoData.OpenGraphType))
                project.OpenGraph.Type = seoData.OpenGraphType;

            if (!string.IsNullOrEmpty(seoData.OpenGraphImage))
                project.OpenGraph.Image = seoData.OpenGraphImage;

            if (!string.IsNullOrEmpty(seoData.OpenGraphImageAlt))
                project.OpenGraph.ImageAlt = seoData.OpenGraphImageAlt;
        }

        // Update Twitter Card fields - ENSURE these are applied
        if (project.TwitterCard != null)
        {
            if (!string.IsNullOrEmpty(seoData.TwitterTitle))
                project.TwitterCard.Title = seoData.TwitterTitle;

            if (!string.IsNullOrEmpty(seoData.TwitterDescription))
                project.TwitterCard.Description = seoData.TwitterDescription;

            if (!string.IsNullOrEmpty(seoData.TwitterImage))
                project.TwitterCard.Image = seoData.TwitterImage;

            if (!string.IsNullOrEmpty(seoData.TwitterImageAlt))
                project.TwitterCard.ImageAlt = seoData.TwitterImageAlt;
        }

        // Update Repository fields
        if (project.Repository != null)
        {
            if (!string.IsNullOrEmpty(seoData.RepositoryProvider))
                project.Repository.Provider = seoData.RepositoryProvider;

            if (!string.IsNullOrEmpty(seoData.RepositoryName))
                project.Repository.Name = seoData.RepositoryName;

            if (!string.IsNullOrEmpty(seoData.RepositoryVisibility))
                project.Repository.Visibility = seoData.RepositoryVisibility;

            if (!string.IsNullOrEmpty(seoData.RepositoryBranch))
                project.Repository.Branch = seoData.RepositoryBranch;

            if (!string.IsNullOrEmpty(seoData.RepositoryNotes))
                project.Repository.Notes = seoData.RepositoryNotes;
        }

        // Update Promotion fields - ENSURE these are applied
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
                project.Promotion.Environments.Clear();
                foreach (var env in seoData.Environments)
                {
                    if (!string.IsNullOrEmpty(env.Name))
                    {
                        project.Promotion.Environments.Add(new PromotionEnvironment
                        {
                            Name = env.Name,
                            Url = env.Url,
                            Status = env.Status ?? "Active",
                            Version = env.Version,
                            Notes = env.Notes
                        });
                    }
                }

                // Ensure we have at least 3 environment slots for the UI
                while (project.Promotion.Environments.Count < 3)
                {
                    project.Promotion.Environments.Add(new PromotionEnvironment());
                }
            }
        }

        _logger.LogInformation("[ProjectService] Generated data applied to project: {ProjectTitle}", project.Title);
        _logger.LogInformation("[ProjectService] Final field status - SEO: {HasSeo}, OpenGraph: {HasOG}, Twitter: {HasTwitter}, Repository: {HasRepo}, Promotion: {HasPromo}, Link: {HasLink}", 
            project.Seo != null && !string.IsNullOrEmpty(project.Seo.Title), 
            project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Title), 
            project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.Title), 
            project.Repository != null && !string.IsNullOrEmpty(project.Repository.Provider), 
            project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.Pipeline),
            !string.IsNullOrEmpty(project.Link));
    }

    private void LoadProjects()
    {
        try
        {
            lock (_lock)
            {
                _logger.LogInformation("[ProjectService] ===== LOADING PROJECTS FROM FILE =====");
                _logger.LogInformation("[ProjectService] File path: {FilePath}", _jsonFilePath);
                
                if (!File.Exists(_jsonFilePath))
                {
                    _logger.LogWarning("[ProjectService] Projects file does not exist, initializing empty list");
                    _projects = new List<ProjectModel>();
                    return;
                }

                _logger.LogInformation("[ProjectService] Reading projects file...");
                string json = File.ReadAllText(_jsonFilePath);
                _logger.LogInformation("[ProjectService] File read successful. JSON length: {Length} characters", json.Length);
                
                // Log first 200 characters for debugging
                var preview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                _logger.LogDebug("[ProjectService] JSON preview: {JsonPreview}", preview);
                
                var loadedProjects = JsonSerializer.Deserialize<List<ProjectModel>>(json) ?? new List<ProjectModel>();
                _projects = loadedProjects;
                
                _logger.LogInformation("[ProjectService] Projects loaded successfully. Count: {Count}", _projects.Count);
                _logger.LogInformation("[ProjectService] ===== PROJECTS LOADED FROM FILE =====");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] ===== FAILED TO LOAD PROJECTS =====");
            throw new InvalidOperationException($"Failed to load projects from {_jsonFilePath}", ex);
        }
    }

    public List<ProjectModel> GetProjects()
    {
        lock (_lock)
        {
            // FORCE FRESH READ: Always reload from file to ensure current data
            LoadProjects();
            return _projects.Select(CloneProject).ToList();
        }
    }

    public ProjectModel? GetProjectById(int id)
    {
        lock (_lock)
        {
            // FORCE FRESH READ: Always reload from file to ensure current data
            LoadProjects();
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
            _logger.LogInformation("[ProjectService] ===== STARTING ADD PROJECT PROCESS =====");
            _logger.LogInformation("[ProjectService] Adding project: {Title}", project.Title);
            _logger.LogInformation("[ProjectService] Current project count: {Count}", _projects.Count);
            _logger.LogInformation("[ProjectService] JSON file path: {FilePath}", _jsonFilePath);
            
            // Log what we have BEFORE PrepareForPersistence
            _logger.LogInformation("[ProjectService] Before PrepareForPersistence - SEO: {HasSeo}, OG: {HasOG}, Twitter: {HasTwitter}, Repo: {HasRepo}, Promotion: {HasPromo}",
                project.Seo != null && !string.IsNullOrEmpty(project.Seo.Title),
                project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Title),
                project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.Title),
                project.Repository != null && !string.IsNullOrEmpty(project.Repository.Provider),
                project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.Pipeline));

            // Log detailed field values before preparation
            _logger.LogInformation("[ProjectService] Pre-preparation field details - Title: {Title}, Description: {Desc}, Link: {Link}, Slug: {Slug}",
                project.Title, 
                project.Description?.Substring(0, Math.Min(50, project.Description?.Length ?? 0)),
                project.Link,
                project.Slug);

            try
            {
                _logger.LogInformation("[ProjectService] Calling PrepareForPersistence...");
                PrepareForPersistence(project);
                _logger.LogInformation("[ProjectService] PrepareForPersistence completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProjectService] ERROR in PrepareForPersistence");
                throw;
            }

            // Log what we have AFTER PrepareForPersistence
            _logger.LogInformation("[ProjectService] After PrepareForPersistence - SEO: {HasSeo}, OG: {HasOG}, Twitter: {HasTwitter}, Repo: {HasRepo}, Promotion: {HasPromo}",
                project.Seo != null && !string.IsNullOrEmpty(project.Seo?.Title),
                project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph?.Title),
                project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard?.Title),
                project.Repository != null && !string.IsNullOrEmpty(project.Repository?.Provider),
                project.Promotion != null && !string.IsNullOrEmpty(project.Promotion?.Pipeline));

            // Log detailed field values after preparation
            _logger.LogInformation("[ProjectService] Post-preparation field details - Title: {Title}, Description: {Desc}, Link: {Link}, Slug: {Slug}",
                project.Title, 
                project.Description?.Substring(0, Math.Min(50, project.Description?.Length ?? 0)),
                project.Link,
                project.Slug);

            // Validate required fields
            var validationErrors = new List<string>();
            if (string.IsNullOrEmpty(project.Title)) validationErrors.Add("Title is empty");
            if (string.IsNullOrEmpty(project.Description)) validationErrors.Add("Description is empty");
            if (string.IsNullOrEmpty(project.Link)) validationErrors.Add("Link is empty");
            if (string.IsNullOrEmpty(project.Slug)) validationErrors.Add("Slug is empty");

            if (validationErrors.Any())
            {
                _logger.LogError("[ProjectService] Validation failed for required fields: {Errors}", string.Join(", ", validationErrors));
                throw new InvalidOperationException($"Required field validation failed: {string.Join(", ", validationErrors)}");
            }

            _logger.LogInformation("[ProjectService] Required field validation passed");

            // Assign ID
            if (project.Id == 0)
            {
                var newId = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1;
                project.Id = newId;
                _logger.LogInformation("[ProjectService] Assigned new ID: {NewId}", newId);
            }
            else
            {
                _logger.LogInformation("[ProjectService] Using existing ID: {ExistingId}", project.Id);
            }

            // Check for duplicate ID
            if (_projects.Any(p => p.Id == project.Id))
            {
                _logger.LogError("[ProjectService] Duplicate ID detected: {ProjectId}", project.Id);
                throw new InvalidOperationException($"Project with ID {project.Id} already exists.");
            }

            _logger.LogInformation("[ProjectService] No duplicate ID found, proceeding with clone and add");

            try
            {
                var clonedProject = CloneProject(project);
                _logger.LogInformation("[ProjectService] Project cloned successfully");
                
                _projects.Add(clonedProject);
                _logger.LogInformation("[ProjectService] Project added to in-memory list. New count: {Count}", _projects.Count);
                
                _logger.LogInformation("[ProjectService] Calling SaveProjects...");
                SaveProjects();
                _logger.LogInformation("[ProjectService] SaveProjects completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProjectService] ERROR during clone/add/save process");
                throw;
            }
            
            _logger.LogInformation("[ProjectService] Project added successfully with ID: {ProjectId}, Total projects: {TotalProjects}", 
                project.Id, _projects.Count);
            _logger.LogInformation("[ProjectService] ===== ADD PROJECT PROCESS COMPLETED =====");
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
            _logger.LogInformation("[ProjectService] ===== STARTING SAVE PROJECTS TO FILE =====");
            _logger.LogInformation("[ProjectService] File path: {FilePath}", _jsonFilePath);
            _logger.LogInformation("[ProjectService] Project count to save: {Count}", _projects.Count);
            
            // Check if directory exists
            var directory = Path.GetDirectoryName(_jsonFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                _logger.LogInformation("[ProjectService] Directory: {Directory}", directory);
                if (!Directory.Exists(directory))
                {
                    _logger.LogWarning("[ProjectService] Directory does not exist, creating: {Directory}", directory);
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    _logger.LogInformation("[ProjectService] Directory exists");
                }
            }

            // Check file permissions before serialization
            if (File.Exists(_jsonFilePath))
            {
                _logger.LogInformation("[ProjectService] File exists, checking write permissions");
                try
                {
                    using var testStream = File.OpenWrite(_jsonFilePath);
                    _logger.LogInformation("[ProjectService] File is writable");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ProjectService] File write permission check failed");
                    throw new InvalidOperationException($"Cannot write to file {_jsonFilePath}: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogInformation("[ProjectService] File does not exist, will be created");
            }

            _logger.LogInformation("[ProjectService] Starting JSON serialization...");
            string json;
            try
            {
                json = JsonSerializer.Serialize(_projects, _jsonSerializerOptions);
                _logger.LogInformation("[ProjectService] JSON serialization successful. JSON length: {Length} characters", json.Length);
                
                // Log first 200 characters of JSON for debugging
                var preview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                _logger.LogDebug("[ProjectService] JSON preview: {JsonPreview}", preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProjectService] JSON serialization failed");
                throw new InvalidOperationException($"Failed to serialize projects to JSON: {ex.Message}", ex);
            }

            _logger.LogInformation("[ProjectService] Writing JSON to file...");
            try
            {
                File.WriteAllText(_jsonFilePath, json);
                _logger.LogInformation("[ProjectService] File write successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProjectService] File write failed");
                throw new InvalidOperationException($"Failed to write JSON to file {_jsonFilePath}: {ex.Message}", ex);
            }

            // Verify the file was written correctly
            if (File.Exists(_jsonFilePath))
            {
                var fileSize = new FileInfo(_jsonFilePath).Length;
                _logger.LogInformation("[ProjectService] File verification successful. File size: {FileSize} bytes", fileSize);
                
                // Try to read back and count projects
                try
                {
                    var readBackJson = File.ReadAllText(_jsonFilePath);
                    var readBackProjects = JsonSerializer.Deserialize<List<ProjectModel>>(readBackJson);
                    _logger.LogInformation("[ProjectService] Read-back verification successful. Projects in file: {Count}", readBackProjects?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ProjectService] Read-back verification failed");
                }
            }
            else
            {
                _logger.LogError("[ProjectService] File verification failed - file does not exist after write");
                throw new InvalidOperationException($"File {_jsonFilePath} does not exist after write operation");
            }

            _logger.LogInformation("[ProjectService] ===== SAVE PROJECTS COMPLETED SUCCESSFULLY =====");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] ===== SAVE PROJECTS FAILED =====");
            throw new InvalidOperationException($"Failed to save projects to {_jsonFilePath}: {ex.Message}", ex);
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
        // Note: Can't use instance logger in static method, but the calling method will log the results
        
        project.Title = project.Title?.Trim() ?? string.Empty;
        project.Description = project.Description?.Trim() ?? string.Empty;
        project.Link = project.Link?.Trim() ?? string.Empty;
        project.Image = project.Image?.Trim() ?? string.Empty;
        
        // Auto-generate slug if missing
        if (string.IsNullOrWhiteSpace(project.Slug) && !string.IsNullOrWhiteSpace(project.Title))
        {
            project.Slug = Slugify(project.Title);
        }
        else
        {
            project.Slug = Slugify(project.Slug);
        }
        
        project.Summary = string.IsNullOrWhiteSpace(project.Summary) ? string.Empty : project.Summary.Trim();
        project.Keywords = string.IsNullOrWhiteSpace(project.Keywords) ? string.Empty : project.Keywords.Trim();

        // Handle Repository
        if (project.Repository != null)
        {
            if (project.Repository.IsEmpty())
            {
                project.Repository = null; // Remove completely empty repository
            }
        }

        // Handle Promotion
        if (project.Promotion != null)
        {
            project.Promotion.TrimEmptyEnvironments();

            if (project.Promotion.IsEmpty())
            {
                project.Promotion = null; // Remove completely empty promotion
            }
        }

        // PRESERVE AI-GENERATED CONTENT: Only remove SEO/Social fields if they are TRULY empty
        // Don't remove fields that contain meaningful AI-generated content
        
        // Handle SEO
        if (project.Seo != null)
        {
            NormalizeSeoModel(project.Seo);
            // Only remove if ALL fields are truly empty (not just whitespace after AI generation)
            if (IsCompletelyEmpty(project.Seo))
            {
                project.Seo = null; // Only remove if completely empty
            }
        }

        // Handle OpenGraph
        if (project.OpenGraph != null)
        {
            NormalizeOpenGraph(project.OpenGraph);
            // Only remove if ALL fields are truly empty (not just whitespace after AI generation)
            if (IsCompletelyEmpty(project.OpenGraph))
            {
                project.OpenGraph = null; // Only remove if completely empty
            }
        }

        // Handle TwitterCard
        if (project.TwitterCard != null)
        {
            NormalizeTwitterCard(project.TwitterCard);
            // Only remove if ALL fields are truly empty (not just whitespace after AI generation)
            if (IsCompletelyEmpty(project.TwitterCard))
            {
                project.TwitterCard = null; // Only remove if completely empty
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

    /// <summary>
    /// More conservative check - only remove objects if they are completely empty
    /// This preserves AI-generated content that might have been populated
    /// </summary>
    private static bool IsCompletelyEmpty(SeoModel seo)
    {
        // Only consider empty if EVERY field is null or truly empty
        return (seo.Title == null || seo.Title.Trim() == string.Empty)
               && (seo.TitleSuffix == null || seo.TitleSuffix.Trim() == string.Empty)
               && (seo.Description == null || seo.Description.Trim() == string.Empty)
               && (seo.Keywords == null || seo.Keywords.Trim() == string.Empty)
               && (seo.Canonical == null || seo.Canonical.Trim() == string.Empty)
               && (seo.Robots == null || seo.Robots.Trim() == string.Empty);
    }

    /// <summary>
    /// More conservative check - only remove objects if they are completely empty
    /// This preserves AI-generated content that might have been populated
    /// </summary>
    private static bool IsCompletelyEmpty(OpenGraphModel og)
    {
        // Only consider empty if EVERY field is null or truly empty
        return (og.Title == null || og.Title.Trim() == string.Empty)
               && (og.Description == null || og.Description.Trim() == string.Empty)
               && (og.Type == null || og.Type.Trim() == string.Empty)
               && (og.Image == null || og.Image.Trim() == string.Empty)
               && (og.ImageAlt == null || og.ImageAlt.Trim() == string.Empty);
    }

    /// <summary>
    /// More conservative check - only remove objects if they are completely empty
    /// This preserves AI-generated content that might have been populated
    /// </summary>
    private static bool IsCompletelyEmpty(TwitterCardModel twitter)
    {
        // Only consider empty if EVERY field is null or truly empty
        return (twitter.Title == null || twitter.Title.Trim() == string.Empty)
               && (twitter.Description == null || twitter.Description.Trim() == string.Empty)
               && (twitter.Image == null || twitter.Image.Trim() == string.Empty)
               && (twitter.ImageAlt == null || twitter.ImageAlt.Trim() == string.Empty);
    }

    private static string? TrimOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    /// <summary>
    /// Debug method to verify current projects in file and memory
    /// </summary>
    public (int memoryCount, int fileCount, string filePath) GetProjectCounts()
    {
        lock (_lock)
        {
            var memoryCount = _projects.Count;
            var fileCount = 0;
            
            try
            {
                if (File.Exists(_jsonFilePath))
                {
                    var json = File.ReadAllText(_jsonFilePath);
                    var fileProjects = JsonSerializer.Deserialize<List<ProjectModel>>(json);
                    fileCount = fileProjects?.Count ?? 0;
                    
                    _logger.LogInformation("[ProjectService] Project count verification - Memory: {MemoryCount}, File: {FileCount}", 
                        memoryCount, fileCount);
                }
                else
                {
                    _logger.LogWarning("[ProjectService] Projects file does not exist: {FilePath}", _jsonFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProjectService] Error reading projects file for count verification");
            }

            _logger.LogInformation("[ProjectService] Project counts - Memory: {MemoryCount}, File: {FileCount}, Path: {FilePath}",
                memoryCount, fileCount, _jsonFilePath);
            
            return (memoryCount, fileCount, _jsonFilePath);
        }
    }

    /// <summary>
    /// Quick method to get just the current project count from file
    /// </summary>
    public int GetProjectCountFromFile()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                _logger.LogInformation("[ProjectService] Projects file does not exist, count is 0");
                return 0;
            }

            var json = File.ReadAllText(_jsonFilePath);
            var projects = JsonSerializer.Deserialize<List<ProjectModel>>(json);
            var count = projects?.Count ?? 0;
            
            _logger.LogInformation("[ProjectService] Current project count from file: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProjectService] Error getting project count from file");
            return 0;
        }
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
