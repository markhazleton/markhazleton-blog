namespace mwhWebAdmin.Project;

public class ProjectService
{
    private readonly string _jsonFilePath;

    public ProjectService(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
    }

    public List<ProjectModel> GetProjects()
    {
        string json = File.ReadAllText(_jsonFilePath);
        List<ProjectModel> projects = JsonSerializer.Deserialize<List<ProjectModel>>(json);
        return projects;
    }

    public void SaveProjects(List<ProjectModel> projects)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(projects, options);
        File.WriteAllText(_jsonFilePath, json);
    }
}

public class ProjectModel
{
    [JsonPropertyName("image")]
    public string Image { get; set; }
    [JsonPropertyName("p")]
    public string Title { get; set; }
    [JsonPropertyName("d")]
    public string Description { get; set; }
    [JsonPropertyName("h")]
    public string Link { get; set; }
}





