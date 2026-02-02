namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class CourseDetailViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = null!;
    public string InstructorName { get; set; } = null!;
    public bool IsEnrolled { get; set; }
    public IEnumerable<ModuleViewModel> Modules { get; set; } = new List<ModuleViewModel>();
}

public class ModuleViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public IEnumerable<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}

public class LessonViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsCurrent { get; set; }
}

public class CourseLearnViewModel
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public LessonViewModel? CurrentLesson { get; set; }
    public IEnumerable<ModuleViewModel> Modules { get; set; } = new List<ModuleViewModel>();
}
