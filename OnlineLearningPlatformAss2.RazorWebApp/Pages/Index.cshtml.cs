using OnlineLearningPlatformAss2.RazorWebApp.Models;

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

    public IEnumerable<CourseViewModel> FeaturedCourses { get; set; } = new List<CourseViewModel>();
    public IEnumerable<LearningPathViewModel> FeaturedPaths { get; set; } = new List<LearningPathViewModel>();
    public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? SelectedCategoryId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool ViewAll { get; set; }

    public async Task OnGetAsync()
    {
        IEnumerable<CourseViewModel> courses;
        
        // Logic:
        // - If searching: show all matching courses
        // - If filtering by category: show all courses in that category
        // - If viewAll = true: show all courses
        // - Otherwise: show featured (top 6) courses, fallback to all if empty
        
        if (!string.IsNullOrEmpty(SearchTerm) || SelectedCategoryId.HasValue || ViewAll)
        {
            // User is searching, filtering, or viewing all
            courses = await _courseService.GetAllCoursesAsync(SearchTerm, SelectedCategoryId);
        }
        else
        {
            // Default: show featured courses
            courses = await _courseService.GetFeaturedCoursesAsync();
            
            // Fallback: if no featured courses, show all courses
            if (!courses.Any())
            {
                courses = await _courseService.GetAllCoursesAsync();
            }
        }

        FeaturedCourses = courses;
        FeaturedPaths = await _learningPathService.GetFeaturedLearningPathsAsync();
        Categories = await _courseService.GetAllCategoriesAsync();
    }
}
