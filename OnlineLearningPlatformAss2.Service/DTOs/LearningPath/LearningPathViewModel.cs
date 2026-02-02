using OnlineLearningPlatformAss2.Service.DTOs.Course;

namespace OnlineLearningPlatformAss2.Service.DTOs.LearningPath;

public class LearningPathViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public IEnumerable<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();
}

public class UserLearningPathWithProgressDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string EnrollmentStatus { get; set; } = null!;
    public int TotalCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCustomPath { get; set; }
}

public class LearningPathDetailsWithProgressDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public bool IsEnrolled { get; set; }
    public int ProgressPercentage { get; set; }
    public IEnumerable<CourseViewModel> Courses { get; set; } = new List<CourseViewModel>();
}
