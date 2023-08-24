using mwhWebAdmin.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Construct the relative path to articles.json
string filePath = Path.Combine("..", "..", "src", "articles.json");
// Get the full path
string fullFilePath = Path.GetFullPath(filePath);

builder.Services.AddSingleton<ArticleService>(provider =>
{
    return new ArticleService(fullFilePath);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
