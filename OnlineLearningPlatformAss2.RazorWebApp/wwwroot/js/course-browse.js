// Course Browse JavaScript - Simple and effective filtering

class CourseBrowser {
    constructor() {
        this.searchInput = document.getElementById('searchInput');
        this.categoryFilter = document.getElementById('categoryFilter');
        this.sortFilter = document.getElementById('sortFilter');
        this.searchClear = document.getElementById('searchClear');
        this.courseGrid = document.getElementById('courseGrid');
        this.loadingIndicator = document.getElementById('loadingIndicator');
        this.activeFilters = document.getElementById('activeFilters');
        this.resultsCount = document.getElementById('resultsCount');
        this.totalCoursesDisplay = document.getElementById('totalCoursesDisplay');
        this.clearAllFilters = document.getElementById('clearAllFilters');
        this.filterTags = document.getElementById('filterTags');

        this.searchTimeout = null;
        this.isLoading = false;

        this.init();
    }

    init() {
        console.log('CourseBrowser initialized');
        console.log('Initial search term from input:', this.searchInput?.value);
        console.log('Initial search term from URL:', new URLSearchParams(window.location.search).get('SearchTerm'));
        
        this.syncInputWithURL();
        this.attachEventListeners();
        this.logCurrentState();
    }

    syncInputWithURL() {
        // Sync input box with URL parameter on page load
        const urlParams = new URLSearchParams(window.location.search);
        const urlSearchTerm = urlParams.get('SearchTerm') || '';
        
        if (this.searchInput && urlSearchTerm) {
            this.searchInput.value = urlSearchTerm;
            this.updateSearchClearButton();
        }
    }

    attachEventListeners() {
        // Search with debounce
        this.searchInput?.addEventListener('input', () => {
            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(() => this.applyFilters(), 500);
            this.updateSearchClearButton();
        });

        // Search on Enter key
        this.searchInput?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(this.searchTimeout);
                this.applyFilters();
            }
        });

        // Clear search
        this.searchClear?.addEventListener('click', () => {
            this.searchInput.value = '';
            this.updateSearchClearButton();
            this.applyFilters();
        });

        // Category filter
        this.categoryFilter?.addEventListener('change', () => this.applyFilters());

        // Sort filter
        this.sortFilter?.addEventListener('change', () => this.applyFilters());

        // Remove individual filters
        document.addEventListener('click', (e) => {
            const removeBtn = e.target.closest('.filter-remove');
            if (removeBtn) {
                const filterType = removeBtn.dataset.filter;
                this.removeFilter(filterType);
            }
        });

        // Clear all filters
        this.clearAllFilters?.addEventListener('click', () => this.clearAll());
    }

    async applyFilters() {
        if (this.isLoading) {
            console.log('Already loading, skipping...');
            return;
        }

        const searchTerm = this.searchInput?.value.trim() || '';
        const categoryId = this.categoryFilter?.value || '';
        const sortBy = this.sortFilter?.value || 'newest';

        console.log('=== Applying filters ===');
        console.log('Search term:', searchTerm);
        console.log('Category ID:', categoryId);
        console.log('Sort by:', sortBy);

        this.isLoading = true;
        this.showLoading();

        try {
            const params = new URLSearchParams();
            if (searchTerm) params.append('searchTerm', searchTerm);
            if (categoryId) params.append('categoryId', categoryId);
            params.append('sortBy', sortBy);

            const url = `/Course/Browse?handler=Courses&${params.toString()}`;
            console.log('Fetching:', url);

            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            console.log('Response status:', response.status);

            if (!response.ok) {
                throw new Error(`Server returned ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();
            console.log('Received HTML length:', html.length);
            console.log('HTML preview:', html.substring(0, 200));

            this.courseGrid.innerHTML = html;
            
            // Update UI
            this.updateResultsCount();
            this.updateActiveFilters();
            this.updateURL(params);

            console.log('=== Filters applied successfully ===');

        } catch (error) {
            console.error('Error applying filters:', error);
            this.showError(error.message);
        } finally {
            this.hideLoading();
            this.isLoading = false;
        }
    }

    showLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'block';
        }
        if (this.courseGrid) {
            this.courseGrid.style.opacity = '0.5';
        }
    }

    hideLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'none';
        }
        if (this.courseGrid) {
            this.courseGrid.style.opacity = '1';
        }
    }

    updateResultsCount() {
        const courseCards = this.courseGrid.querySelectorAll('.course-card');
        const count = courseCards.length;
        
        console.log('Found course cards:', count);
        
        if (this.resultsCount) {
            this.resultsCount.textContent = count;
        }
        if (this.totalCoursesDisplay) {
            this.totalCoursesDisplay.textContent = count;
        }
    }

    updateActiveFilters() {
        const searchTerm = this.searchInput?.value.trim() || '';
        const categoryId = this.categoryFilter?.value || '';
        
        if (!searchTerm && !categoryId) {
            if (this.activeFilters) {
                this.activeFilters.style.display = 'none';
            }
            return;
        }

        if (this.activeFilters) {
            this.activeFilters.style.display = 'flex';
        }

        if (this.filterTags) {
            let html = '';
            
            if (searchTerm) {
                html += `
                    <span class="filter-tag">
                        Search: "${searchTerm}"
                        <button type="button" class="filter-remove" data-filter="search">
                            <i class="bi bi-x"></i>
                        </button>
                    </span>
                `;
            }
            
            if (categoryId && this.categoryFilter) {
                const categoryName = this.categoryFilter.options[this.categoryFilter.selectedIndex].text;
                html += `
                    <span class="filter-tag">
                        Category: ${categoryName}
                        <button type="button" class="filter-remove" data-filter="category">
                            <i class="bi bi-x"></i>
                        </button>
                    </span>
                `;
            }
            
            this.filterTags.innerHTML = html;
        }
    }

    updateSearchClearButton() {
        const hasText = this.searchInput?.value.trim().length > 0;
        if (this.searchClear) {
            this.searchClear.style.display = hasText ? 'block' : 'none';
        }
    }

    removeFilter(filterType) {
        console.log('Removing filter:', filterType);
        
        if (filterType === 'search' && this.searchInput) {
            this.searchInput.value = '';
            this.updateSearchClearButton();
        } else if (filterType === 'category' && this.categoryFilter) {
            this.categoryFilter.value = '';
        }
        
        this.applyFilters();
    }

    clearAll() {
        console.log('Clearing all filters');
        
        if (this.searchInput) {
            this.searchInput.value = '';
            this.updateSearchClearButton();
        }
        if (this.categoryFilter) {
            this.categoryFilter.value = '';
        }
        if (this.sortFilter) {
            this.sortFilter.value = 'newest';
        }
        
        this.applyFilters();
    }

    updateURL(params) {
        const newUrl = params.toString() 
            ? `${window.location.pathname}?${params.toString()}`
            : window.location.pathname;
        
        console.log('Updating URL to:', newUrl);
        window.history.replaceState({}, '', newUrl);
    }

    showError(message) {
        const errorHtml = `
            <div class="alert alert-danger" role="alert">
                <i class="bi bi-exclamation-triangle me-2"></i>
                <strong>Error:</strong> ${message}
            </div>
            <div class="text-center mt-3">
                <button class="btn btn-primary" onclick="location.reload()">
                    <i class="bi bi-arrow-clockwise me-1"></i> Reload Page
                </button>
            </div>
        `;
        this.courseGrid.innerHTML = errorHtml;
    }

    logCurrentState() {
        console.log('=== Current State ===');
        console.log('Search:', this.searchInput?.value);
        console.log('Category:', this.categoryFilter?.value);
        console.log('Sort:', this.sortFilter?.value);
        console.log('Course cards:', this.courseGrid?.querySelectorAll('.course-card').length);
        console.log('===================');
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('courseGrid')) {
        console.log('Initializing CourseBrowser...');
        window.courseBrowser = new CourseBrowser();
    }
});

// Global clear function
function clearAllFilters() {
    if (window.courseBrowser) {
        window.courseBrowser.clearAll();
    }
}
