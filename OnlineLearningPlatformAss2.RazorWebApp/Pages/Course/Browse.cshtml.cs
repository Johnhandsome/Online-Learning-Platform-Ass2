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
            _ => allCourses.OrderBy(c => c.Title) // default sorting by title since CreatedAt doesn't exist
        };
        
        Courses = allCourses.ToList();
        TotalCourses = Courses.Count();
    }

    public async Task<IActionResult> OnGetCoursesAsync()
    {
        await OnGetAsync();
        
        return Partial("_CourseGrid", new { 
            Courses = Courses, 
            TotalCourses = TotalCourses,
            SearchTerm = SearchTerm,
            SelectedCategoryName = SelectedCategoryName
        });
    }
}
