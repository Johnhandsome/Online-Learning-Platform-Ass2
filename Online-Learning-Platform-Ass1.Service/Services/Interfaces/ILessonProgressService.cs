using Online_Learning_Platform_Ass1.Service.DTOs.Lesson;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;

public interface ILessonProgressService
{
    Task<LessonProgressDto?> GetAsync(Guid enrollmentId, Guid lessonId);
    Task<IEnumerable<LessonProgressDto>> GetByEnrollmentAsync(Guid enrollmentId);

    Task UpdateProgressAsync(
        Guid enrollmentId,
        Guid lessonId,
        int watchedSeconds,
        bool isCompleted
    );
}
