using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using mwhWebAdmin.Article;

// Test program to verify slug processing logic
class Program
{
    static void Main(string[] args)
    {
        // Setup logging and services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddHttpClient();

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ArticleService>>();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

        // Path to articles.json - this should be the actual path in your project
        string articlesJsonPath = @"c:\GitHub\MarkHazleton\markhazleton-blog\src\articles.json";

        try
        {
            // Create ArticleService instance
            var articleService = new ArticleService(logger, httpClientFactory, articlesJsonPath);

            // Test cases for directory-based URLs ending with /
            string[] testSlugs = {
                "projectmechanics/leadership/",
                "projectmechanics/change-management/",
                "projectmechanics/conflict-management/",
                "projectmechanics/",
                "projectmechanics/leadership",  // without trailing slash
                "nonexistent/directory/",
                "projectmechanics/leadership.html"
            };

            Console.WriteLine("Testing slug-to-source-path conversion:");
            Console.WriteLine("===========================================");

            foreach (var slug in testSlugs)
            {
                Console.WriteLine($"\nTesting slug: '{slug}'");
                var result = articleService.TestSlugToSourcePath(slug);
                Console.WriteLine($"Result: '{result}'");

                // Check if the resulting file actually exists
                if (!string.IsNullOrEmpty(result))
                {
                    string fullPath = result.Replace("/src/pug/", @"c:\GitHub\MarkHazleton\markhazleton-blog\src\pug\").Replace('/', '\\');
                    bool exists = File.Exists(fullPath);
                    Console.WriteLine($"File exists: {exists} ({fullPath})");
                }
                else
                {
                    Console.WriteLine("No source file found");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
