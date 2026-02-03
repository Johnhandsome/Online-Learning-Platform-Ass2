// Course Browse JavaScript - AJAX filtering without page reload

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
        this.clearAllFilters = document.getElementById('clearAllFilters');

        this.searchTimeout = null;
        this.isLoading = false;

        this.initEventListeners();
        this.updateActiveFilters();
    }

    initEventListeners() {
        // Search input with debounce và loading indication
        this.searchInput?.addEventListener('input', (e) => {
            clearTimeout(this.searchTimeout);
            
            // Show subtle loading hint
            this.searchInput.style.borderColor = '#94a3b8';
            
            this.searchTimeout = setTimeout(() => {
                this.filterCourses();
                this.toggleSearchClear();
            }, 300); // Reduced debounce time for better responsiveness
        });

        // Search on Enter key
        this.searchInput?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(this.searchTimeout);
                this.filterCourses();
            }
        });

        // Clear search button
        this.searchClear?.addEventListener('click', () => {
            this.searchInput.value = '';
            this.toggleSearchClear();
            this.filterCourses();
        });

        // Category filter
        this.categoryFilter?.addEventListener('change', () => {
            this.filterCourses();
        });

        // Sort filter
        this.sortFilter?.addEventListener('change', () => {
            this.filterCourses();
        });

        // Filter remove buttons
        document.addEventListener('click', (e) => {
            if (e.target.closest('.filter-remove')) {
                const filterType = e.target.closest('.filter-remove').dataset.filter;
                this.removeFilter(filterType);
            }
        });

        // Clear all filters
        this.clearAllFilters?.addEventListener('click', () => {
            this.clearAllFiltersMethod();
        });

        // Prevent default form submission
        const searchForm = this.searchInput?.closest('form');
        if (searchForm) {
            searchForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.filterCourses();
            });
        }
    }

    async filterCourses() {
        if (this.isLoading) return;

        this.isLoading = true;
        this.showLoading();

        try {
            const params = new URLSearchParams();
            
            const searchTerm = this.searchInput?.value.trim();
            if (searchTerm) {
                params.append('searchTerm', searchTerm);
            }

            const categoryId = this.categoryFilter?.value;
            if (categoryId) {
                params.append('categoryId', categoryId);
            }

            const sortBy = this.sortFilter?.value;
            if (sortBy) {
                params.append('sortBy', sortBy);
            }

            const url = `/Course/Browse?handler=Courses&${params.toString()}`;
            
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const html = await response.text();
            this.courseGrid.innerHTML = html;

            // Update browser URL without reloading
            this.updateUrl(params);
            
            // Update active filters
            this.updateActiveFilters();

            // Update results count
            this.updateResultsCount();

            // Smooth scroll to results (không scroll v? ??u trang)
            this.smoothScrollToResults();

        } catch (error) {
            console.error('Error filtering courses:', error);
            this.showError('Failed to load courses. Please try again.');
        } finally {
            this.hideLoading();
            this.isLoading = false;
        }
    }

    showLoading() {
        this.courseGrid.classList.add('loading');
        this.courseGrid.style.opacity = '0.6';
        this.courseGrid.style.pointerEvents = 'none';
        this.loadingIndicator.style.display = 'block';
    }

    hideLoading() {
        this.courseGrid.classList.remove('loading');
        this.courseGrid.style.opacity = '1';
        this.courseGrid.style.pointerEvents = 'auto';
        this.loadingIndicator.style.display = 'none';
        
        // Add fade-in animation to new content
        const courseCards = this.courseGrid.querySelectorAll('.course-card');
        courseCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            setTimeout(() => {
                card.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 50); // Stagger animation
        });
    }

    updateUrl(params) {
        const newUrl = params.toString() ? 
            `${window.location.pathname}?${params.toString()}` : 
            window.location.pathname;
        
        window.history.replaceState({}, '', newUrl);
    }

    updateActiveFilters() {
        const searchTerm = this.searchInput?.value.trim();
        const categoryId = this.categoryFilter?.value;
        const hasActiveFilters = searchTerm || categoryId;

        if (this.activeFilters) {
            this.activeFilters.style.display = hasActiveFilters ? 'flex' : 'none';
        }

        // Update filter tags
        this.updateFilterTags();
    }

    updateFilterTags() {
        const filterTags = this.activeFilters?.querySelector('.filter-tags');
        if (!filterTags) return;

        let tagsHtml = '';
        
        const searchTerm = this.searchInput?.value.trim();
        if (searchTerm) {
            tagsHtml += `
                <span class="filter-tag">
                    Search: "${searchTerm}"
                    <button type="button" class="filter-remove" data-filter="search">
                        <i class="bi bi-x"></i>
                    </button>
                </span>
            `;
        }

        const categoryId = this.categoryFilter?.value;
        if (categoryId) {
            const categoryName = this.categoryFilter.options[this.categoryFilter.selectedIndex].text;
            tagsHtml += `
                <span class="filter-tag">
                    Category: ${categoryName}
                    <button type="button" class="filter-remove" data-filter="category">
                        <i class="bi bi-x"></i>
                    </button>
                </span>
            `;
        }

        filterTags.innerHTML = tagsHtml;
    }

    removeFilter(filterType) {
        switch (filterType) {
            case 'search':
                this.searchInput.value = '';
                this.toggleSearchClear();
                break;
            case 'category':
                this.categoryFilter.value = '';
                break;
        }
        
        this.filterCourses();
    }

    clearAllFiltersMethod() {
        if (this.searchInput) {
            this.searchInput.value = '';
            this.toggleSearchClear();
        }
        
        if (this.categoryFilter) {
            this.categoryFilter.value = '';
        }
        
        if (this.sortFilter) {
            this.sortFilter.value = 'newest';
        }
        
        this.filterCourses();
    }

    toggleSearchClear() {
        if (this.searchClear) {
            this.searchClear.style.display = 
                this.searchInput.value.trim() ? 'block' : 'none';
        }
    }

    updateResultsCount() {
        const courseCards = this.courseGrid.querySelectorAll('.course-card');
        if (this.resultsCount) {
            this.resultsCount.textContent = courseCards.length;
        }
    }

    smoothScrollToResults() {
        // Ch? scroll n?u ng??i dùng ?ang ? phía trên khu v?c k?t qu?
        const courseGridTop = this.courseGrid.offsetTop - 100; // 100px buffer t? top
        const currentScroll = window.pageYOffset;
        
        // Ch? scroll xu?ng n?u ?ang ? trên khu v?c k?t qu?
        // Không scroll n?u ?ã ? d??i ho?c g?n khu v?c k?t qu?
        if (currentScroll < courseGridTop) {
            window.scrollTo({
                top: courseGridTop,
                behavior: 'smooth'
            });
        }
        // N?u ?ã ? trong khu v?c k?t qu?, không scroll gì c?
    }

    showError(message) {
        // Show error message
        const errorHtml = `
            <div class="alert alert-danger" role="alert">
                <i class="bi bi-exclamation-triangle me-2"></i>
                ${message}
            </div>
        `;
        this.courseGrid.innerHTML = errorHtml;
    }
}

// Category filter functionality for homepage
class CategoryFilter {
    constructor() {
        this.initCategoryButtons();
    }

    initCategoryButtons() {
        // Add event listeners for category buttons on homepage
        document.addEventListener('click', (e) => {
            if (e.target.closest('.category-card') || e.target.closest('[data-category]')) {
                e.preventDefault();
                const categoryElement = e.target.closest('.category-card') || e.target.closest('[data-category]');
                const categoryId = categoryElement.dataset.category;
                const categoryName = categoryElement.dataset.categoryName;
                
                // Navigate to browse page with category filter
                this.navigateToBrowseWithCategory(categoryId, categoryName);
            }
        });
    }

    navigateToBrowseWithCategory(categoryId, categoryName) {
        const url = `/Course/Browse?categoryId=${categoryId}`;
        window.location.href = url;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Initialize course browser only on browse page
    if (document.getElementById('courseGrid')) {
        new CourseBrowser();
    }

    // Initialize category filter for all pages
    new CategoryFilter();
});

// Export for use in other scripts
window.CourseBrowser = CourseBrowser;
window.CategoryFilter = CategoryFilter;

// Global function for clearing filters (accessible from HTML)
function clearAllFilters() {
    if (window.courseBrowserInstance) {
        window.courseBrowserInstance.clearAllFiltersMethod();
    }
}

// Make instance globally accessible
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('courseGrid')) {
        window.courseBrowserInstance = new CourseBrowser();
    }
});
