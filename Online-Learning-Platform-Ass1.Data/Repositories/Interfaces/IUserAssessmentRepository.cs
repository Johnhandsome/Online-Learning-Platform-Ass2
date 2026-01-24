using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

public interface IUserAssessmentRepository
{
    Task<UserAssessment> CreateAssessmentAsync(UserAssessment assessment);
    Task<UserAssessment?> GetUserLatestAssessmentAsync(Guid userId);
    Task<UserAssessment?> GetAssessmentWithAnswersAsync(Guid assessmentId);
    Task<int> SaveChangesAsync();
}
