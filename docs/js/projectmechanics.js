// Project Mechanics specific JavaScript
document.addEventListener("DOMContentLoaded", function () {
    // Ensure Bootstrap is available
    if (typeof bootstrap === "undefined") {
        console.error("Bootstrap is not loaded");
        return;
    }

    // Initialize Bootstrap dropdowns
    var dropdownElementList = [].slice.call(
        document.querySelectorAll(".dropdown-toggle"),
    );
    var dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });

    // Initialize Bootstrap tooltips if any
    var tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]'),
    );
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener("click", function (e) {
            const target = document.querySelector(this.getAttribute("href"));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            }
        });
    });

    // Collapse responsive navbar when nav-link is clicked
    const navbarToggler = document.querySelector(".navbar-toggler");
    const navbarCollapse = document.querySelector("#navbarNav");

    if (navbarToggler && navbarCollapse) {
        const responsiveNavItems = document.querySelectorAll(
            "#navbarNav .nav-link",
        );
        responsiveNavItems.forEach(function (responsiveNavItem) {
            responsiveNavItem.addEventListener("click", () => {
                if (window.getComputedStyle(navbarToggler).display !== "none") {
                    const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
                        toggle: false,
                    });
                    bsCollapse.hide();
                }
            });
        });
    }

    // Handle accordion functionality if present
    const accordionElement = document.getElementById("accordionArticles");
    if (accordionElement) {
        // Initialize accordion
        const accordion = new bootstrap.Collapse(accordionElement, {
            toggle: false,
        });

        // Handle accordion button icons
        document.querySelectorAll(".accordion-button").forEach((button) => {
            button.addEventListener("click", function () {
                const icon = this.querySelector("i");
                if (icon) {
                    // Toggle between plus and minus icons
                    setTimeout(() => {
                        const isCollapsed =
                            this.classList.contains("collapsed");
                        icon.classList.toggle("fa-plus", isCollapsed);
                        icon.classList.toggle("fa-minus", !isCollapsed);
                    }, 10);
                }
            });
        });
    }

    // Add loading states to external links
    document.querySelectorAll('a[target="_blank"]').forEach((link) => {
        link.addEventListener("click", function () {
            this.style.opacity = "0.7";
            setTimeout(() => {
                this.style.opacity = "1";
            }, 1000);
        });
    });

    console.log("Project Mechanics JavaScript initialized successfully");
});
