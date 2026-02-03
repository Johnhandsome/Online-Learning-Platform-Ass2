using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

public class BrowseModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ILogger<BrowseModel> _logger;

    public BrowseModel(ICourseService courseService, ILogger<BrowseModel> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    public IEnumerable<CourseViewModel> Courses { get; set; } = [];
    public IEnumerable<CategoryViewModel> Categories { get; set; } = [];
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; } = "newest";
    
    public int TotalCourses { get; set; }
    public string SelectedCategoryName { get; set; } = "All Categories";

    public async Task OnGetAsync()
    {
        _logger.LogInformation("OnGetAsync called - SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, SortBy: {SortBy}", 
            SearchTerm, CategoryId, SortBy);
        
        await LoadCoursesAsync();
    }

    public async Task<IActionResult> OnGetCoursesAsync(string? searchTerm, Guid? categoryId, string? sortBy)
    {
        _logger.LogInformation("OnGetCoursesAsync handler - searchTerm: {SearchTerm}, categoryId: {CategoryId}, sortBy: {SortBy}", 
            searchTerm, categoryId, sortBy);
        
        SearchTerm = searchTerm;
        CategoryId = categoryId;
        SortBy = sortBy ?? "newest";

        await LoadCoursesAsync();
        
        ViewData["Courses"] = Courses;
        ViewData["TotalCourses"] = TotalCourses;
        ViewData["SearchTerm"] = SearchTerm;
        ViewData["SelectedCategoryName"] = SelectedCategoryName;
        
        return Partial("_CourseGrid");
    }

    private async Task LoadCoursesAsync()
    {
        try
        {
            // Load categories
            Categories = await _courseService.GetAllCategoriesAsync();
            
            // Get category name if selected
            if (CategoryId.HasValue && Categories.Any())
            {
                var selectedCategory = Categories.FirstOrDefault(c => c.Id == CategoryId.Value);
                SelectedCategoryName = selectedCategory?.Name ?? "All Categories";
            }

            // Get courses with filters
            var courses = await _courseService.GetAllCoursesAsync(SearchTerm, CategoryId);
            
            _logger.LogInformation("Retrieved {Count} courses from service", courses.Count());

            // Apply sorting
            var sortedCourses = SortBy switch
            {
                "price_low" => courses.OrderBy(c => c.Price),
                "price_high" => courses.OrderByDescending(c => c.Price),
                "title" => courses.OrderBy(c => c.Title),
                "rating" => courses.OrderByDescending(c => c.Rating),
                _ => courses.OrderByDescending(c => c.Id) // newest
            };
            
            Courses = sortedCourses.ToList();
            TotalCourses = Courses.Count();
            
            _logger.LogInformation("Final course count after sorting: {Count}", TotalCourses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses");
            Courses = [];
            TotalCourses = 0;
            
            try
            {
                Categories = await _courseService.GetAllCategoriesAsync();
            }
            catch
            {
                Categories = [];
            }
        }
    }
}
