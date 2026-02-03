using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

public class BrowseModel : PageModel
{
    private readonly ICourseService _courseService;

    public BrowseModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public IEnumerable<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    
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
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Debug logging
            Console.WriteLine($"Browse LoadDataAsync - SearchTerm: '{SearchTerm}', CategoryId: {CategoryId}, SortBy: {SortBy}");

            Categories = await _courseService.GetAllCategoriesAsync();
            
            if (CategoryId.HasValue)
            {
                var selectedCategory = Categories.FirstOrDefault(c => c.Id == CategoryId.Value);
                SelectedCategoryName = selectedCategory?.Name ?? "All Categories";
            }

            var allCourses = await _courseService.GetAllCoursesAsync(SearchTerm, CategoryId);
            
            // Apply sorting
            allCourses = SortBy switch
            {
                "price_low" => allCourses.OrderBy(c => c.Price),
                "price_high" => allCourses.OrderByDescending(c => c.Price),
                "title" => allCourses.OrderBy(c => c.Title),
                "rating" => allCourses.OrderByDescending(c => c.Rating),
                "newest" => allCourses.OrderByDescending(c => c.Id),
                _ => allCourses.OrderBy(c => c.Title)
            };
            
            Courses = allCourses.ToList();
            TotalCourses = Courses.Count();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Browse LoadDataAsync: {ex}");
            // Return at least some data even on error to prevent total page failure
            Courses = new List<CourseViewModel>();
            TotalCourses = 0;
            Categories = await _courseService.GetAllCategoriesAsync();
        }
    }

    public async Task<IActionResult> OnGetCoursesAsync(string? searchTerm, Guid? categoryId, string? sortBy)
    {
        // Handle parameters from both property binding and explicit handler parameters
        SearchTerm = searchTerm ?? SearchTerm;
        CategoryId = categoryId ?? CategoryId;
        SortBy = sortBy ?? SortBy;

        await LoadDataAsync();
        
        // Ensure ViewData is populated for the partial view
        ViewData["Courses"] = Courses;
        ViewData["TotalCourses"] = TotalCourses;
        ViewData["SearchTerm"] = SearchTerm;
        ViewData["SelectedCategoryName"] = SelectedCategoryName;
        
        return Partial("_CourseGrid");
    }

    public async Task<IActionResult> OnGetDebugCoursesAsync(string? searchTerm, Guid? categoryId)
    {
        SearchTerm = searchTerm ?? SearchTerm;
        CategoryId = categoryId ?? CategoryId;
        await LoadDataAsync();
        
        return new JsonResult(new { 
            searchTerm = SearchTerm,
            categoryId = CategoryId,
            totalCourses = TotalCourses,
            courseCount = Courses.Count()
        });
    }
}
