using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

    public List<CourseDisplayModel> Courses { get; set; } = new();
    public List<LearningPathDisplayModel> FeaturedPaths { get; set; } = new();
    public List<CategoryDisplayModel> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public bool ViewAll { get; set; }
    public string? SearchTerm { get; set; }

    public async Task OnGetAsync(string? category = null, bool viewAll = false, string? searchTerm = null)
    {
        SelectedCategory = category;
        ViewAll = viewAll;
        SearchTerm = searchTerm;

        // Load categories
        Categories = GetSampleCategories();
        
        // Get courses
        await LoadCoursesAsync();
        
        // Get featured learning paths
        await LoadLearningPathsAsync();
    }

    private async Task LoadCoursesAsync()
    {
        try
        {
            // For now, create sample data since CourseService might not be fully implemented
            Courses = GetSampleCourses();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(SelectedCategory))
            {
                Courses = Courses.Where(c => c.CategoryName.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filter by search term if specified
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Courses = Courses.Where(c => 
                    c.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.CategoryName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Limit courses if not viewing all
            if (!ViewAll && string.IsNullOrEmpty(SearchTerm))
            {
                Courses = Courses.Take(6).ToList();
            }
        }
        catch
        {
            // Fallback to sample data
            Courses = GetSampleCourses().Take(6).ToList();
        }
    }

    private async Task LoadLearningPathsAsync()
    {
        try
        {
            // For now, create sample data since LearningPathService might not be fully implemented
            FeaturedPaths = GetSampleLearningPaths();
        }
        catch
        {
            // Fallback to sample data
            FeaturedPaths = GetSampleLearningPaths();
        }
    }

    private List<CourseDisplayModel> GetSampleCourses()
    {
        return new List<CourseDisplayModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Complete Web Development Bootcamp",
                Description = "Learn HTML, CSS, JavaScript, React, Node.js and build real-world projects",
                CategoryName = "Web Development",
                InstructorName = "Expert Instructor",
                Price = 49.99m,
                Rating = 4.8m,
                ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=225&fit=crop",
                IsFeatured = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Data Science with Python",
                Description = "Master data analysis, visualization, and machine learning with Python",
                CategoryName = "Data Science",
                InstructorName = "Dr. Data Expert",
                Price = 59.99m,
                Rating = 4.7m,
                ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=225&fit=crop",
                IsFeatured = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "UI/UX Design Fundamentals",
                Description = "Learn user interface and user experience design principles and tools",
                CategoryName = "Design",
                InstructorName = "Creative Designer",
                Price = 39.99m,
                Rating = 4.6m,
                ImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400&h=225&fit=crop",
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Digital Marketing Mastery",
                Description = "Complete guide to digital marketing, SEO, social media, and advertising",
                CategoryName = "Business",
                InstructorName = "Marketing Pro",
                Price = 44.99m,
                Rating = 4.5m,
                ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400&h=225&fit=crop",
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced CSS Layouts",
                Description = "Master CSS Grid, Flexbox, and modern layout techniques",
                CategoryName = "Web Development",
                InstructorName = "CSS Expert",
                Price = 29.99m,
                Rating = 4.4m,
                ImageUrl = "https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=400&h=225&fit=crop",
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Machine Learning Fundamentals",
                Description = "Introduction to machine learning algorithms and applications",
                CategoryName = "Data Science",
                InstructorName = "ML Researcher",
                Price = 69.99m,
                Rating = 4.9m,
                ImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400&h=225&fit=crop",
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Business Strategy & Analytics",
                Description = "Learn strategic thinking and data-driven decision making",
                CategoryName = "Business",
                InstructorName = "Strategy Consultant",
                Price = 54.99m,
                Rating = 4.3m,
                ImageUrl = "https://images.unsplash.com/photo-1551434678-e076c223a692?w=400&h=225&fit=crop",
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Graphic Design Essentials",
                Description = "Master Adobe Creative Suite and design principles",
                CategoryName = "Design",
                InstructorName = "Visual Artist",
                Price = 34.99m,
                Rating = 4.6m,
                ImageUrl = "https://images.unsplash.com/photo-1626785774625-0b1c2c4eab67?w=400&h=225&fit=crop",
                IsFeatured = false
            }
        };
    }

    private List<LearningPathDisplayModel> GetSampleLearningPaths()
    {
        return new List<LearningPathDisplayModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Full Stack Web Developer",
                Description = "Master both front-end and back-end development with modern technologies",
                Price = 299.99m,
                CourseCount = 7,
                BadgeText = "Most Popular"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Data Science Professional",
                Description = "From Python basics to advanced machine learning and AI",
                Price = 399.99m,
                CourseCount = 6,
                BadgeText = "High Demand"
            }
        };
    }

    private List<CategoryDisplayModel> GetSampleCategories()
    {
        return new List<CategoryDisplayModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Web Development" },
            new() { Id = Guid.NewGuid(), Name = "Data Science" },
            new() { Id = Guid.NewGuid(), Name = "Design" },
            new() { Id = Guid.NewGuid(), Name = "Business" },
            new() { Id = Guid.NewGuid(), Name = "Mobile Development" },
            new() { Id = Guid.NewGuid(), Name = "Marketing" }
        };
    }
}

public class CourseDisplayModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }

    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
    public string RatingDisplay => new string('★', (int)Math.Round(Rating)) + new string('☆', 5 - (int)Math.Round(Rating));
}

public class LearningPathDisplayModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CourseCount { get; set; }
    public string BadgeText { get; set; } = string.Empty;

    public string FormattedPrice => Price == 0 ? "Free" : Price.ToString("C");
}

public class CategoryDisplayModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
