using Online_Learning_Platform_Ass1.Service.Enums;


namespace Online_Learning_Platform_Ass1.Service.DTOs.Course;


public record CourseViewModel
{
    public Guid Id { get; init; }
    public Guid? EnrollmentId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public string InstructorName { get; init; } = null!;
    public string CategoryName { get; init; } = null!;

}

public record CourseDetailViewModel : CourseViewModel
{
    public IEnumerable<ModuleViewModel> Modules { get; init; } = new List<ModuleViewModel>();
    public bool IsEnrolled { get; init; }
}

public record CourseLearnViewModel : CourseViewModel
{

    public IEnumerable<ModuleViewModel> Modules { get; init; } = new List<ModuleViewModel>();

    public LessonViewModel? CurrentLesson { get; set; }
}

public record ModuleViewModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public IEnumerable<LessonViewModel> Lessons { get; init; } = new List<LessonViewModel>();
}

public record LessonViewModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? VideoUrl { get; init; }
    public string? Content { get; init; }
    public int Duration { get; init; }
    public bool IsCurrent { get; set; }

    public bool IsCompleted { get; set; }

    // AI
    public AiSummaryStatus AiSummaryStatus { get; set; }
    public string? AiSummary { get; set; }
}
