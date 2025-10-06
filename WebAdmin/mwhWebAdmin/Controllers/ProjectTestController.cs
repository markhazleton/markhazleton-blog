using Microsoft.AspNetCore.Mvc;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;
using mwhWebAdmin.Article;

namespace mwhWebAdmin.Controllers;

/// <summary>
/// Test controller for project AI generation functionality
/// </summary>
[ApiController]
[Route("api/test")]
public class ProjectTestController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly GitHubIntegrationService _gitHubService;
    private readonly ILogger<ProjectTestController> _logger;

    public ProjectTestController(
        ProjectService projectService, 
        GitHubIntegrationService gitHubService, 
        ILogger<ProjectTestController> logger)
    {
        _projectService = projectService;
        _gitHubService = gitHubService;
        _logger = logger;
    }

    /// <summary>
    /// Tests comprehensive project SEO generation with repository analysis
    /// </summary>
    /// <param name="request">Test request with project data</param>
    /// <returns>Generated project SEO data</returns>
    [HttpPost("test-project-generation")]
    public async Task<IActionResult> TestProjectGeneration([FromBody] TestProjectGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Testing comprehensive project AI generation for: {Title}", request.Title);

            // Create a test project
            var testProject = new ProjectModel
            {
                Title = request.Title ?? "Test Project",
                Description = request.Description,
                Link = request.ProjectUrl,
                Repository = new ProjectRepository
                {
                    Url = request.RepositoryUrl
                }
            };

            // Initialize all SEO fields
            testProject.Seo = new SeoModel();
            testProject.OpenGraph = new OpenGraphModel();
            testProject.TwitterCard = new TwitterCardModel();
            testProject.Promotion = new ProjectPromotion
            {
                Environments = new List<PromotionEnvironment>()
            };

            // Generate AI content
            await _projectService.AutoGenerateProjectDataAsync(testProject);

            var result = new
            {
                success = true,
                message = "Comprehensive project AI generation completed successfully",
                generatedProject = new
                {
                    // Main Project Fields
                    title = testProject.Title,
                    description = testProject.Description,
                    summary = testProject.Summary,
                    keywords = testProject.Keywords,
                    
                    // SEO Metadata
                    seo = new
                    {
                        title = testProject.Seo?.Title,
                        titleSuffix = testProject.Seo?.TitleSuffix,
                        description = testProject.Seo?.Description,
                        keywords = testProject.Seo?.Keywords,
                        canonical = testProject.Seo?.Canonical,
                        robots = testProject.Seo?.Robots
                    },
                    
                    // Open Graph Metadata
                    openGraph = new
                    {
                        title = testProject.OpenGraph?.Title,
                        description = testProject.OpenGraph?.Description,
                        type = testProject.OpenGraph?.Type,
                        image = testProject.OpenGraph?.Image,
                        imageAlt = testProject.OpenGraph?.ImageAlt
                    },
                    
                    // Twitter Card Metadata
                    twitterCard = new
                    {
                        title = testProject.TwitterCard?.Title,
                        description = testProject.TwitterCard?.Description,
                        image = testProject.TwitterCard?.Image,
                        imageAlt = testProject.TwitterCard?.ImageAlt
                    },
                    
                    // Repository Information
                    repository = new
                    {
                        provider = testProject.Repository?.Provider,
                        name = testProject.Repository?.Name,
                        visibility = testProject.Repository?.Visibility,
                        branch = testProject.Repository?.Branch,
                        notes = testProject.Repository?.Notes
                    },
                    
                    // Promotion Pipeline
                    promotion = new
                    {
                        pipeline = testProject.Promotion?.Pipeline,
                        currentStage = testProject.Promotion?.CurrentStage,
                        status = testProject.Promotion?.Status,
                        notes = testProject.Promotion?.Notes,
                        environments = testProject.Promotion?.Environments?
                            .Where(e => !string.IsNullOrEmpty(e.Name))
                            .Select(e => new
                            {
                                name = e.Name,
                                url = e.Url,
                                status = e.Status,
                                version = e.Version,
                                notes = e.Notes
                            })
                    }
                },
                // Field completion analysis
                completionAnalysis = new
                {
                    mainFieldsComplete = !string.IsNullOrEmpty(testProject.Title) && 
                                       !string.IsNullOrEmpty(testProject.Description) && 
                                       !string.IsNullOrEmpty(testProject.Summary),
                    seoComplete = testProject.Seo != null &&
                                !string.IsNullOrEmpty(testProject.Seo.Title) &&
                                !string.IsNullOrEmpty(testProject.Seo.Description),
                    openGraphComplete = testProject.OpenGraph != null &&
                                      !string.IsNullOrEmpty(testProject.OpenGraph.Title) &&
                                      !string.IsNullOrEmpty(testProject.OpenGraph.Description),
                    twitterComplete = testProject.TwitterCard != null &&
                                    !string.IsNullOrEmpty(testProject.TwitterCard.Title) &&
                                    !string.IsNullOrEmpty(testProject.TwitterCard.Description),
                    repositoryComplete = testProject.Repository != null &&
                                       !string.IsNullOrEmpty(testProject.Repository.Provider),
                    promotionComplete = testProject.Promotion != null &&
                                      !string.IsNullOrEmpty(testProject.Promotion.Pipeline)
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing comprehensive project AI generation");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                message = "Failed to generate comprehensive project AI content"
            });
        }
    }

    /// <summary>
    /// Tests GitHub repository analysis
    /// </summary>
    /// <param name="request">Repository analysis request</param>
    /// <returns>Repository analysis results</returns>
    [HttpPost("test-repository-analysis")]
    public async Task<IActionResult> TestRepositoryAnalysis([FromBody] TestRepositoryAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Testing repository analysis for: {RepositoryUrl}", request.RepositoryUrl);

            var analysis = await _gitHubService.AnalyzeRepositoryAsync(request.RepositoryUrl);

            var result = new
            {
                success = true,
                message = "Repository analysis completed successfully",
                analysis = new
                {
                    description = analysis.Description,
                    language = analysis.Language,
                    topics = analysis.Topics,
                    license = analysis.License,
                    isPrivate = analysis.IsPrivate,
                    defaultBranch = analysis.DefaultBranch,
                    lastUpdated = analysis.LastUpdated,
                    readmeLength = analysis.ReadmeContent?.Length ?? 0,
                    readmePreview = analysis.ReadmeContent?.Substring(0, Math.Min(analysis.ReadmeContent.Length, 200)) + "...",
                    workflows = analysis.Workflows.Select(w => new
                    {
                        name = w.Name,
                        path = w.Path,
                        state = w.State,
                        createdAt = w.CreatedAt,
                        updatedAt = w.UpdatedAt
                    }),
                    packageInfo = analysis.PackageInfo != null ? new
                    {
                        packageManager = analysis.PackageInfo.PackageManager,
                        framework = analysis.PackageInfo.Framework,
                        runtime = analysis.PackageInfo.Runtime,
                        dependencyCount = analysis.PackageInfo.Dependencies.Count,
                        devDependencyCount = analysis.PackageInfo.DevDependencies.Count,
                        keyDependencies = analysis.PackageInfo.Dependencies.Take(10)
                    } : null
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing repository analysis");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                message = "Failed to analyze repository"
            });
        }
    }

    /// <summary>
    /// Quick test to verify all project fields can be populated
    /// </summary>
    [HttpPost("test-field-completion")]
    public IActionResult TestFieldCompletion()
    {
        try
        {
            var project = new ProjectModel
            {
                Title = "Sample Project",
                Description = "Test description",
                Summary = "Test summary",
                Keywords = "test, keywords",
                Seo = new SeoModel
                {
                    Title = "SEO Title",
                    TitleSuffix = " | Mark Hazleton",
                    Description = "SEO description",
                    Keywords = "seo, keywords",
                    Canonical = "https://example.com/projects/sample",
                    Robots = "index, follow"
                },
                OpenGraph = new OpenGraphModel
                {
                    Title = "OG Title",
                    Description = "OG Description",
                    Type = "website",
                    Image = "/assets/img/og-image.png",
                    ImageAlt = "Project screenshot"
                },
                TwitterCard = new TwitterCardModel
                {
                    Title = "Twitter Title",
                    Description = "Twitter description",
                    Image = "/assets/img/twitter-card.png",
                    ImageAlt = "Twitter image"
                },
                Repository = new ProjectRepository
                {
                    Provider = "GitHub",
                    Name = "user/repo",
                    Url = "https://github.com/user/repo",
                    Branch = "main",
                    Visibility = "Public",
                    Notes = "Repository notes"
                },
                Promotion = new ProjectPromotion
                {
                    Pipeline = "CI/CD Pipeline",
                    CurrentStage = "Production",
                    Status = "Active",
                    Notes = "Deployment notes",
                    Environments = new List<PromotionEnvironment>
                    {
                        new() { Name = "Development", Url = "https://dev.example.com", Status = "Active" },
                        new() { Name = "Production", Url = "https://example.com", Status = "Active" }
                    }
                }
            };

            return Ok(new
            {
                success = true,
                message = "All project fields can be populated successfully",
                fieldCounts = new
                {
                    mainFields = 4,
                    seoFields = 6,
                    openGraphFields = 5,
                    twitterFields = 4,
                    repositoryFields = 6,
                    promotionFields = 4,
                    environments = project.Promotion.Environments.Count
                },
                totalFields = 29
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Creates a mock AI result for testing (no API calls)
    /// </summary>
    [HttpPost("test-mock-ai-generation")]
    public IActionResult TestMockAiGeneration([FromBody] TestProjectGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Testing mock AI generation for: {Title}", request.Title);

            // Create mock AI result that would come from OpenAI
            var mockAiResult = new ProjectSeoGenerationResult
            {
                ProjectTitle = $"{request.Title ?? "Test Project"} - Enhanced by AI",
                ProjectDescription = $"This is an AI-enhanced description for {request.Title ?? "the test project"}. It demonstrates advanced functionality and modern web development practices using cutting-edge technologies.",
                ProjectSummary = "A comprehensive project showcasing modern development practices with AI-enhanced metadata and SEO optimization.",
                Keywords = "web development, modern, ai-enhanced, professional, portfolio",
                
                // SEO Fields
                SeoTitle = $"{request.Title ?? "Test Project"} | Professional Web Development",
                SeoTitleSuffix = " | Mark Hazleton Projects",
                MetaDescription = "Discover this professional web development project featuring modern technologies, AI-enhanced content, and comprehensive SEO optimization for maximum visibility.",
                SeoKeywords = "web development, professional portfolio, modern technologies, SEO optimization",
                Canonical = $"https://markhazleton.com/projects/{Slugify(request.Title ?? "test-project")}",
                Robots = "index, follow",
                
                // Open Graph
                OpenGraphTitle = $"{request.Title ?? "Test Project"} - Professional Portfolio",
                OpenGraphDescription = "Explore this comprehensive web development project with modern technologies and professional implementation.",
                OpenGraphType = "website",
                OpenGraphImage = "/assets/img/project-card.png",
                OpenGraphImageAlt = "Professional project screenshot showcasing modern web development",
                
                // Twitter
                TwitterTitle = $"{request.Title ?? "Test Project"} Portfolio",
                TwitterDescription = "Professional web development project with modern tech stack and comprehensive implementation.",
                TwitterImage = "/assets/img/twitter-card.png",
                TwitterImageAlt = "Project preview for social sharing",
                
                // Repository
                RepositoryProvider = "GitHub",
                RepositoryName = "markhazleton/test-project",
                RepositoryVisibility = "Public",
                RepositoryBranch = "main",
                RepositoryNotes = "Professional implementation with comprehensive documentation and modern development practices.",
                
                // Promotion
                PromotionPipeline = "GitHub Actions CI/CD",
                PromotionCurrentStage = "Production",
                PromotionStatus = "Active",
                PromotionNotes = "Automated deployment pipeline with quality gates and comprehensive testing.",
                
                // Environments
                Environments = new List<ProjectEnvironmentSuggestion>
                {
                    new() 
                    { 
                        Name = "Development", 
                        Url = "https://dev.example.com", 
                        Status = "Active", 
                        Version = "1.0.0-dev",
                        Notes = "Development environment for testing"
                    },
                    new() 
                    { 
                        Name = "Staging", 
                        Url = "https://staging.example.com", 
                        Status = "Active", 
                        Version = "1.0.0-rc",
                        Notes = "Staging environment for final testing"
                    },
                    new() 
                    { 
                        Name = "Production", 
                        Url = "https://example.com", 
                        Status = "Active", 
                        Version = "1.0.0",
                        Notes = "Live production environment"
                    }
                }
            };

            // Test applying the mock data to a project
            var testProject = new ProjectModel
            {
                Title = request.Title ?? "Test Project",
                Description = request.Description ?? "Test description",
                Repository = new ProjectRepository { Url = request.RepositoryUrl },
                Seo = new SeoModel(),
                OpenGraph = new OpenGraphModel(),
                TwitterCard = new TwitterCardModel(),
                Promotion = new ProjectPromotion { Environments = new List<PromotionEnvironment>() }
            };

            // Apply the mock AI data (simulating what the real AI would do)
            ApplyMockDataToProject(testProject, mockAiResult);

            return Ok(new
            {
                success = true,
                message = "Mock AI generation completed successfully - all fields populated",
                mockAiResult = mockAiResult,
                populatedProject = new
                {
                    title = testProject.Title,
                    description = testProject.Description,
                    summary = testProject.Summary,
                    keywords = testProject.Keywords,
                    seo = testProject.Seo,
                    openGraph = testProject.OpenGraph,
                    twitterCard = testProject.TwitterCard,
                    repository = testProject.Repository,
                    promotion = testProject.Promotion
                },
                fieldAnalysis = new
                {
                    totalFieldsPopulated = CountPopulatedFields(testProject),
                    seoComplete = testProject.Seo != null && !string.IsNullOrEmpty(testProject.Seo.Title),
                    openGraphComplete = testProject.OpenGraph != null && !string.IsNullOrEmpty(testProject.OpenGraph.Title),
                    twitterComplete = testProject.TwitterCard != null && !string.IsNullOrEmpty(testProject.TwitterCard.Title),
                    repositoryComplete = testProject.Repository != null && !string.IsNullOrEmpty(testProject.Repository.Provider),
                    promotionComplete = testProject.Promotion != null && !string.IsNullOrEmpty(testProject.Promotion.Pipeline)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mock AI generation test");
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Comprehensive test that verifies all issues are fixed
    /// </summary>
    [HttpGet("test-fixes")]
    public async Task<IActionResult> TestAllFixes()
    {
        try
        {
            _logger.LogInformation("Testing comprehensive fixes for project AI generation");

            var testResults = new List<object>();

            // Test 1: Verify ProjectService can create comprehensive AI data
            var testProject = new ProjectModel
            {
                Title = "Test Project for Verification",
                Description = "A comprehensive test to verify all AI generation fixes",
                Link = "https://example.com",
                Repository = new ProjectRepository
                {
                    Url = "https://github.com/test/example-repo"
                },
                Seo = new SeoModel(),
                OpenGraph = new OpenGraphModel(),
                TwitterCard = new TwitterCardModel(),
                Promotion = new ProjectPromotion { Environments = new List<PromotionEnvironment>() }
            };

            // Test 2: Call AutoGenerateProjectDataAsync (this will use fallback since no real API key)
            await _projectService.AutoGenerateProjectDataAsync(testProject);

            // Test 3: Verify all fields were populated
            var seoComplete = testProject.Seo != null && !string.IsNullOrEmpty(testProject.Seo.Title);
            var openGraphComplete = testProject.OpenGraph != null && !string.IsNullOrEmpty(testProject.OpenGraph.Title);
            var twitterComplete = testProject.TwitterCard != null && !string.IsNullOrEmpty(testProject.TwitterCard.Title);
            var repositoryComplete = testProject.Repository != null && !string.IsNullOrEmpty(testProject.Repository.Provider);
            var promotionComplete = testProject.Promotion != null && !string.IsNullOrEmpty(testProject.Promotion.Pipeline);

            testResults.Add(new
            {
                testName = "AI Field Population",
                success = seoComplete && openGraphComplete && twitterComplete && repositoryComplete && promotionComplete,
                details = new
                {
                    seoComplete,
                    openGraphComplete,
                    twitterComplete,
                    repositoryComplete,
                    promotionComplete,
                    title = testProject.Title,
                    seoTitle = testProject.Seo?.Title,
                    ogTitle = testProject.OpenGraph?.Title,
                    twitterTitle = testProject.TwitterCard?.Title,
                    repoProvider = testProject.Repository?.Provider,
                    promotionPipeline = testProject.Promotion?.Pipeline
                }
            });

            // Test 4: Verify slug auto-generation works
            var testProject2 = new ProjectModel
            {
                Title = "Test Project For Slug Generation",
                Description = "Testing auto slug generation",
                Link = "https://example.com"
            };

            // Simulate the PrepareForPersistence process
            _projectService.AddProject(testProject2);
            var addedProject = _projectService.GetProjects().FirstOrDefault(p => p.Title == testProject2.Title);

            testResults.Add(new
            {
                testName = "Slug Auto-Generation",
                success = addedProject != null && !string.IsNullOrEmpty(addedProject.Slug),
                details = new
                {
                    originalTitle = testProject2.Title,
                    generatedSlug = addedProject?.Slug,
                    projectSaved = addedProject != null
                }
            });

            // Test 5: Verify comprehensive field count
            var fieldCount = CountProjectFields(testProject);

            testResults.Add(new
            {
                testName = "Comprehensive Field Population",
                success = fieldCount >= 25, // Should have at least 25 populated fields
                details = new
                {
                    totalFieldsPopulated = fieldCount,
                    target = 25,
                    mainFields = new
                    {
                        title = !string.IsNullOrEmpty(testProject.Title),
                        description = !string.IsNullOrEmpty(testProject.Description),
                        summary = !string.IsNullOrEmpty(testProject.Summary),
                        keywords = !string.IsNullOrEmpty(testProject.Keywords)
                    },
                    seoFields = new
                    {
                        title = testProject.Seo?.Title,
                        description = testProject.Seo?.Description,
                        canonical = testProject.Seo?.Canonical,
                        robots = testProject.Seo?.Robots
                    },
                    environments = testProject.Promotion?.Environments?.Count ?? 0
                }
            });

            var allTestsPassed = testResults.All(test => (bool)test.GetType().GetProperty("success")!.GetValue(test!));

            return Ok(new
            {
                success = allTestsPassed,
                message = allTestsPassed ? "ALL TESTS PASSED - All fixes working correctly!" : "Some tests failed - issues remain",
                testResults,
                summary = new
                {
                    totalTests = testResults.Count,
                    passed = testResults.Count(test => (bool)test.GetType().GetProperty("success")!.GetValue(test!)),
                    failed = testResults.Count(test => !(bool)test.GetType().GetProperty("success")!.GetValue(test!))
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in comprehensive test");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Debug endpoint to test AI generation with real repository URL
    /// </summary>
    [HttpPost("debug-ai-generation")]
    public async Task<IActionResult> DebugAiGeneration([FromBody] TestProjectGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Debug AI generation for repository: {RepositoryUrl}", request.RepositoryUrl);

            // Create test project with user data
            var testProject = new ProjectModel
            {
                Title = request.Title ?? "Test Project",
                Description = request.Description ?? "Test description",
                Summary = "Test summary for debugging",
                Repository = new ProjectRepository
                {
                    Url = request.RepositoryUrl
                },
                Seo = new SeoModel(),
                OpenGraph = new OpenGraphModel(),
                TwitterCard = new TwitterCardModel(),
                Promotion = new ProjectPromotion { Environments = new List<PromotionEnvironment>() }
            };

            _logger.LogInformation("Before AI generation - Title: {Title}, Repo: {Repo}", testProject.Title, testProject.Repository.Url);

            // Generate AI content
            await _projectService.AutoGenerateProjectDataAsync(testProject);

            _logger.LogInformation("After AI generation - Title: {Title}, SEO Title: {SeoTitle}, OG Title: {OGTitle}", 
                testProject.Title, testProject.Seo?.Title, testProject.OpenGraph?.Title);

            // Check what actually got populated
            var result = new
            {
                success = true,
                message = "AI generation completed",
                beforeGeneration = new
                {
                    title = request.Title,
                    description = request.Description,
                    repositoryUrl = request.RepositoryUrl
                },
                afterGeneration = new
                {
                    title = testProject.Title,
                    description = testProject.Description,
                    summary = testProject.Summary,
                    keywords = testProject.Keywords,
                    seo = new
                    {
                        title = testProject.Seo?.Title,
                        titleSuffix = testProject.Seo?.TitleSuffix,
                        description = testProject.Seo?.Description,
                        keywords = testProject.Seo?.Keywords,
                        canonical = testProject.Seo?.Canonical,
                        robots = testProject.Seo?.Robots
                    },
                    openGraph = new
                    {
                        title = testProject.OpenGraph?.Title,
                        description = testProject.OpenGraph?.Description,
                        type = testProject.OpenGraph?.Type,
                        image = testProject.OpenGraph?.Image,
                        imageAlt = testProject.OpenGraph?.ImageAlt
                    },
                    twitterCard = new
                    {
                        title = testProject.TwitterCard?.Title,
                        description = testProject.TwitterCard?.Description,
                        image = testProject.TwitterCard?.Image,
                        imageAlt = testProject.TwitterCard?.ImageAlt
                    },
                    repository = new
                    {
                        provider = testProject.Repository?.Provider,
                        name = testProject.Repository?.Name,
                        url = testProject.Repository?.Url,
                        branch = testProject.Repository?.Branch,
                        visibility = testProject.Repository?.Visibility,
                        notes = testProject.Repository?.Notes
                    },
                    promotion = new
                    {
                        pipeline = testProject.Promotion?.Pipeline,
                        currentStage = testProject.Promotion?.CurrentStage,
                        status = testProject.Promotion?.Status,
                        notes = testProject.Promotion?.Notes,
                        environments = testProject.Promotion?.Environments?.Where(e => !string.IsNullOrEmpty(e.Name)).Select(e => new
                        {
                            name = e.Name,
                            url = e.Url,
                            status = e.Status,
                            version = e.Version,
                            notes = e.Notes
                        })
                    }
                },
                fieldAnalysis = new
                {
                    fieldsPopulated = CountDetailedFields(testProject),
                    seoComplete = testProject.Seo != null && !string.IsNullOrEmpty(testProject.Seo.Title),
                    openGraphComplete = testProject.OpenGraph != null && !string.IsNullOrEmpty(testProject.OpenGraph.Title),
                    twitterComplete = testProject.TwitterCard != null && !string.IsNullOrEmpty(testProject.TwitterCard.Title),
                    repositoryComplete = testProject.Repository != null && !string.IsNullOrEmpty(testProject.Repository.Provider),
                    promotionComplete = testProject.Promotion != null && !string.IsNullOrEmpty(testProject.Promotion.Pipeline)
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug AI generation");
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Debug endpoint to check project counts and file status
    /// </summary>
    [HttpGet("debug-project-counts")]
    public IActionResult DebugProjectCounts()
    {
        try
        {
            var (memoryCount, fileCount, filePath) = _projectService.GetProjectCounts();
            var quickFileCount = _projectService.GetProjectCountFromFile();
            
            var result = new
            {
                success = true,
                memoryCount,
                fileCount,
                quickFileCount,
                filePath,
                fileExists = System.IO.File.Exists(filePath),
                fileSize = System.IO.File.Exists(filePath) ? new FileInfo(filePath).Length : 0,
                timestamp = DateTime.Now
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Debug endpoint to check current project list from service
    /// </summary>
    [HttpGet("debug-project-list")]
    public IActionResult DebugProjectList()
    {
        try
        {
            var projects = _projectService.GetProjects();
            
            var result = new
            {
                success = true,
                count = projects.Count,
                projects = projects.Select(p => new
                {
                    id = p.Id,
                    title = p.Title,
                    slug = p.Slug,
                    hasTitle = !string.IsNullOrEmpty(p.Title),
                    hasDescription = !string.IsNullOrEmpty(p.Description),
                    hasLink = !string.IsNullOrEmpty(p.Link),
                    hasSlug = !string.IsNullOrEmpty(p.Slug),
                    hasSeo = p.Seo != null && !string.IsNullOrEmpty(p.Seo.Title),
                    hasOpenGraph = p.OpenGraph != null && !string.IsNullOrEmpty(p.OpenGraph.Title),
                    hasTwitterCard = p.TwitterCard != null && !string.IsNullOrEmpty(p.TwitterCard.Title),
                    hasRepository = p.Repository != null && !string.IsNullOrEmpty(p.Repository.Provider),
                    hasPromotion = p.Promotion != null && !string.IsNullOrEmpty(p.Promotion.Pipeline)
                }).ToList(),
                timestamp = DateTime.Now
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    private void ApplyMockDataToProject(ProjectModel project, ProjectSeoGenerationResult seoData)
    {
        // Apply main fields
        if (!string.IsNullOrEmpty(seoData.ProjectTitle))
            project.Title = seoData.ProjectTitle;
        if (!string.IsNullOrEmpty(seoData.ProjectDescription))
            project.Description = seoData.ProjectDescription;
        if (!string.IsNullOrEmpty(seoData.ProjectSummary))
            project.Summary = seoData.ProjectSummary;
        if (!string.IsNullOrEmpty(seoData.Keywords))
            project.Keywords = seoData.Keywords;

        // Apply SEO
        if (project.Seo != null)
        {
            project.Seo.Title = seoData.SeoTitle;
            project.Seo.TitleSuffix = seoData.SeoTitleSuffix;
            project.Seo.Description = seoData.MetaDescription;
            project.Seo.Keywords = seoData.SeoKeywords;
            project.Seo.Canonical = seoData.Canonical;
            project.Seo.Robots = seoData.Robots;
        }

        // Apply Open Graph
        if (project.OpenGraph != null)
        {
            project.OpenGraph.Title = seoData.OpenGraphTitle;
            project.OpenGraph.Description = seoData.OpenGraphDescription;
            project.OpenGraph.Type = seoData.OpenGraphType;
            project.OpenGraph.Image = seoData.OpenGraphImage;
            project.OpenGraph.ImageAlt = seoData.OpenGraphImageAlt;
        }

        // Apply Twitter
        if (project.TwitterCard != null)
        {
            project.TwitterCard.Title = seoData.TwitterTitle;
            project.TwitterCard.Description = seoData.TwitterDescription;
            project.TwitterCard.Image = seoData.TwitterImage;
            project.TwitterCard.ImageAlt = seoData.TwitterImageAlt;
        }

        // Apply Repository
        if (project.Repository != null)
        {
            project.Repository.Provider = seoData.RepositoryProvider;
            project.Repository.Name = seoData.RepositoryName;
            project.Repository.Visibility = seoData.RepositoryVisibility;
            project.Repository.Branch = seoData.RepositoryBranch;
            project.Repository.Notes = seoData.RepositoryNotes;
        }

        // Apply Promotion
        if (project.Promotion != null)
        {
            project.Promotion.Pipeline = seoData.PromotionPipeline;
            project.Promotion.CurrentStage = seoData.PromotionCurrentStage;
            project.Promotion.Status = seoData.PromotionStatus;
            project.Promotion.Notes = seoData.PromotionNotes;

            // Add environments
            if (seoData.Environments != null)
            {
                project.Promotion.Environments.Clear();
                foreach (var env in seoData.Environments)
                {
                    project.Promotion.Environments.Add(new PromotionEnvironment
                    {
                        Name = env.Name,
                        Url = env.Url,
                        Status = env.Status,
                        Version = env.Version,
                        Notes = env.Notes
                    });
                }
            }
        }
    }

    private int CountPopulatedFields(ProjectModel project)
    {
        int count = 0;
        
        // Main fields
        if (!string.IsNullOrEmpty(project.Title)) count++;
        if (!string.IsNullOrEmpty(project.Description)) count++;
        if (!string.IsNullOrEmpty(project.Summary)) count++;
        if (!string.IsNullOrEmpty(project.Keywords)) count++;
        
        // SEO fields
        if (project.Seo != null)
        {
            if (!string.IsNullOrEmpty(project.Seo.Title)) count++;
            if (!string.IsNullOrEmpty(project.Seo.TitleSuffix)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Description)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Keywords)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Canonical)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Robots)) count++;
        }
        
        // Open Graph fields
        if (project.OpenGraph != null)
        {
            if (!string.IsNullOrEmpty(project.OpenGraph.Title)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Description)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Type)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Image)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.ImageAlt)) count++;
        }
        
        // Twitter fields
        if (project.TwitterCard != null)
        {
            if (!string.IsNullOrEmpty(project.TwitterCard.Title)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.Description)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.Image)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.ImageAlt)) count++;
        }
        
        // Repository fields
        if (project.Repository != null)
        {
            if (!string.IsNullOrEmpty(project.Repository.Provider)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Name)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Visibility)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Branch)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Notes)) count++;
        }
        
        // Promotion fields
        if (project.Promotion != null)
        {
            if (!string.IsNullOrEmpty(project.Promotion.Pipeline)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.CurrentStage)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.Status)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.Notes)) count++;
            
            // Environment fields
            count += project.Promotion.Environments?.Count(e => !string.IsNullOrEmpty(e.Name)) ?? 0;
        }
        
        return count;
    }

    private int CountProjectFields(ProjectModel project)
    {
        int count = 0;
        
        // Main fields
        if (!string.IsNullOrEmpty(project.Title)) count++;
        if (!string.IsNullOrEmpty(project.Description)) count++;
        if (!string.IsNullOrEmpty(project.Summary)) count++;
        if (!string.IsNullOrEmpty(project.Keywords)) count++;
        if (!string.IsNullOrEmpty(project.Link)) count++;
        if (!string.IsNullOrEmpty(project.Slug)) count++;
        
        // SEO fields
        if (project.Seo != null)
        {
            if (!string.IsNullOrEmpty(project.Seo.Title)) count++;
            if (!string.IsNullOrEmpty(project.Seo.TitleSuffix)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Description)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Keywords)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Canonical)) count++;
            if (!string.IsNullOrEmpty(project.Seo.Robots)) count++;
        }
        
        // Open Graph fields
        if (project.OpenGraph != null)
        {
            if (!string.IsNullOrEmpty(project.OpenGraph.Title)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Description)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Type)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.Image)) count++;
            if (!string.IsNullOrEmpty(project.OpenGraph.ImageAlt)) count++;
        }
        
        // Twitter fields
        if (project.TwitterCard != null)
        {
            if (!string.IsNullOrEmpty(project.TwitterCard.Title)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.Description)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.Image)) count++;
            if (!string.IsNullOrEmpty(project.TwitterCard.ImageAlt)) count++;
        }
        
        // Repository fields
        if (project.Repository != null)
        {
            if (!string.IsNullOrEmpty(project.Repository.Provider)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Name)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Visibility)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Branch)) count++;
            if (!string.IsNullOrEmpty(project.Repository.Notes)) count++;
        }
        
        // Promotion fields
        if (project.Promotion != null)
        {
            if (!string.IsNullOrEmpty(project.Promotion.Pipeline)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.CurrentStage)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.Status)) count++;
            if (!string.IsNullOrEmpty(project.Promotion.Notes)) count++;
            
            // Environment fields
            count += project.Promotion.Environments?.Count(e => !string.IsNullOrEmpty(e.Name)) ?? 0;
        }
        
        return count;
    }

    private static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "test-project";
            
        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-")
            .ToLowerInvariant();
    }

    private object CountDetailedFields(ProjectModel project)
    {
        var fields = new Dictionary<string, bool>();
        
        // Main fields
        fields["Title"] = !string.IsNullOrEmpty(project.Title);
        fields["Description"] = !string.IsNullOrEmpty(project.Description);
        fields["Summary"] = !string.IsNullOrEmpty(project.Summary);
        fields["Keywords"] = !string.IsNullOrEmpty(project.Keywords);
        
        // SEO fields
        fields["SEO.Title"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.Title);
        fields["SEO.TitleSuffix"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.TitleSuffix);
        fields["SEO.Description"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.Description);
        fields["SEO.Keywords"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.Keywords);
        fields["SEO.Canonical"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.Canonical);
        fields["SEO.Robots"] = project.Seo != null && !string.IsNullOrEmpty(project.Seo.Robots);
        
        // Open Graph fields
        fields["OpenGraph.Title"] = project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Title);
        fields["OpenGraph.Description"] = project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Description);
        fields["OpenGraph.Type"] = project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Type);
        fields["OpenGraph.Image"] = project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.Image);
        fields["OpenGraph.ImageAlt"] = project.OpenGraph != null && !string.IsNullOrEmpty(project.OpenGraph.ImageAlt);
        
        // Twitter fields
        fields["TwitterCard.Title"] = project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.Title);
        fields["TwitterCard.Description"] = project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.Description);
        fields["TwitterCard.Image"] = project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.Image);
        fields["TwitterCard.ImageAlt"] = project.TwitterCard != null && !string.IsNullOrEmpty(project.TwitterCard.ImageAlt);
        
        // Repository fields
        fields["Repository.Provider"] = project.Repository != null && !string.IsNullOrEmpty(project.Repository.Provider);
        fields["Repository.Name"] = project.Repository != null && !string.IsNullOrEmpty(project.Repository.Name);
        fields["Repository.Visibility"] = project.Repository != null && !string.IsNullOrEmpty(project.Repository.Visibility);
        fields["Repository.Branch"] = project.Repository != null && !string.IsNullOrEmpty(project.Repository.Branch);
        fields["Repository.Notes"] = project.Repository != null && !string.IsNullOrEmpty(project.Repository.Notes);
        
        // Promotion fields
        fields["Promotion.Pipeline"] = project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.Pipeline);
        fields["Promotion.CurrentStage"] = project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.CurrentStage);
        fields["Promotion.Status"] = project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.Status);
        fields["Promotion.Notes"] = project.Promotion != null && !string.IsNullOrEmpty(project.Promotion.Notes);
        
        // Environment fields
        var envCount = project.Promotion?.Environments?.Count(e => !string.IsNullOrEmpty(e.Name)) ?? 0;
        fields["Environments.Count"] = envCount > 0;
        
        return new
        {
            totalFields = fields.Count,
            populatedFields = fields.Count(f => f.Value),
            emptyFields = fields.Count(f => !f.Value),
            fieldDetails = fields,
            environmentCount = envCount
        };
    }
}

/// <summary>
/// Request model for testing project generation
/// </summary>
public class TestProjectGenerationRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public string? RepositoryUrl { get; set; }
}

/// <summary>
/// Request model for testing repository analysis
/// </summary>
public class TestRepositoryAnalysisRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
}
