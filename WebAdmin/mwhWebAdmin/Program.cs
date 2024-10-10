global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Xml;
using Microsoft.Extensions.FileProviders;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;

var builder = WebApplication.CreateBuilder(args);

// Load paths from configuration or fallback to defaults
var articlesPath = Path.GetFullPath(Path.Combine("..", "..", "src", "articles.json"));
var projectsPath =  Path.GetFullPath(Path.Combine("..", "..", "src", "projects.json"));
var imgAssetsPath = Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"));

if (!Directory.Exists(imgAssetsPath))
{
    Console.WriteLine($"Directory does not exist: {imgAssetsPath}");
    // You can handle the error here, log it, or throw an exception
}



// Logging setup
builder.Services.AddLogging(configure => configure.AddConsole());

// Razor Pages
builder.Services.AddRazorPages();

// Article and Project Services with configuration
builder.Services.AddSingleton<ArticleService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ArticleService>>();
    return new ArticleService(articlesPath, logger);
});

builder.Services.AddSingleton<ProjectService>(provider =>
{
    return new ProjectService(projectsPath);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    // Optional custom error logging
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An unhandled exception occurred.");
            throw;
        }
    });
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve static files for images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imgAssetsPath),
    RequestPath = "/assets/img"
});


app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
