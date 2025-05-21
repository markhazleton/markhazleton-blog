# Article Authoring Guide

This guide provides step-by-step instructions and best practices for adding new articles to the site. It covers updating the `articles.json` file, creating a new `.pug` article file, and ensuring your article is search engine optimized (SEO) and visually appealing using Bootstrap 5, Bootstrap Icons, and PrismJS for code samples.

---

## 1. Update `articles.json`

1. **Open** `src/articles.json`.
2. **Add a new entry** at the end of the array (before the closing `]`). Each article object should include:
   - `id`: Next available integer (increment from the last entry).
   - `Section`: The section/category for the article.
   - `slug`: The output HTML path (e.g., `my-new-article.html` or `folder/my-new-article.html`).
   - `name`: The article title.
   - `content`: (Optional) Short summary or null.
   - `description`: A concise, keyword-rich description for SEO.
   - `keywords`: Comma-separated keywords for SEO.
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
  "description": "A concise, keyword-rich description for SEO.",
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
3. **Use the following template as a starting point:**

```pug
extends ../layouts/articles

block pagehead
  title My New Article Title
  meta(name='description', content='A concise, keyword-rich description for SEO.')
  meta(name='keywords', content='keyword1, keyword2, keyword3')
  meta(name='author', content='Your Name')
  link(rel='canonical', href='https://markhazleton.com/articles/my-new-article.html')

block layout-content
  article#post.painteddesert-section.painteddesert-section-background
    .painteddesert-section-content
      h1 My New Article Title
      h2.subheading.mb-3 A compelling subheading
      p.lead. 
        Write a strong introduction that summarizes the article and includes primary keywords.
      //- Article content goes here
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

When embedding a YouTube video, update your meta tags for best SEO and social sharing:

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
  - Always provide descriptive `alt` text for preview images for accessibility and SEO.
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
- [ ] SEO meta tags and keywords set
- [ ] Bootstrap 5 and icons used for layout and visuals
- [ ] Code samples use PrismJS markup
- [ ] Images have descriptive alt text
- [ ] Article builds and previews correctly

---

By following these steps and guidelines, you’ll ensure your new article is well-structured, visually appealing, and optimized for both users and search engines.
