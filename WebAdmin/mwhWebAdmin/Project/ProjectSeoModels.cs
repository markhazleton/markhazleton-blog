using System.Text.Json.Serialization;

namespace mwhWebAdmin.Project;

/// <summary>
/// Result model for AI-generated project SEO data
/// </summary>
public class ProjectSeoGenerationResult
{
    [JsonPropertyName("projectTitle")]
    public string? ProjectTitle { get; set; }

    [JsonPropertyName("projectDescription")]
    public string? ProjectDescription { get; set; }

    [JsonPropertyName("projectSummary")]
    public string? ProjectSummary { get; set; }

    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }

    [JsonPropertyName("seoTitle")]
    public string? SeoTitle { get; set; }

    [JsonPropertyName("metaDescription")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("ogTitle")]
    public string? OpenGraphTitle { get; set; }

    [JsonPropertyName("ogDescription")]
    public string? OpenGraphDescription { get; set; }

    [JsonPropertyName("twitterTitle")]
    public string? TwitterTitle { get; set; }

    [JsonPropertyName("twitterDescription")]
    public string? TwitterDescription { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("techStack")]
    public string? TechStack { get; set; }

    [JsonPropertyName("projectType")]
    public string? ProjectType { get; set; }

    [JsonPropertyName("repositoryProvider")]
    public string? RepositoryProvider { get; set; }

    [JsonPropertyName("repositoryVisibility")]
    public string? RepositoryVisibility { get; set; }

    [JsonPropertyName("repositoryNotes")]
    public string? RepositoryNotes { get; set; }

    [JsonPropertyName("promotionPipeline")]
    public string? PromotionPipeline { get; set; }

    [JsonPropertyName("promotionCurrentStage")]
    public string? PromotionCurrentStage { get; set; }

    [JsonPropertyName("promotionStatus")]
    public string? PromotionStatus { get; set; }

    [JsonPropertyName("promotionNotes")]
    public string? PromotionNotes { get; set; }

    [JsonPropertyName("environments")]
    public List<ProjectEnvironmentSuggestion>? Environments { get; set; }
}

/// <summary>
/// AI-suggested environment configuration
/// </summary>
public class ProjectEnvironmentSuggestion
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// GitHub repository analysis result
/// </summary>
public class GitHubRepositoryAnalysis
{
    public string? ReadmeContent { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public List<string> Topics { get; set; } = new();
    public string? License { get; set; }
    public bool IsPrivate { get; set; }
    public string? DefaultBranch { get; set; }
    public DateTime? LastUpdated { get; set; }
    public List<GitHubWorkflow> Workflows { get; set; } = new();
    public PackageInfo? PackageInfo { get; set; }
}

/// <summary>
/// GitHub Actions workflow information
/// </summary>
public class GitHubWorkflow
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? State { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Package/dependency information extracted from repository
/// </summary>
public class PackageInfo
{
    public string? PackageManager { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> DevDependencies { get; set; } = new();
    public string? Framework { get; set; }
    public string? Runtime { get; set; }
}
