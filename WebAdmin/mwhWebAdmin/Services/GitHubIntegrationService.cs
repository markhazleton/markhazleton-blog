using System.Text.Json;
using System.Text.RegularExpressions;
using mwhWebAdmin.Project;

namespace mwhWebAdmin.Services;

/// <summary>
/// Service for integrating with GitHub API and analyzing repositories
/// </summary>
public class GitHubIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubIntegrationService> _logger;
    private readonly string? _githubToken;

    public GitHubIntegrationService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        ILogger<GitHubIntegrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _githubToken = _configuration["GITHUB_API_TOKEN"];
    }

    /// <summary>
    /// Analyzes a GitHub repository and extracts comprehensive information
    /// </summary>
    /// <param name="repositoryUrl">The GitHub repository URL</param>
    /// <returns>Repository analysis with README content, metadata, and workflows</returns>
    public async Task<GitHubRepositoryAnalysis> AnalyzeRepositoryAsync(string repositoryUrl)
    {
        try
        {
            _logger.LogInformation("Starting GitHub repository analysis for: {RepositoryUrl}", repositoryUrl);

            var (owner, repoName) = ExtractOwnerAndRepo(repositoryUrl);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName))
            {
                _logger.LogWarning("Could not extract owner and repository name from URL: {RepositoryUrl}", repositoryUrl);
                return new GitHubRepositoryAnalysis();
            }

            using var httpClient = _httpClientFactory.CreateClient();
            ConfigureHttpClient(httpClient);

            var analysis = new GitHubRepositoryAnalysis();

            // Fetch repository metadata
            await FetchRepositoryMetadata(httpClient, owner, repoName, analysis);

            // Fetch README content
            await FetchReadmeContent(httpClient, owner, repoName, analysis);

            // Fetch workflows information
            await FetchWorkflows(httpClient, owner, repoName, analysis);

            // Analyze package information
            await AnalyzePackageInfo(httpClient, owner, repoName, analysis);

            _logger.LogInformation("GitHub repository analysis completed for: {Owner}/{RepoName}", owner, repoName);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing GitHub repository: {RepositoryUrl}", repositoryUrl);
            return new GitHubRepositoryAnalysis();
        }
    }

    /// <summary>
    /// Extracts owner and repository name from GitHub URL
    /// </summary>
    /// <param name="repositoryUrl">GitHub repository URL</param>
    /// <returns>Tuple of (owner, repoName)</returns>
    private static (string owner, string repoName) ExtractOwnerAndRepo(string repositoryUrl)
    {
        try
        {
            // Handle various GitHub URL formats
            var patterns = new[]
            {
                @"github\.com[/:]([\w-]+)/([\w.-]+?)(?:\.git|/.*)?$",
                @"^([\w-]+)/([\w.-]+)$" // Direct owner/repo format
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(repositoryUrl, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return (match.Groups[1].Value, match.Groups[2].Value);
                }
            }

            return (string.Empty, string.Empty);
        }
        catch (Exception)
        {
            return (string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Configures HTTP client with GitHub API headers
    /// </summary>
    /// <param name="httpClient">HTTP client to configure</param>
    private void ConfigureHttpClient(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", "mwhWebAdmin-Project-Analyzer/1.0");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

        if (!string.IsNullOrEmpty(_githubToken))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_githubToken}");
        }
    }

    /// <summary>
    /// Fetches repository metadata from GitHub API
    /// </summary>
    private async Task FetchRepositoryMetadata(HttpClient httpClient, string owner, string repoName, GitHubRepositoryAnalysis analysis)
    {
        try
        {
            var apiUrl = $"https://api.github.com/repos/{owner}/{repoName}";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var repoData = JsonDocument.Parse(json);

                analysis.Description = repoData.RootElement.GetProperty("description").GetString();
                analysis.Language = repoData.RootElement.GetProperty("language").GetString();
                analysis.IsPrivate = repoData.RootElement.GetProperty("private").GetBoolean();
                analysis.DefaultBranch = repoData.RootElement.GetProperty("default_branch").GetString();

                if (repoData.RootElement.TryGetProperty("license", out var licenseElement) && 
                    licenseElement.ValueKind != JsonValueKind.Null)
                {
                    analysis.License = licenseElement.GetProperty("name").GetString();
                }

                if (repoData.RootElement.TryGetProperty("updated_at", out var updatedAtElement))
                {
                    if (DateTime.TryParse(updatedAtElement.GetString(), out var updatedAt))
                    {
                        analysis.LastUpdated = updatedAt;
                    }
                }

                if (repoData.RootElement.TryGetProperty("topics", out var topicsElement))
                {
                    foreach (var topic in topicsElement.EnumerateArray())
                    {
                        var topicValue = topic.GetString();
                        if (!string.IsNullOrEmpty(topicValue))
                        {
                            analysis.Topics.Add(topicValue);
                        }
                    }
                }

                _logger.LogInformation("Repository metadata fetched successfully for {Owner}/{RepoName}", owner, repoName);
            }
            else
            {
                _logger.LogWarning("Failed to fetch repository metadata. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repository metadata for {Owner}/{RepoName}", owner, repoName);
        }
    }

    /// <summary>
    /// Fetches README content from the repository
    /// </summary>
    private async Task FetchReadmeContent(HttpClient httpClient, string owner, string repoName, GitHubRepositoryAnalysis analysis)
    {
        try
        {
            var readmeVariants = new[] { "README.md", "readme.md", "README", "readme" };

            foreach (var readmeFile in readmeVariants)
            {
                var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repoName}/{analysis.DefaultBranch ?? "main"}/{readmeFile}";
                var response = await httpClient.GetAsync(rawUrl);

                if (response.IsSuccessStatusCode)
                {
                    analysis.ReadmeContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("README content fetched successfully for {Owner}/{RepoName} (file: {ReadmeFile})", owner, repoName, readmeFile);
                    break;
                }
            }

            if (string.IsNullOrEmpty(analysis.ReadmeContent))
            {
                _logger.LogWarning("No README file found for {Owner}/{RepoName}", owner, repoName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching README content for {Owner}/{RepoName}", owner, repoName);
        }
    }

    /// <summary>
    /// Fetches GitHub Actions workflows information
    /// </summary>
    private async Task FetchWorkflows(HttpClient httpClient, string owner, string repoName, GitHubRepositoryAnalysis analysis)
    {
        try
        {
            var apiUrl = $"https://api.github.com/repos/{owner}/{repoName}/actions/workflows";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var workflowData = JsonDocument.Parse(json);

                if (workflowData.RootElement.TryGetProperty("workflows", out var workflowsElement))
                {
                    foreach (var workflow in workflowsElement.EnumerateArray())
                    {
                        var workflowInfo = new GitHubWorkflow
                        {
                            Name = workflow.GetProperty("name").GetString(),
                            Path = workflow.GetProperty("path").GetString(),
                            State = workflow.GetProperty("state").GetString()
                        };

                        if (workflow.TryGetProperty("created_at", out var createdAtElement))
                        {
                            if (DateTime.TryParse(createdAtElement.GetString(), out var createdAt))
                            {
                                workflowInfo.CreatedAt = createdAt;
                            }
                        }

                        if (workflow.TryGetProperty("updated_at", out var updatedAtElement))
                        {
                            if (DateTime.TryParse(updatedAtElement.GetString(), out var updatedAt))
                            {
                                workflowInfo.UpdatedAt = updatedAt;
                            }
                        }

                        analysis.Workflows.Add(workflowInfo);
                    }
                }

                _logger.LogInformation("Workflows information fetched successfully for {Owner}/{RepoName}. Found {WorkflowCount} workflows", 
                    owner, repoName, analysis.Workflows.Count);
            }
            else
            {
                _logger.LogWarning("Failed to fetch workflows. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflows for {Owner}/{RepoName}", owner, repoName);
        }
    }

    /// <summary>
    /// Analyzes package information from repository files
    /// </summary>
    private async Task AnalyzePackageInfo(HttpClient httpClient, string owner, string repoName, GitHubRepositoryAnalysis analysis)
    {
        try
        {
            var packageFiles = new Dictionary<string, string>
            {
                ["package.json"] = "npm",
                ["requirements.txt"] = "pip",
                ["Pipfile"] = "pipenv",
                ["pom.xml"] = "maven",
                ["build.gradle"] = "gradle",
                ["Cargo.toml"] = "cargo",
                ["go.mod"] = "go",
                ["composer.json"] = "composer",
                [".csproj"] = "nuget"
            };

            foreach (var (fileName, packageManager) in packageFiles)
            {
                var content = await FetchFileContent(httpClient, owner, repoName, fileName, analysis.DefaultBranch ?? "main");
                if (!string.IsNullOrEmpty(content))
                {
                    analysis.PackageInfo = AnalyzePackageFileContent(content, packageManager, fileName);
                    break; // Use the first package file found
                }
            }

            if (analysis.PackageInfo != null)
            {
                _logger.LogInformation("Package information analyzed for {Owner}/{RepoName} using {PackageManager}", 
                    owner, repoName, analysis.PackageInfo.PackageManager);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing package information for {Owner}/{RepoName}", owner, repoName);
        }
    }

    /// <summary>
    /// Fetches content of a specific file from the repository
    /// </summary>
    private async Task<string?> FetchFileContent(HttpClient httpClient, string owner, string repoName, string fileName, string branch)
    {
        try
        {
            // For .csproj files, we need to search for them
            if (fileName == ".csproj")
            {
                var treeUrl = $"https://api.github.com/repos/{owner}/{repoName}/git/trees/{branch}?recursive=1";
                var treeResponse = await httpClient.GetAsync(treeUrl);
                
                if (treeResponse.IsSuccessStatusCode)
                {
                    var treeJson = await treeResponse.Content.ReadAsStringAsync();
                    var treeData = JsonDocument.Parse(treeJson);
                    
                    if (treeData.RootElement.TryGetProperty("tree", out var treeElement))
                    {
                        foreach (var item in treeElement.EnumerateArray())
                        {
                            var path = item.GetProperty("path").GetString();
                            if (!string.IsNullOrEmpty(path) && path.EndsWith(".csproj"))
                            {
                                fileName = path;
                                break;
                            }
                        }
                    }
                }
            }

            var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repoName}/{branch}/{fileName}";
            var response = await httpClient.GetAsync(rawUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching file content for {FileName} from {Owner}/{RepoName}", fileName, owner, repoName);
            return null;
        }
    }

    /// <summary>
    /// Analyzes package file content to extract dependency information
    /// </summary>
    private PackageInfo AnalyzePackageFileContent(string content, string packageManager, string fileName)
    {
        var packageInfo = new PackageInfo { PackageManager = packageManager };

        try
        {
            switch (packageManager.ToLowerInvariant())
            {
                case "npm":
                    AnalyzePackageJson(content, packageInfo);
                    break;
                case "nuget":
                    AnalyzeCsprojFile(content, packageInfo);
                    break;
                case "pip":
                    AnalyzeRequirementsTxt(content, packageInfo);
                    break;
                // Add more package managers as needed
                default:
                    _logger.LogWarning("Package manager {PackageManager} analysis not implemented", packageManager);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing {PackageManager} package file", packageManager);
        }

        return packageInfo;
    }

    /// <summary>
    /// Analyzes package.json content
    /// </summary>
    private void AnalyzePackageJson(string content, PackageInfo packageInfo)
    {
        var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("dependencies", out var dependencies))
        {
            foreach (var dependency in dependencies.EnumerateObject())
            {
                packageInfo.Dependencies.Add(dependency.Name);
            }
        }

        if (json.RootElement.TryGetProperty("devDependencies", out var devDependencies))
        {
            foreach (var dependency in devDependencies.EnumerateObject())
            {
                packageInfo.DevDependencies.Add(dependency.Name);
            }
        }

        // Detect framework
        var allDeps = packageInfo.Dependencies.Concat(packageInfo.DevDependencies).ToList();
        if (allDeps.Any(d => d.Contains("react")))
            packageInfo.Framework = "React";
        else if (allDeps.Any(d => d.Contains("vue")))
            packageInfo.Framework = "Vue.js";
        else if (allDeps.Any(d => d.Contains("angular")))
            packageInfo.Framework = "Angular";
        else if (allDeps.Any(d => d.Contains("express")))
            packageInfo.Framework = "Express.js";
        else if (allDeps.Any(d => d.Contains("next")))
            packageInfo.Framework = "Next.js";

        packageInfo.Runtime = "Node.js";
    }

    /// <summary>
    /// Analyzes .csproj file content
    /// </summary>
    private void AnalyzeCsprojFile(string content, PackageInfo packageInfo)
    {
        // Extract target framework
        var targetFrameworkMatch = Regex.Match(content, @"<TargetFramework[s]?>(.*?)</TargetFramework[s]?>", RegexOptions.IgnoreCase);
        if (targetFrameworkMatch.Success)
        {
            packageInfo.Framework = targetFrameworkMatch.Groups[1].Value;
        }

        // Extract package references
        var packageReferenceMatches = Regex.Matches(content, @"<PackageReference\s+Include=""([^""]+)""", RegexOptions.IgnoreCase);
        foreach (Match match in packageReferenceMatches)
        {
            packageInfo.Dependencies.Add(match.Groups[1].Value);
        }

        packageInfo.Runtime = ".NET";
    }

    /// <summary>
    /// Analyzes requirements.txt content
    /// </summary>
    private void AnalyzeRequirementsTxt(string content, PackageInfo packageInfo)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!trimmedLine.StartsWith("#") && !string.IsNullOrEmpty(trimmedLine))
            {
                // Extract package name (before version specifiers)
                var packageName = Regex.Match(trimmedLine, @"^([a-zA-Z0-9_-]+)").Groups[1].Value;
                if (!string.IsNullOrEmpty(packageName))
                {
                    packageInfo.Dependencies.Add(packageName);
                }
            }
        }

        packageInfo.Runtime = "Python";

        // Detect common Python frameworks
        var allDeps = packageInfo.Dependencies;
        if (allDeps.Any(d => d.ToLowerInvariant().Contains("django")))
            packageInfo.Framework = "Django";
        else if (allDeps.Any(d => d.ToLowerInvariant().Contains("flask")))
            packageInfo.Framework = "Flask";
        else if (allDeps.Any(d => d.ToLowerInvariant().Contains("fastapi")))
            packageInfo.Framework = "FastAPI";
    }
}
