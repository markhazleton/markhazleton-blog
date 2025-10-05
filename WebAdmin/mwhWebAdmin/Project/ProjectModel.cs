using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using mwhWebAdmin.Article;

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

    public ProjectService(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        LoadProjects();
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
