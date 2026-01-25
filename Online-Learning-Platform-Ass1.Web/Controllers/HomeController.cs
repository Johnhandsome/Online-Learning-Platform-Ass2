using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Online_Learning_Platform_Ass1.Data.Models;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;
using Online_Learning_Platform_Ass1.Web.Models;
using Online_Learning_Platform_Ass1.Service.DTOs.Course;

namespace Online_Learning_Platform_Ass1.Data.Controllers;

public class HomeController(
    ICourseService courseService,
    ILearningPathService learningPathService) : Controller
{
    public async Task<IActionResult> IndexAsync(string? searchTerm = null, Guid? categoryId = null, bool viewAll = false)
    {
        IEnumerable<CourseViewModel> courses;
        
        // Logic:
        // - If searching: show all matching courses
        // - If filtering by category: show all courses in that category
        // - If viewAll = true: show all courses
        // - Otherwise: show featured (top 6) courses, fallback to all if empty
        
        if (!string.IsNullOrEmpty(searchTerm) || categoryId.HasValue || viewAll)
        {
            // User is searching, filtering, or viewing all
            courses = await courseService.GetAllCoursesAsync(searchTerm, categoryId);
        }
        else
        {
            // Default: show featured courses
            courses = await courseService.GetFeaturedCoursesAsync();
            
            // Fallback: if no featured courses, show all courses
            if (!courses.Any())
            {
                courses = await courseService.GetAllCoursesAsync();
            }
        }

        var paths = await learningPathService.GetFeaturedLearningPathsAsync();
        var categories = await courseService.GetAllCategoriesAsync();

        var model = new HomeViewModel
        {
            FeaturedCourses = courses,
            FeaturedPaths = paths,
            SearchTerm = searchTerm,
            SelectedCategoryId = categoryId,
            ViewAll = viewAll,
            Categories = categories
        };
        return View(model);
    }

    public IActionResult PrivacyAsync() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ErrorAsync() => View(new ErrorViewModel
        { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
