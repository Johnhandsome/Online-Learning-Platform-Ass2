namespace Online_Learning_Platform_Ass1.Service.DTOs.Lesson;

public record LessonProgressDto
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid LessonId { get; init; }
    public bool IsCompleted { get; init; }
    public int? LastWatchedPosition { get; init; }
    public string? Transcript { get; init; }
    public string? AiSummary { get; init; }
    public string? AiSummaryStatus { get; init; }
}
