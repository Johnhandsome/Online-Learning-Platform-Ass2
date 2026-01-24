using Microsoft.EntityFrameworkCore;
using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class UserAssessmentRepository(OnlineLearningContext context) : IUserAssessmentRepository
{
    public async Task<UserAssessment> CreateAssessmentAsync(UserAssessment assessment)
    {
        await context.UserAssessments.AddAsync(assessment);
        await context.SaveChangesAsync();
        return assessment;
    }

    public async Task<UserAssessment?> GetUserLatestAssessmentAsync(Guid userId)
    {
        return await context.UserAssessments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CompletedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<UserAssessment?> GetAssessmentWithAnswersAsync(Guid assessmentId)
    {
        return await context.UserAssessments
            .AsNoTracking()
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
                    .ThenInclude(q => q.Category)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
