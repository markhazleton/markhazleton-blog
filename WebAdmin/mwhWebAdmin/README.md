# mwhWebAdmin

## Overview

`mwhWebAdmin` is an ASP.NET Core Razor Pages web application for administrating articles and projects for the MarkHazleton.com website. It provides a user-friendly interface for managing content, including adding, editing, and organizing articles and projects. The application also supports SEO keyword generation, RSS feed, and sitemap generation.

## Features

- **Article Management**: Add, edit, and organize articles with metadata (section, slug, name, description, keywords, image, etc.).
- **Project Management**: Add, edit, and organize projects with title, description, link, and image.
- **SEO Automation**: Automatically generate SEO-optimized keywords for articles using OpenAI's API.
- **RSS Feed & Sitemap**: Generate RSS feeds and sitemaps for the website.
- **Image Asset Management**: Select images for articles and projects from the assets directory.
- **User Secrets**: Securely manage sensitive configuration data (e.g., API keys).

## Folder Structure

- `Article/` - Article models and service logic
- `Project/` - Project models and service logic
- `Pages/` - Razor Pages for UI (Articles, Projects, Add/Edit forms)
- `wwwroot/` - Static files (CSS, JS, images)

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- User secrets configured for OpenAI API key (for keyword generation)

### Running the Application

```pwsh
# Navigate to the project directory
cd WebAdmin/mwhWebAdmin

# Run the application
pwsh.exe -c "dotnet run"
```

The app will be available at `https://localhost:5001` (or as configured).

### User Secrets Example

```pwsh
dotnet user-secrets set "OPENAI_API_KEY" "your-openai-key-here"
```

## Article Management

- **View Articles**: Go to `/Articles` to see all articles grouped by section.
- **Add Article**: `/ArticleAdd` provides a form to add a new article.
- **Edit Article**: `/ArticleEdit/{id}` allows editing an existing article.

#### Article Model Example

```csharp
public class ArticleModel {
    public int Id { get; set; }
    public string Section { get; set; }
    public string Slug { get; set; }
    public string Name { get; set; }
    public string ArticleContent { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string ImgSrc { get; set; }
    public string LastModified { get; set; }
    public string ChangeFrequency { get; set; }
}
```

## Project Management

- **View Projects**: `/Projects` lists all projects with images, titles, and descriptions.
- **Edit Project**: `/ProjectEdit/{id}` allows editing a project's details and selecting an image from assets.

#### Project Model Example

```csharp
public class ProjectModel {
    public int Id { get; set; }
    public string Image { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
}
```

## SEO Keyword Generation

Keywords for articles are generated using OpenAI's API by analyzing the article content. To enable this, set your OpenAI API key using user secrets.

## RSS Feed & Sitemap

- RSS feed and sitemap are generated from the current articles and saved as `rss.xml` and `SiteMap.xml`.

## Code Samples

### Adding an Article (C#)

```csharp
var newArticle = new ArticleModel {
    Section = "Development",
    Name = "New Article",
    Description = "A new article description.",
    ArticleContent = "<p>Article body here</p>",
    ImgSrc = "assets/img/sample.jpg"
};
articleService.AddArticle(newArticle);
```

### Editing a Project (C#)

```csharp
var project = projectService.GetProjectById(1);
project.Title = "Updated Title";
projectService.UpdateProject(project);
```

## Configuration

- `appsettings.json` and environment-specific files for configuration.
- User secrets for sensitive data.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](../../LICENSE)
