using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using OnlineLearningPlatformAss2.Data.Database;
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
        var path = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course)
            .ThenInclude(c => c.Category)
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course.Instructor)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (path == null)
            return null;

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

    public async Task<IEnumerable<LearningPathViewModel>> GetFeaturedLearningPathsAsync()
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

        return paths;
    }

    public async Task<IEnumerable<LearningPathViewModel>> GetPublishedPathsAsync()
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

        return paths;
    }

    public async Task<IEnumerable<UserLearningPathWithProgressDto>> GetUserEnrolledPathsAsync(Guid userId)
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
                CompletedCourses = 0,
                Progress = 0m
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<UserLearningPathWithProgressDto?> GetUserPathProgressAsync(Guid userId, Guid pathId)
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
            CompletedCourses = 0,
            Progress = 0m
        };
    }

    public async Task<LearningPathDetailsWithProgressDto?> GetPathDetailsWithProgressAsync(Guid pathId, Guid? userId = null)
    {
        var path = await _context.LearningPaths
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course)
            .ThenInclude(c => c.Category)
            .Include(lp => lp.PathCourses)
            .ThenInclude(pc => pc.Course.Instructor)
            .FirstOrDefaultAsync(lp => lp.Id == pathId);

        if (path == null)
            return null;

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
            Progress = 0m,
            WhatYouWillLearn = new List<string>(),
            Prerequisites = new List<string>(),
            Courses = path.PathCourses.OrderBy(pc => pc.OrderIndex).Select((pc, index) => new PathCourseWithProgressDto
            {
                CourseId = pc.Course.Id,
                Title = pc.Course.Title,
                Description = pc.Course.Description,
                ImageUrl = pc.Course.ImageUrl,
                OrderIndex = pc.OrderIndex,
                InstructorName = pc.Course.Instructor.Username,
                Duration = 180,
                Level = "All Levels",
                IsCompleted = false,
                IsCurrentCourse = false,
                IsLocked = !isEnrolled,
                Progress = 0m
            }).ToList()
        };
    }
}
