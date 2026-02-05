using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync();
    Task<IEnumerable<CourseViewModel>> GetPendingCoursesAsync();
    Task<bool> ApproveCourseAsync(Guid courseId);
    Task<bool> RejectCourseAsync(Guid courseId);
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string searchTerm = null);
    Task<bool> ToggleUserStatusAsync(Guid userId);
    Task<bool> ChangeUserRoleAsync(Guid userId, string roleName);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> AddInternalUserAsync(string username, string email, string password, string role);
}
