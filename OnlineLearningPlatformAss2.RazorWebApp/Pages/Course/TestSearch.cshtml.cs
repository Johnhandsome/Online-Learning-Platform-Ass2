using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

public class TestSearchModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ILogger<TestSearchModel> _logger;

    public TestSearchModel(ICourseService courseService, ILogger<TestSearchModel> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    public IEnumerable<CourseViewModel> Courses { get; set; } = [];

    public async Task OnGetAsync()
    {
        _logger.LogInformation("TestSearch - SearchTerm: '{SearchTerm}', CategoryId: {CategoryId}", SearchTerm, CategoryId);

        try
        {
            var courses = await _courseService.GetAllCoursesAsync(SearchTerm, CategoryId);
            Courses = courses.ToList();
            
            _logger.LogInformation("Retrieved {Count} courses", Courses.Count());
            
            foreach (var course in Courses)
            {
                _logger.LogInformation("Course: {Title} - {Category}", course.Title, course.CategoryName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses");
            Courses = [];
        }
    }
}
