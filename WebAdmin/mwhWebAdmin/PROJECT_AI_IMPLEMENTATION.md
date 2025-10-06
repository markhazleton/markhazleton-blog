# Project AI Agents Implementation

This implementation adds comprehensive AI agents to the Projects admin section, similar to the existing article AI agents, with enhanced GitHub repository integration and SEO optimization.

## Features Implemented

### 1. GitHub Repository Integration
- **Repository Analysis**: Automatically analyzes GitHub repositories to extract:
  - README content
  - Repository metadata (language, topics, license)
  - Dependency information (package.json, .csproj, requirements.txt, etc.)
  - GitHub Actions workflows for CI/CD pipeline detection
  - Tech stack and framework identification

### 2. AI-Powered Project Enhancement
- **Comprehensive SEO Generation**: Generates optimized:
  - Project titles and descriptions
  - SEO metadata (titles, descriptions, keywords)
  - Open Graph and Twitter Card metadata
  - Project summaries for social sharing

- **Technical Analysis**: AI analyzes repository content to determine:
  - Project category and type
  - Technology stack
  - Development environments
  - Deployment pipeline suggestions

### 3. Automated Field Population
- **Repository Details**: Auto-populates repository provider, visibility, branch, and notes
- **Promotion Pipeline**: Suggests CI/CD pipeline configuration based on detected workflows
- **Environment Configuration**: Recommends development, staging, and production environments

## Configuration Required

### Environment Variables / User Secrets
```json
{
  "OPENAI_API_KEY": "your-openai-api-key-here",
  "GITHUB_API_TOKEN": "your-github-token-here"
}
```

### GitHub API Token Setup
1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate a new token with `public_repo` scope (for public repositories)
3. Add the token to your user secrets or environment variables

## Usage

### Adding New Projects
1. Navigate to **Add Project** page
2. Enter at least one of:
   - Project title
   - Project description
   - GitHub repository URL
3. Click **"Generate AI Content"** button
4. Review and adjust the AI-generated content
5. Save the project

### Editing Existing Projects
1. Navigate to **Edit Project** page for any project
2. Click **"Generate AI Content"** button to enhance existing data
3. AI will analyze current information and repository (if provided)
4. Review updates and save

## File Structure

### New Files Created
```
Project/
├── ProjectSeoModels.cs              # AI generation result models
Services/
├── GitHubIntegrationService.cs      # GitHub API integration
Configuration/
├── ProjectSeoLlmPromptConfig.cs     # AI prompt configuration
Controllers/
├── ProjectTestController.cs         # Test API endpoints
```

### Modified Files
```
Project/ProjectModel.cs              # Added AI generation methods
Pages/ProjectAdd.cshtml              # Added AI button and enhancements
Pages/ProjectAdd.cshtml.cs           # Added AI handler
Pages/ProjectEdit.cshtml.cs          # Added AI handler
Program.cs                           # Registered new services
```

## AI Capabilities

### Repository Analysis
- **README Processing**: Extracts and analyzes README content for project understanding
- **Tech Stack Detection**: Identifies frameworks, languages, and dependencies
- **Workflow Analysis**: Examines GitHub Actions for deployment patterns
- **Metadata Extraction**: Pulls repository topics, license, and visibility

### SEO Optimization
- **Keyword Generation**: Creates relevant technical and business keywords
- **Meta Description**: Generates compelling descriptions with action words
- **Social Media Optimization**: Creates platform-specific titles and descriptions
- **Canonical URLs**: Auto-generates proper canonical URLs

### Content Enhancement
- **Professional Descriptions**: Creates business-value focused project descriptions
- **Technical Summaries**: Generates concise technical overviews
- **Category Classification**: Auto-categorizes projects by type and domain

## Testing

### Test Endpoints Available
- `POST /api/test/test-project-generation` - Test full project AI generation
- `POST /api/test/test-repository-analysis` - Test GitHub repository analysis

### Example Test Request
```json
{
  "title": "My Web Application",
  "description": "A modern web app built with React",
  "repositoryUrl": "https://github.com/username/my-project"
}
```

## Backward Compatibility

- All existing project data remains unchanged
- New AI fields are added only when AI generation is used
- projects.json structure is fully backward compatible
- No breaking changes to existing functionality

## Error Handling

- Graceful degradation when GitHub API is unavailable
- Fallback behavior when OpenAI API calls fail
- Comprehensive logging for debugging
- User-friendly error messages

## Performance Considerations

- GitHub API calls are cached per session
- OpenAI API calls use structured outputs for efficiency
- Repository analysis is performed asynchronously
- Large README files are truncated to prevent token limits

## Security

- GitHub API token is securely stored in user secrets
- OpenAI API key is protected through configuration
- All API calls include proper error handling
- No sensitive repository data is logged

## Future Enhancements

- Support for additional repository providers (GitLab, Azure DevOps)
- Caching of repository analysis results
- Batch processing for multiple projects
- Integration with project dashboard analytics
- Automated project categorization based on content analysis
