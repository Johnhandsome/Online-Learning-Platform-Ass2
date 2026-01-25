using Online_Learning_Platform_Ass1.Service.DTOs.Course;

namespace Online_Learning_Platform_Ass1.Service.DTOs.Order;

public class OrderPreviewDto
{
    public Guid? CourseId { get; set; }
    public Guid? PathId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // "Course" or "Learning Path"
    public decimal OriginalPrice { get; set; }
    public List<OwnedCourseDto> OwnedCourses { get; set; } = new();
    public decimal TotalDiscount { get; set; }
    public decimal FinalPrice { get; set; }
    public bool CanPurchase { get; set; }
    public string? BlockReason { get; set; }
}

public class OwnedCourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
