using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Instructor;

[Authorize(Roles = "Instructor")]
public class DashboardModel : PageModel
{
    private readonly ICourseService _courseService;

    public DashboardModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public List<CourseViewModel> MyCourses { get; set; } = new();
    public int TotalStudents { get; set; }
    public decimal TotalEarnings { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        var courses = await _courseService.GetInstructorCoursesAsync(userId);
        MyCourses = courses.ToList();
        
        TotalStudents = MyCourses.Sum(c => c.StudentCount);
        TotalEarnings = await _courseService.GetInstructorEarningsAsync(userId);
        AverageRating = MyCourses.Any(c => c.Rating > 0) ? MyCourses.Where(c => c.Rating > 0).Average(c => c.Rating) : 0;

        return Page();
    }

    public decimal AverageRating { get; set; }
}
