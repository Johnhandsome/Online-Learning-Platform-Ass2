using Online_Learning_Platform_Ass1.Service.DTOs.Course;

namespace Online_Learning_Platform_Ass1.Service.DTOs.Assessment;

public class LearningPathRecommendationDto
{
    public string RecommendationTitle { get; set; } = null!;
    public string RecommendationReason { get; set; } = null!;
    public string SkillLevel { get; set; } = null!;
    public List<CourseViewModel> RecommendedCourses { get; set; } = new();
}
