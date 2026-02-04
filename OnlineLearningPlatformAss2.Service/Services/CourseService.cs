using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;
using OnlineLearningPlatformAss2.Data.Database;
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
                Rating = 4.5m,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id),
                IsFeatured = true
            })
            .ToListAsync();

        return courses;
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => 
                c.Title.Contains(searchTerm) ||
                c.Description.Contains(searchTerm) ||
                c.Category.Name.Contains(searchTerm) ||
                c.Instructor.Username.Contains(searchTerm)
            );
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
                Rating = 4.5m,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id),
                IsFeatured = false
            })
            .ToListAsync();

        return courses;
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return null;

        var isEnrolled = false;
        if (userId.HasValue)
        {
            isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId.Value && e.CourseId == id);
        }

        var studentCount = await _context.Enrollments.CountAsync(e => e.CourseId == id);

        return new CourseDetailViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            ImageUrl = course.ImageUrl,
            CategoryName = course.Category.Name,
            InstructorName = course.Instructor.Username,
            Rating = 4.5m,
            ReviewCount = 0,
            StudentCount = studentCount,
            Level = "All Levels",
            Language = "English",
            IsEnrolled = isEnrolled,
            WhatYouWillLearn = new List<string>(),
            Requirements = new List<string>(),
            HasCertificate = true,
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
                    Duration = 15,
                    OrderIndex = l.OrderIndex,
                    IsPreview = l.OrderIndex <= 2
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList()
        };
    }

    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
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
                StudentCount = _context.Enrollments.Count(en => en.CourseId == e.CourseId),
                EnrollmentDate = e.EnrolledAt,
                Progress = 0
            })
            .ToListAsync();

        return enrolledCourses;
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
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

        return categories;
    }

    public async Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId)
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
            Progress = 0,
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
}
