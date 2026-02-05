using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Data.Database.Entities;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.Service.Services;

public class AdminService : IAdminService
{
    private readonly OnlineLearningContext _context;

    public AdminService(OnlineLearningContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var stats = new AdminStatsDto();
        stats.TotalUsers = await _context.Users.CountAsync();
        stats.TotalInstructors = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Instructor");
        stats.TotalCourses = await _context.Courses.CountAsync();
        stats.PendingCourses = await _context.Courses.CountAsync(c => c.Status == "Pending");
        stats.TotalEnrollments = await _context.Enrollments.CountAsync();
        stats.TotalRevenue = await _context.Orders.Where(o => o.Status == "Completed").SumAsync(o => o.TotalAmount);

        stats.RecentOrders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Course)
            .Include(o => o.LearningPath)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.Id,
                Username = o.User.Username,
                ItemTitle = o.Course != null ? o.Course.Title : (o.LearningPath != null ? o.LearningPath.Title : "Bundle"),
                Amount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return stats;
    }

    public async Task<IEnumerable<CourseViewModel>> GetPendingCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.Status == "Pending")
            .Select(c => new CourseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Price = c.Price,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                ImageUrl = c.ImageUrl,
                Status = c.Status,
                RejectionReason = c.RejectionReason
            })
            .ToListAsync();
    }

    public async Task<bool> ApproveCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        course.Status = "Published";
        course.RejectionReason = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectCourseAsync(Guid courseId, string reason)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        course.Status = "Rejected";
        course.RejectionReason = reason;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SuspendCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        course.Status = "Suspended";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsuspendCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        course.Status = "Published";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CourseViewModel>> GetAllCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Select(c => new CourseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Price = c.Price,
                CategoryName = c.Category.Name,
                InstructorName = c.Instructor.Username,
                ImageUrl = c.ImageUrl,
                Status = c.Status,
                StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string searchTerm = null)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(searchTerm) || 
                                     u.Email.ToLower().Contains(searchTerm));
        }

        return await query
            .OrderByDescending(u => u.CreateAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                RoleName = u.Role != null ? u.Role.Name : "User",
                IsActive = u.IsActive,
                CreatedAt = u.CreateAt
            })
            .ToListAsync();
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return false;

        user.RoleId = role.Id;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.Username == "admin") return false;

        // Safety check: Cannot delete instructor with active students
        if (user.Role?.Name == "Instructor")
        {
            var hasStudents = await _context.Enrollments.AnyAsync(e => e.Course.InstructorId == userId);
            if (hasStudents) return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddInternalUserAsync(string username, string email, string password, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return false;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = password, // In a real app, hash this properly.
            RoleId = role.Id,
            CreateAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
