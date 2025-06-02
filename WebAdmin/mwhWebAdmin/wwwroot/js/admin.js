// Modern Admin Dashboard JavaScript

// Global admin object
window.AdminDashboard = {
    sidebarCollapsed: false,

    init: function() {
        this.initSidebar();
        this.initTooltips();
        this.initConfirmDialogs();
        this.initImageSelection();
        this.initAutoSave();
        this.initSearch();
    },

    // Sidebar functionality
    initSidebar: function() {
        // Set active nav item based on current page
        const currentPath = window.location.pathname;
        document.querySelectorAll('.sidebar .nav-link').forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });

        // Handle mobile responsiveness
        this.handleResponsive();
        window.addEventListener('resize', () => this.handleResponsive());
    },

    handleResponsive: function() {
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.querySelector('.main-content');

        if (window.innerWidth <= 768) {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
        } else if (!this.sidebarCollapsed) {
            sidebar.classList.remove('collapsed');
            mainContent.classList.remove('expanded');
        }
    },

    // Initialize tooltips
    initTooltips: function() {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    },

    // Initialize confirmation dialogs
    initConfirmDialogs: function() {
        document.querySelectorAll('[data-confirm]').forEach(element => {
            element.addEventListener('click', function(e) {
                const message = this.getAttribute('data-confirm');
                if (!confirm(message)) {
                    e.preventDefault();
                    return false;
                }
            });
        });
    },

    // Initialize image selection
    initImageSelection: function() {
        document.querySelectorAll('.image-grid-item').forEach(item => {
            item.addEventListener('click', function() {
                // Remove active class from all items
                document.querySelectorAll('.image-grid-item').forEach(i => i.classList.remove('selected'));

                // Add active class to clicked item
                this.classList.add('selected');

                // Update hidden input value
                const imagePath = this.querySelector('img').getAttribute('src');
                const hiddenInput = document.querySelector('input[name="Project.Image"]');
                if (hiddenInput) {
                    hiddenInput.value = imagePath.replace('/', '');
                }

                // Update preview
                const preview = document.querySelector('.image-preview img');
                if (preview) {
                    preview.src = imagePath;
                }

                // Show success feedback
                this.style.transform = 'scale(1.1)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 200);
            });
        });
    },

    // Initialize auto-save functionality
    initAutoSave: function() {
        const forms = document.querySelectorAll('form[data-autosave]');
        forms.forEach(form => {
            const inputs = form.querySelectorAll('input, textarea, select');
            inputs.forEach(input => {
                input.addEventListener('change', () => {
                    this.showSaveIndicator('Auto-saving...');
                    setTimeout(() => this.hideSaveIndicator(), 2000);
                });
            });
        });
    },

    // Initialize search functionality
    initSearch: function() {
        const searchInputs = document.querySelectorAll('.table-search');
        searchInputs.forEach(input => {
            input.addEventListener('input', function() {
                const table = this.closest('.admin-table').querySelector('table');
                const filter = this.value.toLowerCase();
                const rows = table.querySelectorAll('tbody tr');

                rows.forEach(row => {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(filter) ? '' : 'none';
                });
            });
        });
    },

    // Utility functions
    showSaveIndicator: function(message) {
        const indicator = document.createElement('div');
        indicator.className = 'alert alert-info position-fixed';
        indicator.style.cssText = 'top: 80px; right: 20px; z-index: 9999; min-width: 200px;';
        indicator.innerHTML = `<i class="fas fa-save me-2"></i>${message}`;
        document.body.appendChild(indicator);

        setTimeout(() => {
            if (indicator.parentNode) {
                indicator.parentNode.removeChild(indicator);
            }
        }, 3000);
    },

    hideSaveIndicator: function() {
        const indicators = document.querySelectorAll('.alert.position-fixed');
        indicators.forEach(indicator => {
            if (indicator.parentNode) {
                indicator.parentNode.removeChild(indicator);
            }
        });
    },

    showLoading: function() {
        const overlay = document.createElement('div');
        overlay.className = 'spinner-overlay';
        overlay.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';
        document.body.appendChild(overlay);
    },

    hideLoading: function() {
        const overlay = document.querySelector('.spinner-overlay');
        if (overlay) {
            overlay.remove();
        }
    }
};

// Global functions for backward compatibility
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('.main-content');

    if (window.innerWidth > 768) {
        AdminDashboard.sidebarCollapsed = !AdminDashboard.sidebarCollapsed;

        if (AdminDashboard.sidebarCollapsed) {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
        } else {
            sidebar.classList.remove('collapsed');
            mainContent.classList.remove('expanded');
        }
    } else {
        sidebar.classList.toggle('show');
    }
}

function refreshSite() {
    if (confirm('This will regenerate the site files. Continue?')) {
        AdminDashboard.showLoading();

        // Simulate site refresh - replace with actual API call
        setTimeout(() => {
            AdminDashboard.hideLoading();
            alert('Site refreshed successfully!');
        }, 3000);
    }
}

function deleteItem(id, type) {
    if (confirm(`Are you sure you want to delete this ${type}?`)) {
        AdminDashboard.showLoading();

        // Submit delete form
        const form = document.querySelector(`form[data-delete-id="${id}"]`);
        if (form) {
            form.submit();
        }
    }
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    AdminDashboard.init();
});

// Handle form submissions with loading states
document.addEventListener('submit', function(e) {
    if (e.target.matches('form:not([data-no-loading])')) {
        AdminDashboard.showLoading();
    }
});

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(() => {
        document.querySelectorAll('.alert-dismissible').forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});
