using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

public interface IAssessmentQuestionRepository
{
    Task<IEnumerable<AssessmentQuestion>> GetActiveQuestionsAsync();
    Task<AssessmentQuestion?> GetQuestionWithOptionsAsync(Guid id);
}
