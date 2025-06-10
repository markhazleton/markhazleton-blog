
window.addEventListener("DOMContentLoaded", (event) => {
    // Activate Bootstrap scrollspy on the main nav element
    const sideNav = document.body.querySelector("#sideNav");
    if (sideNav) {
        new bootstrap.ScrollSpy(document.body, {
            target: "#sideNav",
            rootMargin: '0px 0px -40%',
        });
    }

    // Collapse responsive navbar when toggler is visible
    const navbarToggler = document.body.querySelector(".navbar-toggler");
    const responsiveNavItems = [].slice.call(
        document.querySelectorAll("#navbarResponsive .nav-link")
    );
    responsiveNavItems.map(function (responsiveNavItem) {
        responsiveNavItem.addEventListener("click", () => {
            if (window.getComputedStyle(navbarToggler).display !== "none") {
                navbarToggler.click();
            }
        });
    });

    // Initialize accordion if it exists
    const accordionArticlesEl = document.getElementById('accordionArticles');
    if (accordionArticlesEl) {
        var myAccordion = new bootstrap.Collapse(accordionArticlesEl, {
            toggle: false // This prevents the default behavior of toggling an item when clicked
        });
    }

    // Initialize Prism if it exists
    if (typeof Prism !== 'undefined') {
        Prism.highlightAll();
    }

    // Handle accordion buttons (toggle icons) if they exist
    const accordionButtons = document.querySelectorAll('.accordion-button');
    if (accordionButtons.length > 0) {
        accordionButtons.forEach(button => {
            button.addEventListener('click', function() {
                const icon = this.querySelector('i');
                if (icon) {
                    icon.classList.toggle('fa-plus');
                    icon.classList.toggle('fa-minus');
                }
            });
        });
    }    // Initialize header search functionality
    initializeHeaderSearch();

    // Initialize search form submission
    initializeSearchForm();
});

// Global search function for form submission - defined outside DOMContentLoaded
window.searchArticles = function(event) {
    console.log('searchArticles called with event:', event);
    if (event) event.preventDefault();

    const query = document.getElementById('headerSearchInput').value.trim();
    if (!query) {
        alert('Please enter a search term');
        return false;
    }

    performHeaderSearch();
    return false;
};

// Header search functionality
function initializeHeaderSearch() {
    const headerSearchInput = document.getElementById('headerSearchInput');
    if (!headerSearchInput) return;

    // Handle Enter key for quick search
    headerSearchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            performHeaderSearch();
        }
    });

    // Optional: Add typeahead suggestions for header search
    let timeoutId;
    headerSearchInput.addEventListener('input', function() {
        clearTimeout(timeoutId);
        const query = this.value.trim();

        if (query.length >= 2) {
            timeoutId = setTimeout(() => {
                showHeaderSuggestions(query);
            }, 300);
        } else {
            hideHeaderSuggestions();
        }
    });

    // Hide suggestions when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.header-search-container')) {
            hideHeaderSuggestions();
        }
    });
}

// Perform header search
function performHeaderSearch() {
    const query = document.getElementById('headerSearchInput').value.trim();
    console.log('performHeaderSearch called, query:', query);
    if (query) {
        console.log('Redirecting to search page with query:', query);
        window.location.href = `/search.html?q=${encodeURIComponent(query)}`;
    } else {
        console.log('No query provided');
        alert('Please enter a search term');
    }
}

// Show header search suggestions (lightweight version)
async function showHeaderSuggestions(query) {
    try {
        // Only load articles if not already loaded
        if (!window.articlesCache) {
            const response = await fetch('/articles.json');
            window.articlesCache = await response.json();
        }

        const suggestions = getHeaderQuickMatches(query, 5);
        if (suggestions.length === 0) return;

        const searchContainer = document.querySelector('.search-box');
        if (!searchContainer) return;

        searchContainer.classList.add('header-search-container');
        searchContainer.style.position = 'relative';

        let suggestionsHtml = '<div class="header-search-suggestions position-absolute bg-white border rounded shadow-sm mt-1 w-100" style="z-index: 1050; top: 100%;">';
        suggestions.forEach(article => {
            suggestionsHtml += `
                <div class="suggestion-item p-2 border-bottom text-dark" onclick="selectHeaderSuggestion('${article.slug}')" style="cursor: pointer;">
                    <div class="fw-bold small">${highlightHeaderMatch(article.name, query)}</div>
                    <small class="text-muted">${article.Section}</small>
                </div>
            `;
        });
        suggestionsHtml += '</div>';

        // Remove existing suggestions
        const existing = searchContainer.querySelector('.header-search-suggestions');
        if (existing) existing.remove();

        searchContainer.insertAdjacentHTML('beforeend', suggestionsHtml);
    } catch (error) {
        console.error('Error loading search suggestions:', error);
    }
}

// Hide header search suggestions
function hideHeaderSuggestions() {
    const suggestions = document.querySelector('.header-search-suggestions');
    if (suggestions) suggestions.remove();
}

// Get quick matches for header suggestions
function getHeaderQuickMatches(query, limit) {
    if (!window.articlesCache) return [];

    const lowercaseQuery = query.toLowerCase();
    return window.articlesCache
        .filter(article =>
            article.name.toLowerCase().includes(lowercaseQuery) ||
            article.keywords.toLowerCase().includes(lowercaseQuery) ||
            article.Section.toLowerCase().includes(lowercaseQuery)
        )
        .slice(0, limit);
}

// Highlight matching text for header
function highlightHeaderMatch(text, query) {
    const regex = new RegExp(`(${query})`, 'gi');
    return text.replace(regex, '<mark style="background-color: #fff3cd; padding: 0 2px;">$1</mark>');
}

// Select header suggestion
function selectHeaderSuggestion(slug) {
    window.location.href = `/${slug}`;
}

// Initialize search form submission handler
function initializeSearchForm() {
    // Find all forms with search functionality (handle both single and double quotes)
    const searchForms = document.querySelectorAll('form[onsubmit*="searchArticles"], form[onsubmit*="searchArticles"]');
    console.log('Found', searchForms.length, 'search forms');

    // Alternative approach - find forms containing search boxes
    const searchBoxes = document.querySelectorAll('.search-box');
    console.log('Found', searchBoxes.length, 'search boxes');

    searchBoxes.forEach(box => {
        const form = box.closest('form');
        if (form) {
            console.log('Adding event listener to form containing search box');
            form.addEventListener('submit', function(event) {
                console.log('Search form submitted via search box event listener');
                event.preventDefault();

                const query = document.getElementById('headerSearchInput').value.trim();
                if (!query) {
                    alert('Please enter a search term');
                    return false;
                }

                performHeaderSearch();
                return false;
            });
        }
    });

    // Also add click handler to search buttons directly
    const searchButtons = document.querySelectorAll('button[type="submit"][aria-label="Search"]');
    console.log('Found', searchButtons.length, 'search buttons');

    searchButtons.forEach(button => {
        button.addEventListener('click', function(event) {
            console.log('Search button clicked directly');
            event.preventDefault();

            const query = document.getElementById('headerSearchInput').value.trim();
            if (!query) {
                alert('Please enter a search term');
                return false;
            }

            performHeaderSearch();
            return false;
        });
    });
}
