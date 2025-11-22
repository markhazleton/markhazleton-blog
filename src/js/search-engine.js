// Search Engine for Mark Hazleton Blog
// Comprehensive client-side search functionality

class ArticleSearchEngine {
    constructor() {
        this.articles = [];
        this.searchIndex = new Map();
        this.isLoaded = false;
        this.currentResults = [];
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.currentQuery = "";
        this.currentCategory = "";
    }

    // Initialize the search engine
    async init() {
        try {
            await this.loadArticles();
            this.buildSearchIndex();
            this.setupEventListeners();
            this.handleUrlParameters();
            this.isLoaded = true;
            console.log(
                "Search engine initialized with",
                this.articles.length,
                "articles",
            );
        } catch (error) {
            console.error("Failed to initialize search engine:", error);
            this.showError("Failed to load articles. Please try again.");
        }
    }

    // Load articles from JSON
    async loadArticles() {
        const response = await fetch("/search-index.json");
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        this.articles = await response.json();
    }

    // Build search index for faster searching
    buildSearchIndex() {
        this.articles.forEach((article, index) => {
            const searchableText = [
                article.name,
                article.description,
                article.keywords,
                article.Section,
                article.slug,
            ]
                .join(" ")
                .toLowerCase();

            // Create word index
            const words = searchableText.split(/\s+/);
            words.forEach((word) => {
                word = word.replace(/[^\w]/g, "");
                if (word.length > 2) {
                    if (!this.searchIndex.has(word)) {
                        this.searchIndex.set(word, new Set());
                    }
                    this.searchIndex.get(word).add(index);
                }
            });
        });
    }

    // Setup event listeners
    setupEventListeners() {
        const searchForm = document.getElementById("searchForm");
        const searchInput = document.getElementById("searchInput");
        const categoryFilter = document.getElementById("categoryFilter");

        if (searchForm) {
            searchForm.addEventListener("submit", (e) => this.handleSearch(e));
        }

        if (searchInput) {
            // Real-time search suggestions
            let timeoutId;
            searchInput.addEventListener("input", () => {
                clearTimeout(timeoutId);
                timeoutId = setTimeout(() => this.showSuggestions(), 300);
            });

            // Handle Enter key
            searchInput.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    e.preventDefault();
                    this.performSearch();
                }
            });
        }

        if (categoryFilter) {
            categoryFilter.addEventListener("change", () => {
                if (this.currentQuery) {
                    this.performSearch();
                }
            });
        }
    }

    // Handle URL parameters for direct search links
    handleUrlParameters() {
        const urlParams = new URLSearchParams(window.location.search);
        const query = urlParams.get("q");
        const category = urlParams.get("category");

        if (query) {
            const searchInput = document.getElementById("searchInput");
            if (searchInput) {
                searchInput.value = query;
            }
            this.currentQuery = query;
        }

        if (category) {
            const categoryFilter = document.getElementById("categoryFilter");
            if (categoryFilter) {
                categoryFilter.value = category;
            }
            this.currentCategory = category;
        }

        // Perform search if we have parameters
        if (query || category) {
            setTimeout(() => this.performSearch(), 100);
        }
    }

    // Handle search form submission
    handleSearch(event) {
        if (event) {
            event.preventDefault();
        }
        this.performSearch();
        return false;
    }

    // Perform the actual search
    performSearch() {
        const startTime = performance.now();

        const searchInput = document.getElementById("searchInput");
        const categoryFilter = document.getElementById("categoryFilter");

        this.currentQuery = searchInput ? searchInput.value.trim() : "";
        this.currentCategory = categoryFilter ? categoryFilter.value : "";
        this.currentPage = 1;

        // Update URL without reload
        this.updateUrl();

        // Get search results
        this.currentResults = this.searchArticles(
            this.currentQuery,
            this.currentCategory,
        );

        // Display results
        this.displayResults();

        const endTime = performance.now();
        this.updateSearchStats(this.currentResults.length, endTime - startTime);

        // Hide suggestions
        this.hideSuggestions();
    }

    // Core search algorithm
    searchArticles(query, category = "") {
        let results = [...this.articles];

        // Filter by category first
        if (category) {
            results = results.filter((article) => article.Section === category);
        }

        // If no query, return category-filtered results
        if (!query) {
            return results.sort(
                (a, b) => new Date(b.publishedDate) - new Date(a.publishedDate),
            );
        }

        // Search scoring
        const queryTerms = query
            .toLowerCase()
            .split(/\s+/)
            .filter((term) => term.length > 0);
        const scoredResults = [];

        results.forEach((article) => {
            let score = 0;
            const searchableText = [
                article.name,
                article.description,
                article.keywords,
                article.Section,
            ]
                .join(" ")
                .toLowerCase();

            // Calculate relevance score
            queryTerms.forEach((term) => {
                // Exact title match (highest score)
                if (article.name.toLowerCase().includes(term)) {
                    score += 10;
                }

                // Description match
                if (article.description.toLowerCase().includes(term)) {
                    score += 5;
                }

                // Keywords match
                if (article.keywords.toLowerCase().includes(term)) {
                    score += 7;
                }

                // Section match
                if (article.Section.toLowerCase().includes(term)) {
                    score += 3;
                }

                // Slug match
                if (article.slug.toLowerCase().includes(term)) {
                    score += 4;
                }
            });

            // Add article to results if it has any relevance
            if (score > 0) {
                scoredResults.push({ ...article, searchScore: score });
            }
        });

        // Sort by score (descending) then by date (descending)
        return scoredResults.sort((a, b) => {
            if (b.searchScore !== a.searchScore) {
                return b.searchScore - a.searchScore;
            }
            return new Date(b.publishedDate) - new Date(a.publishedDate);
        });
    }

    // Display search results
    displayResults() {
        const resultsContainer = document.getElementById("searchResults");
        const loadingElement = document.getElementById("loadingResults");
        const noResultsElement = document.getElementById("noResults");

        if (!resultsContainer) return;

        // Hide loading
        if (loadingElement) loadingElement.classList.add("d-none");

        // Clear previous results
        resultsContainer.innerHTML = "";

        if (this.currentResults.length === 0) {
            if (noResultsElement) {
                noResultsElement.classList.remove("d-none");
            }
            this.hidePagination();
            return;
        }

        // Hide no results message
        if (noResultsElement) {
            noResultsElement.classList.add("d-none");
        }

        // Calculate pagination
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const pageResults = this.currentResults.slice(startIndex, endIndex);

        // Display results
        pageResults.forEach((article) => {
            const resultHtml = this.createResultCard(article);
            resultsContainer.insertAdjacentHTML("beforeend", resultHtml);
        });

        // Update pagination
        this.updatePagination();
    }

    // Create individual result card
    createResultCard(article) {
        const highlightedName = this.highlightSearchTerms(
            article.name,
            this.currentQuery,
        );
        const highlightedDescription = this.highlightSearchTerms(
            article.description,
            this.currentQuery,
        );
        const publishedDate = new Date(
            article.publishedDate,
        ).toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
            day: "numeric",
        });

        return `
            <div class="card mb-4 search-result-card">
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-8">
                            <h5 class="card-title">
                                <a href="/${article.slug}" class="text-decoration-none">
                                    ${highlightedName}
                                </a>
                            </h5>
                            <p class="card-text text-muted mb-2">${highlightedDescription}</p>
                            <div class="d-flex flex-wrap align-items-center">
                                <span class="badge bg-primary me-2 mb-1">${article.Section}</span>
                                <small class="text-muted me-3 mb-1">
                                    <i class="fas fa-calendar-alt me-1"></i>
                                    ${publishedDate}
                                </small>
                                ${article.searchScore ? `<small class="text-success mb-1">Relevance: ${article.searchScore}</small>` : ""}
                            </div>
                        </div>
                        <div class="col-md-4 text-md-end">
                            <a href="/${article.slug}" class="btn btn-outline-primary btn-sm">
                                Read Article
                                <i class="fas fa-arrow-right ms-1"></i>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Highlight search terms in text
    highlightSearchTerms(text, query) {
        if (!query) return text;

        const terms = query.split(/\s+/).filter((term) => term.length > 0);
        let highlightedText = text;

        terms.forEach((term) => {
            const regex = new RegExp(`(${this.escapeRegex(term)})`, "gi");
            highlightedText = highlightedText.replace(
                regex,
                '<mark class="search-highlight">$1</mark>',
            );
        });

        return highlightedText;
    }

    // Escape regex special characters
    escapeRegex(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    }

    // Show search suggestions (typeahead)
    showSuggestions() {
        const searchInput = document.getElementById("searchInput");
        if (!searchInput) return;

        const query = searchInput.value.trim();
        if (query.length < 2) {
            this.hideSuggestions();
            return;
        }

        const suggestions = this.getSuggestions(query, 5);
        if (suggestions.length === 0) return;

        let suggestionsHtml =
            '<div id="searchSuggestions" class="position-absolute bg-white border rounded shadow-sm mt-1 w-100" style="z-index: 1050;">';
        suggestions.forEach((suggestion) => {
            suggestionsHtml += `
                <div class="suggestion-item p-3 border-bottom" onclick="searchEngine.selectSuggestion('${suggestion.text}')" style="cursor: pointer;">
                    <div class="fw-bold">${this.highlightSearchTerms(suggestion.text, query)}</div>
                    <small class="text-muted">${suggestion.type} • ${suggestion.count} articles</small>
                </div>
            `;
        });
        suggestionsHtml += "</div>";

        // Remove existing suggestions
        this.hideSuggestions();

        // Add to search container
        const searchContainer =
            searchInput.closest(".input-group").parentElement;
        if (searchContainer) {
            searchContainer.style.position = "relative";
            searchContainer.insertAdjacentHTML("beforeend", suggestionsHtml);
        }
    }

    // Get search suggestions
    getSuggestions(query, limit) {
        const suggestions = [];
        const added = new Set();
        const lowercaseQuery = query.toLowerCase();

        // Title suggestions
        this.articles.forEach((article) => {
            if (
                article.name.toLowerCase().includes(lowercaseQuery) &&
                !added.has(article.name)
            ) {
                suggestions.push({
                    text: article.name,
                    type: "Article",
                    count: 1,
                });
                added.add(article.name);
            }
        });

        // Category suggestions
        const categories = [...new Set(this.articles.map((a) => a.Section))];
        categories.forEach((category) => {
            if (
                category.toLowerCase().includes(lowercaseQuery) &&
                !added.has(category)
            ) {
                const count = this.articles.filter(
                    (a) => a.Section === category,
                ).length;
                suggestions.push({
                    text: category,
                    type: "Category",
                    count: count,
                });
                added.add(category);
            }
        });

        return suggestions.slice(0, limit);
    }

    // Select a suggestion
    selectSuggestion(text) {
        const searchInput = document.getElementById("searchInput");
        if (searchInput) {
            searchInput.value = text;
            this.hideSuggestions();
            this.performSearch();
        }
    }

    // Hide suggestions
    hideSuggestions() {
        const suggestions = document.getElementById("searchSuggestions");
        if (suggestions) {
            suggestions.remove();
        }
    }

    // Update search statistics
    updateSearchStats(resultCount, searchTime) {
        const statsElement = document.getElementById("searchStats");
        const statsText = document.getElementById("statsText");
        const searchTimeElement = document.getElementById("searchTime");

        if (statsElement && statsText && searchTimeElement) {
            statsElement.classList.remove("d-none");

            let statsMessage = `Found ${resultCount} article${resultCount !== 1 ? "s" : ""}`;
            if (this.currentQuery) {
                statsMessage += ` for "${this.currentQuery}"`;
            }
            if (this.currentCategory) {
                statsMessage += ` in ${this.currentCategory}`;
            }

            statsText.textContent = statsMessage;
            searchTimeElement.textContent = `Search completed in ${searchTime.toFixed(2)}ms`;
        }
    }

    // Update pagination
    updatePagination() {
        const paginationContainer = document.getElementById("searchPagination");
        if (!paginationContainer) return;

        const totalPages = Math.ceil(
            this.currentResults.length / this.itemsPerPage,
        );

        if (totalPages <= 1) {
            paginationContainer.classList.add("d-none");
            return;
        }

        paginationContainer.classList.remove("d-none");

        let paginationHtml =
            '<nav aria-label="Search results pagination"><ul class="pagination justify-content-center">';

        // Previous button
        paginationHtml += `
            <li class="page-item ${this.currentPage === 1 ? "disabled" : ""}">
                <a class="page-link" href="#" onclick="searchEngine.goToPage(${this.currentPage - 1}); return false;" aria-label="Previous">
                    <span aria-hidden="true">&laquo;</span>
                </a>
            </li>
        `;

        // Page numbers
        const startPage = Math.max(1, this.currentPage - 2);
        const endPage = Math.min(totalPages, this.currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === this.currentPage ? "active" : ""}">
                    <a class="page-link" href="#" onclick="searchEngine.goToPage(${i}); return false;">${i}</a>
                </li>
            `;
        }

        // Next button
        paginationHtml += `
            <li class="page-item ${this.currentPage === totalPages ? "disabled" : ""}">
                <a class="page-link" href="#" onclick="searchEngine.goToPage(${this.currentPage + 1}); return false;" aria-label="Next">
                    <span aria-hidden="true">&raquo;</span>
                </a>
            </li>
        `;

        paginationHtml += "</ul></nav>";
        paginationContainer.innerHTML = paginationHtml;
    }

    // Go to specific page
    goToPage(page) {
        const totalPages = Math.ceil(
            this.currentResults.length / this.itemsPerPage,
        );
        if (page < 1 || page > totalPages) return;

        this.currentPage = page;
        this.displayResults();

        // Scroll to top of results
        const resultsContainer = document.getElementById("searchResults");
        if (resultsContainer) {
            resultsContainer.scrollIntoView({
                behavior: "smooth",
                block: "start",
            });
        }
    }

    // Hide pagination
    hidePagination() {
        const paginationContainer = document.getElementById("searchPagination");
        if (paginationContainer) {
            paginationContainer.classList.add("d-none");
        }
    }

    // Update URL with search parameters
    updateUrl() {
        const params = new URLSearchParams();

        if (this.currentQuery) {
            params.set("q", this.currentQuery);
        }

        if (this.currentCategory) {
            params.set("category", this.currentCategory);
        }

        const newUrl = `${window.location.pathname}${params.toString() ? "?" + params.toString() : ""}`;
        window.history.replaceState({}, "", newUrl);
    }

    // Show error message
    showError(message) {
        const container = document.querySelector(".container");
        if (container) {
            const errorHtml = `
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
            `;
            container.insertAdjacentHTML("afterbegin", errorHtml);
        }
    }
}

// Global search engine instance
let searchEngine;

// Global functions for search form and page interactions
window.performSearch = function (event) {
    if (event) event.preventDefault();
    if (searchEngine) {
        searchEngine.performSearch();
    }
    return false;
};

// Initialize search engine when DOM is loaded
document.addEventListener("DOMContentLoaded", async function () {
    // Only initialize if we're on the search page
    if (document.getElementById("searchForm")) {
        searchEngine = new ArticleSearchEngine();
        await searchEngine.init();
    }
});

// Handle clicks outside suggestions to hide them
document.addEventListener("click", function (e) {
    if (searchEngine && !e.target.closest(".input-group")) {
        searchEngine.hideSuggestions();
    }
});
