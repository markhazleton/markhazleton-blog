global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Xml;
using Microsoft.Extensions.FileProviders;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Setup configuration and User Secrets
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();

// Load paths from configuration or fallback to defaults
var articlesPath = Path.GetFullPath(Path.Combine("..", "..", "src", "articles.json"));
var projectsPath = Path.GetFullPath(Path.Combine("..", "..", "src", "projects.json"));
var imgAssetsPath = Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"));

if (!Directory.Exists(imgAssetsPath))
{
    Console.WriteLine($"Directory does not exist: {imgAssetsPath}");
    // You can handle the error here, log it, or throw an exception
}
// Add this line to register IHttpClientFactory
builder.Services.AddHttpClient();

// Logging setup
builder.Services.AddLogging(configure => configure.AddConsole());

// API Controllers and Razor Pages
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// GitHub Integration Service - changed to Singleton to match ProjectService lifetime
builder.Services.AddSingleton<GitHubIntegrationService>();

// Article and Project Services with configuration
builder.Services.AddSingleton<ArticleService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<ArticleService>>();
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var contentService = provider.GetRequiredService<ArticleContentService>();
    return new ArticleService(articlesPath, logger, config, factory, contentService);
});

builder.Services.AddSingleton<ProjectService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<ProjectService>>();
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var gitHubService = provider.GetRequiredService<GitHubIntegrationService>();
    return new ProjectService(projectsPath, logger, factory, config, gitHubService);
});

// SEO Validation Service
builder.Services.AddScoped<SeoValidationService>();

// Article Content Service (for external content file management)
builder.Services.AddSingleton<ArticleContentService>();

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

// Map both API controllers and Razor Pages
app.MapControllers();
app.MapRazorPages();

app.Run();
