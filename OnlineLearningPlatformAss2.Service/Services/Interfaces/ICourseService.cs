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
    Task<bool> UpdateLessonProgressAsync(Guid enrollmentId, Guid lessonId, bool isCompleted);
    Task<Guid?> GetEnrollmentIdAsync(Guid userId, Guid courseId);
    Task<bool> SubmitReviewAsync(Guid userId, SubmitReviewDto review);
    Task<IEnumerable<ReviewViewModel>> GetCourseReviewsAsync(Guid courseId);
    Task<bool> EnrollUserAsync(Guid userId, Guid courseId);
    Task<bool> ToggleWishlistAsync(Guid userId, Guid courseId);
    Task<IEnumerable<CourseViewModel>> GetWishlistAsync(Guid userId);
    Task<IEnumerable<CourseViewModel>> GetInstructorCoursesAsync(Guid instructorId);
    Task<bool> UpdateCourseAsync(Guid courseId, CourseUpdateDto dto, Guid instructorId);
}
