// High Contrast Mode Compatibility Script
// This script helps handle the transition from -ms-high-contrast to forced-colors
// and can suppress deprecation warnings from external libraries

(function() {
  'use strict';

  // Check if we're in a high contrast mode
  function isHighContrastMode() {
    // Modern approach using forced-colors
    if (window.matchMedia && window.matchMedia('(forced-colors: active)').matches) {
      return true;
    }

    // Legacy fallback (still functional but deprecated)
    if (window.matchMedia && window.matchMedia('(-ms-high-contrast: active)').matches) {
      return true;
    }

    return false;
  }

  // Apply high contrast adjustments if needed
  function applyHighContrastStyles() {
    if (isHighContrastMode()) {
      document.documentElement.classList.add('high-contrast-mode');

      // Ensure critical elements are properly styled for accessibility
      const style = document.createElement('style');
      style.textContent = `
        .high-contrast-mode .card {
          border: 2px solid !important;
        }
        .high-contrast-mode .btn {
          border: 2px solid !important;
        }
        .high-contrast-mode .alert {
          border: 2px solid !important;
        }
        .high-contrast-mode .accordion-button:focus,
        .high-contrast-mode .btn:focus,
        .high-contrast-mode .form-control:focus {
          outline: 3px solid !important;
          outline-offset: 2px;
        }
      `;
      document.head.appendChild(style);
    }
  }

  // Monitor for changes in contrast mode
  function setupContrastModeListener() {
    // Modern approach
    if (window.matchMedia) {
      const modernQuery = window.matchMedia('(forced-colors: active)');
      if (modernQuery.addEventListener) {
        modernQuery.addEventListener('change', applyHighContrastStyles);
      } else if (modernQuery.addListener) {
        modernQuery.addListener(applyHighContrastStyles);
      }

      // Legacy fallback for compatibility
      const legacyQuery = window.matchMedia('(-ms-high-contrast: active)');
      if (legacyQuery.addEventListener) {
        legacyQuery.addEventListener('change', applyHighContrastStyles);
      } else if (legacyQuery.addListener) {
        legacyQuery.addListener(applyHighContrastStyles);
      }
    }
  }

  // Suppress deprecation warnings in console (optional)
  function suppressDeprecationWarnings() {
    const originalWarn = console.warn;
    console.warn = function(...args) {
      const message = args.join(' ');
      if (message.includes('-ms-high-contrast') && message.includes('deprecated')) {
        // Silently ignore this specific deprecation warning
        return;
      }
      originalWarn.apply(console, args);
    };
  }

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
      applyHighContrastStyles();
      setupContrastModeListener();
      // Uncomment the next line if you want to suppress the specific deprecation warning
      // suppressDeprecationWarnings();
    });
  } else {
    applyHighContrastStyles();
    setupContrastModeListener();
    // Uncomment the next line if you want to suppress the specific deprecation warning
    // suppressDeprecationWarnings();
  }
})();
