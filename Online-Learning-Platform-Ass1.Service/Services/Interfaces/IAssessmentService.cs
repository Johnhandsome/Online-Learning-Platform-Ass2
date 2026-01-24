using Online_Learning_Platform_Ass1.Service.DTOs.Assessment;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;

public interface IAssessmentService
{
    Task<IEnumerable<AssessmentQuestionDto>> GetAssessmentQuestionsAsync();
    Task<Guid> SubmitAssessmentAsync(Guid userId, SubmitAssessmentDto dto);
}
