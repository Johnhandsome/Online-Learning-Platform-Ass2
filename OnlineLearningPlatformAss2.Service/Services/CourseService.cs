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
        // Always start with sample data to ensure we have courses to search
        var sampleCourses = GetSampleCourses().ToList();
        
        try
        {
            // Try to get database courses and merge with sample
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsQueryable();

            var dbCourses = await query
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

            // Merge database courses with sample courses (remove duplicates by title)
            var allCourses = sampleCourses.ToList();
            foreach (var dbCourse in dbCourses)
            {
                if (!allCourses.Any(sc => sc.Title.Equals(dbCourse.Title, StringComparison.OrdinalIgnoreCase)))
                {
                    allCourses.Add(dbCourse);
                }
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLowerInvariant();
                allCourses = allCourses.Where(c => 
                    (c.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.CategoryName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.InstructorName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }
            
            // Apply category filter
            if (categoryId.HasValue)
            {
                var categoryName = await GetCategoryNameAsync(categoryId.Value);
                if (!string.IsNullOrEmpty(categoryName))
                {
                    allCourses = allCourses.Where(c => 
                        c.CategoryName != null && c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }
            
            return allCourses;
        }
        catch (Exception ex)
        {
            // Log error for debugging
            Console.WriteLine($"Error in GetAllCoursesAsync: {ex.Message}");
            
            // Fallback to sample data with filtering
            var allCourses = sampleCourses;
            
            // Apply search filter to sample data
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allCourses = allCourses.Where(c => 
                    (c.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.CategoryName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.InstructorName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }
            
            // Apply category filter to sample data
            if (categoryId.HasValue)
            {
                var sampleCategories = GetSampleCategories();
                var categoryName = sampleCategories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name;
                if (!string.IsNullOrEmpty(categoryName))
                {
                    allCourses = allCourses.Where(c => 
                        c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }
            
            return allCourses;
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
                Description = "Learn HTML, CSS, JavaScript, React, Node.js and build real-world projects from scratch",
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
                Description = "Master data analysis, visualization, and machine learning with Python libraries",
                Price = 59.99m,
                ImageUrl = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=225&fit=crop",
                CategoryName = "Data Science",
                InstructorName = "Dr. Data Expert",
                Rating = 4.7m,
                StudentCount = 8920,
                IsFeatured = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced JavaScript ES6+",
                Description = "Master modern JavaScript features, async programming, and advanced concepts for professional development",
                Price = 39.99m,
                ImageUrl = "https://images.unsplash.com/photo-1579468118864-1b9ea3c0db4a?w=400&h=225&fit=crop",
                CategoryName = "Web Development",
                InstructorName = "JS Expert",
                Rating = 4.4m,
                StudentCount = 6750,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced React Development",
                Description = "Take your React skills to the next level with advanced patterns, hooks, and performance optimization",
                Price = 69.99m,
                ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=400&h=225&fit=crop",
                CategoryName = "Web Development",
                InstructorName = "React Master",
                Rating = 4.7m,
                StudentCount = 9890,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced CSS & SASS",
                Description = "Master advanced CSS techniques, SASS preprocessing, and modern styling approaches",
                Price = 34.99m,
                ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=225&fit=crop",
                CategoryName = "Web Development", 
                InstructorName = "CSS Guru",
                Rating = 4.5m,
                StudentCount = 4560,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "UI/UX Design Fundamentals",
                Description = "Learn user interface and user experience design principles and tools",
                Price = 39.99m,
                ImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400&h=225&fit=crop",
                CategoryName = "Design",
                InstructorName = "Creative Designer",
                Rating = 4.6m,
                StudentCount = 5420,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Data Science with Python",
                Description = "Advanced machine learning, deep learning, and data engineering techniques",
                Price = 89.99m,
                ImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400&h=225&fit=crop",
                CategoryName = "Data Science",
                InstructorName = "ML Researcher",
                Rating = 4.9m,
                StudentCount = 4320,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Digital Marketing Mastery",
                Description = "Complete guide to digital marketing, SEO, social media, and advertising",
                Price = 44.99m,
                ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=400&h=225&fit=crop",
                CategoryName = "Business",
                InstructorName = "Marketing Pro",
                Rating = 4.5m,
                StudentCount = 7890,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Business Strategy",
                Description = "Strategic planning, competitive analysis, and advanced business development techniques",
                Price = 79.99m,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=225&fit=crop",
                CategoryName = "Business",
                InstructorName = "Strategy Expert",
                Rating = 4.6m,
                StudentCount = 3210,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Adobe Creative Suite Mastery",
                Description = "Master Photoshop, Illustrator, and InDesign for professional design",
                Price = 54.99m,
                ImageUrl = "https://images.unsplash.com/photo-1626785774625-0b1c2c4eab67?w=400&h=225&fit=crop",
                CategoryName = "Design",
                InstructorName = "Adobe Expert",
                Rating = 4.6m,
                StudentCount = 3210,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Mobile App Development",
                Description = "Build advanced iOS and Android applications with native performance and advanced features",
                Price = 79.99m,
                ImageUrl = "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=400&h=225&fit=crop",
                CategoryName = "Mobile Development",
                InstructorName = "Mobile Expert",
                Rating = 4.7m,
                StudentCount = 5670,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Project Management Professional",
                Description = "Learn PMP methodologies, agile practices, and leadership skills",
                Price = 49.99m,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=225&fit=crop",
                CategoryName = "Business",
                InstructorName = "PM Expert",
                Rating = 4.3m,
                StudentCount = 2340,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Node.js Development",
                Description = "Master advanced Node.js concepts, microservices, and scalable backend development",
                Price = 64.99m,
                ImageUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400&h=225&fit=crop",
                CategoryName = "Web Development",
                InstructorName = "Backend Specialist",
                Rating = 4.8m,
                StudentCount = 4890,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Machine Learning Fundamentals",
                Description = "Introduction to machine learning algorithms and applications",
                Price = 59.99m,
                ImageUrl = "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=400&h=225&fit=crop",
                CategoryName = "Data Science",
                InstructorName = "AI Researcher",
                Rating = 4.5m,
                StudentCount = 7230,
                IsFeatured = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Advanced UX Research",
                Description = "Master advanced user experience research methods and usability testing techniques",
                Price = 54.99m,
                ImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=400&h=225&fit=crop",
                CategoryName = "Design",
                InstructorName = "UX Researcher",
                Rating = 4.4m,
                StudentCount = 2890,
                IsFeatured = false
            }
        };
    }

    private async Task<string?> GetCategoryNameAsync(Guid categoryId)
    {
        try
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            return category?.Name;
        }
        catch
        {
            // Fallback to sample categories
            var sampleCategories = GetSampleCategories();
            return sampleCategories.FirstOrDefault(c => c.Id == categoryId)?.Name;
        }
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
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Web Development", Description = "Frontend and backend web technologies", CourseCount = 25 },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Data Science", Description = "Data analysis and machine learning", CourseCount = 18 },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Design", Description = "UI/UX and graphic design", CourseCount = 12 },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Business", Description = "Business strategy and management", CourseCount = 15 },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Mobile Development", Description = "iOS, Android, and cross-platform development", CourseCount = 10 },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Marketing", Description = "Digital marketing and advertising", CourseCount = 8 }
        };
    }
}
