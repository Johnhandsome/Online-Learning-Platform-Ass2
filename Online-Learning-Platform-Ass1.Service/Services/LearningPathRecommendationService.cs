using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
using Online_Learning_Platform_Ass1.Service.DTOs.Assessment;
using Online_Learning_Platform_Ass1.Service.DTOs.Course;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;

public class LearningPathRecommendationService(
    IUserAssessmentRepository assessmentRepository,
    ICourseRepository courseRepository) : ILearningPathRecommendationService
{
    public async Task<IEnumerable<LearningPathRecommendationDto>> GenerateRecommendationsAsync(Guid assessmentId)
    {
        var assessment = await assessmentRepository.GetAssessmentWithAnswersAsync(assessmentId);
        if (assessment == null)
        {
            return Enumerable.Empty<LearningPathRecommendationDto>();
        }

        // Analyze answers to determine user preferences
        var categoryInterests = new Dictionary<Guid, int>();
        var skillLevelCounts = new Dictionary<string, int>();

        foreach (var answer in assessment.Answers)
        {
            // Count category interests
            if (answer.Question.CategoryId.HasValue)
            {
                var categoryId = answer.Question.CategoryId.Value;
                categoryInterests[categoryId] = categoryInterests.GetValueOrDefault(categoryId, 0) + 1;
            }

            // Count skill levels
            if (!string.IsNullOrEmpty(answer.SelectedOption.SkillLevel))
            {
                var level = answer.SelectedOption.SkillLevel;
                skillLevelCounts[level] = skillLevelCounts.GetValueOrDefault(level, 0) + 1;
            }
        }

        // Determine primary skill level
        var primarySkillLevel = skillLevelCounts.OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault().Key ?? "Beginner";

        // Get all courses
        var allCourses = await courseRepository.GetAllAsync();

        // Generate recommendations based on top categories
        var recommendations = new List<LearningPathRecommendationDto>();
        var topCategories = categoryInterests.OrderByDescending(kvp => kvp.Value).Take(3);

        foreach (var categoryInterest in topCategories)
        {
            var categoryId = categoryInterest.Key;
            var categoryCourses = allCourses
                .Where(c => c.CategoryId == categoryId && c.Status == "published")
                .Take(5)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    InstructorName = c.Instructor?.Username ?? "Unknown",
                    CategoryName = c.Category?.Name ?? "General"
                })
                .ToList();

            if (categoryCourses.Any())
            {
                var categoryName = categoryCourses.First().CategoryName;
                recommendations.Add(new LearningPathRecommendationDto
                {
                    RecommendationTitle = $"Lộ trình {categoryName}",
                    RecommendationReason = $"Dựa trên sở thích của bạn về {categoryName}",
                    SkillLevel = primarySkillLevel,
                    RecommendedCourses = categoryCourses
                });
            }
        }

        // If no category-based recommendations, provide general beginner path
        if (!recommendations.Any())
        {
            var generalCourses = allCourses
                .Where(c => c.Status == "published")
                .Take(5)
                .Select(c => new CourseViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    InstructorName = c.Instructor?.Username ?? "Unknown",
                    CategoryName = c.Category?.Name ?? "General"
                })
                .ToList();

            recommendations.Add(new LearningPathRecommendationDto
            {
                RecommendationTitle = "Lộ trình Khám phá",
                RecommendationReason = "Các khóa học phổ biến để bắt đầu",
                SkillLevel = primarySkillLevel,
                RecommendedCourses = generalCourses
            });
        }

        return recommendations;
    }
}
