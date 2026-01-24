namespace Online_Learning_Platform_Ass1.Service.DTOs.Assessment;

public class SubmitAssessmentDto
{
    public Dictionary<Guid, Guid> Answers { get; set; } = new();
    // Key: QuestionId, Value: SelectedOptionId
}
