using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;
public class LessonProgressRepository (OnlineLearningContext _context) : ILessonProgressRepository
{
    public async Task<LessonProgress?> GetAsync(Guid enrollmentId, Guid lessonId)
    {
        return await _context.LessonProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == enrollmentId &&
                p.LessonId == lessonId);
    }

    public async Task<IEnumerable<LessonProgress>> GetByEnrollmentAsync(Guid enrollmentId)
    {
        return await _context.LessonProgresses
            .AsNoTracking()
            .Where(p => p.EnrollmentId == enrollmentId)
            .ToListAsync();
    }

    public async Task UpsertAsync(LessonProgress progress)
    {
        var existing = await _context.LessonProgresses
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == progress.EnrollmentId &&
                p.LessonId == progress.LessonId);

        if (existing == null)
        {
            progress.Id = Guid.NewGuid();
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            existing.IsCompleted = progress.IsCompleted;
            existing.Transcript = progress.Transcript;
            existing.AiSummary = progress.AiSummary;
            existing.AiSummaryStatus = progress.AiSummaryStatus;
            existing.LastWatchedPosition = progress.LastWatchedPosition;
            _context.LessonProgresses.Update(existing);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LessonProgress progress)
    {
        _context.LessonProgresses.Update(progress);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(LessonProgress progress)
    {
        _context.LessonProgresses.Add(progress);
        await _context.SaveChangesAsync();
    }
}
