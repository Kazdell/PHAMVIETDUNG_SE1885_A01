// Offline Mode Detection and Management
(function () {
    'use strict';

    const OfflineManager = {
        isOnline: navigator.onLine,
        cachedData: {},

        // Initialize on DOM ready
        init: function () {
            this.bindEvents();
            this.setupInterceptors();
            this.updateUI();
            this.loadCachedData();
            // Initial check
            if (this.isOnline) {
                this.checkConnection();
            }
        },

        bindEvents: function () {
            window.addEventListener('online', () => this.handleOnline());
            window.addEventListener('offline', () => this.handleOffline());
        },

        setupInterceptors: function () {
            const self = this;

            // 1. Intercept Fetch API
            const originalFetch = window.fetch;
            window.fetch = function () {
                return originalFetch.apply(this, arguments)
                    .then(response => {
                        // If successful response and we were offline, check if we should go online
                        if (!self.isOnline && response.ok) {
                            self.handleOnline();
                        }
                        return response;
                    })
                    .catch(error => {
                        // TypeError represents network error
                        if (error instanceof TypeError) {
                            console.warn('Fetch error detected (Offline?):', error);
                            self.handleOffline();
                        }
                        throw error;
                    });
            };

            // 2. Intercept jQuery AJAX (if available)
            if (window.jQuery) {
                $(document).ajaxError(function (event, jqXHR, settings, thrownError) {
                    // status 0 usually means no connection / cors error / server down
                    if (jqXHR.status === 0) {
                        console.warn('jQuery AJAX error detected (Offline/Server Down):', thrownError);
                        self.handleOffline();
                    }
                });

                $(document).ajaxSuccess(function () {
                    if (!self.isOnline) {
                        self.handleOnline();
                    }
                });
            }
        },

        checkConnection: function () {
            // Simple ping to check if server is actually reachable
            // We use a HEAD request to a static file or lightweight endpoint
            fetch('/favicon.ico', { method: 'HEAD', cache: 'no-cache' })
                .then(() => {
                    if (!this.isOnline) this.handleOnline();
                })
                .catch(() => {
                    if (this.isOnline) this.handleOffline();
                });
        },

        handleOnline: function () {
            if (this.isOnline) return; // Already online
            console.log('Online Mode Restored');
            this.isOnline = true;
            this.updateUI();
            this.showToast('You are back online!', 'success');
        },

        handleOffline: function () {
            if (!this.isOnline) return; // Already offline
            console.log('Offline Mode Triggered');
            this.isOnline = false;
            this.updateUI();
            this.showToast('Connection lost. Offline Mode enabled.', 'warning');
        },

        updateUI: function () {
            const banner = document.getElementById('offline-banner');
            const crudButtons = document.querySelectorAll('[data-requires-online="true"]');

            // Show/hide offline banner
            if (banner) {
                if (!this.isOnline) {
                    banner.style.display = 'block';
                    banner.innerHTML = '<strong>Offline Mode</strong> - Server unreachable. Features disabled.';
                } else {
                    banner.style.display = 'none';
                }
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
            const newsCache = this.getCachedData('news_list');

            // Only broadcast cache if we are offline OR if we want to support "Stale-While-Revalidate" UI
            // Ideally, we check connectivity first. 
            // If offline, we MUST load cache.
            if (!this.isOnline && newsCache) {
                console.log('Loaded cached news data (Offline)');
                window.dispatchEvent(new CustomEvent('offlineDataLoaded', { detail: newsCache }));
            }
        },

        showToast: function (message, type) {
            const toastContainer = document.getElementById('toast-container');
            if (!toastContainer) return;

            const toastId = 'toast-' + Date.now();
            const bgClass = type === 'success' ? 'bg-success' : 'bg-warning';

            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white border-0" style="background-color: #198754;" role="alert">
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
        },

        // Manual trigger for testing
        triggerOffline: function () {
            this.handleOffline();
        },
        triggerOnline: function () {
            this.handleOnline();
        }
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', () => OfflineManager.init());

    // Expose globally
    window.OfflineManager = OfflineManager;
})();
