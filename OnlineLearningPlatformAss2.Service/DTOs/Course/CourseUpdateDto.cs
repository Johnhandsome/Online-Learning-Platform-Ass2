namespace OnlineLearningPlatformAss2.Service.DTOs.Course;

public class CourseUpdateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsFeatured { get; set; }
}
