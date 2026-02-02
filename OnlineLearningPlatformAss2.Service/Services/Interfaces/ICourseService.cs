using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.DTOs.Category;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseViewModel>> GetFeaturedCoursesAsync();
    Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync(string? searchTerm = null, Guid? categoryId = null);
    Task<CourseDetailViewModel?> GetCourseDetailsAsync(Guid id, Guid? userId = null);
    Task<IEnumerable<CourseViewModel>> GetEnrolledCoursesAsync(Guid userId);
    Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync();
    Task<CourseLearnViewModel?> GetCourseLearnAsync(Guid enrollmentId);
}
