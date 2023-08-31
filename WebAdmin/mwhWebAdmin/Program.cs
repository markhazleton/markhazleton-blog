global using mwhWebAdmin.Models;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Xml;
global using System.Text.Json.Serialization;
using mwhWebAdmin.Project;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Construct the relative path to articles.json
string filePath = Path.Combine("..", "..", "src", "articles.json");
string fullFilePath = Path.GetFullPath(filePath);

builder.Services.AddSingleton<ArticleService>(provider =>
{
    return new ArticleService(fullFilePath);
});


filePath = Path.Combine("..", "..", "src", "projects.json");
fullFilePath = Path.GetFullPath(filePath);
builder.Services.AddSingleton<ProjectService>(provider =>
{
    return new ProjectService(fullFilePath);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Serve files from the external folder
var imagePath = Path.Combine("..", "..", "src", "assets", "img");
var fullImagePath = Path.GetFullPath(imagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fullImagePath),
    RequestPath = "/assets/img"
});

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
