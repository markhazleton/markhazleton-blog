// Diagnostic script to check Bootstrap and JavaScript functionality
console.log("=== Project Mechanics Diagnostic ===");

// Check if Bootstrap is loaded
if (typeof bootstrap !== "undefined") {
    console.log("✅ Bootstrap is loaded successfully");
    console.log(
        "Bootstrap version:",
        bootstrap.Tooltip.VERSION || "Version not available",
    );
} else {
    console.log("❌ Bootstrap is NOT loaded");
}

// Check if jQuery is loaded (some legacy scripts might need it)
if (typeof $ !== "undefined") {
    console.log("✅ jQuery is loaded");
} else {
    console.log("ℹ️ jQuery is not loaded (this is okay for Bootstrap 5)");
}

// Check dropdowns
document.addEventListener("DOMContentLoaded", function () {
    const dropdowns = document.querySelectorAll(".dropdown-toggle");
    console.log(`Found ${dropdowns.length} dropdown toggles`);

    dropdowns.forEach((dropdown, index) => {
        try {
            const bsDropdown = new bootstrap.Dropdown(dropdown);
            console.log(`✅ Dropdown ${index + 1} initialized successfully`);
        } catch (error) {
            console.log(
                `❌ Dropdown ${index + 1} failed to initialize:`,
                error,
            );
        }
    });

    // Check navbar functionality
    const navbarToggler = document.querySelector(".navbar-toggler");
    const navbarCollapse = document.querySelector("#navbarNav");

    if (navbarToggler && navbarCollapse) {
        console.log("✅ Navbar elements found");
        try {
            const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
                toggle: false,
            });
            console.log("✅ Navbar collapse initialized");
        } catch (error) {
            console.log("❌ Navbar collapse failed:", error);
        }
    } else {
        console.log("❌ Navbar elements not found");
    }

    // Check tooltips
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    console.log(`Found ${tooltips.length} tooltip elements`);

    // Test a simple Bootstrap component
    console.log("=== Bootstrap Component Test ===");
    try {
        const testModal = document.createElement("div");
        testModal.className = "modal";
        testModal.style.display = "none";
        document.body.appendChild(testModal);

        const modal = new bootstrap.Modal(testModal);
        console.log("✅ Bootstrap Modal component works");

        document.body.removeChild(testModal);
    } catch (error) {
        console.log("❌ Bootstrap Modal component failed:", error);
    }

    console.log("=== Diagnostic Complete ===");
});

// Add this to any page to run diagnostics
window.runDiagnostics = function () {
    console.log("Running manual diagnostics...");

    // Test dropdown manually
    const firstDropdown = document.querySelector(".dropdown-toggle");
    if (firstDropdown) {
        try {
            const dropdown =
                bootstrap.Dropdown.getOrCreateInstance(firstDropdown);
            dropdown.toggle();
            setTimeout(() => dropdown.toggle(), 2000);
            console.log("✅ Manual dropdown test successful");
        } catch (error) {
            console.log("❌ Manual dropdown test failed:", error);
        }
    }
};
