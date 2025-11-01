using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace mwhWebAdmin.Services;

/// <summary>
/// Service for managing external article content files
/// </summary>
public class ArticleContentService
{
    private readonly string _contentDirectory;
private readonly ILogger<ArticleContentService> _logger;

    public ArticleContentService(IConfiguration configuration, ILogger<ArticleContentService> logger)
    {
        // Use fully qualified path from configuration
        var srcPath = configuration["SrcPath"] ?? 
 throw new InvalidOperationException("SrcPath not configured in appsettings.json");
        
        _contentDirectory = Path.Combine(srcPath, "content");
  _logger = logger;
        
        // Ensure content directory exists
        if (!Directory.Exists(_contentDirectory))
    {
       Directory.CreateDirectory(_contentDirectory);
            _logger.LogInformation("Created content directory: {ContentDirectory}", _contentDirectory);
        }
    }

    /// <summary>
    /// Loads article content from external file
 /// </summary>
    /// <param name="contentFileName">The content file name (e.g., "my-article.md")</param>
    /// <returns>The article content as markdown</returns>
    public async Task<string> LoadContentAsync(string contentFileName)
    {
        if (string.IsNullOrEmpty(contentFileName))
      {
        _logger.LogWarning("Content file name is null or empty");
 return string.Empty;
        }

        var filePath = Path.Combine(_contentDirectory, contentFileName);
        
        if (!File.Exists(filePath))
    {
            _logger.LogWarning("Content file not found: {FilePath}", filePath);
            return string.Empty;
        }
        
        try
        {
       var content = await File.ReadAllTextAsync(filePath);
            _logger.LogDebug("Loaded content from: {FilePath} ({Length} characters)", filePath, content.Length);
            return content;
        }
      catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to load content from: {FilePath}", filePath);
    return string.Empty;
        }
    }

    /// <summary>
    /// Saves article content to external file
/// </summary>
    /// <param name="contentFileName">The content file name</param>
    /// <param name="content">The markdown content to save</param>
    public async Task SaveContentAsync(string contentFileName, string content)
    {
        if (string.IsNullOrEmpty(contentFileName))
        {
       throw new ArgumentException("Content file name cannot be null or empty", nameof(contentFileName));
        }

        var filePath = Path.Combine(_contentDirectory, contentFileName);
        
 try
        {
  await File.WriteAllTextAsync(filePath, content ?? string.Empty);
        _logger.LogInformation("Content saved to: {FilePath} ({Length} characters)", filePath, content?.Length ?? 0);
   }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to save content to: {FilePath}", filePath);
  throw;
        }
    }

 /// <summary>
    /// Generates content filename from article slug
    /// </summary>
    /// <param name="slug">The article slug (e.g., "articles/my-article.html")</param>
    /// <returns>Content filename (e.g., "my-article.md")</returns>
    public string GenerateContentFileName(string slug)
    {
 if (string.IsNullOrEmpty(slug))
        {
    throw new ArgumentException("Slug cannot be null or empty", nameof(slug));
        }

        // Remove 'articles/' prefix, '.html' extension, and replace slashes with hyphens
 var fileName = slug
      .Replace("articles/", "", StringComparison.OrdinalIgnoreCase)
          .Replace(".html", "", StringComparison.OrdinalIgnoreCase)
            .Replace("/", "-")
            .Trim();
        
        return $"{fileName}.md";
    }

    /// <summary>
    /// Deletes content file
/// </summary>
    /// <param name="contentFileName">The content file name to delete</param>
    public void DeleteContentFile(string contentFileName)
    {
        if (string.IsNullOrEmpty(contentFileName))
        {
       _logger.LogWarning("Cannot delete content file: name is null or empty");
        return;
        }

        var filePath = Path.Combine(_contentDirectory, contentFileName);
        
      if (File.Exists(filePath))
        {
     try
      {
     File.Delete(filePath);
              _logger.LogInformation("Content file deleted: {FilePath}", filePath);
            }
      catch (Exception ex)
        {
    _logger.LogError(ex, "Failed to delete content file: {FilePath}", filePath);
       throw;
            }
        }
        else
        {
            _logger.LogWarning("Content file not found for deletion: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Gets the full path to a content file
    /// </summary>
    /// <param name="contentFileName">The content file name</param>
    /// <returns>Full file system path</returns>
    public string GetContentFilePath(string contentFileName)
    {
        if (string.IsNullOrEmpty(contentFileName))
    {
   return string.Empty;
        }

        return Path.Combine(_contentDirectory, contentFileName);
    }

    /// <summary>
    /// Checks if content file exists
    /// </summary>
    /// <param name="contentFileName">The content file name</param>
    /// <returns>True if file exists, false otherwise</returns>
    public bool ContentFileExists(string contentFileName)
    {
        if (string.IsNullOrEmpty(contentFileName))
   {
      return false;
        }

        var filePath = Path.Combine(_contentDirectory, contentFileName);
      return File.Exists(filePath);
    }

    /// <summary>
 /// Gets all content file names in the content directory
  /// </summary>
    /// <returns>List of content file names</returns>
    public List<string> GetAllContentFiles()
    {
        if (!Directory.Exists(_contentDirectory))
        {
            return new List<string>();
        }

     try
     {
      return Directory.GetFiles(_contentDirectory, "*.md")
        .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
         .ToList()!;
        }
    catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to get content files from: {ContentDirectory}", _contentDirectory);
 return new List<string>();
        }
  }
}
