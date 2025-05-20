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

### SEO Guidelines

- **Title**: Use a clear, descriptive, and keyword-rich title.
- **Meta Description**: Write a concise summary (150–160 characters) with target keywords.
- **Keywords**: List relevant keywords in the `meta` tag.
- **Headings**: Use `h1` for the main title, `h2` for subheadings, and `h3`/`h4` for further structure.
- **Internal Links**: Link to other relevant articles on the site.
- **Alt Text**: Add descriptive `alt` text for all images.
- **Canonical Link**: Ensure the canonical URL is set correctly.

### Visual Appeal with Bootstrap 5 & Icons

- Use Bootstrap 5 classes for layout, spacing, and components (e.g., `.container`, `.row`, `.col`, `.card`).
- Use [Bootstrap Icons](https://icons.getbootstrap.com/) for visual enhancement:

  ```pug
  i.bi.bi-lightbulb // Example icon
  ```

- Use `.lead`, `.fw-bold`, `.text-primary`, `.bg-light`, etc., for emphasis and readability.

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
