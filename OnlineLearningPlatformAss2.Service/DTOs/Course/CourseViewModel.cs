namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class CourseViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = null!;
    public string InstructorName { get; set; } = null!;
    public bool IsEnrolled { get; set; }
}
