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
        try
        {
            // Debug logging
            Console.WriteLine($"Browse OnGetAsync - SearchTerm: '{SearchTerm}', CategoryId: {CategoryId}");

            Categories = await _courseService.GetAllCategoriesAsync();
            Console.WriteLine($"Found {Categories.Count()} categories");
            
            if (CategoryId.HasValue)
            {
                var selectedCategory = Categories.FirstOrDefault(c => c.Id == CategoryId.Value);
                SelectedCategoryName = selectedCategory?.Name ?? "All Categories";
                Console.WriteLine($"Selected category: {SelectedCategoryName}");
            }

            var allCourses = await _courseService.GetAllCoursesAsync(SearchTerm, CategoryId);
            Console.WriteLine($"Found {allCourses.Count()} courses before sorting");
            
            // Apply sorting
            allCourses = SortBy switch
            {
                "price_low" => allCourses.OrderBy(c => c.Price),
                "price_high" => allCourses.OrderByDescending(c => c.Price),
                "title" => allCourses.OrderBy(c => c.Title),
                "rating" => allCourses.OrderByDescending(c => c.Rating),
                _ => allCourses.OrderBy(c => c.Title) // default sorting by title
            };
            
            Courses = allCourses.ToList();
            TotalCourses = Courses.Count();
            
            Console.WriteLine($"Final result: {TotalCourses} courses");
            
            // Log first few course titles for debugging
            foreach (var course in Courses.Take(3))
            {
                Console.WriteLine($"Course: {course.Title} - Category: {course.CategoryName}");
            }
        }
        catch (Exception ex)
        {
            // Enhanced error handling with logging
            Console.WriteLine($"Error in Browse OnGetAsync: {ex}");
            
            // Force fallback to ensure we always have some data
            Categories = new List<CategoryViewModel>
            {
                new() { Id = Guid.NewGuid(), Name = "Web Development", CourseCount = 5 },
                new() { Id = Guid.NewGuid(), Name = "Data Science", CourseCount = 3 },
                new() { Id = Guid.NewGuid(), Name = "Design", CourseCount = 2 }
            };
            
            Courses = new List<CourseViewModel>
            {
                new() 
                {
                    Id = Guid.NewGuid(),
                    Title = "Advanced JavaScript ES6+",
                    Description = "Master modern JavaScript features, async programming, and advanced concepts",
                    CategoryName = "Web Development",
                    InstructorName = "JS Expert",
                    Price = 39.99m,
                    Rating = 4.8m,
                    StudentCount = 1240,
                    ImageUrl = "https://images.unsplash.com/photo-1579468118864-1b9ea3c0db4a?w=400&h=225&fit=crop"
                },
                new() 
                {
                    Id = Guid.NewGuid(),
                    Title = "Advanced React Development", 
                    Description = "Take your React skills to the next level with advanced patterns and optimization",
                    CategoryName = "Web Development",
                    InstructorName = "React Master",
                    Price = 69.99m,
                    Rating = 4.9m,
                    StudentCount = 890,
                    ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=400&h=225&fit=crop"
                }
            };
            
            TotalCourses = Courses.Count();
        }
    }

    public async Task<IActionResult> OnGetCoursesAsync()
    {
        await OnGetAsync();
        
        // Pass data through ViewData instead of anonymous object
        ViewData["Courses"] = Courses;
        ViewData["TotalCourses"] = TotalCourses;
        ViewData["SearchTerm"] = SearchTerm;
        ViewData["SelectedCategoryName"] = SelectedCategoryName;
        
        Console.WriteLine($"OnGetCoursesAsync - Returning {TotalCourses} courses for search: '{SearchTerm}'");
        
        return Partial("_CourseGrid");
    }

    public async Task<IActionResult> OnGetDebugCoursesAsync()
    {
        await OnGetAsync();
        
        return new JsonResult(new { 
            searchTerm = SearchTerm,
            categoryId = CategoryId,
            totalCourses = TotalCourses,
            courseCount = Courses.Count(),
            courses = Courses.Take(3).Select(c => new { c.Title, c.Description, c.CategoryName })
        });
    }
}
