# Building My First React Site Using Vite

## Introduction

Creating a React site can be a daunting task, especially for beginners. However, with the right tools and guidance, it becomes a manageable and rewarding experience. In this article, we will explore how to build and deploy a React site using Vite, a fast and modern build tool, and GitHub Pages for hosting. We will also address common issues such as CORS and provide solutions to overcome them.

## Why Choose Vite?

Vite is a build tool that aims to provide a faster and leaner development experience for modern web projects. It offers several advantages:

- **Fast Development**: Vite uses native ES modules in the browser, providing instant server start and lightning-fast HMR (Hot Module Replacement).
- **Optimized Build**: It uses Rollup for production builds, ensuring optimized and efficient output.
- **Rich Features**: Vite supports TypeScript, JSX, CSS, and more out of the box.

## Setting Up Your React Project

### Step 1: Install Vite

First, ensure you have Node.js installed. Then, run the following command to create a new Vite project:

```bash
npm create vite@latest my-react-app --template react
```

This command sets up a new React project using Vite.

### Step 2: Navigate and Start the Development Server

Navigate to your project directory and start the development server:

```bash
cd my-react-app
npm install
npm run dev
```

You should see your React app running at `http://localhost:3000`.

## Deploying to GitHub Pages

### Step 1: Configure Your Repository

Create a new repository on GitHub and push your local project to it:

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin <your-repo-url>
git push -u origin main
```

### Step 2: Deploy with GitHub Actions

Create a `.github/workflows/deploy.yml` file in your project with the following content:

```yaml
name: Deploy to GitHub Pages

on:
    push:
        branches:
            - main

jobs:
    build:
        runs-on: ubuntu-latest
        steps:
            - uses: actions/checkout@v2
            - uses: actions/setup-node@v2
              with:
                  node-version: "14"
            - run: npm install
            - run: npm run build
            - uses: peaceiris/actions-gh-pages@v3
              with:
                  github_token: ${{ secrets.GITHUB_TOKEN }}
                  publish_dir: ./dist
```

This workflow will automatically build and deploy your site to GitHub Pages whenever you push to the `main` branch.

## Troubleshooting Common Issues

### Handling CORS

Cross-Origin Resource Sharing (CORS) can be a common issue when deploying web applications. Here are some tips to handle CORS:

- **Use Proxy**: Configure a proxy in your `vite.config.js` to handle API requests during development.
- **Server Configuration**: Ensure your server is configured to allow CORS requests.

## Conclusion

Building a React site using Vite and deploying it to GitHub Pages is a straightforward process that offers speed and efficiency. By following the steps outlined above, you can quickly get your site up and running while addressing common issues like CORS.

## Additional Resources

- [Vite Documentation](https://vitejs.dev/)
- [React Documentation](https://reactjs.org/)
- [GitHub Pages Guide](https://pages.github.com/)

Start building your React site today and enjoy the seamless experience that Vite provides!
