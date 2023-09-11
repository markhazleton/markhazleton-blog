using System.ComponentModel.DataAnnotations;

namespace mwhWebAdmin.Project;

public class ProjectService
{
    private readonly string _jsonFilePath;
    private List<ProjectModel> _projects;

    public ProjectService(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
        LoadProjects();
    }

    private void LoadProjects()
    {
        string json = File.ReadAllText(_jsonFilePath);
        _projects = JsonSerializer.Deserialize<List<ProjectModel>>(json);
    }


    public List<ProjectModel> GetProjects()
    {
        string json = File.ReadAllText(_jsonFilePath);
        _projects = JsonSerializer.Deserialize<List<ProjectModel>>(json);
        return _projects;
    }

    public void SaveProjects(List<ProjectModel> projects)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(projects, options);
        File.WriteAllText(_jsonFilePath, json);
    }

    public ProjectModel GetProjectById(int id)
    {
        return _projects.FirstOrDefault(p => p.Id == id);
    }

    public void UpdateProject(ProjectModel updatedProject)
    {
        if (updatedProject == null)
        {
            throw new ArgumentNullException(nameof(updatedProject));
        }

        var existingProject = _projects.FirstOrDefault(p => p.Id == updatedProject.Id);
        if (existingProject == null)
        {
            throw new InvalidOperationException("Project not found.");
        }

        // Update the project details
        existingProject.Title = updatedProject.Title;
        existingProject.Description = updatedProject.Description;
        existingProject.Link = updatedProject.Link;

        // Save the updated projects back to the JSON file
        SaveProjects();
    }

    private void SaveProjects()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(_projects, options);
        File.WriteAllText(_jsonFilePath, json);
    }
}

public class ProjectModel
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("image")]
    public string Image { get; set; }
    [JsonPropertyName("p")]
    public string Title { get; set; }
    [JsonPropertyName("d")]
    public string Description { get; set; }
    [JsonPropertyName("h")]
    public string Link { get; set; }
}





