# Article Authoring Guide

This comprehensive guide provides step-by-step instructions and best practices for adding new articles to the site using the modern-layout. It covers updating the `articles.json` file, creating `.pug` article files, and creating visually appealing content using Bootstrap 5, Bootstrap Icons, and PrismJS for code samples.

> **SEO Guidelines**: For comprehensive SEO validation rules, character limits, and optimization best practices, see [SEO.md](SEO.md).

---

## Table of Contents

1. [Converting Legacy Articles to Modern Layout](#0-converting-legacy-articles-to-modern-layout)
2. [Update articles.json](#1-update-articlesjson)
3. [Create the .pug Article File](#2-create-the-pug-article-file)
4. [Best Practices for Article Authoring](#3-best-practices-for-article-authoring)
5. [Build and Preview](#4-build-and-preview)
6. [Checklist Before Publishing](#5-checklist-before-publishing)
7. [AI Content Generation](#6-ai-content-generation)

---

## 0. Converting Legacy Articles to Modern Layout

If you're updating an existing article from the legacy `articles` layout to the `modern-layout`, follow these conversion steps:

### Layout Extension Update

- Change `extends ../layouts/articles` to `extends ../layouts/modern-layout`

### Meta Tag Restructuring

Organize meta tags into proper blocks:

- Move canonical link to its own `block canonical`
- Separate Open Graph tags into `block og_overrides`
- Separate Twitter Card tags into `block twitter_overrides`

### Content Structure Conversion

```pug
block layout-content
  br
  // Hero Section
  section.bg-gradient-primary.py-5
    .container
      .row.align-items-center
        .col-lg-10.mx-auto.text-center
          h1.display-4.fw-bold.mb-3
            i.bi.bi-[icon].me-3
            | [Article Title]
          h2.h3.mb-4 [Subtitle]
          p.lead.mb-5
            | [Introduction]

  // Main Article Content
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
```

### Section Modernization

Convert simple headers to Bootstrap card-based sections with icons and proper spacing.

**Reference:** See `MODERN-LAYOUT-CONVERSION-STEPS.md` for detailed conversion guidelines.

---

## 1. Update `articles.json`

1. **Open** `src/articles.json`.
2. **Add a new entry** at the end of the array (before the closing `]`). Each article object should include:
   - `id`: Next available integer (increment from the last entry).
   - `Section`: The section/category for the article.
   - `slug`: The output HTML path (e.g., `my-new-article.html` or `folder/my-new-article.html`).
   - `name`: The article title.
   - `content`: (Optional) Short summary or null.
   - `description`: A concise description for meta tags (see [SEO.md](SEO.md) for character limits).
   - `keywords`: Comma-separated keywords for meta tags.
   - `img_src`: Path to a relevant image (e.g., `assets/img/my-image.jpg`).
   - `lastmod`: Date in `YYYY-MM-DD` format.
   - `changefreq`: Update frequency (e.g., `monthly`).

**Example:**

```json
{
  "id": 99,
  "Section": "Tech Insights",
  "slug": "articles/my-new-article.html",
  "name": "My New Article Title",
  "content": "A short summary of the article.",
  "description": "A concise description for meta tags.",
  "keywords": "keyword1, keyword2, keyword3",
  "img_src": "assets/img/my-image.jpg",
  "lastmod": "2025-05-20",
  "changefreq": "monthly"
}
```

---

## 2. Create the `.pug` Article File

1. **Navigate to** `src/pug/articles/`.
2. **Create a new file** named after your article (e.g., `my-new-article.pug`).
3. **Use the modern-layout template:**

```pug
extends ../layouts/modern-layout

block pagehead
  title My New Article Title
  meta(name='description', content='A concise description for meta tags.')
  meta(name='keywords', content='keyword1, keyword2, keyword3')
  meta(name='author', content='Mark Hazleton')

block canonical
  link(rel='canonical', href='https://markhazleton.com/articles/my-new-article.html')

block og_overrides
  meta(property='og:title', content='My New Article Title')
  meta(property='og:description', content='A concise description for social sharing.')
  meta(property='og:url', content='https://markhazleton.com/articles/my-new-article.html')
  meta(property='og:type', content='article')

block twitter_overrides
  meta(name='twitter:title', content='My New Article Title')
  meta(name='twitter:description', content='A concise description for Twitter sharing.')

block layout-content
  br
  // Hero Section
  section.bg-gradient-primary.py-5
    .container
      .row.align-items-center
        .col-lg-10.mx-auto.text-center
          h1.display-4.fw-bold.mb-3
            i.bi.bi-lightbulb.me-3
            | My New Article Title
          h2.h3.mb-4 A compelling subheading
          p.lead.mb-5
            | Write a strong introduction that summarizes the article and includes primary keywords.

  // Main Article Content
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
          // Table of Contents (for long articles)
          nav#table-of-contents.mb-5(aria-label='Table of Contents')
            .card.bg-light
              .card-header
                h3.card-title.mb-0.fw-bold
                  i.bi.bi-list-ul.me-2
                  | Table of Contents
              .card-body
                ul.list-group.list-group-flush
                  li.list-group-item: a.text-decoration-none(href='#section1') Section 1
                  li.list-group-item: a.text-decoration-none(href='#section2') Section 2

          // Article sections go here
          section#section1.mb-5
            h2.h3.text-primary.mb-4
              i.bi.bi-gear.me-2
              | Section Title
            p.lead Content goes here...
```

---

## 3. Best Practices for Article Authoring

### Embedding YouTube Videos Responsively

- **Use Bootstrap’s Responsive Ratio Utility**
  - Wrap YouTube `iframe` embeds in a `.ratio.ratio-16x9` container for responsive design:

    ```pug
    .ratio.ratio-16x9
      iframe(
        src="https://www.youtube.com/embed/VIDEO_ID"
        title="Descriptive video title"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
        allowfullscreen
      )
    ```

- **Accessibility and SEO**
  - Always provide a descriptive `title` attribute for the `iframe`.
  - Place the video in a visually distinct section, such as inside a `.card` or with a heading, to provide context.
  - Optionally, add a short caption or summary below the video for users and search engines.

#### Example: Embedding a YouTube Video in a Card

```pug
.card.mb-3
  .card-header.bg-primary.text-white
    h5.card-title Watch the Deep Dive Podcast
  .card-body
    .ratio.ratio-16x9
      iframe(
        src="https://www.youtube.com/embed/VIDEO_ID"
        title="Descriptive video title"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
        allowfullscreen
      )
  .card-footer
    p.card-text Short caption or context for the video.
```

### Meta Tags for YouTube Videos and Preview Images

When embedding a YouTube video, update your meta tags for best social sharing:

- **Open Graph (og) tags:**

  ```pug
  meta(property='og:video', content='https://www.youtube.com/embed/VIDEO_ID')
  meta(property='og:video:type', content='text/html')
  meta(property='og:video:width', content='560')
  meta(property='og:video:height', content='315')
  meta(property='og:video:url', content='https://www.youtube.com/embed/VIDEO_ID')
  meta(property='og:video:secure_url', content='https://www.youtube.com/embed/VIDEO_ID')
  meta(property='og:image', content='https://img.youtube.com/vi/VIDEO_ID/maxresdefault.jpg')
  meta(property='og:image:alt', content='Descriptive preview image alt text')
  ```

- **Twitter Card tags:**

  ```pug
  meta(name='twitter:card', content='player')
  meta(name='twitter:player', content='https://www.youtube.com/embed/VIDEO_ID')
  meta(name='twitter:player:width', content='560')
  meta(name='twitter:player:height', content='315')
  meta(name='twitter:image', content='https://img.youtube.com/vi/VIDEO_ID/maxresdefault.jpg')
  meta(name='twitter:image:alt', content='Descriptive preview image alt text')
  ```

- **Best Practices:**

  - Use the YouTube video’s `maxresdefault.jpg` as the preview image for best quality.
  - Always provide descriptive `alt` text for preview images for accessibility.
  - Ensure all video and image URLs are correct and use HTTPS.
  - Include both video and image meta tags for optimal display on all platforms.

### Visual Appeal with Bootstrap 5 & Icons

- Use Bootstrap 5 classes for layout, spacing, and components (e.g., `.container`, `.row`, `.col`, `.card`).
- Use [Bootstrap Icons](https://icons.getbootstrap.com/) for visual enhancement:

  ```pug
  i.bi.bi-lightbulb // Example icon
  ```

- Use `.lead`, `.fw-bold`, `.text-primary`, `.bg-light`, etc., for emphasis and readability.

### Image Best Practices

- Always use descriptive `alt` and `title` attributes for images.
- Use Bootstrap classes like `.img-fluid`, `.rounded`, `.mx-auto`, and `.d-block` for responsive, accessible images.

### Code Samples with PrismJS

- Wrap code samples in `pre` and `code` tags with the appropriate language class:

  ```pug
  pre.language-javascript
    code.language-javascript.
      // Your code here
  ```

- For long code samples, consider using Bootstrap’s collapse component:

  ```pug
  button.btn.btn-primary(type='button', data-bs-toggle='collapse', data-bs-target='#codeBlock') Show/Hide Code
  .collapse#codeBlock
    pre.language-javascript
      code.language-javascript.
        // Your code here
  ```

- Use PrismJS plugins for line numbers and copy-to-clipboard if needed.

### General Writing Tips

- Write in a clear, engaging, and concise style.
- Use short paragraphs and bullet points for readability.
- Add images, diagrams, or videos to illustrate key points.
- Proofread for grammar, spelling, and clarity.

### Table of Contents for Long Articles

- For articles with multiple sections, include a Table of Contents at the top using a Bootstrap-styled list or navigation. This improves navigation and user experience, especially for in-depth or reference articles.
- Example:

```pug
nav#table-of-contents.mb-4(aria-label='Table of Contents')
  h3.fw-bold Table of Contents
  ul.list-group.list-group-flush
    li.list-group-item: a(href='#section1') Section 1
    li-list-group-item: a(href='#section2') Section 2
    // ...
```

### Section Navigation

- For long articles, add "Back to Top" links after major sections to help users quickly return to the Table of Contents.
- Example:

```pug
a(href='#table-of-contents' class='d-block mb-4 text-decoration-none')
  i.bi.bi-arrow-up-circle.me-1(aria-hidden)
  | Back to Top
```

### Accordions for Principles, Timelines, or FAQs

- Use Bootstrap accordions to present lists of principles, historical timelines, FAQs, or code comparisons. This keeps the page organized and allows users to expand only the sections they are interested in.
- Example:

```pug
.accordion#example-accordion
  .accordion-item
    span.accordion-header
      button.accordion-button(type='button', data-bs-toggle='collapse', data-bs-target='#collapseOne')
        | Principle 1
    .accordion-collapse.collapse#collapseOne
      .accordion-body
        p Principle details here.
```

### Code Comparison for Multi-Language Readers

- When comparing code between languages (e.g., Python vs. C#), use side-by-side or accordion-based code blocks with PrismJS highlighting for each language.
- Example:

```pug
pre.language-csharp
  code.language-csharp.
    // C# code here
pre.language-python
  code.language-python.
    # Python code here
```

### Use of Bootstrap Icons

- Use Bootstrap Icons for visual cues, such as next to navigation links or section headers, to enhance clarity and user experience.
- Example:

```pug
i.bi.bi-arrow-up-circle.me-1(aria-hidden)
```

### Glossary for Technical Terms

- For technical or educational articles, include a glossary section at the end using a Bootstrap accordion. Define key terms and link to reputable sources (e.g., Wikipedia) for further reading.
- Example:

```pug
h2.mt-5.mb-3 Glossary of Key Terms
.accordion#glossaryAccordion
  .accordion-item
    h2.accordion-header#glossaryTerm
      button.accordion-button.collapsed(type='button', data-bs-toggle='collapse', data-bs-target='#collapseTerm') Term
    .accordion-collapse.collapse#collapseTerm
      .accordion-body
        p Definition and context. See more on 
          a(href='https://en.wikipedia.org/wiki/Term' target='_blank' rel='noopener') Wikipedia
```

### General Writing and Layout

- Use clear, descriptive headings for each section.
- Use `.lead` for introductory paragraphs.
- Use `.fw-bold`, `.mb-3`, `.mb-4`, etc., for spacing and emphasis.
- For historical or process-oriented content, use timelines or step-by-step accordions.
- For FAQs, use accordions for each question/answer pair.

---

## 4. Build and Preview

1. **Run the build scripts** to generate the HTML output:
   - In the terminal, run:

     ```pwsh
     node scripts/build-pug.js
     ```

2. **Preview your article** in the `docs/articles/` folder or by running the local server:

   - ```pwsh
     node simple-serve.js
     ```

   - Open `http://localhost:8080/articles/my-new-article.html` in your browser.

---

## 5. Checklist Before Publishing

- [ ] Article entry added to `articles.json`
- [ ] `.pug` file created and content added
- [ ] Meta tags properly set (see [SEO.md](SEO.md) for guidelines)
- [ ] Bootstrap 5 and icons used for layout and visuals
- [ ] Code samples use PrismJS markup
- [ ] Images have descriptive alt text
- [ ] Article builds and previews correctly

---

## 6. AI Content Generation

The Web Admin system provides powerful AI-powered content generation capabilities that can automatically create comprehensive SEO metadata and content sections.

### Using the Generate AI Content Feature

1. **Access Web Admin**: Navigate to the article edit page in the Web Admin interface
2. **Ensure Content Exists**: The article must have content in the main content field
3. **Click "Generate AI Content"**: The button will validate content and begin generation
4. **Wait for Completion**: The process may take 30-60 seconds to complete
5. **Review Generated Fields**: All updated fields will be highlighted in green

### Fields Generated by AI

When using AI content generation, the following fields are automatically populated:

#### Core Article Fields

- **Keywords**: SEO-optimized keyword list (3-8 keywords)
- **Description**: Article description for SEO (120-160 characters)
- **Summary**: Article introduction/summary

#### SEO Metadata

- **SEO Title**: Search engine optimized title (30-60 characters)
- **SEO Description**: Meta description for search results (120-160 characters)

#### Social Media Optimization

- **Open Graph Title**: Facebook/LinkedIn preview title (30-65 characters)
- **Open Graph Description**: Facebook/LinkedIn preview description (100-300 characters)
- **Twitter Title**: Twitter card title (max 50 characters)
- **Twitter Description**: Twitter card description (120-160 characters)

#### Conclusion Section

- **Conclusion Title**: Section heading for conclusion
- **Conclusion Summary**: Summary of main points
- **Key Takeaway Heading**: Heading for key insight
- **Key Takeaway Text**: Main actionable insight
- **Final Thoughts**: Closing thoughts and call to action

### AI Generation Best Practices

#### Content Requirements

- **Minimum Length**: Ensure article has substantial content (500+ words recommended)
- **Clear Structure**: Well-structured content with headings and sections
- **Complete Drafts**: AI works best with complete article content rather than fragments

#### Review and Refinement

- **Manual Review**: Always review AI-generated content for accuracy and tone
- **Brand Alignment**: Ensure generated content matches your brand voice
- **SEO Optimization**: Verify character counts match SEO requirements
- **Keyword Relevance**: Confirm keywords are relevant and properly targeted

#### Technical Considerations

- **API Limits**: Be aware of OpenAI API rate limits during content generation
- **Timeout Handling**: The system includes 60-second timeout protection
- **Error Recovery**: If generation fails, check content length and try again
- **Logging**: Full debugging logs available for troubleshooting

### Troubleshooting AI Generation

#### Common Issues

1. **Button Doesn't Respond**
   - Check browser console for JavaScript errors
   - Ensure article content is present
   - Verify you're on the correct edit page

2. **Generation Fails**
   - Check content length (too short may cause issues)
   - Verify OpenAI API configuration
   - Check network connectivity

3. **Fields Not Updated**
   - Look for field highlighting (green background)
   - Check success message at top of page
   - Verify all expected fields received content

#### Debug Information

The system provides comprehensive logging:

- **Browser Console**: Client-side validation and submission tracking
- **Server Logs**: API call status and field update confirmation
- **Visual Feedback**: 8-second field highlighting for updated content

### Manual Override

While AI generation is powerful, you can always:

- **Edit Generated Content**: Modify any AI-generated field as needed
- **Partial Use**: Use some AI fields while manually creating others
- **Iterative Improvement**: Run generation multiple times with refined content

---

By following these steps and guidelines, you’ll ensure your new article is well-structured, visually appealing, and optimized for both users and search engines.
