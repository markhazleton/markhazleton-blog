using System.ComponentModel.DataAnnotations;

namespace mwhWebAdmin.Project;

public class ProjectService
{
    private readonly string _jsonFilePath;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
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
            // Reload from file to get latest changes
            LoadProjects();
            return new List<ProjectModel>(_projects); // Return a copy to prevent external modifications
        }
    }

    public ProjectModel? GetProjectById(int id)
    {
        lock (_lock)
        {
            return _projects.FirstOrDefault(p => p.Id == id);
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
            // Auto-assign ID if not set
            if (project.Id == 0)
            {
                project.Id = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1;
            }

            // Check for duplicate ID
            if (_projects.Any(p => p.Id == project.Id))
            {
                throw new InvalidOperationException($"Project with ID {project.Id} already exists.");
            }

            _projects.Add(project);
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

            // Update the project details
            existingProject.Title = updatedProject.Title;
            existingProject.Description = updatedProject.Description;
            existingProject.Link = updatedProject.Link;
            existingProject.Image = updatedProject.Image;

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
}





