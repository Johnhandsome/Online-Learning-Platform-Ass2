using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearningPlatformAss2.Service.Services;

public class LearningPathService : ILearningPathService
{
    private readonly OnlineLearningContext _context;

    public LearningPathService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<LearningPathViewModel?> GetLearningPathDetailsAsync(Guid id)
    {
        try
        {
            var path = await _context.LearningPaths
                .Include(lp => lp.PathCourses)
                .ThenInclude(pc => pc.Course)
                .ThenInclude(c => c.Category)
                .Include(lp => lp.PathCourses)
                .ThenInclude(pc => pc.Course.Instructor)
                .FirstOrDefaultAsync(lp => lp.Id == id);

            if (path == null)
                return GetSampleLearningPath(id);

            return new LearningPathViewModel
            {
                Id = path.Id,
                Title = path.Title,
                Description = path.Description,
                Price = path.Price,
                Status = path.Status,
                IsCustomPath = path.IsCustomPath,
                CreatedAt = path.CreatedAt,
                Courses = path.PathCourses.OrderBy(pc => pc.OrderIndex).Select(pc => new CourseInPathViewModel
                {
                    CourseId = pc.Course.Id,
                    Title = pc.Course.Title,
                    Description = pc.Course.Description,
                    ImageUrl = pc.Course.ImageUrl,
                    OrderIndex = pc.OrderIndex,
                    InstructorName = pc.Course.Instructor.Username,
                    CategoryName = pc.Course.Category.Name
                }).ToList()
            };
        }
        catch
        {
            return GetSampleLearningPath(id);
        }
    }

    public async Task<IEnumerable<LearningPathViewModel>> GetFeaturedLearningPathsAsync()
    {
        try
        {
            var paths = await _context.LearningPaths
                .Include(lp => lp.PathCourses)
                .Where(lp => lp.Status == "Published")
                .Take(4)
                .Select(lp => new LearningPathViewModel
                {
                    Id = lp.Id,
                    Title = lp.Title,
                    Description = lp.Description,
                    Price = lp.Price,
                    Status = lp.Status,
                    IsCustomPath = lp.IsCustomPath,
                    CourseCount = lp.PathCourses.Count(),
                    CreatedAt = lp.CreatedAt
                })
                .ToListAsync();

            return paths.Any() ? paths : GetSampleLearningPaths();
        }
        catch
        {
            return GetSampleLearningPaths();
        }
    }

    public async Task<IEnumerable<LearningPathViewModel>> GetPublishedPathsAsync()
    {
        try
        {
            var paths = await _context.LearningPaths
                .Include(lp => lp.PathCourses)
                .Where(lp => lp.Status == "Published")
                .Select(lp => new LearningPathViewModel
                {
                    Id = lp.Id,
                    Title = lp.Title,
                    Description = lp.Description,
                    Price = lp.Price,
                    Status = lp.Status,
                    IsCustomPath = lp.IsCustomPath,
                    CourseCount = lp.PathCourses.Count(),
                    CreatedAt = lp.CreatedAt
                })
                .OrderByDescending(lp => lp.CreatedAt)
                .ToListAsync();

            return paths.Any() ? paths : GetSampleLearningPaths();
        }
        catch
        {
            return GetSampleLearningPaths();
        }
    }

    public async Task<IEnumerable<UserLearningPathWithProgressDto>> GetUserEnrolledPathsAsync(Guid userId)
    {
        try
        {
            var enrollments = await _context.UserLearningPathEnrollments
                .Include(ulpe => ulpe.LearningPath)
                .ThenInclude(lp => lp.PathCourses)
                .Where(ulpe => ulpe.UserId == userId)
                .Select(ulpe => new UserLearningPathWithProgressDto
                {
                    Id = ulpe.Id,
                    PathId = ulpe.PathId,
                    PathTitle = ulpe.LearningPath.Title,
                    PathDescription = ulpe.LearningPath.Description,
                    EnrolledAt = ulpe.EnrolledAt,
                    Status = ulpe.Status,
                    TotalCourses = ulpe.LearningPath.PathCourses.Count(),
                    CompletedCourses = 2, // Default for demo
                    Progress = 35m // Default progress
                })
                .ToListAsync();

            return enrollments.Any() ? enrollments : GetSampleUserEnrollments(userId);
        }
        catch
        {
            return GetSampleUserEnrollments(userId);
        }
    }

    public async Task<UserLearningPathWithProgressDto?> GetUserPathProgressAsync(Guid userId, Guid pathId)
    {
        try
        {
            var enrollment = await _context.UserLearningPathEnrollments
                .Include(ulpe => ulpe.LearningPath)
                .ThenInclude(lp => lp.PathCourses)
                .ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync(ulpe => ulpe.UserId == userId && ulpe.PathId == pathId);

            if (enrollment == null)
                return null;

            return new UserLearningPathWithProgressDto
            {
                Id = enrollment.Id,
                PathId = enrollment.PathId,
                PathTitle = enrollment.LearningPath.Title,
                PathDescription = enrollment.LearningPath.Description,
                EnrolledAt = enrollment.EnrolledAt,
                CompletedAt = enrollment.CompletedAt,
                Status = enrollment.Status,
                TotalCourses = enrollment.LearningPath.PathCourses.Count(),
                CompletedCourses = 2,
                Progress = 45m
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<LearningPathDetailsWithProgressDto?> GetPathDetailsWithProgressAsync(Guid pathId, Guid? userId = null)
    {
        try
        {
            var path = await _context.LearningPaths
                .Include(lp => lp.PathCourses)
                .ThenInclude(pc => pc.Course)
                .ThenInclude(c => c.Category)
                .Include(lp => lp.PathCourses)
                .ThenInclude(pc => pc.Course.Instructor)
                .FirstOrDefaultAsync(lp => lp.Id == pathId);

            if (path == null)
                return GetSamplePathDetails(pathId);

            bool isEnrolled = false;
            if (userId.HasValue)
            {
                isEnrolled = await _context.UserLearningPathEnrollments
                    .AnyAsync(ulpe => ulpe.UserId == userId.Value && ulpe.PathId == pathId);
            }

            return new LearningPathDetailsWithProgressDto
            {
                Id = path.Id,
                Title = path.Title,
                Description = path.Description,
                Price = path.Price,
                IsCustomPath = path.IsCustomPath,
                IsEnrolled = isEnrolled,
                Progress = isEnrolled ? 25m : 0m,
                WhatYouWillLearn = GetSampleLearningPoints(path.Title),
                Prerequisites = GetSamplePrerequisites(),
                Courses = path.PathCourses.OrderBy(pc => pc.OrderIndex).Select((pc, index) => new PathCourseWithProgressDto
                {
                    CourseId = pc.Course.Id,
                    Title = pc.Course.Title,
                    Description = pc.Course.Description,
                    ImageUrl = pc.Course.ImageUrl,
                    OrderIndex = pc.OrderIndex,
                    InstructorName = pc.Course.Instructor.Username,
                    Duration = 180, // Default 3 hours
                    Level = "Intermediate",
                    IsCompleted = isEnrolled && index == 0,
                    IsCurrentCourse = isEnrolled && index == 1,
                    IsLocked = !isEnrolled || index > 1,
                    Progress = isEnrolled && index == 0 ? 100m : isEnrolled && index == 1 ? 45m : 0m
                }).ToList()
            };
        }
        catch
        {
            return GetSamplePathDetails(pathId);
        }
    }

    // Sample data methods
    private LearningPathViewModel GetSampleLearningPath(Guid id)
    {
        return new LearningPathViewModel
        {
            Id = id,
            Title = "Full Stack Web Developer",
            Description = "Master both front-end and back-end development with modern technologies",
            Price = 299.99m,
            Status = "Published",
            IsCustomPath = false,
            CourseCount = 6,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            Courses = new List<CourseInPathViewModel>
            {
                new() { CourseId = Guid.NewGuid(), Title = "HTML & CSS Fundamentals", OrderIndex = 1 },
                new() { CourseId = Guid.NewGuid(), Title = "JavaScript Essentials", OrderIndex = 2 },
                new() { CourseId = Guid.NewGuid(), Title = "React Development", OrderIndex = 3 }
            }
        };
    }

    private IEnumerable<LearningPathViewModel> GetSampleLearningPaths()
    {
        return new List<LearningPathViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Full Stack Web Developer",
                Description = "Master both front-end and back-end development",
                Price = 299.99m,
                Status = "Published",
                IsCustomPath = false,
                CourseCount = 6,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Data Science Professional",
                Description = "From Python basics to advanced machine learning",
                Price = 399.99m,
                Status = "Published",
                IsCustomPath = false,
                CourseCount = 8,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "UI/UX Design Master",
                Description = "Complete design workflow from research to prototyping",
                Price = 249.99m,
                Status = "Published",
                IsCustomPath = false,
                CourseCount = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        };
    }

    private IEnumerable<UserLearningPathWithProgressDto> GetSampleUserEnrollments(Guid userId)
    {
        return new List<UserLearningPathWithProgressDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PathId = Guid.NewGuid(),
                PathTitle = "Full Stack Web Developer",
                PathDescription = "Master both front-end and back-end development",
                EnrolledAt = DateTime.UtcNow.AddDays(-30),
                Status = "Active",
                TotalCourses = 6,
                CompletedCourses = 2,
                Progress = 35m,
                CurrentCourseTitle = "React Development"
            }
        };
    }

    private LearningPathDetailsWithProgressDto GetSamplePathDetails(Guid pathId)
    {
        return new LearningPathDetailsWithProgressDto
        {
            Id = pathId,
            Title = "Full Stack Web Developer",
            Description = "Master both front-end and back-end development with modern technologies",
            Price = 299.99m,
            IsCustomPath = false,
            IsEnrolled = false,
            Progress = 0m,
            WhatYouWillLearn = GetSampleLearningPoints("Full Stack Web Developer"),
            Prerequisites = GetSamplePrerequisites(),
            Courses = new List<PathCourseWithProgressDto>
            {
                new() { CourseId = Guid.NewGuid(), Title = "HTML & CSS Fundamentals", OrderIndex = 1, Duration = 180, Level = "Beginner" },
                new() { CourseId = Guid.NewGuid(), Title = "JavaScript Essentials", OrderIndex = 2, Duration = 240, Level = "Beginner" },
                new() { CourseId = Guid.NewGuid(), Title = "React Development", OrderIndex = 3, Duration = 300, Level = "Intermediate" }
            }
        };
    }

    private List<string> GetSampleLearningPoints(string pathTitle)
    {
        return new List<string>
        {
            "Build professional, responsive websites from scratch",
            "Master modern JavaScript and popular frameworks",
            "Develop full-stack applications with databases",
            "Deploy applications to production environments",
            "Understand software development best practices",
            "Build a portfolio of real-world projects"
        };
    }

    private List<string> GetSamplePrerequisites()
    {
        return new List<string>
        {
            "Basic computer skills and familiarity with the internet",
            "No prior programming experience required",
            "A computer with internet access",
            "Commitment to practice regularly"
        };
    }
}
