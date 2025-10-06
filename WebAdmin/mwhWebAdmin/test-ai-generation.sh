#!/bin/bash

echo "🔍 Testing AI Generation with js-dev-env Repository"
echo "================================================="

echo ""
echo "Testing with the specific repository you mentioned: https://github.com/markhazleton/js-dev-env"

# Create test request JSON
cat > test-request.json << 'EOF'
{
  "title": "JavaScript Development Environment",
  "description": "A comprehensive JavaScript development environment setup",
  "repositoryUrl": "https://github.com/markhazleton/js-dev-env"
}
EOF

echo ""
echo "📋 Test Request Data:"
cat test-request.json

echo ""
echo ""
echo "🚀 To Test (when server is running on https://localhost:7219):"
echo "================================================================"

echo ""
echo "Method 1: Test AI Generation via API"
echo "curl -X POST \"https://localhost:7219/api/test/debug-ai-generation\" \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d @test-request.json \\"
echo "     -k"

echo ""
echo "Method 2: Test via Browser"
echo "1. Start the application: dotnet run"
echo "2. Go to: Add Project page"
echo "3. Enter:"
echo "   - GitHub Repository URL: https://github.com/markhazleton/js-dev-env"
echo "   - Project Title: JavaScript Development Environment"
echo "   - Project Summary: A comprehensive JavaScript development environment setup"
echo "4. Click 'Generate AI Content'"
echo "5. Check browser developer console (F12) for detailed logs"
echo "6. Verify all sections are populated with content"

echo ""
echo "🔍 What to Look For:"
echo "==================="
echo "✅ SEO Metadata section populated with title, description, keywords"
echo "✅ Social Sharing section populated with Open Graph and Twitter data"
echo "✅ Repository section populated with provider, name, branch, visibility"
echo "✅ Deployment Pipeline section populated with pipeline info and environments"
echo "✅ Success message showing which fields were updated"

echo ""
echo "🐛 Debugging Steps:"
echo "=================="
echo "1. Check application logs for '[ProjectService]' entries"
echo "2. Look for 'Pre-AI Generation' and 'Post-AI Generation' log entries"
echo "3. Verify GitHub repository analysis is working"
echo "4. Check if fallback generation is being used (no OpenAI API key)"
echo "5. Confirm ModelState.Clear() is allowing form updates"

echo ""
echo "📊 Expected Results:"
echo "==================="
echo "If using FALLBACK generation (no OpenAI API key):"
echo "- Should populate 25+ fields with professional content"
echo "- SEO titles, descriptions, social media metadata"
echo "- Repository provider, branch, visibility from GitHub API"
echo "- Deployment pipeline suggestions"
echo "- Environment configurations (Dev, Staging, Production)"

echo ""
echo "If using REAL OpenAI API:"
echo "- Should analyze README content from js-dev-env repository"
echo "- Generate contextual SEO and social content"
echo "- Detect JavaScript/Node.js tech stack"
echo "- Create relevant deployment pipeline suggestions"

echo ""
echo "🎯 Key Fix Applied:"
echo "=================="
echo "✅ Added ModelState.Clear() to allow updated model values to display"
echo "✅ Enhanced logging to track what data is generated"
echo "✅ Added field verification to confirm AI population"
echo "✅ Improved error handling and user feedback"

# Clean up
rm -f test-request.json

echo ""
echo "🚀 Ready to test! Start the application and try the AI generation."
