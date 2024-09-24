global using mwhWebAdmin.Models;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Xml;
using Microsoft.Extensions.FileProviders;
using mwhWebAdmin.Project;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(configure => configure.AddConsole());

builder.Services.AddRazorPages();

builder.Services.AddSingleton<ArticleService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ArticleService>>();
    return new ArticleService(Path.GetFullPath(Path.Combine("..", "..", "src", "articles.json")),logger);
});

builder.Services.AddSingleton<ProjectService>(provider =>
{
    return new ProjectService(Path.GetFullPath(Path.Combine("..", "..", "src", "projects.json")));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(Path.Combine("..", "..", "src", "assets", "img"))),
    RequestPath = "/assets/img"
});

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
