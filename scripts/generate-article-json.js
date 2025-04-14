"use strict";
const upath = require("upath");
const fs = require("fs").promises;
const fsSync = require("fs");
const pug = require("pug");
const cheerio = require("cheerio");
const path = require("path");
const { program } = require("commander");

// Setup command line arguments
program
  .option("-s, --src <path>", "Source directory containing Pug files")
  .option("-o, --out <path>", "Output directory for JSON files")
  .option("-a, --articles <path>", "Path to articles.json file")
  .option("-c, --combine", "Generate a combined JSON file of all articles", false)
  .option("-v, --verbose", "Show verbose output", false)
  .parse(process.argv);

const options = program.opts();
const verbose = options.verbose;

// Define paths (with command line overrides)
const srcArticlesJsonPath = options.articles
  ? upath.resolve(process.cwd(), options.articles)
  : upath.resolve(upath.dirname(__filename), "../src/articles.json");
const pugSourceBaseDir = options.src
  ? upath.resolve(process.cwd(), options.src)
  : upath.resolve(upath.dirname(__filename), "../src/pug");
const jsonOutputBaseDir = options.out
  ? upath.resolve(process.cwd(), options.out)
  : upath.resolve(upath.dirname(__filename), "../docs/json");

// Logging utility with verbosity control
const log = {
    info: (message) => console.log(`[INFO] ${message}`),
    verbose: (message) => verbose && console.log(`[VERBOSE] ${message}`),
    warn: (message) => console.warn(`[WARNING] ${message}`),
    error: (message) => console.error(`[ERROR] ${message}`),
    progress: (current, total) => {
        const percent = Math.floor((current / total) * 100);
        process.stdout.write(`\rProcessing articles: ${current}/${total} (${percent}%) complete`);
        if (current === total) process.stdout.write("\n");
    }
};

// Helper function to create directory recursively if it doesn't exist
async function ensureDirectoryExists(dirPath) {
    try {
        await fs.mkdir(dirPath, { recursive: true });
    } catch (err) {
        if (err.code !== 'EEXIST') {
            throw err;
        }
    }
}

// Helper function to extract metadata from pug content using regex
function extractMetadataWithRegex(content, pattern) {
    const match = content.match(pattern);
    return match ? match[1] : "";
}

// Helper function to calculate reading time
function calculateReadingTime(text) {
    if (!text) return 1;
    const wordsPerMinute = 200;
    const wordCount = text.split(/\s+/).length;
    const readingTime = Math.ceil(wordCount / wordsPerMinute);
    return readingTime < 1 ? 1 : readingTime;
}

// Helper function to generate a summary
function generateSummary(text, maxLength = 160) {
    if (!text) return "";

    // Find the first paragraph or generate from beginning
    const firstParagraph = text.split(/\n\s*\n/)[0] || text;

    if (firstParagraph.length <= maxLength) {
        return firstParagraph;
    }

    // Truncate to max length and ensure it ends at a complete word
    let summary = firstParagraph.substring(0, maxLength);
    summary = summary.substring(0, summary.lastIndexOf(" ")) + "...";
    return summary;
}

// Helper function to get the Pug file path from a slug
async function findPugFileFromSlug(slug) {
    // If the slug is empty, return null
    if (!slug) return null;

    // If the slug ends with .html, replace it with .pug
    const pugFileName = slug.endsWith('.html')
        ? slug.replace('.html', '.pug')
        : slug;

    // Remove any leading / from slug
    const cleanedSlug = pugFileName.startsWith('/')
        ? pugFileName.substring(1)
        : pugFileName;

    // Extract path components
    const slugParts = cleanedSlug.split('/');
    const fileNamePart = slugParts.pop(); // Last part is the filename

    try {
        // Skip if the filename part is empty
        if (!fileNamePart) {
            log.warn(`Could not extract a valid filename from slug: ${slug}`);
            return null;
        }

        // First, try the most specific path based on the slug structure
        let candidatePaths = [];

        // Build full path from slug directory structure
        if (slugParts.length > 0) {
            // Try the exact path from the slug
            candidatePaths.push(upath.join(pugSourceBaseDir, ...slugParts, fileNamePart));
        }

        // Try the articles directory
        candidatePaths.push(upath.join(pugSourceBaseDir, "articles", fileNamePart));

        // Try the root pug directory
        candidatePaths.push(upath.join(pugSourceBaseDir, fileNamePart));

        // Check all candidate paths
        for (const candidatePath of candidatePaths) {
            if (fsSync.existsSync(candidatePath) && !fsSync.statSync(candidatePath).isDirectory()) {
                return candidatePath;
            }

            // Also check with .pug extension if not already present
            if (!candidatePath.endsWith('.pug')) {
                const pathWithExt = candidatePath + '.pug';
                if (fsSync.existsSync(pathWithExt) && !fsSync.statSync(pathWithExt).isDirectory()) {
                    return pathWithExt;
                }
            }
        }

        // If direct path lookup failed, search all pug files to find a match by filename
        let foundFile = null;
        const searchDirectories = [
            upath.join(pugSourceBaseDir, "articles"),
            pugSourceBaseDir
        ];

        for (const searchDir of searchDirectories) {
            if (!fsSync.existsSync(searchDir)) continue;

            const files = await fs.readdir(searchDir);
            const pugFiles = files.filter(file => file.endsWith(".pug"));

            // Find a file that matches the slug's filename part
            const matchingFile = pugFiles.find(file => {
                return file === fileNamePart || file === fileNamePart + '.pug';
            });

            if (matchingFile) {
                foundFile = upath.join(searchDir, matchingFile);
                break;
            }
        }

        if (foundFile) {
            return foundFile;
        }

        log.warn(`Could not find a matching Pug file for slug: ${slug}`);
        return null;
    } catch (err) {
        log.error(`Error finding Pug file for slug ${slug}: ${err.message}`);
        return null;
    }
}

// Extract content from rendered HTML using Cheerio
async function extractContentFromHTML(pugFilePath, renderedContent) {
    try {
        const $ = cheerio.load(renderedContent);

        // Extract the title from a specific tag, e.g., <h1>
        const title = $("h1").first().text().trim();

        // Enhanced approach to ensure spaces between words in the body content
        // First, replace all block-level elements with their text plus a newline
        $('body').find('p, div, h1, h2, h3, h4, h5, h6, li').each(function() {
            $(this).append('\n');
        });

        // Extract code snippets before processing text
        const codeSnippets = [];
        $('pre code, code').each(function(index) {
            const language = $(this).attr('class') || '';
            const code = $(this).text().trim();
            codeSnippets.push({
                id: `code-${index}`,
                language: language.replace('language-', '').trim(),
                code: code
            });
            // Replace with a placeholder
            $(this).text(`[CODE_SNIPPET_${index}]`);
        });

        // Get the text and normalize spaces
        let body = $("body").text()
            .replace(/\s+/g, " ")  // Replace multiple whitespace with single space
            .replace(/\s*\n\s*/g, " ") // Replace newlines with spaces
            .replace(/\s+([.,;:!?])/g, "$1") // Remove spaces before punctuation
            .replace(/([.,;:!?])\s*/g, "$1 ") // Ensure space after punctuation
            .replace(/\s+/g, " ")  // Final cleanup of any remaining multiple spaces
            .trim();

        // Sanitize content - remove any remaining HTML tags
        body = body.replace(/<[^>]*>/g, '');

        // Extract images
        const images = [];
        $("img").each(function() {
            const src = $(this).attr("src");
            const alt = $(this).attr("alt") || "";
            if (src) {
                images.push({ src, alt });
            }
        });

        // Extract canonical URL
        const canonicalLink = $('link[rel="canonical"]').attr('href') || '';

        return {
            title: title || path.basename(pugFilePath, '.pug'),
            body,
            codeSnippets: codeSnippets.length > 0 ? codeSnippets : undefined,
            images: images.length > 0 ? images : undefined,
            canonicalUrl: canonicalLink
        };
    } catch (err) {
        log.error(`Error extracting content from HTML for ${pugFilePath}: ${err.message}`);
        return null;
    }
}

// Function to extract meta data and content from Pug file
async function extractPugMetaAndContent(pugFilePath) {
    try {
        // Skip if the path is invalid or a directory
        if (!pugFilePath || fsSync.statSync(pugFilePath).isDirectory()) {
            return { metaData: {}, content: null };
        }

        const pugContent = await fs.readFile(pugFilePath, "utf8");

        // Try to compile and render the Pug file
        try {
            const compiledFunction = pug.compile(pugContent, {
                filename: pugFilePath,
                pretty: false
            });
            const renderedHTML = compiledFunction({ articles: [] });

            // Extract content from the rendered HTML
            const htmlContent = await extractContentFromHTML(pugFilePath, renderedHTML);

            // Extract basic metadata with regex from the Pug file
            const metaData = {
                title: extractMetadataWithRegex(pugContent, /title\s+([^\n]+)/),
                description: extractMetadataWithRegex(pugContent, /meta\(name=['"]description['"],\s*content=['"]([^'"]+)['"]\)/),
                keywords: extractMetadataWithRegex(pugContent, /meta\(name=["']keywords["'],\s*content=["']([^"']+)["']\)/),
                category: extractMetadataWithRegex(pugContent, /meta\(name=["']category["'],\s*content=["']([^"']+)["']\)/),
                tags: extractMetadataWithRegex(pugContent, /meta\(name=["']tags["'],\s*content=["']([^"']+)["']\)/),
                date: extractMetadataWithRegex(pugContent, /meta\(name=["']date["'],\s*content=["']([^"']+)["']\)/)
            };

            // Process tags into an array if present
            if (metaData.tags) {
                metaData.tags = metaData.tags.split(',').map(tag => tag.trim());
            }

            return { metaData, content: htmlContent };
        } catch (renderError) {
            log.warn(`Unable to render Pug file ${pugFilePath}: ${renderError.message}`);

            // Fall back to regex-based extraction if rendering fails
            log.verbose(`Falling back to regex-based extraction for ${pugFilePath}`);

            // Extract metadata
            const metaData = {
                title: extractMetadataWithRegex(pugContent, /title\s+([^\n]+)/),
                description: extractMetadataWithRegex(pugContent, /meta\(name=['"]description['"],\s*content=['"]([^'"]+)['"]\)/),
                keywords: extractMetadataWithRegex(pugContent, /meta\(name=["']keywords["'],\s*content=["']([^"']+)["']\)/)
            };

            // Extract the article body content
            const articleText = extractPlainTextFromPug(pugContent);

            return {
                metaData,
                content: {
                    title: metaData.title || path.basename(pugFilePath, '.pug'),
                    body: articleText,
                }
            };
        }
    } catch (err) {
        log.error(`Error extracting data from Pug file ${pugFilePath}: ${err.message}`);
        return { metaData: {}, content: null };
    }
}

// Helper to extract plain text content from pug
function extractPlainTextFromPug(pugContent) {
    // Extract the article content from the pug file - look for paragraphs, headings, and text
    let articleText = '';

    // Find all paragraph content - this is the main text
    const paragraphs = pugContent.match(/p\.\r?\n\s+([^\n]+(\r?\n\s+[^\n]+)*)/g) || [];
    for (const p of paragraphs) {
        const textContent = p.replace(/p\.\r?\n\s+/, '').replace(/\r?\n\s+/g, ' ');
        articleText += textContent + '\n\n';
    }

    // Find all headings (h1, h2, h3, etc.)
    const headings = pugContent.match(/h[1-6][ \t]+(.*?)(\r?\n|$)/g) || [];
    for (const h of headings) {
        const textContent = h.replace(/h[1-6][ \t]+/, '').trim();
        articleText += textContent + '\n\n';
    }

    // Clean up the text
    articleText = articleText
        .replace(/\|\s+/g, '')  // Remove Pug pipe operators followed by spaces
        .replace(/\s+/g, ' ')   // Normalize all whitespace
        .trim();

    return articleText;
}

// Process all Pug files in a directory recursively
async function processPugFilesInDirectory(pugDir, outputBaseDir, processedSlugs) {
    try {
        // Ensure the output directory exists
        await ensureDirectoryExists(outputBaseDir);

        // Get all files in the directory
        const files = await fs.readdir(pugDir);
        const allArticles = [];

        for (const file of files) {
            const filePath = upath.join(pugDir, file);
            const stats = await fs.stat(filePath);

            if (stats.isDirectory()) {
                // Recursively process subdirectories
                const outputSubDir = upath.join(outputBaseDir, file);
                const subDirArticles = await processPugFilesInDirectory(filePath, outputSubDir, processedSlugs);
                allArticles.push(...subDirArticles);
            } else if (file.endsWith('.pug')) {
                // Process the Pug file
                try {
                    // Skip article-stub.pug or other template files
                    if (file === 'article-stub.pug' || file.startsWith('_')) {
                        continue;
                    }

                    // Generate a slug from the file path
                    const fileSlug = path.relative(pugSourceBaseDir, filePath).replace(/\.pug$/, '.html');

                    // Skip if this slug has already been processed
                    if (processedSlugs.has(fileSlug)) {
                        log.verbose(`Skipping already processed file: ${file}`);
                        continue;
                    }

                    // Extract meta data and content from Pug file
                    const { metaData, content } = await extractPugMetaAndContent(filePath);

                    if (!content) {
                        log.warn(`No content extracted from ${file}, skipping.`);
                        continue;
                    }

                    // Create a new JSON object with basic properties
                    const articleData = {
                        name: metaData.title || content.title || path.basename(file, '.pug'),
                        slug: fileSlug,
                        metaTitle: metaData.title || content.title || "",
                        metaDescription: metaData.description || "",
                        metaKeywords: metaData.keywords || "",
                        body: content.body || "",
                        readingTimeMinutes: calculateReadingTime(content.body),
                        pugFilePath: upath.relative(upath.dirname(__filename), filePath),
                        generatedAt: new Date().toISOString()
                    };

                    // Add additional properties if they exist
                    if (content.images && content.images.length > 0) {
                        articleData.images = content.images;
                        articleData.mainImage = content.images[0].src;
                    }

                    if (content.codeSnippets && content.codeSnippets.length > 0) {
                        articleData.codeSnippets = content.codeSnippets;
                    }

                    if (content.canonicalUrl) {
                        articleData.canonicalUrl = content.canonicalUrl;
                    }

                    if (metaData.category) {
                        articleData.category = metaData.category;
                    }

                    if (metaData.tags) {
                        articleData.tags = metaData.tags;
                    }

                    if (metaData.date) {
                        articleData.publicationDate = metaData.date;
                    }

                    if (!articleData.description && articleData.body) {
                        articleData.description = generateSummary(articleData.body);
                    }

                    // Create the output JSON file name
                    const outputFileName = path.basename(file, '.pug') + '.json';
                    const outputFilePath = upath.join(outputBaseDir, outputFileName);

                    // Write the article data to a JSON file
                    await fs.writeFile(outputFilePath, JSON.stringify(articleData, null, 2));

                    log.verbose(`Generated JSON from Pug: ${outputFileName}`);

                    // Add to list of all articles
                    allArticles.push(articleData);
                } catch (err) {
                    log.error(`Error processing Pug file ${file}: ${err.message}`);
                }
            }
        }

        return allArticles;
    } catch (err) {
        log.error(`Error processing directory ${pugDir}: ${err.message}`);
        return [];
    }
}

// Main function to process all articles
async function processArticles() {
    try {
        // Ensure the base output directory exists
        await ensureDirectoryExists(jsonOutputBaseDir);
        log.info(`Base output directory: ${jsonOutputBaseDir}`);

        // Load articles from the source JSON file
        log.info(`Reading articles from: ${srcArticlesJsonPath}`);
        const articlesJsonRaw = await fs.readFile(srcArticlesJsonPath, "utf8");
        const articlesData = JSON.parse(articlesJsonRaw);

        log.info(`Found ${articlesData.length} articles in the source JSON file.`);

        let processedCount = 0;
        let errorCount = 0;
        const allArticles = [];
        const processedSlugs = new Set();

        // Process each article
        for (const [index, article] of articlesData.entries()) {
            try {
                log.progress(index, articlesData.length);

                // Skip entries without a slug or with an empty slug
                if (!article.slug || article.slug === "") {
                    log.warn(`Article at index ${index} has no slug or empty slug. Skipping.`);
                    continue;
                }

                // Find the corresponding Pug file
                const pugFilePath = await findPugFileFromSlug(article.slug);

                // Create a new object with all properties from the article
                const articleData = { ...article };

                // Add the Pug file path and extract content if found
                if (pugFilePath) {
                    articleData.pugFilePath = upath.relative(upath.dirname(__filename), pugFilePath);
                    articleData.pugFileExists = true;

                    // Extract meta data and content from Pug file
                    const { metaData, content } = await extractPugMetaAndContent(pugFilePath);

                    // Add meta data from Pug file
                    articleData.metaTitle = metaData.title || articleData.name || "";
                    articleData.metaDescription = metaData.description || articleData.description || "";
                    articleData.metaKeywords = metaData.keywords || "";

                    // Add more detailed metadata if available
                    if (metaData.category && !articleData.category) {
                        articleData.category = metaData.category;
                    }

                    if (metaData.tags && (!articleData.tags || !articleData.tags.length)) {
                        articleData.tags = metaData.tags;
                    }

                    if (metaData.date && !articleData.publicationDate) {
                        articleData.publicationDate = metaData.date;
                    }

                    // Add content from Pug file if available
                    if (content) {
                        // Add body content
                        articleData.body = content.body || "";

                        // Add reading time
                        articleData.readingTimeMinutes = calculateReadingTime(articleData.body);

                        // Add description if not already present
                        if (!articleData.description && articleData.body) {
                            articleData.description = generateSummary(articleData.body);
                        }

                        // Add code snippets if present
                        if (content.codeSnippets && content.codeSnippets.length > 0) {
                            articleData.codeSnippets = content.codeSnippets;
                        }

                        // Add images if present
                        if (content.images && content.images.length > 0) {
                            articleData.images = content.images;

                            // Set main image if not already set
                            if (!articleData.mainImage) {
                                articleData.mainImage = content.images[0].src;
                            } else if (!articleData.mainImage.startsWith('http')) {
                                // If the mainImage is a relative path, make sure it exists
                                const mainImageFound = content.images.some(img => img.src === articleData.mainImage);
                                if (!mainImageFound) {
                                    articleData.mainImage = content.images[0].src;
                                }
                            }
                        }

                        // Add canonical URL if present
                        if (content.canonicalUrl && !articleData.canonicalUrl) {
                            articleData.canonicalUrl = content.canonicalUrl;
                        }
                    }
                } else {
                    articleData.pugFileExists = false;
                }

                // Add generation timestamp
                articleData.generatedAt = new Date().toISOString();

                // Determine the output directory and file name from the slug
                if (article.slug) {
                    // Mark this slug as processed
                    processedSlugs.add(article.slug);

                    // Remove any leading / from slug
                    const cleanedSlug = article.slug.startsWith('/')
                        ? article.slug.substring(1)
                        : article.slug;

                    // Split the slug into path components
                    const slugParts = cleanedSlug.split('/');
                    const fileName = slugParts.pop(); // Last part is the filename

                    // Create the output file name
                    const outputFileName = fileName.replace(/\.html$/, '.json');

                    // Create the output directory path based on slug path
                    let outputDirPath;
                    if (slugParts.length > 0) {
                        outputDirPath = upath.join(jsonOutputBaseDir, ...slugParts);
                    } else {
                        outputDirPath = jsonOutputBaseDir;
                    }

                    // Ensure the output directory exists
                    await ensureDirectoryExists(outputDirPath);

                    // Write the article data to a JSON file
                    const outputFilePath = upath.join(outputDirPath, outputFileName);
                    await fs.writeFile(outputFilePath, JSON.stringify(articleData, null, 2));

                    // Add to the list of all articles
                    allArticles.push(articleData);

                    processedCount++;
                    log.verbose(`Generated JSON: ${upath.relative(jsonOutputBaseDir, outputFilePath)}`);
                } else {
                    log.warn(`Article at index ${index} has an invalid slug structure. Skipping.`);
                }
            } catch (err) {
                errorCount++;
                log.error(`Error processing article index ${index}: ${err.message}`);
            }
        }

        // Now process all Pug files in src/pug directory that aren't covered by articles.json
        log.info(`Processing Pug files not covered in articles.json...`);
        const additionalArticles = await processPugFilesInDirectory(pugSourceBaseDir, jsonOutputBaseDir, processedSlugs);
        allArticles.push(...additionalArticles);

        // Generate combined JSON if requested
        if (options.combine) {
            try {
                // Sort articles by publication date (newest first)
                allArticles.sort((a, b) => {
                    const dateA = a.publicationDate ? new Date(a.publicationDate) : new Date(0);
                    const dateB = b.publicationDate ? new Date(b.publicationDate) : new Date(0);
                    return dateB - dateA;
                });

                const combinedFilePath = upath.join(jsonOutputBaseDir, "all-articles.json");
                await fs.writeFile(
                    combinedFilePath,
                    JSON.stringify({
                        count: allArticles.length,
                        generatedAt: new Date().toISOString(),
                        articles: allArticles
                    }, null, 2)
                );
                log.info(`Generated combined JSON file: ${combinedFilePath}`);
            } catch (err) {
                log.error(`Error generating combined JSON: ${err.message}`);
            }
        }

        // Final progress update
        log.progress(articlesData.length, articlesData.length);

        log.info(`Process completed. Generated ${processedCount} JSON files from articles.json with ${errorCount} errors.`);
        log.info(`Additional JSON files were generated from Pug files not in articles.json.`);
        log.info(`Output directory: ${jsonOutputBaseDir}`);
    } catch (err) {
        log.error(`Fatal error: ${err.message}`);
        process.exit(1);
    }
}

// Run the process
processArticles().catch(err => {
    console.error(`Fatal error: ${err.message}`);
    process.exit(1);
});
