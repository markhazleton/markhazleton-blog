# Content Directory

This directory contains all the markdown content files for the Project Mechanics website.

## File Naming Convention

Content files should follow this naming pattern:
- `index.md` - Homepage content
- `[topic].md` - Individual topic pages

## Frontmatter

Each markdown file should include frontmatter with the following fields:

```yaml
---
title: Page Title
description: Brief description for SEO and social sharing
author: Mark Hazleton
slug: page-url-slug
---
```

## Example

```markdown
---
title: Project Life Cycle
description: Understanding the stages of project execution from inception to completion
author: Mark Hazleton
slug: project-life-cycle
---

# Project Life Cycle

Your content here...
```

## Adding New Content

1. Create a new `.md` file in this directory
2. Add appropriate frontmatter
3. Write your content in markdown
4. Run `npm run build` from the project root
5. The HTML will be generated in `public/` directory
