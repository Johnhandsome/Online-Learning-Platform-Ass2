using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.Course;
using Online_Learning_Platform_Ass1.Service.DTOs.Category;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;
using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class CourseService(
    ICourseRepository courseRepository,
    IEnrollmentRepository enrollmentRepository,
    ICategoryRepository categoryRepository,
    ILessonProgressService lessonProgressService) : ICourseService
{
    public async Task<IEnumerable<CourseViewModel>> GetFeaturedCoursesAsync()
    {
        var courses = await courseRepository.GetFeaturableCoursesAsync(6); // Get top 6
        return courses.Select(c => new CourseViewModel
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Price = c.Price,
            ImageUrl = c.ImageUrl,
            InstructorName = c.Instructor.Username, // Assuming Username or Profile Name
            CategoryName = c.Category.Name
        });
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null)
    {
        var courses = await courseRepository.GetCoursesAsync(searchTerm, categoryId);
        return courses.Select(c => new CourseViewModel
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Price = c.Price,
            ImageUrl = c.ImageUrl,
            InstructorName = c.Instructor.Username,
            CategoryName = c.Category.Name
        });
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllCategoriesAsync();
        return categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name
        });
    }

    public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null)
    {
        var course = await courseRepository.GetByIdAsync(id);
        if (course == null) return null;

        var isEnrolled = false;
        if (userId.HasValue)
        {
            isEnrolled = await enrollmentRepository.IsEnrolledAsync(userId.Value, id);
        }

        return new CourseDetailViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            ImageUrl = course.ImageUrl,
            InstructorName = course.Instructor.Username,
            CategoryName = course.Category.Name,
            IsEnrolled = isEnrolled,
            Modules = course.Modules.Select(m => new ModuleViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Lessons = m.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    VideoUrl = l.ContentUrl // Map ContentUrl to VideoUrl
                })
            })
        };
    }
    public async Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId)
    {
        var enrollments = await enrollmentRepository.GetStudentEnrollmentsAsync(userId);
        return enrollments.Select(e => new CourseViewModel
        {
            Id = e.Course.Id,
            EnrollmentId = e.Id,
            Title = e.Course.Title,
            Description = e.Course.Description,
            Price = e.Course.Price,
            ImageUrl = e.Course.ImageUrl,
            InstructorName = e.Course.Instructor?.Username ?? "Unknown",
            CategoryName = e.Course.Category?.Name ?? "General"
        });
    }

    public async Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            return null;

        var progresses = await lessonProgressService
            .GetByEnrollmentAsync(enrollmentId);

        var progressMap = progresses
            .ToDictionary(p => p.LessonId);

        return new CourseLearnViewModel
        {
            Id = enrollment.Course.Id,
            EnrollmentId = enrollment.Id,
            Title = enrollment.Course.Title,
            Description = enrollment.Course.Description,
            ImageUrl = enrollment.Course.ImageUrl,
            InstructorName = enrollment.Course.Instructor?.Username ?? "",
            CategoryName = enrollment.Course.Category?.Name ?? "",

            Modules = enrollment.Course.Modules
                .OrderBy(m => m.OrderIndex)
                .Select(m => new ModuleViewModel
                {
                    Id = m.Id,
                    Title = m.Title,

                    Lessons = m.Lessons
                        .OrderBy(l => l.OrderIndex)
                        .Select(l => new LessonViewModel
                        {
                            Id = l.Id,
                            Title = l.Title,
                            Duration = l.Duration ?? 0,
                            VideoUrl = l.Type == "video" ? l.ContentUrl : null,
                            Content = l.Type == "text" ? l.ContentUrl : null,
                            IsCompleted =
                                progressMap.TryGetValue(l.Id, out var p)
                                && p.IsCompleted
                        }).ToList()
                }).ToList()
        };
    }
}
