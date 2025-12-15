window.addEventListener("DOMContentLoaded", (event) => {
    console.log('[Typeahead] DOMContentLoaded event fired');
    console.log('[Typeahead] Current URL:', window.location.href);
    console.log('[Typeahead] User Agent:', navigator.userAgent);
    
    // Activate Bootstrap scrollspy on the main nav element
    const sideNav = document.body.querySelector("#sideNav");
    if (sideNav) {
        new bootstrap.ScrollSpy(document.body, {
            target: "#sideNav",
            rootMargin: "0px 0px -40%",
        });
    }

    // Collapse responsive navbar when toggler is visible
    const navbarToggler = document.body.querySelector(".navbar-toggler");
    const responsiveNavItems = [].slice.call(
        document.querySelectorAll("#navbarResponsive .nav-link"),
    );
    responsiveNavItems.map(function (responsiveNavItem) {
        responsiveNavItem.addEventListener("click", () => {
            if (window.getComputedStyle(navbarToggler).display !== "none") {
                navbarToggler.click();
            }
        });
    });

    // Initialize accordion if it exists
    const accordionArticlesEl = document.getElementById("accordionArticles");
    if (accordionArticlesEl) {
        var myAccordion = new bootstrap.Collapse(accordionArticlesEl, {
            toggle: false, // This prevents the default behavior of toggling an item when clicked
        });
    }

    // Initialize Prism if it exists
    if (typeof Prism !== "undefined") {
        Prism.highlightAll();
    }

    // Handle accordion buttons (toggle icons) if they exist
    const accordionButtons = document.querySelectorAll(".accordion-button");
    if (accordionButtons.length > 0) {
        accordionButtons.forEach((button) => {
            button.addEventListener("click", function () {
                const icon = this.querySelector("i");
                if (icon) {
                    icon.classList.toggle("fa-plus");
                    icon.classList.toggle("fa-minus");
                }
            });
        });
    }

    // Initialize header search functionality with Edge-specific fixes
    initializeHeaderSearch();

    // Initialize search form submission
    initializeSearchForm();

    // Initialize LinkedIn sharing functionality
    initializeLinkedInSharing();

    // Edge-specific polyfills and fixes
    if (
        navigator.userAgent.indexOf("Edge") > -1 ||
        navigator.userAgent.indexOf("Edg") > -1
    ) {
        // Add specific Edge compatibility fixes
        console.log("[Typeahead] Microsoft Edge detected, applying compatibility fixes");

        // Ensure articles cache is preloaded for Edge
        setTimeout(function () {
            console.log('[Typeahead] Edge preload timer fired');
            if (!window.articlesCache) {
                console.log('[Typeahead] Preloading articles cache for Edge...');
                fetch("/articles.json")
                    .then((response) => {
                        console.log('[Typeahead] Edge preload fetch response:', response.status);
                        return response.json();
                    })
                    .then((data) => {
                        window.articlesCache = data;
                        console.log("[Typeahead] Articles cache preloaded for Edge:", data.length, 'articles');
                    })
                    .catch((error) => {
                        console.error("[Typeahead] Error preloading articles for Edge:", error);
                        console.error('[Typeahead] Error details:', error.message, error.stack);
                    });
            } else {
                console.log('[Typeahead] Articles cache already loaded, skipping Edge preload');
            }
        }, 500);
    }
});

// Global search function for form submission - defined outside DOMContentLoaded
window.searchArticles = function (event) {
    console.log("searchArticles called with event:", event);
    if (event) event.preventDefault();

    const query = document.getElementById("headerSearchInput").value.trim();
    if (!query) {
        alert("Please enter a search term");
        return false;
    }

    performHeaderSearch();
    return false;
};

// Header search functionality
function initializeHeaderSearch() {
    console.log('[Typeahead] Initializing header search...');
    const headerSearchInput = document.getElementById("headerSearchInput");
    if (!headerSearchInput) {
        console.warn('[Typeahead] headerSearchInput element not found');
        return;
    }
    console.log('[Typeahead] headerSearchInput element found');

    // Handle Enter key for quick search
    headerSearchInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
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
        console.log('[Typeahead] Input event fired, query:', query);

        if (query.length >= 2) {
            console.log('[Typeahead] Query length >= 2, scheduling suggestions');
            timeoutId = setTimeout(() => {
                showHeaderSuggestions(query);
            }, 300);
        } else {
            console.log('[Typeahead] Query too short, hiding suggestions');
            hideHeaderSuggestions();
        }
    }

    // Add multiple event listeners for better browser support
    headerSearchInput.addEventListener("input", handleSearchInput);
    headerSearchInput.addEventListener("keyup", handleSearchInput);
    headerSearchInput.addEventListener("propertychange", handleSearchInput); // IE/Edge legacy support

    // Also handle paste events
    headerSearchInput.addEventListener("paste", function () {
        setTimeout(handleSearchInput.bind(this), 100);
    });

    // Hide suggestions when clicking outside
    document.addEventListener("click", function (e) {
        if (!e.target.closest(".header-search-container")) {
            hideHeaderSuggestions();
        }
    });

    // Focus and blur events for better UX
    headerSearchInput.addEventListener("focus", function () {
        const query = this.value.trim();
        if (query.length >= 2) {
            showHeaderSuggestions(query);
        }
    });

    headerSearchInput.addEventListener("blur", function () {
        // Delay hiding to allow for suggestion clicks
        setTimeout(hideHeaderSuggestions, 150);
    });
}

// Perform header search
function performHeaderSearch() {
    const query = document.getElementById("headerSearchInput").value.trim();
    console.log("performHeaderSearch called, query:", query);
    if (query) {
        console.log("Redirecting to search page with query:", query);
        window.location.href = `/search.html?q=${encodeURIComponent(query)}`;
    } else {
        console.log("No query provided");
        alert("Please enter a search term");
    }
}

// Show header search suggestions (lightweight version)
async function showHeaderSuggestions(query) {
    console.log('[Typeahead] showHeaderSuggestions called with query:', query);
    try {
        // Only load articles if not already loaded
        if (!window.articlesCache) {
            console.log('[Typeahead] Articles cache not found, fetching /articles.json...');
            const fetchStart = performance.now();
            const response = await fetch("/articles.json");
            const fetchEnd = performance.now();
            console.log(`[Typeahead] Fetch completed in ${(fetchEnd - fetchStart).toFixed(2)}ms`);
            console.log('[Typeahead] Response status:', response.status);
            console.log('[Typeahead] Response headers:', {
                contentType: response.headers.get('Content-Type'),
                cacheControl: response.headers.get('Cache-Control'),
                contentLength: response.headers.get('Content-Length')
            });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const parseStart = performance.now();
            window.articlesCache = await response.json();
            const parseEnd = performance.now();
            console.log(`[Typeahead] JSON parsed in ${(parseEnd - parseStart).toFixed(2)}ms`);
            console.log('[Typeahead] Articles cache loaded:', window.articlesCache.length, 'articles');
        } else {
            console.log('[Typeahead] Using cached articles:', window.articlesCache.length, 'articles');
        }

        const suggestions = getHeaderQuickMatches(query, 5);
        console.log('[Typeahead] Found', suggestions.length, 'matching suggestions');
        if (suggestions.length === 0) {
            console.log('[Typeahead] No suggestions found, exiting');
            return;
        }

        const searchContainer = document.querySelector(".search-box");
        if (!searchContainer) {
            console.warn('[Typeahead] .search-box container not found');
            return;
        }
        console.log('[Typeahead] Search container found, displaying suggestions');

        searchContainer.classList.add("header-search-container");
        searchContainer.style.position = "relative";

        let suggestionsHtml =
            '<div class="header-search-suggestions position-absolute bg-white border rounded shadow-sm mt-1 w-100" style="z-index: 1050; top: 100%; max-height: 300px; overflow-y: auto; border: 1px solid #dee2e6; box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); -webkit-box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075); -moz-box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);">';
        suggestions.forEach((article) => {
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
        suggestionsHtml += "</div>";

        // Remove existing suggestions
        const existing = searchContainer.querySelector(
            ".header-search-suggestions",
        );
        if (existing) existing.remove();

        searchContainer.insertAdjacentHTML("beforeend", suggestionsHtml);

        // Add keyboard navigation support
        addKeyboardNavigation();
        console.log('[Typeahead] Suggestions displayed successfully');
    } catch (error) {
        console.error('[Typeahead] Error loading search suggestions:', error);
        console.error('[Typeahead] Error type:', error.name);
        console.error('[Typeahead] Error message:', error.message);
        console.error('[Typeahead] Error stack:', error.stack);
        console.error('[Typeahead] Current URL:', window.location.href);
        console.error('[Typeahead] articlesCache state:', window.articlesCache ? 'loaded' : 'not loaded');
        // Fallback: still allow search to work without suggestions
    }
}

// Hide header search suggestions
function hideHeaderSuggestions() {
    const suggestions = document.querySelector(".header-search-suggestions");
    if (suggestions) suggestions.remove();
}

// Get quick matches for header suggestions
function getHeaderQuickMatches(query, limit) {
    console.log('[Typeahead] getHeaderQuickMatches called:', { query, limit });
    if (!window.articlesCache) {
        console.warn('[Typeahead] articlesCache not available in getHeaderQuickMatches');
        return [];
    }

    try {
        const lowercaseQuery = query.toLowerCase();
        console.log('[Typeahead] Filtering', window.articlesCache.length, 'articles with query:', lowercaseQuery);
        return window.articlesCache
            .filter((article) => {
                // More robust filtering with null checks
                const name = (article.name || "").toLowerCase();
                const keywords = (article.keywords || "").toLowerCase();
                const section = (article.Section || "").toLowerCase();
                const description = (article.description || "").toLowerCase();

                return (
                    name.includes(lowercaseQuery) ||
                    keywords.includes(lowercaseQuery) ||
                    section.includes(lowercaseQuery) ||
                    description.includes(lowercaseQuery)
                );
            })
            .slice(0, limit);
    } catch (error) {
        console.error('[Typeahead] Error filtering articles:', error);
        console.error('[Typeahead] Error details:', error.message);
        return [];
    }
}

// Highlight matching text for header
function highlightHeaderMatch(text, query) {
    if (!text || !query) return text || "";

    try {
        const regex = new RegExp(
            `(${query.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")})`,
            "gi",
        );
        return text.replace(
            regex,
            '<mark style="background-color: #fff3cd; padding: 0 2px;">$1</mark>',
        );
    } catch (error) {
        console.error("Error highlighting text:", error);
        return text;
    }
}

// Select header suggestion
function selectHeaderSuggestion(slug) {
    window.location.href = `/${slug}`;
}

// Add keyboard navigation support for suggestions
function addKeyboardNavigation() {
    const headerSearchInput = document.getElementById("headerSearchInput");
    if (!headerSearchInput) return;

    let currentSelection = -1;

    // Remove existing keydown listener if any
    headerSearchInput.removeEventListener("keydown", handleKeydownNavigation);
    headerSearchInput.addEventListener("keydown", handleKeydownNavigation);

    function handleKeydownNavigation(e) {
        const suggestions = document.querySelectorAll(
            ".header-search-suggestions .suggestion-item",
        );
        if (suggestions.length === 0) return;

        switch (e.key) {
            case "ArrowDown":
                e.preventDefault();
                currentSelection = Math.min(
                    currentSelection + 1,
                    suggestions.length - 1,
                );
                updateSelection(suggestions, currentSelection);
                break;
            case "ArrowUp":
                e.preventDefault();
                currentSelection = Math.max(currentSelection - 1, -1);
                updateSelection(suggestions, currentSelection);
                break;
            case "Enter":
                e.preventDefault();
                if (currentSelection >= 0 && suggestions[currentSelection]) {
                    suggestions[currentSelection].click();
                } else {
                    performHeaderSearch();
                }
                break;
            case "Escape":
                hideHeaderSuggestions();
                currentSelection = -1;
                break;
        }
    }

    function updateSelection(suggestions, index) {
        suggestions.forEach((item, i) => {
            if (i === index) {
                item.style.backgroundColor = "#e9ecef";
                item.scrollIntoView({ block: "nearest" });
            } else {
                item.style.backgroundColor = "";
            }
        });
    }
}

// Initialize search form submission handler
function initializeSearchForm() {
    // Find all forms with search functionality (handle both single and double quotes)
    const searchForms = document.querySelectorAll(
        'form[onsubmit*="searchArticles"], form[onsubmit*="searchArticles"]',
    );
    console.log("Found", searchForms.length, "search forms");

    // Alternative approach - find forms containing search boxes
    const searchBoxes = document.querySelectorAll(".search-box");
    console.log("Found", searchBoxes.length, "search boxes");

    searchBoxes.forEach((box) => {
        const form = box.closest("form");
        if (form) {
            console.log("Adding event listener to form containing search box");
            form.addEventListener("submit", function (event) {
                console.log(
                    "Search form submitted via search box event listener",
                );
                event.preventDefault();

                const query = document
                    .getElementById("headerSearchInput")
                    .value.trim();
                if (!query) {
                    alert("Please enter a search term");
                    return false;
                }

                performHeaderSearch();
                return false;
            });
        }
    });

    // Also add click handler to search buttons directly
    const searchButtons = document.querySelectorAll(
        'button[type="submit"][aria-label="Search"]',
    );
    console.log("Found", searchButtons.length, "search buttons");

    searchButtons.forEach((button) => {
        button.addEventListener("click", function (event) {
            console.log("Search button clicked directly");
            event.preventDefault();

            const query = document
                .getElementById("headerSearchInput")
                .value.trim();
            if (!query) {
                alert("Please enter a search term");
                return false;
            }

            performHeaderSearch();
            return false;
        });
    });
}

// LinkedIn Sharing Functions
function openLinkedInShare(url) {
    const width = 600;
    const height = 400;
    const left = (screen.width - width) / 2;
    const top = (screen.height - height) / 2;

    window.open(
        url,
        "linkedin-share",
        `width=${width},height=${height},left=${left},top=${top},scrollbars=yes,resizable=yes`,
    );

    // Track sharing event if analytics is available
    if (typeof gtag !== "undefined") {
        gtag("event", "share", {
            method: "LinkedIn",
            content_type: "article",
            item_id: window.location.pathname,
        });
    }

    return false;
}

function openTwitterShare(url) {
    const width = 600;
    const height = 400;
    const left = (screen.width - width) / 2;
    const top = (screen.height - height) / 2;

    window.open(
        url,
        "twitter-share",
        `width=${width},height=${height},left=${left},top=${top},scrollbars=yes,resizable=yes`,
    );

    // Track sharing event if analytics is available
    if (typeof gtag !== "undefined") {
        gtag("event", "share", {
            method: "Twitter",
            content_type: "article",
            item_id: window.location.pathname,
        });
    }

    return false;
}

function copyToClipboard(text) {
    if (navigator.clipboard && window.isSecureContext) {
        // Use the modern clipboard API
        navigator.clipboard
            .writeText(text)
            .then(() => {
                showCopyFeedback("Link copied to clipboard!");
            })
            .catch((err) => {
                console.error("Failed to copy: ", err);
                fallbackCopyToClipboard(text);
            });
    } else {
        // Fallback for older browsers or non-secure contexts
        fallbackCopyToClipboard(text);
    }

    // Track copy event if analytics is available
    if (typeof gtag !== "undefined") {
        gtag("event", "share", {
            method: "Copy Link",
            content_type: "article",
            item_id: window.location.pathname,
        });
    }
}

function fallbackCopyToClipboard(text) {
    const textArea = document.createElement("textarea");
    textArea.value = text;
    textArea.style.position = "fixed";
    textArea.style.left = "-999999px";
    textArea.style.top = "-999999px";
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        document.execCommand("copy");
        showCopyFeedback("Link copied to clipboard!");
    } catch (err) {
        console.error("Fallback copy failed: ", err);
        showCopyFeedback("Failed to copy link. Please copy manually.", "error");
    }

    document.body.removeChild(textArea);
}

function showCopyFeedback(message, type = "success") {
    // Create and show a temporary toast/alert
    const toast = document.createElement("div");
    toast.className = `alert alert-${type === "error" ? "danger" : "success"} position-fixed`;
    toast.style.cssText =
        "top: 20px; right: 20px; z-index: 9999; min-width: 200px;";
    toast.innerHTML = `
        <i class="fas fa-${type === "error" ? "exclamation-triangle" : "check"} me-2"></i>
        ${message}
    `;

    document.body.appendChild(toast);

    // Auto-remove after 3 seconds
    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 3000);
}

// Initialize LinkedIn sharing on page load
function initializeLinkedInSharing() {
    // Add event listeners to LinkedIn sharing buttons
    const linkedinButtons = document.querySelectorAll(
        'a[onclick*="openLinkedInShare"]',
    );
    linkedinButtons.forEach((button) => {
        // The onclick handler is already set in the HTML, so we don't need to add another
        // But we can add hover effects or other enhancements here if needed
    });

    // Initialize floating share button visibility on scroll (if present)
    const floatingShare = document.querySelector(".floating-linkedin-share");
    if (floatingShare) {
        let isVisible = false;

        function toggleFloatingShare() {
            const scrollPosition = window.scrollY;
            const shouldShow = scrollPosition > 500; // Show after scrolling 500px

            if (shouldShow && !isVisible) {
                floatingShare.style.opacity = "1";
                floatingShare.style.pointerEvents = "auto";
                isVisible = true;
            } else if (!shouldShow && isVisible) {
                floatingShare.style.opacity = "0";
                floatingShare.style.pointerEvents = "none";
                isVisible = false;
            }
        }

        // Initialize as hidden
        floatingShare.style.opacity = "0";
        floatingShare.style.pointerEvents = "none";
        floatingShare.style.transition = "opacity 0.3s ease-in-out";

        window.addEventListener("scroll", toggleFloatingShare);
    }
}
