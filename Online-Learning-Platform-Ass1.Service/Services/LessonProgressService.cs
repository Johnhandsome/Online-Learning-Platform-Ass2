using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;
public class LessonProgressService(ILessonProgressRepository lessonProgressRepository) : ILessonProgressService
{
    private readonly ILessonProgressRepository _lessonProgressRepository = lessonProgressRepository;

    public Task<LessonProgress?> GetAsync(Guid enrollmentId, Guid lessonId)
        => _lessonProgressRepository.GetAsync(enrollmentId, lessonId);

    public Task<IEnumerable<LessonProgress>> GetByEnrollmentAsync(Guid enrollmentId)
        => _lessonProgressRepository.GetByEnrollmentAsync(enrollmentId);

    public async Task UpdateProgressAsync(
        Guid enrollmentId,
        Guid lessonId,
        int watchedSeconds,
        bool isCompleted
    )
    {
        var progress = await _lessonProgressRepository.GetAsync(enrollmentId, lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                EnrollmentId = enrollmentId,
                LessonId = lessonId,
                LastWatchedPosition = watchedSeconds, 
                IsCompleted = isCompleted,
            };
        }
        else
        {
            progress.LastWatchedPosition = watchedSeconds;
            progress.IsCompleted = isCompleted;
        }

        await _lessonProgressRepository.UpsertAsync(progress);
    }
}
