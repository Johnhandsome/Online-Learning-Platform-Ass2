namespace Online_Learning_Platform_Ass1.Service.DTOs.Assessment;

public class AssessmentOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public string? SkillLevel { get; set; }
    public int OrderIndex { get; set; }
}

public class AssessmentQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public string QuestionType { get; set; } = null!;
    public string? CategoryName { get; set; }
    public int OrderIndex { get; set; }
    public List<AssessmentOptionDto> Options { get; set; } = new();
}
