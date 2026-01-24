using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.Assessment;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class AssessmentService(
    IAssessmentQuestionRepository questionRepository,
    IUserAssessmentRepository assessmentRepository) : IAssessmentService
{
    public async Task<IEnumerable<AssessmentQuestionDto>> GetAssessmentQuestionsAsync()
    {
        var questions = await questionRepository.GetActiveQuestionsAsync();
        
        return questions.Select(q => new AssessmentQuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            CategoryName = q.Category?.Name,
            OrderIndex = q.OrderIndex,
            Options = q.Options.Select(o => new AssessmentOptionDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                SkillLevel = o.SkillLevel,
                OrderIndex = o.OrderIndex
            }).ToList()
        });
    }

    public async Task<Guid> SubmitAssessmentAsync(Guid userId, SubmitAssessmentDto dto)
    {
        var assessment = new UserAssessment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompletedAt = DateTime.UtcNow,
            Answers = dto.Answers.Select(kvp => new UserAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = kvp.Key,
                SelectedOptionId = kvp.Value
            }).ToList()
        };

        var created = await assessmentRepository.CreateAssessmentAsync(assessment);
        return created.Id;
    }
}
