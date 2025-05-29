<SYSTEM>
You are an AI programming assistant that is specialized in applying code changes to an existing document.
Follow Microsoft content policies.
Avoid content that violates copyrights.
If you are asked to generate content that is harmful, hateful, racist, sexist, lewd, violent, or completely irrelevant to software engineering, only respond with "Sorry, I can't assist with that."
Keep your answers short and impersonal.
The user has a code block that represents a suggestion for a code change and a instructions file opened in a code editor.
Rewrite the existing document to fully incorporate the code changes in the provided code block.
For the response, always follow these instructions:
1. Analyse the code block and the existing document to decide if the code block should replace existing code or should be inserted.
2. If necessary, break up the code block in multiple parts and insert each part at the appropriate location.
3. Preserve whitespace and newlines right after the parts of the file that you modify.
4. The final result must be syntactically valid, properly formatted, and correctly indented. It should not contain any ...existing code... comments.
5. Finally, provide the fully rewritten file. You must output the complete file.
</SYSTEM>

I have the following code open in the editor, starting from line 1 to line 1006.

````instructions
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available.# GitHub Copilot Instructions for Article Authoring with PUG

## Project Overview
This project uses PUG (formerly Jade) template engine for creating dynamic, SEO-optimized articles with Bootstrap 5, Bootstrap Icons, and PrismJS. Articles are configured through `articles.json` and built using custom Node.js scripts.

## File Structure Context
```
src/
├── articles.json           # Article configuration and metadata
├── pug/
│   ├── articles/          # Individual article PUG files
│   ├── layouts/           # PUG layout templates
│   └── mixins/            # Reusable PUG components
├── assets/                # Static assets (images, styles)
│   ├── img/               # images for articles
│   └── video/             # Video assets
docs/
└── articles/              # Generated HTML output
```

## Article Creation Workflow

### 1. Article Configuration in `articles.json`
When creating new articles, add entries with these required fields:
- `id`: Sequential integer (increment from last entry)
- `Section`: Category/section name
- `slug`: Output path (e.g., `articles/my-article.html`)
- `name`: Article title for display
- `content`: Optional summary or null
- `description`: SEO meta description (150-160 characters)
- `keywords`: Comma-separated SEO keywords
- `img_src`: Featured image path
- `lastmod`: Date in YYYY-MM-DD format
- `changefreq`: Update frequency (`monthly`, `weekly`, etc.)

**Example JSON Entry:**
```json
{
  "id": 99,
  "Section": "Tech Insights",
  "slug": "articles/understanding-pug-templates.html",
  "name": "Understanding PUG Template Engine",
  "content": "A comprehensive guide to PUG templating",
  "description": "Learn PUG template engine syntax, best practices, and integration with Bootstrap for modern web development.",
  "keywords": "pug template, jade template, nodejs templating, bootstrap integration",
  "img_src": "assets/img/pug-templating.jpg",
  "lastmod": "2025-05-28",
  "changefreq": "monthly"
}
```

### 2. Modern Layout Template Structure
All files should use the `modern-layout.pug` template which provides:
- Complete responsive Bootstrap 5 framework
- Bootstrap Icons integration
- Font Awesome icons
- SEO-optimized metadata structure
- Performance-optimized CDN links
- Google Analytics integration
- Structured data (JSON-LD) support

**Base Article Template Structure:**
```pug
extends ../layouts/modern-layout

block variables
  - var pageTitle = 'Article Title Here'
  - var pageDescription = 'SEO-optimized description (150-160 characters)'
  - var pageKeywords = 'keyword1, keyword2, keyword3'
  - var pageCanonical = 'https://markhazleton.com/articles/article-slug.html'

block pagehead
  title= pageTitle
  meta(name='description', content=pageDescription)
  meta(name='keywords', content=pageKeywords)

block og_overrides
  meta(property='og:title', content=pageTitle)
  meta(property='og:description', content=pageDescription)
  meta(property='og:url', content=pageCanonical)
  meta(property='og:type', content='article')

block twitter_overrides
  meta(name='twitter:title', content=pageTitle)
  meta(name='twitter:description', content=pageDescription)

block canonical
  link(rel='canonical', href=pageCanonical)

block layout-content
  // Hero Section (OPTIONAL - follow hero section guidelines below)
  section.bg-gradient-primary.py-5
    .container
      .row.align-items-center
        .col-lg-10.mx-auto.text-center
          h1.display-4.fw-bold.mb-3
            i.bi.bi-lightbulb.me-3
            | Article Title
          h2.h3.mb-4 Article Subtitle or Theme
          p.lead.mb-5
            | Compelling introduction paragraph that sets the stage for the article content.
            | Use this space to provide context and draw readers into the main content.
            | Multiple paragraphs can be included as needed to properly introduce the topic.

  // Main Article Content (immediate transition - no spacing)
  article#main-article
    .container
      .row
        .col-lg-8.mx-auto
          // Article sections go here
          section.mb-5
            h2.h3.mb-4 Section Heading
            p Regular paragraph content with proper spacing.

        .col-lg-4.col-xl-3
          aside.sticky-top
            // Sidebar content
```

### Hero Section Guidelines

**IMPORTANT HERO SECTION RULES:**
1. **Text Colors**: NEVER use `text-white`, `text-light`, or explicit light text classes
   - Use Bootstrap's default text colors which work well with gradient backgrounds
   - Let Bootstrap handle text contrast automatically

2. **Spacing**: NO padding/margin between hero section and main article content
   - Hero section ends with `.py-5` padding
   - Article content starts immediately with `article#main-article` (no additional spacing classes)

3. **Buttons**: NO call-to-action buttons that link to content within the same article
   - Hero sections should not contain "Read More" or "Learn More" buttons
   - Keep hero content focused on introduction and context

4. **Structure**: Follow this exact pattern when including hero sections:
   ```pug
   // Hero Section
   section.bg-gradient-primary.py-5
     .container
       .row.align-items-center
         .col-lg-10.mx-auto.text-center
           h1.display-4.fw-bold.mb-3
             i.bi.bi-[icon-name].me-3
             | Article Title
           h2.h3.mb-4 Subtitle or Theme
           p.lead.mb-5
             | Introduction content that can span multiple lines.
             | Provide comprehensive context and draw readers into the content.
             | End with mb-5 for proper spacing within the hero section.

   // Main Article Content (immediate transition)
   article#main-article
     .container
       .row
         .col-lg-8.mx-auto
           // Content here
   ```

5. **When to Use Heroes**: Hero sections are optional and should be used sparingly
   - Best for feature articles, major announcements, or comprehensive guides
   - Skip for shorter articles or technical documentation
   - Consider the article's importance and visual impact needs

## Content Structure Best Practices

### Article Layout Pattern
```pug
block layout-content
  article#main-article.py-5
    .container
      .row
        .col-lg-8.col-xl-9
          // Main article content
          header.mb-5
            h1.display-4.fw-bold Article Title
            .article-meta.text-muted.mb-4
              time(datetime='2025-05-28') May 28, 2025
              span.mx-2 •
              span.author by Mark Hazleton
              span.mx-2 •
              span.reading-time 8 min read

          .article-content
            p.lead Introduction paragraph with primary keywords and article overview.

            section.mb-5
              h2.h3.mb-4 Section Heading
              p Regular paragraph content with proper spacing.

            section.mb-5
              h2.h3.mb-4 Another Section
              // More content

        .col-lg-4.col-xl-3
          // Sidebar with related content
          aside.sticky-top
            .card.mb-4
              .card-header.bg-primary.text-white
                h5.mb-0 Table of Contents
              .card-body
                // TOC links
```

### Table of Contents
```pug
nav#table-of-contents.mb-5(aria-label='Table of Contents')
  .card
    .card-header.bg-primary.text-white
      h3.card-title.mb-0
        i.bi.bi-list-ul.me-2
        | Table of Contents
    .card-body
      ol.list-group.list-group-numbered.list-group-flush
        li.list-group-item
          a.text-decoration-none(href='#introduction') Introduction
        li.list-group-item
          a.text-decoration-none(href='#getting-started') Getting Started
        li.list-group-item
          a.text-decoration-none(href='#advanced-topics') Advanced Topics
```

### Accordion Sections
```pug
.accordion#faqAccordion.mb-4
  - var faqs = [{q: 'What is PUG?', a: 'PUG is a template engine...'}, {q: 'Why use PUG?', a: 'PUG offers clean syntax...'}]
  each faq, index in faqs
    .accordion-item
      h2.accordion-header(id=`heading${index}`)
        button.accordion-button.collapsed(
          type='button'
          data-bs-toggle='collapse'
          data-bs-target=`#collapse${index}`
          aria-expanded='false'
          aria-controls=`collapse${index}`
        )= faq.q
      .accordion-collapse.collapse(
        id=`collapse${index}`
        aria-labelledby=`heading${index}`
        data-bs-parent='#faqAccordion'
      )
        .accordion-body= faq.a
```

### Image Handling
```pug
//- Responsive images with proper attributes
figure.mb-4
  img.img-fluid.rounded(
    src='assets/img/example.jpg'
    alt='Descriptive alt text for accessibility'
    title='Image title for additional context'
    loading='lazy'
  )
  figcaption.text-muted.mt-2.small Image caption explaining the content

//- Image with lightbox/modal trigger
.text-center.mb-4
  img.img-fluid.rounded.cursor-pointer(
    src='assets/img/thumbnail.jpg'
    alt='Click to enlarge'
    data-bs-toggle='modal'
    data-bs-target='#imageModal'
  )
```

## PUG Template Engine Specifics

### Core Syntax Rules
1. **Indentation-based structure**: Use 2-space indentation consistently
2. **No closing tags**: Structure determined by indentation
3. **Element syntax**: `tagname(attributes) content` or `tagname content`
4. **Class shortcuts**: `div.className` or `.className` (implies div)
5. **ID shortcuts**: `div#idName` or `#idName` (implies div)
6. **Multiple classes/IDs**: `.class1.class2#myId`

### Attributes in PUG
```pug
//- Single attributes
a(href='https://example.com') Link text
img(src='image.jpg', alt='Description', title='Image title')

//- Multiple attributes (space or comma separated)
input(type='text' name='username' required)
button(type='submit', class='btn btn-primary', disabled)

//- Dynamic attributes
a(href=linkUrl, class=buttonClass) #{linkText}

//- Conditional attributes
div(class=isActive ? 'active' : 'inactive')
```

### Variables and Interpolation
```pug
//- Variable assignment
- var title = 'My Article'
- var author = 'John Doe'

//- String interpolation
h1 #{title} by #{author}
p Welcome to #{title.toLowerCase()}

//- Expression interpolation
p Published on #{new Date().getFullYear()}

//- Unescaped interpolation (use cautiously)
div !{htmlContent}
```

### Control Structures

#### Conditionals
```pug
//- If statements
if user.isLoggedIn
  p Welcome back, #{user.name}!
else
  p Please log in

//- Unless statements
unless user.isAdmin
  p You don't have admin privileges

//- Ternary operators
p(class=isHighlighted ? 'highlight' : 'normal') Content here
```

#### Loops
```pug
//- Each loops
- var items = ['item1', 'item2', 'item3']
ul
  each item in items
    li= item

//- Each with index
ul
  each item, index in items
    li #{index + 1}. #{item}

//- Object iteration
- var user = {name: 'John', age: 30, role: 'developer'}
ul
  each value, key in user
    li #{key}: #{value}

//- While loops
- var i = 0
ul
  while i < 3
    li Item #{++i}
```

### Comments
```pug
//- Visible HTML comment
// This will appear in rendered HTML

//- Invisible comment (PUG only)
//- This won't appear in HTML output

//- Block comments
//-
  This is a block comment
  spanning multiple lines
  invisible in HTML
```

### Text Content
```pug
//- Inline text
p This is inline text

//- Piped text
p
  | This is piped text
  | on multiple lines

//- Block text with periods
p.
  This is a text block that preserves
  line breaks and formatting.
  Perfect for longer paragraphs.

//- Raw HTML (use sparingly)
div
  | Some text
  strong bold text
  |  more text
```

## Bootstrap 5 Integration & Styling Guidelines

### **CRITICAL STYLING RULES**
1. **NO INLINE STYLES**: Never use `style` attributes on individual elements
2. **NO CUSTOM CSS**: Do not create custom CSS classes on individual pages
3. **USE BOOTSTRAP UTILITIES**: Leverage Bootstrap 5's extensive utility classes
4. **BOOTSTRAP ICONS FIRST**: Use Bootstrap Icons before Font Awesome when possible
5. **RESPONSIVE DESIGN**: Always use Bootstrap's responsive classes

### Core Bootstrap 5 Utility Classes

#### Spacing (Margin/Padding)
```pug
//- Margin utilities: m-{size}, mt-{size}, mb-{size}, ms-{size}, me-{size}
.mb-4          // margin-bottom: 1.5rem
.mt-3.mb-5     // margin-top: 1rem, margin-bottom: 3rem
.px-4          // padding-left and padding-right: 1.5rem
.py-2          // padding-top and padding-bottom: 0.5rem

//- Responsive spacing
.mb-2.mb-md-4  // Small margin on mobile, larger on desktop
```

#### Text Utilities
```pug
//- Typography
.fs-1          // font-size: calc(1.375rem + 1.5vw)
.fs-4          // font-size: calc(1.275rem + 0.3vw)
.fw-bold       // font-weight: 700
.fw-light      // font-weight: 300
.text-muted    // color: #6c757d
.text-primary  // color: #0d6efd
.text-center   // text-align: center
.text-md-start // text-align: start on md and up

//- Lead text for emphasis
p.lead This is lead text that stands out
```

#### Display & Visibility
```pug
//- Display utilities
.d-none           // display: none
.d-md-block       // display: block on md and up
.d-flex           // display: flex
.d-inline-flex    // display: inline-flex

//- Flex utilities
.justify-content-center    // justify-content: center
.align-items-center        // align-items: center
.flex-column               // flex-direction: column
.flex-md-row               // flex-direction: row on md and up
```

#### Colors & Backgrounds
```pug
//- Background colors
.bg-primary       // Bootstrap primary blue
.bg-light         // Light gray background
.bg-dark          // Dark background
.bg-transparent   // Transparent background

//- Text colors
.text-white       // White text
.text-dark        // Dark text
.text-success     // Success green
.text-danger      // Danger red
```

### Layout Components

#### Responsive Grid System
```pug
//- Modern responsive layout
.container-fluid
  .row.g-4                    // Gap between columns
    .col-12.col-md-8.col-lg-9  // Full width mobile, 8/12 tablet, 9/12 desktop
      // Main content
    .col-12.col-md-4.col-lg-3  // Full width mobile, 4/12 tablet, 3/12 desktop
      // Sidebar content

//- Equal columns with responsive breakpoints
.row.row-cols-1.row-cols-md-2.row-cols-lg-3.g-4
  .col
    .card // Card content
  .col
    .card // Card content
```

#### Card Components
```pug
//- Standard article card
.card.h-100.shadow-sm.hover-card
  .card-img-top(
    src='assets/img/article-image.jpg'
    alt='Article description'
    loading='lazy'
  )
  .card-body.d-flex.flex-column
    h5.card-title Article Title
    p.card-text.flex-grow-1 Article summary text...
    .d-flex.justify-content-between.align-items-center
      small.text-muted May 28, 2025
      .badge.bg-primary Tech Insights

//- Feature card with icon
.card.text-center.border-0.shadow
  .card-body.py-5
    .mb-3
      i.bi.bi-code-slash.display-4.text-primary
    h4.card-title Feature Title
    p.card-text Feature description
```

#### Navigation Components
```pug
//- Breadcrumb navigation
nav(aria-label='breadcrumb')
  ol.breadcrumb
    li.breadcrumb-item
      a.text-decoration-none(href='/') Home
    li.breadcrumb-item
      a.text-decoration-none(href='/articles') Articles
    li.breadcrumb-item.active(aria-current='page') Current Article

//- Pills navigation
ul.nav.nav-pills.justify-content-center.mb-4
  li.nav-item
    a.nav-link.active(href='#overview') Overview
  li.nav-item
    a.nav-link(href='#examples') Examples
```

### Bootstrap Icons Implementation

#### Essential Icon Patterns
```pug
//- Icons with text (always use spacing utilities)
.btn.btn-primary
  i.bi.bi-download.me-2
  | Download File

//- Icon-only buttons with proper accessibility
button.btn.btn-outline-secondary(
  type='button'
  aria-label='Edit article'
  title='Edit article'
)
  i.bi.bi-pencil-square

//- Navigation with icons
ul.nav.nav-pills
  li.nav-item
    a.nav-link.d-flex.align-items-center(href='#')
      i.bi.bi-house.me-2
      | Home
  li.nav-item
    a.nav-link.d-flex.align-items-center(href='#')
      i.bi.bi-journal-text.me-2
      | Articles

//- Feature highlights with large icons
.text-center.mb-4
  i.bi.bi-shield-check.display-1.text-success.mb-3
  h3 Secure & Reliable
  p.text-muted Built with security best practices
```

#### Common Icon Categories
```pug
//- Technology & Development
i.bi.bi-code-slash         // Code/programming
i.bi.bi-terminal           // Command line/terminal
i.bi.bi-git                // Git/version control
i.bi.bi-database           // Database
i.bi.bi-cloud              // Cloud services
i.bi.bi-gear               // Settings/configuration

//- Content & Media
i.bi.bi-journal-text       // Articles/blog
i.bi.bi-camera             // Photography
i.bi.bi-play-circle        // Video content
i.bi.bi-mic                // Audio/podcast
i.bi.bi-file-earmark-text  // Documents

//- Navigation & Actions
i.bi.bi-arrow-right        // Forward/next
i.bi.bi-arrow-left         // Back/previous
i.bi.bi-download           // Download
i.bi.bi-share              // Share
i.bi.bi-bookmark           // Save/bookmark
i.bi.bi-heart              // Like/favorite

//- Social & Communication
i.bi.bi-linkedin           // LinkedIn
i.bi.bi-github             // GitHub
i.bi.bi-youtube            // YouTube
i.bi.bi-envelope           // Email
i.bi.bi-chat               // Chat/messaging
```

### Responsive Design Patterns

#### Mobile-First Approach
```pug
//- Stack on mobile, side-by-side on larger screens
.row
  .col-12.col-md-6.mb-3.mb-md-0
    h4 Mobile-First Content
    p Content that stacks on mobile
  .col-12.col-md-6
    h4 Side-by-side on Desktop
    p Appears next to first column on desktop

//- Hide/show content based on screen size
.d-block.d-md-none Mobile-only content
.d-none.d-md-block Desktop-only content

//- Responsive text sizing
h1.fs-1.fs-md-display-4 Responsive Heading
```

#### Component Spacing
```pug
//- Section spacing
section.py-5                    // Large section padding
  .container
    .row.g-4                   // Gap between grid items
      .col-md-6
        .mb-4                  // Bottom margin for elements
          h3.mb-3             // Consistent heading spacing
          p.mb-0              // Remove default paragraph margin

//- Card spacing in grids
.row.row-cols-1.row-cols-md-2.row-cols-lg-3.g-4
  .col
    .card.h-100.shadow-sm     // Equal height cards with subtle shadow
```

### Accessibility & SEO Integration
```pug
//- Proper heading hierarchy
h1 Main Article Title
h2.mt-5.mb-3 Section Heading
h3.mt-4.mb-3 Subsection Heading

//- Accessible buttons and links
button.btn.btn-primary(
  type='button'
  aria-label='Download PDF version'
)
  i.bi.bi-file-earmark-pdf.me-2
  | Download PDF

//- Skip links for keyboard navigation
a.visually-hidden-focusable.btn.btn-primary(href='#main-content')
  | Skip to main content

//- Proper image attributes
img.img-fluid.rounded(
  src='assets/img/example.jpg'
  alt='Descriptive text for screen readers'
  loading='lazy'
  title='Additional context on hover'
)
```

## Video Embedding (YouTube)

### Responsive Video Embeds
```pug
//- Basic responsive embed
.ratio.ratio-16x9
  iframe(
    src='https://www.youtube.com/embed/VIDEO_ID'
    title='Descriptive video title'
    allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share'
    allowfullscreen
  )

//- Video in card component
.card.mb-4
  .card-header.bg-dark.text-white
    h5.card-title
      i.bi.bi-play-circle.me-2
      | Featured Video
  .card-body.p-0
    .ratio.ratio-16x9
      iframe(
        src='https://www.youtube.com/embed/VIDEO_ID'
        title='Video description'
        allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share'
        allowfullscreen
      )
  .card-footer
    p.card-text.mb-0 Video caption or description
```

### Video Meta Tags
```pug
//- In pagehead block
meta(property='og:video', content='https://www.youtube.com/embed/VIDEO_ID')
meta(property='og:video:type', content='text/html')
meta(property='og:video:width', content='560')
meta(property='og:video:height', content='315')
meta(property='og:image', content='https://img.youtube.com/vi/VIDEO_ID/maxresdefault.jpg')
meta(property='og:image:alt', content='Video preview description')

meta(name='twitter:card', content='player')
meta(name='twitter:player', content='https://www.youtube.com/embed/VIDEO_ID')
meta(name='twitter:player:width', content='560')
meta(name='twitter:player:height', content='315')
meta(name='twitter:image', content='https://img.youtube.com/vi/VIDEO_ID/maxresdefault.jpg')
```

## Code Samples with PrismJS

### Basic Code Blocks
```pug
//- JavaScript code sample
pre.language-javascript
  code.language-javascript.
    function greetUser(name) {
      return `Hello, ${name}!`;
    }

    console.log(greetUser('World'));

//- CSS code sample
pre.language-css
  code.language-css.
    .btn-custom {
      background-color: #007bff;
      border-radius: 8px;
      padding: 12px 24px;
    }

//- HTML code sample
pre.language-html
  code.language-html.
    <div class="container">
      <h1>Welcome</h1>
      <p>This is a sample HTML snippet.</p>
    </div>
```

### Collapsible Code Blocks
```pug
//- Expandable code section
.mb-4
  button.btn.btn-outline-primary(
    type='button'
    data-bs-toggle='collapse'
    data-bs-target='#codeExample'
    aria-expanded='false'
  )
    i.bi.bi-code-slash.me-2
    | Show/Hide Code Example

  .collapse.mt-3#codeExample
    pre.language-javascript
      code.language-javascript.
        // Complex code example here
        class DataProcessor {
          constructor(data) {
            this.data = data;
          }

          process() {
            return this.data.map(item => ({
              ...item,
              processed: true
            }));
          }
        }
```

## PUG Mixins for Reusable Components

### Creating Mixins
```pug
//- In mixins file (e.g., mixins/components.pug)
mixin alertBox(type, message, dismissible = false)
  .alert(class=`alert-${type}`, role='alert')
    if dismissible
      button.btn-close(type='button', data-bs-dismiss='alert')
    | #{message}

mixin codeBlock(language, code)
  pre(class=`language-${language}`)
    code(class=`language-${language}`)= code

mixin iconLink(icon, text, url, external = false)
  a(href=url, target=external ? '_blank' : '_self', rel=external ? 'noopener' : null)
    i(class=`bi bi-${icon}`).me-2
    | #{text}
    if external
      i.bi.bi-box-arrow-up-ne.ms-1
```

### Using Mixins
```pug
//- Include mixins file
include ../mixins/components

//- Use mixins in content
+alertBox('info', 'This is an informational message', true)
+alertBox('warning', 'Please review this section carefully')

+codeBlock('javascript', 'const message = "Hello World";')

+iconLink('github', 'View on GitHub', 'https://github.com/user/repo', true)
+iconLink('house', 'Home', '/')
```

## Advanced PUG Features

### Template Inheritance
```pug
//- Base layout (layouts/base.pug)
doctype html
html(lang='en')
  head
    block head
      title Default Title
  body
    block content
    block scripts

//- Article layout (layouts/articles.pug)
extends base

block head
  block pagehead
  // Common article styles/scripts

block content
  nav.navbar
    // Navigation
  main
    block layout-content
  footer
    // Footer content
```

### Includes for Partials
```pug
//- Navigation partial (partials/nav.pug)
nav.navbar.navbar-expand-lg.navbar-dark.bg-dark
  .container
    a.navbar-brand(href='/') Site Name
    ul.navbar-nav
      li.nav-item
        a.nav-link(href='/') Home
      li.nav-item
        a.nav-link(href='/articles') Articles

//- Including in main template
include partials/nav
```

### Case Statements
```pug
- var articleType = 'tutorial'
.badge(class={
  'tutorial': 'bg-primary',
  'review': 'bg-success',
  'news': 'bg-info',
  'opinion': 'bg-warning'
}[articleType] || 'bg-secondary')= articleType.toUpperCase()
```

## Content Structure Best Practices

### Table of Contents
```pug
nav#table-of-contents.mb-5(aria-label='Table of Contents')
  .card
    .card-header.bg-primary.text-white
      h3.card-title.mb-0
        i.bi.bi-list-ul.me-2
        | Table of Contents
    .card-body
      ol.list-group.list-group-numbered.list-group-flush
        li.list-group-item
          a.text-decoration-none(href='#introduction') Introduction
        li.list-group-item
          a.text-decoration-none(href='#getting-started') Getting Started
        li.list-group-item
          a.text-decoration-none(href='#advanced-topics') Advanced Topics
```

### Accordion Sections
```pug
.accordion#faqAccordion.mb-4
  - var faqs = [{q: 'What is PUG?', a: 'PUG is a template engine...'}, {q: 'Why use PUG?', a: 'PUG offers clean syntax...'}]
  each faq, index in faqs
    .accordion-item
      h2.accordion-header(id=`heading${index}`)
        button.accordion-button.collapsed(
          type='button'
          data-bs-toggle='collapse'
          data-bs-target=`#collapse${index}`
          aria-expanded='false'
          aria-controls=`collapse${index}`
        )= faq.q
      .accordion-collapse.collapse(
        id=`collapse${index}`
        aria-labelledby=`heading${index}`
        data-bs-parent='#faqAccordion'
      )
        .accordion-body= faq.a
```

### Image Handling
```pug
//- Responsive images with proper attributes
figure.mb-4
  img.img-fluid.rounded(
    src='assets/img/example.jpg'
    alt='Descriptive alt text for accessibility'
    title='Image title for additional context'
    loading='lazy'
  )
  figcaption.text-muted.mt-2.small Image caption explaining the content

//- Image with lightbox/modal trigger
.text-center.mb-4
  img.img-fluid.rounded.cursor-pointer(
    src='assets/img/thumbnail.jpg'
    alt='Click to enlarge'
    data-bs-toggle='modal'
    data-bs-target='#imageModal'
  )
```

## SEO Optimization Guidelines

### Meta Tags Structure
```pug
block pagehead
  title Article Title | Site Name
  meta(name='description', content='Compelling 150-160 character description')
  meta(name='keywords', content='primary, secondary, long-tail keywords')
  meta(name='author', content='Author Name')
  meta(name='robots', content='index, follow')

  // Open Graph
  meta(property='og:title', content='Article Title')
  meta(property='og:description', content='Social media description')
  meta(property='og:image', content='https://domain.com/image.jpg')
  meta(property='og:url', content='https://domain.com/articles/slug.html')
  meta(property='og:type', content='article')

  // Twitter Cards
  meta(name='twitter:card', content='summary_large_image')
  meta(name='twitter:title', content='Article Title')
  meta(name='twitter:description', content='Twitter description')
  meta(name='twitter:image', content='https://domain.com/image.jpg')

  // Canonical URL
  link(rel='canonical', href='https://domain.com/articles/slug.html')

  // Article-specific meta
  meta(property='article:published_time', content='2025-05-28T10:00:00Z')
  meta(property='article:modified_time', content='2025-05-28T15:30:00Z')
  meta(property='article:author', content='Author Name')
  meta(property='article:section', content='Technology')
  meta(property='article:tag', content='PUG, Templates, Web Development')
```

### Structured Content
```pug
//- Use semantic HTML elements
article#main-article
  header.article-header.mb-4
    h1.article-title Article Title
    .article-meta.text-muted.mb-3
      time(datetime='2025-05-28') May 28, 2025
      span.mx-2 •
      span.author by #{author}
      span.mx-2 •
      span.reading-time #{readingTime} min read

  .article-content
    section#introduction
      h2 Introduction
      p.lead Article introduction...

    section#main-content
      h2 Main Content
      // Article body

    section#conclusion
      h2 Conclusion
      p Conclusion content...
```

## Build Process Integration

### Development Workflow
1. Create/update entry in `articles.json`
2. Create corresponding `.pug` file in `src/pug/articles/`
3. Run build command: `node scripts/build-pug.js`
4. Preview with: `node simple-serve.js`
5. Test at: `http://localhost:8080/articles/article-name.html`

### File Naming Conventions
- PUG files: Use kebab-case matching the slug
- Image assets: Descriptive names in `assets/img/`
- Follow consistent naming between JSON slug and PUG filename

## Performance Considerations

### Optimization Tips
1. **Lazy load images**: Add `loading='lazy'` to img tags
2. **Minimize mixins**: Don't over-abstract simple components
3. **Efficient loops**: Use early returns in complex iterations
4. **Cache-friendly**: Use consistent class names and structure

### Common PUG Patterns to Avoid
```pug
//- Avoid deep nesting (hard to maintain)
.container
  .row
    .col
      .card
        .card-body
          .content
            .inner-content  // Too deep

//- Better approach with mixins
mixin contentCard(title, content)
  .card.mb-4
    .card-body
      h5.card-title= title
      .card-text= content

+contentCard('Title', 'Content')
```

## Error Handling and Debugging

### Common PUG Errors
1. **Indentation errors**: Use consistent 2-space indentation
2. **Missing commas in attributes**: Separate with commas or spaces
3. **Unclosed blocks**: Check indentation levels
4. **Variable scope issues**: Declare variables with `-` prefix

### Debugging Tips
```pug
//- Debug variables
- console.log('Debug variable:', variableName)

//- Conditional debugging
if process.env.NODE_ENV === 'development'
  pre= JSON.stringify(debugData, null, 2)

//- Comment out sections for testing
//- Temporarily disabled
  .problematic-section
    // Content here
```

## Article Quality Checklist

### Before Publishing
- [ ] **SEO Metadata**: Title, description, keywords properly set
- [ ] **Bootstrap Classes**: No inline styles, only Bootstrap utilities used
- [ ] **Responsive Design**: Test on mobile, tablet, desktop breakpoints
- [ ] **Accessibility**: Alt text for images, proper heading hierarchy, ARIA labels
- [ ] **Performance**: Images use `loading='lazy'`, minimal JavaScript
- [ ] **Icons**: Bootstrap Icons used consistently with proper spacing
- [ ] **Typography**: Proper use of `.lead`, heading levels, text utilities
- [ ] **Links**: All external links have `target='_blank'` and `rel='noopener'`
- [ ] **Code Blocks**: Syntax highlighting with PrismJS classes
- [ ] **Content Structure**: Clear sections with proper spacing utilities

### Bootstrap 5 Quick Reference

#### Essential Utility Classes
```pug
//- Spacing
.m-{0-5}          // Margin (all sides)
.mt-{0-5}         // Margin top
.mb-{0-5}         // Margin bottom
.p-{0-5}          // Padding (all sides)
.py-{0-5}         // Padding top/bottom
.px-{0-5}         // Padding left/right

//- Display & Flex
.d-flex           // Display flex
.d-none           // Hide element
.d-md-block       // Show on medium+ screens
.justify-content-center
.align-items-center
.flex-column
.flex-md-row

//- Colors
.text-primary     // Primary blue text
.text-muted       // Muted gray text
.bg-light         // Light background
.bg-primary       // Primary background

//- Typography
.fs-1 to .fs-6    // Font sizes
.fw-bold          // Bold font weight
.fw-light         // Light font weight
.lead             // Lead paragraph styling
.display-{1-6}    // Large display headings
```

#### Common Bootstrap Components
```pug
//- Cards
.card.shadow-sm
  .card-header.bg-primary.text-white
  .card-body
  .card-footer.text-muted

//- Buttons
.btn.btn-primary
.btn.btn-outline-secondary
.btn.btn-sm
.btn.btn-lg

//- Alerts
.alert.alert-info
.alert.alert-warning.alert-dismissible

//- Badges
.badge.bg-primary
.badge.bg-success
.badge.bg-warning

//- Navigation
.nav.nav-pills
.nav.nav-tabs
.breadcrumb
```

## Final Reminders

### NEVER DO:
1. Add `style` attributes to elements
2. Create custom CSS classes in individual articles
3. Use Font Awesome when Bootstrap Icons are available
4. Forget responsive breakpoints (always think mobile-first)
5. Skip accessibility attributes (alt, aria-label, etc.)

### ALWAYS DO:
1. Use Bootstrap 5 utility classes for all styling
2. Include proper meta tags for SEO and social sharing
3. Test responsiveness across all breakpoints
4. Use semantic HTML elements (article, section, header, aside)
5. Include proper image attributes (alt, loading, title)
6. Follow the established article structure pattern
7. Use consistent spacing with Bootstrap utilities

### Modern Layout Template Benefits
The `modern-layout.pug` template provides:
- ✅ Complete Bootstrap 5 framework
- ✅ Bootstrap Icons integration
- ✅ Optimized meta tags and structured data
- ✅ Performance optimizations (DNS prefetch, preconnect)
- ✅ Google Analytics integration
- ✅ Responsive design foundation
- ✅ Accessibility best practices
- ✅ SEO optimization

By following these guidelines, articles will be consistently styled, performant, accessible, and SEO-optimized while maintaining a modern, professional appearance across all devices.
````
