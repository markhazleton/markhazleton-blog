global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Xml;
using Microsoft.Extensions.FileProviders;
using mwhWebAdmin.Article;
using mwhWebAdmin.Article.Services;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;
using Serilog;
using Serilog.Events;

// Configure Serilog early in the application startup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "C:\\GitHub\\MarkHazleton\\markhazleton-blog\\logs\\app.log",
    rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10485760, // 10 MB
        retainedFileCountLimit: 10,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

  // Add Serilog to the application
    builder.Host.UseSerilog();

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
        Log.Warning("Directory does not exist: {ImgAssetsPath}", imgAssetsPath);
    }

    // Add this line to register IHttpClientFactory
    builder.Services.AddHttpClient();

    // API Controllers and Razor Pages
builder.Services.AddControllers();
    builder.Services.AddRazorPages();

    // GitHub Integration Service - changed to Singleton to match ProjectService lifetime
    builder.Services.AddSingleton<GitHubIntegrationService>();

    // SEO Validation Service
    builder.Services.AddScoped<SeoValidationService>();

    // Article Content Service (for external content file management)
    builder.Services.AddSingleton<ArticleContentService>();

    // Markdown to Pug Converter Service
 builder.Services.AddSingleton<MarkdownToPugConverter>();

    // AI Response Logger Service
    builder.Services.AddSingleton<AiResponseLogger>();

    // Article and Project Services with configuration
    builder.Services.AddSingleton<ArticleService>(provider =>
    {
     var config = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILogger<ArticleService>>();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var contentService = provider.GetRequiredService<ArticleContentService>();
        var markdownConverter = provider.GetRequiredService<MarkdownToPugConverter>();
  var articleService = new ArticleService(articlesPath, logger, config, factory, contentService, markdownConverter);
        
     // Set up AI response logger
    var aiLogger = provider.GetRequiredService<AiResponseLogger>();
   articleService.SetAiResponseLogger(aiLogger);
     
        return articleService;
    });

    builder.Services.AddSingleton<ProjectService>(provider =>
    {
      var config = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILogger<ProjectService>>();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var gitHubService = provider.GetRequiredService<GitHubIntegrationService>();
     return new ProjectService(projectsPath, logger, factory, config, gitHubService);
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
         Log.Error(ex, "An unhandled exception occurred.");
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

    Log.Information("Application configured successfully");
    
 app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
