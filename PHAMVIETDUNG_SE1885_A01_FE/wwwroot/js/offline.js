// Offline Mode Detection and Management
(function () {
    'use strict';

    const OfflineManager = {
        isOnline: navigator.onLine,
        cachedData: {},

        init: function () {
            this.bindEvents();
            this.updateUI();
            this.loadCachedData();
        },

        bindEvents: function () {
            window.addEventListener('online', () => this.handleOnline());
            window.addEventListener('offline', () => this.handleOffline());
        },

        handleOnline: function () {
            this.isOnline = true;
            this.updateUI();
            this.showToast('You are back online!', 'success');
        },

        handleOffline: function () {
            this.isOnline = false;
            this.updateUI();
            this.showToast('You are offline. Some features may be limited.', 'warning');
        },

        updateUI: function () {
            const banner = document.getElementById('offline-banner');
            const crudButtons = document.querySelectorAll('[data-requires-online="true"]');

            // Show/hide offline banner
            if (banner) {
                banner.style.display = this.isOnline ? 'none' : 'block';
            }

            // Enable/disable CRUD buttons
            crudButtons.forEach(btn => {
                if (this.isOnline) {
                    btn.removeAttribute('disabled');
                    btn.classList.remove('disabled');
                } else {
                    btn.setAttribute('disabled', 'disabled');
                    btn.classList.add('disabled');
                }
            });
        },

        // Cache news data to localStorage
        cacheNewsData: function (key, data) {
            try {
                const cacheItem = {
                    timestamp: Date.now(),
                    data: data
                };
                localStorage.setItem('funews_cache_' + key, JSON.stringify(cacheItem));
            } catch (e) {
                console.warn('LocalStorage caching failed:', e);
            }
        },

        // Get cached data
        getCachedData: function (key, maxAgeMs = 3600000) { // Default 1 hour
            try {
                const cached = localStorage.getItem('funews_cache_' + key);
                if (cached) {
                    const parsed = JSON.parse(cached);
                    if (Date.now() - parsed.timestamp < maxAgeMs) {
                        return parsed.data;
                    }
                }
            } catch (e) {
                console.warn('LocalStorage retrieval failed:', e);
            }
            return null;
        },

        loadCachedData: function () {
            // Load any cached data on page load if offline
            if (!this.isOnline) {
                const newsCache = this.getCachedData('news_list');
                if (newsCache) {
                    console.log('Loaded cached news data');
                    // Trigger custom event for views to handle
                    window.dispatchEvent(new CustomEvent('offlineDataLoaded', { detail: newsCache }));
                }
            }
        },

        showToast: function (message, type) {
            const toastContainer = document.getElementById('toast-container');
            if (!toastContainer) return;

            const toastId = 'toast-' + Date.now();
            const bgClass = type === 'success' ? 'bg-success' : 'bg-warning';

            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">
                            <i class="bi bi-${type === 'success' ? 'wifi' : 'wifi-off'} me-2"></i>
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;

            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toastEl = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastEl, { autohide: true, delay: 5000 });
            toast.show();

            toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
        },

        // Public API for caching from other scripts
        saveToCache: function (key, data) {
            this.cacheNewsData(key, data);
        },

        getFromCache: function (key) {
            return this.getCachedData(key);
        },

        getStatus: function () {
            return this.isOnline;
        }
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', () => OfflineManager.init());

    // Expose globally
    window.OfflineManager = OfflineManager;
})();
