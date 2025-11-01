using System.Text.Json;

namespace mwhWebAdmin.Article.Services;

/// <summary>
/// Service for logging AI responses to files for analysis and debugging
/// </summary>
public class AiResponseLogger
{
    private readonly ILogger<AiResponseLogger> _logger;
    private readonly string _logDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AiResponseLogger(ILogger<AiResponseLogger> logger, IConfiguration configuration)
    {
   _logger = logger;
        
      // Get log directory from configuration or use default
        var baseDir = configuration["AiResponseLogs:Directory"] 
            ?? Path.Combine(AppContext.BaseDirectory, "logs", "ai-responses");
        
      _logDirectory = baseDir;
        
        // Ensure log directory exists
    Directory.CreateDirectory(_logDirectory);
     
        _jsonOptions = new JsonSerializerOptions 
        { 
        WriteIndented = true,
     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
        
        _logger.LogInformation("AI Response Logger initialized. Log directory: {LogDirectory}", _logDirectory);
    }

    /// <summary>
    /// Logs an AI request/response pair to a file
    /// </summary>
    /// <param name="articleId">The article ID</param>
    /// <param name="articleName">The article name</param>
    /// <param name="step">The generation step (1-4)</param>
    /// <param name="stepName">The step name</param>
    /// <param name="request">The request sent to AI</param>
    /// <param name="response">The response received from AI</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    public async Task LogAiInteractionAsync(
        int articleId,
        string articleName,
int step,
     string stepName,
        object request,
        string response,
  long durationMs)
 {
        try
        {
            var timestamp = DateTime.UtcNow;
     var sanitizedName = SanitizeFileName(articleName);
            
            // Create a unique filename
            var fileName = $"{timestamp:yyyyMMdd-HHmmss}_article-{articleId}_{sanitizedName}_step{step}_{stepName}.json";
          var filePath = Path.Combine(_logDirectory, fileName);
       
            var logEntry = new AiInteractionLog
         {
       Timestamp = timestamp,
      ArticleId = articleId,
   ArticleName = articleName,
     Step = step,
      StepName = stepName,
       DurationMs = durationMs,
     Request = request,
      Response = response,
    ResponseLength = response?.Length ?? 0
            };
     
  var json = JsonSerializer.Serialize(logEntry, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
  _logger.LogInformation(
    "AI interaction logged: Article {ArticleId}, Step {Step}, Duration {Duration}ms, File: {FileName}",
           articleId, step, durationMs, fileName);
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Failed to log AI interaction for article {ArticleId}, step {Step}", articleId, step);
  }
    }

    /// <summary>
    /// Logs a complete multi-step generation session
    /// </summary>
    public async Task LogGenerationSessionAsync(
     int articleId,
        string articleName,
        GenerationSessionLog sessionLog)
    {
  try
        {
            var timestamp = DateTime.UtcNow;
     var sanitizedName = SanitizeFileName(articleName);
            
      var fileName = $"{timestamp:yyyyMMdd-HHmmss}_article-{articleId}_{sanitizedName}_session.json";
   var filePath = Path.Combine(_logDirectory, fileName);
            
          sessionLog.Timestamp = timestamp;
            sessionLog.ArticleId = articleId;
      sessionLog.ArticleName = articleName;
      
            var json = JsonSerializer.Serialize(sessionLog, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
          
         _logger.LogInformation(
      "Generation session logged: Article {ArticleId}, Total Duration {Duration}ms, Success: {Success}, File: {FileName}",
       articleId, sessionLog.TotalDurationMs, sessionLog.Success, fileName);
     }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to log generation session for article {ArticleId}", articleId);
 }
    }

    /// <summary>
    /// Logs an error during AI generation
    /// </summary>
public async Task LogErrorAsync(
        int articleId,
        string articleName,
        int step,
        string stepName,
        Exception exception,
        object? request = null)
    {
        try
        {
            var timestamp = DateTime.UtcNow;
        var sanitizedName = SanitizeFileName(articleName);
          
    var fileName = $"{timestamp:yyyyMMdd-HHmmss}_article-{articleId}_{sanitizedName}_step{step}_{stepName}_ERROR.json";
            var filePath = Path.Combine(_logDirectory, fileName);
            
            var errorLog = new AiErrorLog
  {
   Timestamp = timestamp,
                ArticleId = articleId,
     ArticleName = articleName,
                Step = step,
     StepName = stepName,
     ErrorMessage = exception.Message,
   ErrorType = exception.GetType().Name,
    StackTrace = exception.StackTrace,
       Request = request
            };
            
         var json = JsonSerializer.Serialize(errorLog, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
    
    _logger.LogError(
       exception,
      "AI error logged: Article {ArticleId}, Step {Step}, File: {FileName}",
     articleId, step, fileName);
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to log AI error for article {ArticleId}, step {Step}", articleId, step);
 }
    }

    /// <summary>
    /// Gets log files for a specific article
 /// </summary>
    public List<string> GetArticleLogs(int articleId)
    {
        try
  {
    var pattern = $"*_article-{articleId}_*.json";
            return Directory.GetFiles(_logDirectory, pattern)
       .OrderByDescending(f => f)
        .ToList();
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Failed to get logs for article {ArticleId}", articleId);
            return new List<string>();
 }
    }

    /// <summary>
    /// Gets recent log files
    /// </summary>
    public List<string> GetRecentLogs(int count = 50)
    {
        try
        {
   return Directory.GetFiles(_logDirectory, "*.json")
         .OrderByDescending(f => File.GetCreationTimeUtc(f))
         .Take(count)
 .ToList();
        }
  catch (Exception ex)
   {
         _logger.LogError(ex, "Failed to get recent logs");
  return new List<string>();
        }
    }

    /// <summary>
    /// Cleans up old log files
    /// </summary>
    public async Task CleanupOldLogsAsync(int daysToKeep = 30)
    {
try
        {
 var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_logDirectory, "*.json");
      
    int deletedCount = 0;
     foreach (var file in files)
   {
      var fileInfo = new FileInfo(file);
           if (fileInfo.CreationTimeUtc < cutoffDate)
           {
   await Task.Run(() => File.Delete(file));
                    deletedCount++;
}
       }
         
   _logger.LogInformation("Cleaned up {Count} old log files older than {Days} days", deletedCount, daysToKeep);
        }
      catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to cleanup old logs");
        }
 }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "unnamed";
     
        var invalidChars = Path.GetInvalidFileNameChars();
      var sanitized = new string(fileName
 .Where(c => !invalidChars.Contains(c))
 .ToArray());
        
// Limit length and replace spaces
        return sanitized
            .Replace(" ", "-")
   .Substring(0, Math.Min(50, sanitized.Length));
    }
}

/// <summary>
/// Represents a single AI interaction log entry
/// </summary>
public class AiInteractionLog
{
    public DateTime Timestamp { get; set; }
    public int ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public int Step { get; set; }
    public string StepName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public object? Request { get; set; }
    public string? Response { get; set; }
    public int ResponseLength { get; set; }
}

/// <summary>
/// Represents a complete generation session log
/// </summary>
public class GenerationSessionLog
{
    public DateTime Timestamp { get; set; }
    public int ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long TotalDurationMs { get; set; }
    public List<StepResult> Steps { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public GenerationMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Represents the result of a single step
/// </summary>
public class StepResult
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long DurationMs { get; set; }
    public int OutputLength { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Metrics for the generation session
/// </summary>
public class GenerationMetrics
{
    public int TotalCharactersGenerated { get; set; }
    public int TotalTokensEstimated { get; set; }
    public int StepsCompleted { get; set; }
    public int StepsFailed { get; set; }
    public Dictionary<string, int> FieldsPopulated { get; set; } = new();
}

/// <summary>
/// Represents an error log entry
/// </summary>
public class AiErrorLog
{
    public DateTime Timestamp { get; set; }
  public int ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public int Step { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public object? Request { get; set; }
}
