const fs = require('fs');
const path = require('path');

// YouTube video data extracted from PUG files
const youtubeVideos = [
  {
    slug: "articles/ai-observability-is-no-joke.html",
    youtubeUrl: "https://www.youtube.com/embed/j5Hm-iceT_M",
    youtubeTitle: "AI Observability Is No Joke - Deep Dive Discussion"
  },
  {
    slug: "using-chatgpt-for-developers.html",
    youtubeUrl: "https://www.youtube.com/embed/JTxsNm9IdYU",
    youtubeTitle: "What is ChatGPT?"
  },
  {
    slug: "articles/using-notebooklm-clipchamp-and-chatgpt-for-podcasts.html",
    youtubeUrl: "https://www.youtube.com/embed/Qn8N_ZexISk",
    youtubeTitle: "Deep Dive: Google NotebookLM"
  },
  {
    slug: "articles/using-large-language-models-to-generate-structured-data.html",
    youtubeUrl: "https://www.youtube.com/embed/TY8zKxYld1E",
    youtubeTitle: "Using Large Language Models to Generate Structured Data"
  },
  {
    slug: "articles/workflow-driven-chat-applications-powered-by-adaptive-cards.html",
    youtubeUrl: "https://www.youtube.com/embed/cErlh1yQ8ds",
    youtubeTitle: "Workflow-Driven Chat Applications With Adaptive Cards"
  },
  {
    slug: "articles/the-new-era-of-individual-agency-how-ai-tools-are-empowering-the-self-starter.html",
    youtubeUrl: "https://www.youtube.com/embed/To7SxGIoEg0",
    youtubeTitle: "The New Era of Individual Agency: How AI Tools Are Empowering the Self-Starter"
  },
  {
    slug: "articles/the-impact-of-input-case-on-llm-categorization.html",
    youtubeUrl: "https://www.youtube.com/embed/2hI79aKyaK0",
    youtubeTitle: "The Impact of Input Case on LLM Categorization"
  },
  {
    slug: "articles/moving-to-markhazletoncom.html",
    youtubeUrl: "https://www.youtube.com/embed/Rm_hziAo14A",
    youtubeTitle: "Screaming Frog SEO Spider Tutorial"
  },
  {
    slug: "articles/building-real-time-chat-with-react-signalr-and-markdown-streaming.html",
    youtubeUrl: "https://www.youtube.com/embed/D82StHCr6ig",
    youtubeTitle: "Building Real-Time Chat with React, SignalR, and Markdown Streaming"
  }
];

function convertEmbedToWatchUrl(embedUrl) {
  // Convert embed URL to watch URL
  const videoIdMatch = embedUrl.match(/embed\/([^?&]+)/);
  if (videoIdMatch) {
    return `https://www.youtube.com/watch?v=${videoIdMatch[1]}`;
  }
  return embedUrl;
}

async function updateArticlesWithYouTubeData() {
  try {
    // Read articles.json
    const articlesPath = path.join(__dirname, 'src', 'articles.json');
    const articlesData = JSON.parse(fs.readFileSync(articlesPath, 'utf8'));

    let updatedCount = 0;

    // Update articles with YouTube data
    for (const article of articlesData) {
      const videoData = youtubeVideos.find(video =>
        article.slug === video.slug ||
        article.slug === video.slug.replace('.html', '') ||
        article.slug === video.slug.replace('articles/', '')
      );

      if (videoData) {
        // Only update if YouTube data is not already present
        if (!article.youtubeUrl || !article.youtubeTitle) {
          article.youtubeUrl = convertEmbedToWatchUrl(videoData.youtubeUrl);
          article.youtubeTitle = videoData.youtubeTitle;
          updatedCount++;

          console.log(`✅ Updated: ${article.name}`);
          console.log(`   Slug: ${article.slug}`);
          console.log(`   YouTube URL: ${article.youtubeUrl}`);
          console.log(`   YouTube Title: ${article.youtubeTitle}`);
          console.log('');
        } else {
          console.log(`⏭️  Skipped (already has YouTube data): ${article.name}`);
        }
      }
    }

    // Write updated articles.json
    fs.writeFileSync(articlesPath, JSON.stringify(articlesData, null, 2));

    console.log(`\n🎉 Successfully updated ${updatedCount} articles with YouTube metadata.`);
    console.log(`📊 Total YouTube videos found: ${youtubeVideos.length}`);

    // Summary report
    console.log('\n📋 Summary Report:');
    console.log('==================');
    youtubeVideos.forEach((video, index) => {
      const article = articlesData.find(a =>
        a.slug === video.slug ||
        a.slug === video.slug.replace('.html', '') ||
        a.slug === video.slug.replace('articles/', '')
      );

      console.log(`${index + 1}. ${video.youtubeTitle}`);
      console.log(`   Video ID: ${video.youtubeUrl.match(/embed\/([^?&]+)/)?.[1] || 'Unknown'}`);
      console.log(`   Article: ${article ? article.name : 'NOT FOUND IN ARTICLES.JSON'}`);
      console.log(`   Status: ${article?.youtubeUrl ? '✅ Updated' : '❌ Not Found'}`);
      console.log('');
    });

  } catch (error) {
    console.error('❌ Error updating articles:', error.message);
    process.exit(1);
  }
}

// Run the update
updateArticlesWithYouTubeData();
