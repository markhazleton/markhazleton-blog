
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
    }    // Initialize Prism if it exists
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


});
