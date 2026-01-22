using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid enrollmentId);

    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(Guid userId);
    Task<bool> IsEnrolledAsync(Guid userId, Guid courseId);
}
