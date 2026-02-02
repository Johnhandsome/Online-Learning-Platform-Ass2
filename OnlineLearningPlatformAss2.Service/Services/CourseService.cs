using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class CourseService : ICourseService
{
    private readonly OnlineLearningContext _context;

    public CourseService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CourseViewModel>> GetFeaturedCoursesAsync()
    {
        try
        {
            // Get sample featured courses from seeded data
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Take(6)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    CategoryName = c.Category.Name,
                    InstructorName = c.Instructor.Username,
                    Rating = 4.5m, // Default rating
                    StudentCount = 1250, // Default student count
                    IsFeatured = true
                })
                .ToListAsync();

            return courses;
        }
        catch
        {
            return GetSampleCourses();
        }
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null)
    {
        try
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm) || 
                                       c.Description.Contains(searchTerm) ||
                                       c.Category.Name.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            var courses = await query
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    CategoryName = c.Category.Name,
                    InstructorName = c.Instructor.Username,
                    Rating = 4.3m,
                    StudentCount = 850,
                    IsFeatured = false
                })
                .ToListAsync();

            return courses.Any() ? courses : GetSampleCourses();
        }
        catch
        {
            return GetSampleCourses();
        }
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return GetSampleCourseDetail(id);
            }

            var isEnrolled = false;
            if (userId.HasValue)
            {
                isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId.Value && e.CourseId == id);
            }

            return new CourseDetailViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                CategoryName = course.Category.Name,
                InstructorName = course.Instructor.Username,
                Rating = 4.6m,
                ReviewCount = 324,
                StudentCount = 2150,
                Level = "Intermediate",
                Language = "English",
                IsEnrolled = isEnrolled,
                WhatYouWillLearn = GetSampleLearningPoints(),
                Requirements = GetSampleRequirements(),
                Modules = course.Modules.Select(m => new ModuleViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    OrderIndex = m.OrderIndex,
                    Lessons = m.Lessons.Select(l => new LessonViewModel
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Duration = 15, // Default duration
                        OrderIndex = l.OrderIndex,
                        IsPreview = l.OrderIndex <= 2 // First 2 lessons are preview
                    }).OrderBy(l => l.OrderIndex).ToList()
                }).OrderBy(m => m.OrderIndex).ToList()
            };
        }
        catch
        {
            return GetSampleCourseDetail(id);
        }
    }

    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
    {
        try
        {
            var enrolledCourses = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .Include(e => e.Course.Instructor)
                .Where(e => e.UserId == userId)
                .Select(e => new CourseViewModel
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    Price = e.Course.Price,
                    ImageUrl = e.Course.ImageUrl,
                    CategoryName = e.Course.Category.Name,
                    InstructorName = e.Course.Instructor.Username,
                    Rating = 4.5m,
                    StudentCount = 1200,
                    EnrollmentDate = e.EnrolledAt,
                    Progress = 45 // Default progress
                })
                .ToListAsync();

            return enrolledCourses.Any() ? enrolledCourses : GetSampleEnrolledCourses();
        }
        catch
        {
            return GetSampleEnrolledCourses();
        }
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CourseCount = _context.Courses.Count(course => course.CategoryId == c.Id)
                })
                .ToListAsync();

            return categories.Any() ? categories : GetSampleCategories();
        }
        catch
        {
            return GetSampleCategories();
        }
    }

    public async Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId)
    {
        try
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return null;

            return new CourseLearnViewModel
            {
                EnrollmentId = enrollment.Id,
                CourseId = enrollment.Course.Id,
                CourseTitle = enrollment.Course.Title,
                CurrentLessonId = enrollment.Course.Modules.FirstOrDefault()?.Lessons.FirstOrDefault()?.Id,
                Progress = 25,
                Modules = enrollment.Course.Modules.Select(m => new ModuleViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    OrderIndex = m.OrderIndex,
                    Lessons = m.Lessons.Select(l => new LessonViewModel
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        VideoUrl = l.VideoUrl,
                        Duration = 15,
                        OrderIndex = l.OrderIndex,
                        IsCompleted = false
                    }).OrderBy(l => l.OrderIndex).ToList()
                }).OrderBy(m => m.OrderIndex).ToList()
            };
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<CourseViewModel> GetSampleCourses()
    {
        return new List<CourseViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Complete Web Development Bootcamp",
                Description = "Learn HTML, CSS, JavaScript, React, Node.js and build real-world projects",
                Price = 49.99m,
                ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=225&fit=crop",
                CategoryName = "Web Development",
                InstructorName = "John Developer",
                Rating = 4.8m,
                StudentCount = 12540,
                IsFeatured = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Data Science with Python",
                Description = "Master data analysis, visualization, and machine learning",
                Price = 59.99m,
                ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=225&fit=crop",
                CategoryName = "Data Science",
                InstructorName = "Dr. Data Expert",
                Rating = 4.7m,
                StudentCount = 8920,
                IsFeatured = true
            }
        };
    }

    private CourseDetailViewModel GetSampleCourseDetail(Guid id)
    {
        return new CourseDetailViewModel
        {
            Id = id,
            Title = "Complete Web Development Bootcamp",
            Description = "Learn to build professional websites and web applications from scratch. This comprehensive course covers everything from HTML and CSS basics to advanced React and Node.js development.",
            Price = 49.99m,
            ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=600&h=400&fit=crop",
            CategoryName = "Web Development",
            InstructorName = "John Developer",
            Rating = 4.8m,
            ReviewCount = 2847,
            StudentCount = 12540,
            Level = "All Levels",
            Language = "English",
            WhatYouWillLearn = GetSampleLearningPoints(),
            Requirements = GetSampleRequirements(),
            Modules = GetSampleModules()
        };
    }

    private List<string> GetSampleLearningPoints()
    {
        return new List<string>
        {
            "Build responsive websites using HTML5, CSS3, and JavaScript",
            "Master modern JavaScript ES6+ features and async programming",
            "Create dynamic web applications with React.js",
            "Develop server-side applications with Node.js and Express",
            "Work with databases using MongoDB and SQL",
            "Deploy applications to production using cloud platforms",
            "Understand web security best practices",
            "Build RESTful APIs and work with third-party APIs"
        };
    }

    private List<string> GetSampleRequirements()
    {
        return new List<string>
        {
            "Basic computer skills and familiarity with using a web browser",
            "No prior programming experience required - we'll teach you everything",
            "A computer with internet connection (Windows, Mac, or Linux)",
            "Willingness to learn and practice coding regularly"
        };
    }

    private List<ModuleViewModel> GetSampleModules()
    {
        return new List<ModuleViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Introduction to Web Development",
                Description = "Getting started with web development fundamentals",
                OrderIndex = 1,
                Lessons = new List<LessonViewModel>
                {
                    new() { Id = Guid.NewGuid(), Title = "Welcome to the Course", Duration = 5, OrderIndex = 1, IsPreview = true },
                    new() { Id = Guid.NewGuid(), Title = "Setting Up Your Development Environment", Duration = 15, OrderIndex = 2, IsPreview = true },
                    new() { Id = Guid.NewGuid(), Title = "How the Internet Works", Duration = 20, OrderIndex = 3 }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "HTML Fundamentals",
                Description = "Master HTML5 and semantic markup",
                OrderIndex = 2,
                Lessons = new List<LessonViewModel>
                {
                    new() { Id = Guid.NewGuid(), Title = "HTML Structure and Elements", Duration = 25, OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Title = "Forms and Input Elements", Duration = 30, OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Title = "Semantic HTML5 Elements", Duration = 20, OrderIndex = 3 }
                }
            }
        };
    }

    private IEnumerable<CourseViewModel> GetSampleEnrolledCourses()
    {
        return new List<CourseViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Complete Web Development Bootcamp",
                Description = "Learn HTML, CSS, JavaScript, React, Node.js",
                Price = 49.99m,
                ImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=400&h=225&fit=crop",
                CategoryName = "Web Development",
                InstructorName = "John Developer",
                Rating = 4.8m,
                StudentCount = 12540,
                EnrollmentDate = DateTime.Now.AddDays(-15),
                Progress = 45
            }
        };
    }

    private IEnumerable<CategoryViewModel> GetSampleCategories()
    {
        return new List<CategoryViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Web Development", Description = "Frontend and backend web technologies", CourseCount = 25 },
            new() { Id = Guid.NewGuid(), Name = "Data Science", Description = "Data analysis and machine learning", CourseCount = 18 },
            new() { Id = Guid.NewGuid(), Name = "Design", Description = "UI/UX and graphic design", CourseCount = 12 },
            new() { Id = Guid.NewGuid(), Name = "Business", Description = "Business strategy and management", CourseCount = 15 }
        };
    }
}
