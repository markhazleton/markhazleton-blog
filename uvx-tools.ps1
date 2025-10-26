# uvx Tools for Blog Development
# This script contains common uvx commands for your blog development workflow

# Function to run Python-based development tools using uvx
function Invoke-UvxTool {
    param(
        [string]$Tool,
        [string[]]$Arguments = @()
    )
    
    Write-Host "Running $Tool with uvx..." -ForegroundColor Green
    uvx $Tool @Arguments
}

# Common development tools you might find useful:
Write-Host @"
Available uvx tools for blog development:

# Code quality and formatting:
uvx ruff check .                    # Fast Python linter
uvx black .                         # Python code formatter  
uvx mypy .                          # Static type checker

# Web development and testing:
uvx httpie GET https://markhazleton.com/    # Test HTTP requests
uvx speedtest-cli                   # Test internet speed
uvx linkchecker https://markhazleton.com/   # Check for broken links

# Content and documentation:
uvx doc2dash                        # Create documentation
uvx mkdocs serve                    # Documentation server (if using MkDocs)

# SEO and analysis:
uvx lighthouse https://markhazleton.com/   # Performance analysis (Python version)

# Utilities:
uvx cowsay -t "Build completed!"    # Fun notifications
uvx python -c "import this"        # Zen of Python

Usage examples:
- uvx TOOL_NAME [arguments]         # Run tool once
- uvx --from PACKAGE COMMAND        # Run specific command from package

"@ -ForegroundColor Cyan