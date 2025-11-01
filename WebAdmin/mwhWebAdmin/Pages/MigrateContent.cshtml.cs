using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mwhWebAdmin.Article;
using mwhWebAdmin.Project;
using mwhWebAdmin.Services;

namespace mwhWebAdmin.Pages;

/// <summary>
/// Page model for migrating article content from articles.json to external .md files
/// </summary>
public class MigrateContentModel : BasePageModel
{
    private readonly ArticleService _articleService;
    private readonly ArticleContentService _contentService;

    public MigrateContentModel(
  ArticleService articleService, 
        ProjectService projectService, 
   ArticleContentService contentService,
        ILogger<MigrateContentModel> logger)
        : base(articleService, projectService, logger)
  {
        _articleService = articleService;
        _contentService = contentService;
    }

    public MigrationResult? Result { get; set; }
    public int TotalArticles { get; set; }
    public int ArticlesNeedingMigration { get; set; }

    public void OnGet()
    {
        var articles = _articleService.GetArticles();
        TotalArticles = articles.Count;
        ArticlesNeedingMigration = articles.Count(a => string.IsNullOrEmpty(a.ContentFile) && !string.IsNullOrEmpty(a.ArticleContent));
    }

    public async Task<IActionResult> OnPostMigrateAsync()
    {
        try
        {
     _baseLogger.LogInformation("Starting content migration to external files");
      Result = await MigrateAllContentToFilesAsync();
  
            if (Result.Failed == 0)
            {
    TempData["SuccessMessage"] = $"Successfully migrated {Result.Migrated} articles to external content files. Skipped {Result.Skipped} articles.";
}
        else
     {
  TempData["WarningMessage"] = $"Migrated {Result.Migrated} articles. {Result.Failed} failed. Check errors below.";
       }
 }
     catch (Exception ex)
        {
       _baseLogger.LogError(ex, "Migration failed");
TempData["ErrorMessage"] = $"Migration failed: {ex.Message}";
        }

   // Refresh counts
        var articles = _articleService.GetArticles();
        TotalArticles = articles.Count;
ArticlesNeedingMigration = articles.Count(a => string.IsNullOrEmpty(a.ContentFile) && !string.IsNullOrEmpty(a.ArticleContent));
        
     return Page();
    }

 private async Task<MigrationResult> MigrateAllContentToFilesAsync()
    {
   var result = new MigrationResult();
        var articles = _articleService.GetArticles();
   
        foreach (var article in articles)
        {
            try
            {
    // Skip if already using external content
      if (!string.IsNullOrEmpty(article.ContentFile))
      {
      result.Skipped++;
               _baseLogger.LogDebug("Article '{ArticleName}' already uses external content file: {ContentFile}", 
       article.Name, article.ContentFile);
  continue;
       }
      
                // Skip if no content to migrate
 if (string.IsNullOrEmpty(article.ArticleContent))
    {
       result.Skipped++;
   _baseLogger.LogDebug("Article '{ArticleName}' has no content to migrate", article.Name);
          continue;
       }
             
     // Generate content file name
       var contentFileName = _contentService.GenerateContentFileName(article.Slug);
    
         // Save content to file
    await _contentService.SaveContentAsync(contentFileName, article.ArticleContent);
 
          // Update article to reference external file
    article.ContentFile = contentFileName;
       
       // Update article in service
                await _articleService.UpdateArticle(article);
      
  result.Migrated++;
         _baseLogger.LogInformation("Migrated article: {ArticleName} → {ContentFile}", 
       article.Name, contentFileName);
            }
       catch (Exception ex)
     {
        result.Failed++;
     result.Errors.Add($"Failed to migrate {article.Name}: {ex.Message}");
     _baseLogger.LogError(ex, "Failed to migrate article: {ArticleName}", article.Name);
     }
        }
        
      return result;
    }
}

public class MigrationResult
{
    public int Migrated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
