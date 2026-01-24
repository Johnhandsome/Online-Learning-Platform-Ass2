using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.Lesson;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class LessonProgressService(ILessonProgressRepository lessonProgressRepository) : ILessonProgressService
{
    private readonly ILessonProgressRepository _lessonProgressRepository = lessonProgressRepository;

    public async Task<LessonProgressDto?> GetAsync(Guid enrollmentId, Guid lessonId)
    {
        var progress = await _lessonProgressRepository.GetAsync(enrollmentId, lessonId);
        return progress == null ? null : MapToDto(progress);
    }

    public async Task<IEnumerable<LessonProgressDto>> GetByEnrollmentAsync(Guid enrollmentId)
    {
        var progressList = await _lessonProgressRepository.GetByEnrollmentAsync(enrollmentId);
        return progressList.Select(MapToDto);
    }

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

    private static LessonProgressDto MapToDto(LessonProgress entity)
    {
        return new LessonProgressDto
        {
            Id = entity.Id,
            EnrollmentId = entity.EnrollmentId,
            LessonId = entity.LessonId,
            IsCompleted = entity.IsCompleted,
            LastWatchedPosition = entity.LastWatchedPosition,
            Transcript = entity.Transcript,
            AiSummary = entity.AiSummary,
            AiSummaryStatus = entity.AiSummaryStatus.ToString()
        };
    }
}
