using Microsoft.EntityFrameworkCore;
using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class EnrollmentRepository(OnlineLearningContext context) : IEnrollmentRepository
{
    public async Task<Enrollment?> GetByIdAsync(Guid enrollmentId)
    {
        return await context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);
    }

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(Guid userId)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(Guid userId, Guid courseId)
    {
        return await context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.Status == "active");
    }
}
