using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ILearningPathService _learningPathService;

    public IndexModel(ICourseService courseService, ILearningPathService learningPathService)
    {
        _courseService = courseService;
        _learningPathService = learningPathService;
    }

    public List<CourseViewModel> Courses { get; set; } = new();
    public List<CourseViewModel> EnrolledCourses { get; set; } = new();
    public List<LearningPathViewModel> FeaturedPaths { get; set; } = new();
    public List<CategoryViewModel> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public bool ViewAll { get; set; }
    public string? SearchTerm { get; set; }
    public bool IsAuthenticated { get; set; }

    public async Task OnGetAsync(string? category = null, bool viewAll = false, string? searchTerm = null)
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        SelectedCategory = category;
        ViewAll = viewAll;
        SearchTerm = searchTerm;

        // Load categories from database
        var categoriesFromDb = await _courseService.GetAllCategoriesAsync();
        Categories = categoriesFromDb.ToList();
        
        // Get courses from database
        await LoadCoursesAsync();
        
        // Get featured learning paths from database
        await LoadLearningPathsAsync();

        // Get enrolled courses if authenticated
        if (IsAuthenticated)
        {
            var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var userId))
            {
                var enrolled = await _courseService.GetEnrolledCoursesAsync(userId);
                EnrolledCourses = enrolled.Take(4).ToList();
            }
        }
    }

    private async Task LoadCoursesAsync()
    {
        // Get courses from database
        IEnumerable<CourseViewModel> allCourses;
        
        if (ViewAll || !string.IsNullOrEmpty(SearchTerm))
        {
            allCourses = await _courseService.GetAllCoursesAsync(SearchTerm, null);
        }
        else
        {
            allCourses = await _courseService.GetFeaturedCoursesAsync();
        }

        Courses = allCourses.ToList();

        // Filter by category if specified
        if (!string.IsNullOrEmpty(SelectedCategory))
        {
            Courses = Courses.Where(c => c.CategoryName.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Limit courses if not viewing all and no search term
        if (!ViewAll && string.IsNullOrEmpty(SearchTerm))
        {
            Courses = Courses.Take(6).ToList();
        }
    }

    private async Task LoadLearningPathsAsync()
    {
        var paths = await _learningPathService.GetFeaturedLearningPathsAsync();
        FeaturedPaths = paths.ToList();
    }
}
