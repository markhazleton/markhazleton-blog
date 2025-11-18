# Project Mechanics - Standalone Website

A modern, markdown-driven static website for Project Mechanics content.

## Overview

This is a standalone website that presents Project Mechanics methodology and best practices for project management. The site is built using markdown content files that are converted to HTML during the build process.

## Features

- **Markdown-driven content**: All content is written in markdown for easy editing
- **Modern responsive UI**: Clean, mobile-friendly design
- **Static site generation**: Fast, secure, and easy to deploy
- **Simple build process**: Node.js-based build system

## Project Structure

```
projectmechanics-site/
├── content/           # Markdown content files
├── templates/         # HTML templates
├── public/           # Generated static files
│   ├── css/         # Stylesheets
│   ├── js/          # JavaScript files
│   └── assets/      # Images and other assets
├── build.js         # Build script
└── package.json     # Node.js dependencies
```

## Getting Started

### Prerequisites

- Node.js (v16 or higher)
- npm

### Installation

```bash
npm install
```

### Development

Build the site and start a local server:

```bash
npm run dev
```

This will build the site and open it in your browser at http://localhost:8080

### Build for Production

```bash
npm run build
```

This generates the static HTML files in the `public/` directory.

### Clean Build

```bash
npm run clean
npm run build
```

## Content Management

### Adding New Content

1. Create a new markdown file in the `content/` directory
2. Add frontmatter with metadata (title, description, etc.)
3. Write your content in markdown
4. Run `npm run build` to generate the HTML

### Markdown Frontmatter Example

```markdown
---
title: Your Article Title
description: A brief description of the article
author: Mark Hazleton
date: 2024-01-15
slug: your-article-slug
---

# Your Content Here

Your article content in markdown...
```

## Deployment

The `public/` directory contains all static files ready for deployment to any static hosting service:

- GitHub Pages
- Netlify
- Vercel
- AWS S3 + CloudFront
- Azure Static Web Apps

## License

MIT License - See LICENSE file for details

## Author

Mark Hazleton - [https://markhazleton.com](https://markhazleton.com)
