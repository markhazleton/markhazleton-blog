
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
    }

    // Initialize header search functionality with Edge-specific fixes
    initializeHeaderSearch();

    // Initialize search form submission
    initializeSearchForm();

    // Edge-specific polyfills and fixes
    if (navigator.userAgent.indexOf('Edge') > -1 || navigator.userAgent.indexOf('Edg') > -1) {
        // Add specific Edge compatibility fixes
        console.log('Microsoft Edge detected, applying compatibility fixes');

        // Ensure articles cache is preloaded for Edge
        setTimeout(function() {
            if (!window.articlesCache) {
                fetch('/articles.json')
                    .then(response => response.json())
                    .then(data => {
                        window.articlesCache = data;
                        console.log('Articles cache preloaded for Edge');
                    })
                    .catch(error => console.error('Error preloading articles:', error));
            }
        }, 500);
    }
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

    // Enhanced typeahead with better browser compatibility
    let timeoutId;

    // Use both 'input' and 'keyup' events for better Edge compatibility
    function handleSearchInput() {
        clearTimeout(timeoutId);
        const query = this.value.trim();

        if (query.length >= 2) {
            timeoutId = setTimeout(() => {
                showHeaderSuggestions(query);
            }, 300);
        } else {
            hideHeaderSuggestions();
        }
    }

    // Add multiple event listeners for better browser support
    headerSearchInput.addEventListener('input', handleSearchInput);
    headerSearchInput.addEventListener('keyup', handleSearchInput);
    headerSearchInput.addEventListener('propertychange', handleSearchInput); // IE/Edge legacy support

    // Also handle paste events
    headerSearchInput.addEventListener('paste', function() {
        setTimeout(handleSearchInput.bind(this), 100);
    });

    // Hide suggestions when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.header-search-container')) {
            hideHeaderSuggestions();
        }
    });

    // Focus and blur events for better UX
    headerSearchInput.addEventListener('focus', function() {
        const query = this.value.trim();
        if (query.length >= 2) {
            showHeaderSuggestions(query);
        }
    });

    headerSearchInput.addEventListener('blur', function() {
        // Delay hiding to allow for suggestion clicks
        setTimeout(hideHeaderSuggestions, 150);
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
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            window.articlesCache = await response.json();
        }

        const suggestions = getHeaderQuickMatches(query, 5);
        if (suggestions.length === 0) return;

        const searchContainer = document.querySelector('.search-box');
        if (!searchContainer) return;

        searchContainer.classList.add('header-search-container');
        searchContainer.style.position = 'relative';

        let suggestionsHtml = '<div class="header-search-suggestions position-absolute bg-white border rounded shadow-sm mt-1 w-100" style="z-index: 1050; top: 100%; max-height: 300px; overflow-y: auto; border: 1px solid #dee2e6; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); -webkit-box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); -moz-box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);">';
        suggestions.forEach(article => {
            suggestionsHtml += `
                <div class="suggestion-item p-2 border-bottom text-dark"
                     onclick="selectHeaderSuggestion('${article.slug}')"
                     onmousedown="event.preventDefault();"
                     style="cursor: pointer; user-select: none; -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; transition: background-color 0.2s ease;">
                    <div class="fw-bold small" style="font-weight: 600; font-size: 0.875rem;">${highlightHeaderMatch(article.name, query)}</div>
                    <small class="text-muted" style="color: #6c757d; font-size: 0.8rem;">${article.Section}</small>
                </div>
            `;
        });
        suggestionsHtml += '</div>';

        // Remove existing suggestions
        const existing = searchContainer.querySelector('.header-search-suggestions');
        if (existing) existing.remove();

        searchContainer.insertAdjacentHTML('beforeend', suggestionsHtml);

        // Add keyboard navigation support
        addKeyboardNavigation();

    } catch (error) {
        console.error('Error loading search suggestions:', error);
        // Fallback: still allow search to work without suggestions
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

    try {
        const lowercaseQuery = query.toLowerCase();
        return window.articlesCache
            .filter(article => {
                // More robust filtering with null checks
                const name = (article.name || '').toLowerCase();
                const keywords = (article.keywords || '').toLowerCase();
                const section = (article.Section || '').toLowerCase();
                const description = (article.description || '').toLowerCase();

                return name.includes(lowercaseQuery) ||
                       keywords.includes(lowercaseQuery) ||
                       section.includes(lowercaseQuery) ||
                       description.includes(lowercaseQuery);
            })
            .slice(0, limit);
    } catch (error) {
        console.error('Error filtering articles:', error);
        return [];
    }
}

// Highlight matching text for header
function highlightHeaderMatch(text, query) {
    if (!text || !query) return text || '';

    try {
        const regex = new RegExp(`(${query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
        return text.replace(regex, '<mark style="background-color: #fff3cd; padding: 0 2px;">$1</mark>');
    } catch (error) {
        console.error('Error highlighting text:', error);
        return text;
    }
}

// Select header suggestion
function selectHeaderSuggestion(slug) {
    window.location.href = `/${slug}`;
}

// Add keyboard navigation support for suggestions
function addKeyboardNavigation() {
    const headerSearchInput = document.getElementById('headerSearchInput');
    if (!headerSearchInput) return;

    let currentSelection = -1;

    // Remove existing keydown listener if any
    headerSearchInput.removeEventListener('keydown', handleKeydownNavigation);
    headerSearchInput.addEventListener('keydown', handleKeydownNavigation);

    function handleKeydownNavigation(e) {
        const suggestions = document.querySelectorAll('.header-search-suggestions .suggestion-item');
        if (suggestions.length === 0) return;

        switch(e.key) {
            case 'ArrowDown':
                e.preventDefault();
                currentSelection = Math.min(currentSelection + 1, suggestions.length - 1);
                updateSelection(suggestions, currentSelection);
                break;
            case 'ArrowUp':
                e.preventDefault();
                currentSelection = Math.max(currentSelection - 1, -1);
                updateSelection(suggestions, currentSelection);
                break;
            case 'Enter':
                e.preventDefault();
                if (currentSelection >= 0 && suggestions[currentSelection]) {
                    suggestions[currentSelection].click();
                } else {
                    performHeaderSearch();
                }
                break;
            case 'Escape':
                hideHeaderSuggestions();
                currentSelection = -1;
                break;
        }
    }

    function updateSelection(suggestions, index) {
        suggestions.forEach((item, i) => {
            if (i === index) {
                item.style.backgroundColor = '#e9ecef';
                item.scrollIntoView({ block: 'nearest' });
            } else {
                item.style.backgroundColor = '';
            }
        });
    }
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
