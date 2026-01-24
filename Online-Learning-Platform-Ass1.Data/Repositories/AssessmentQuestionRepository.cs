using Microsoft.EntityFrameworkCore;
using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class AssessmentQuestionRepository(OnlineLearningContext context) : IAssessmentQuestionRepository
{
    public async Task<IEnumerable<AssessmentQuestion>> GetActiveQuestionsAsync()
    {
        return await context.AssessmentQuestions
            .AsNoTracking()
            .Where(q => q.IsActive)
            .Include(q => q.Options.OrderBy(o => o.OrderIndex))
            .Include(q => q.Category)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<AssessmentQuestion?> GetQuestionWithOptionsAsync(Guid id)
    {
        return await context.AssessmentQuestions
            .AsNoTracking()
            .Include(q => q.Options.OrderBy(o => o.OrderIndex))
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
}
