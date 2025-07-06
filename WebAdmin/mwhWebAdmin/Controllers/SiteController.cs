using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace mwhWebAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly ILogger<SiteController> _logger;
        private readonly IConfiguration _configuration;

        public SiteController(ILogger<SiteController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshSite()
        {
            try
            {
                _logger.LogInformation("Starting site refresh process...");

                // Get the root directory (go up from WebAdmin/mwhWebAdmin to the project root)
                var rootDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));

                _logger.LogInformation($"Root directory: {rootDirectory}");

                // Check if package.json exists
                var packageJsonPath = Path.Combine(rootDirectory, "package.json");
                if (!System.IO.File.Exists(packageJsonPath))
                {
                    _logger.LogError($"package.json not found at: {packageJsonPath}");
                    return BadRequest(new { success = false, message = "package.json not found in project root" });
                }

                var buildResult = new StringBuilder();
                var hasErrors = false;

                // Step 1: Run npm run clean
                _logger.LogInformation("Running npm run clean...");
                var cleanResult = await RunNpmCommand("clean", rootDirectory);
                buildResult.AppendLine("=== NPM CLEAN ===");
                buildResult.AppendLine(cleanResult.Output);

                if (!cleanResult.Success)
                {
                    _logger.LogError($"npm run clean failed with exit code: {cleanResult.ExitCode}");
                    hasErrors = true;
                }

                // Step 2: Run npm run build (only if clean succeeded or if we want to continue anyway)
                _logger.LogInformation("Running npm run build...");
                var buildCommandResult = await RunNpmCommand("build", rootDirectory);
                buildResult.AppendLine("\n=== NPM BUILD ===");
                buildResult.AppendLine(buildCommandResult.Output);

                if (!buildCommandResult.Success)
                {
                    _logger.LogError($"npm run build failed with exit code: {buildCommandResult.ExitCode}");
                    hasErrors = true;
                }

                var response = new
                {
                    success = !hasErrors,
                    message = hasErrors ? "Build completed with errors" : "Site refreshed successfully",
                    output = buildResult.ToString(),
                    cleanSuccess = cleanResult.Success,
                    buildSuccess = buildCommandResult.Success
                };

                _logger.LogInformation($"Site refresh completed. Success: {!hasErrors}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during site refresh");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        private async Task<(bool Success, int ExitCode, string Output)> RunNpmCommand(string command, string workingDirectory)
        {
            var output = new StringBuilder();
            var process = new Process();

            try
            {
                // Use cmd.exe to run npm command to ensure npm is found in PATH
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c npm run {command}";
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                // Capture both stdout and stderr
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        _logger.LogInformation($"[npm {command}] {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine($"ERROR: {e.Data}");
                        _logger.LogWarning($"[npm {command}] ERROR: {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to complete (with timeout)
                var timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
                var completed = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));

                if (!completed)
                {
                    process.Kill();
                    output.AppendLine($"Process timed out after {timeout.TotalMinutes} minutes");
                    return (false, -1, output.ToString());
                }

                return (process.ExitCode == 0, process.ExitCode, output.ToString());
            }
            finally
            {
                process?.Dispose();
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                var rootDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
                var packageJsonPath = Path.Combine(rootDirectory, "package.json");
                var docsPath = Path.Combine(rootDirectory, "docs");

                var status = new
                {
                    rootDirectory = rootDirectory,
                    packageJsonExists = System.IO.File.Exists(packageJsonPath),
                    docsDirectoryExists = Directory.Exists(docsPath),
                    timestamp = DateTime.UtcNow
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting site status");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
