using Online_Learning_Platform_Ass1.Service.DTOs.Assessment;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;

public interface ILearningPathRecommendationService
{
    Task<IEnumerable<LearningPathRecommendationDto>> GenerateRecommendationsAsync(Guid assessmentId);
}
