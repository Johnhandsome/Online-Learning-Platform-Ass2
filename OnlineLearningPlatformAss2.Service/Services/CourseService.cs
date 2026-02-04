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
            .Include(c => c.Reviews)
            .ThenInclude(r => r.User)
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
        var averageRating = course.Reviews.Any() ? (decimal)course.Reviews.Average(r => r.Rating) : 4.5m;

        return new CourseDetailViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            ImageUrl = course.ImageUrl,
            CategoryName = course.Category.Name,
            InstructorName = course.Instructor.Username,
            Rating = averageRating,
            ReviewCount = course.Reviews.Count,
            StudentCount = studentCount,
            Level = "All Levels",
            Language = "English",
            IsEnrolled = isEnrolled,
            WhatYouWillLearn = new List<string> { "Foundational concepts", "Real-world projects", "Best practices" },
            Requirements = new List<string> { "Basic knowledge of the field" },
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
            }).OrderBy(m => m.OrderIndex).ToList(),
            Reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Username = r.User.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> SubmitReviewAsync(Guid userId, SubmitReviewDto reviewDto)
    {
        // Check if enrolled
        var isEnrolled = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == reviewDto.CourseId);
        if (!isEnrolled) return false;

        // Check if already reviewed
        var existingReview = await _context.CourseReviews.FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == reviewDto.CourseId);
        if (existingReview != null)
        {
            existingReview.Rating = reviewDto.Rating;
            existingReview.Comment = reviewDto.Comment;
            existingReview.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.CourseReviews.Add(new CourseReview
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = reviewDto.CourseId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId)
    {
        return await _context.CourseReviews
            .Include(r => r.User)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Username = r.User.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Category)
            .Include(e => e.Course.Instructor)
            .Include(e => e.LessonProgresses)
            .Include(e => e.Course.Modules)
            .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var enrolledCourses = enrollments.Select(e => {
            var totalLessons = e.Course.Modules.Sum(m => m.Lessons.Count);
            var completedLessons = e.LessonProgresses.Count(lp => lp.IsCompleted);
            var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

            return new CourseViewModel
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
                Progress = progress
            };
        });

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
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null)
            return null;

        var totalLessons = enrollment.Course.Modules.Sum(m => m.Lessons.Count);
        var completedLessons = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);
        var progress = totalLessons > 0 ? (int)((decimal)completedLessons / totalLessons * 100) : 0;

        return new CourseLearnViewModel
        {
            EnrollmentId = enrollment.Id,
            CourseId = enrollment.Course.Id,
            CourseTitle = enrollment.Course.Title,
            CurrentLessonId = enrollment.Course.Modules.FirstOrDefault()?.Lessons.FirstOrDefault()?.Id,
            Progress = progress,
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
                    IsCompleted = enrollment.LessonProgresses.Any(lp => lp.LessonId == l.Id && lp.IsCompleted)
                }).OrderBy(l => l.OrderIndex).ToList()
            }).OrderBy(m => m.OrderIndex).ToList()
        };
    }

    public async Task<bool> UpdateLessonProgressAsync(Guid enrollmentId, Guid lessonId, bool isCompleted)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null) return false;

        var progress = enrollment.LessonProgresses.FirstOrDefault(p => p.LessonId == lessonId);
        if (progress == null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                IsCompleted = isCompleted,
                LastAccessedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = isCompleted;
            progress.LastAccessedAt = DateTime.UtcNow;
        }

        // Check if all lessons are completed to update enrollment status
        var course = await _context.Courses
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == enrollment.CourseId);

        if (course != null)
        {
            var totalLessons = course.Modules.Sum(m => m.Lessons.Count);
            var completedCount = enrollment.LessonProgresses.Count(p => p.IsCompleted) + (isCompleted && !enrollment.LessonProgresses.Any(p => p.LessonId == lessonId && p.IsCompleted) ? 1 : 0);
            
            if (completedCount >= totalLessons)
            {
                enrollment.Status = "Completed";
                enrollment.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Guid?> GetEnrollmentIdAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        return enrollment?.Id;
    }
}
