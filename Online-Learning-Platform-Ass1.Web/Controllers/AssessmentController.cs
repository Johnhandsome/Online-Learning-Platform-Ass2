using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Learning_Platform_Ass1.Service.DTOs.Assessment;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Web.Controllers;

[Authorize]
public class AssessmentController(
    IAssessmentService assessmentService,
    ILearningPathRecommendationService recommendationService,
    IUserService userService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Start()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
        {
            var hasCompleted = await userService.HasCompletedAssessmentAsync(userId);
            if (hasCompleted)
            {
                // Allow unlimited retakes - encourage users to update their preferences
                TempData["InfoMessage"] = "Welcome back! Retake the assessment anytime to get updated course recommendations.";
            }
        }
        
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Questions()
    {
        var questions = await assessmentService.GetAssessmentQuestionsAsync();
        return View(questions);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromForm] Dictionary<string, string> formData)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        // Parse form data into SubmitAssessmentDto
        var submitDto = new SubmitAssessmentDto();
        foreach (var kvp in formData)
        {
            if (kvp.Key.StartsWith("question_") && Guid.TryParse(kvp.Key.Replace("question_", ""), out var questionId))
            {
                if (Guid.TryParse(kvp.Value, out var optionId))
                {
                    submitDto.Answers[questionId] = optionId;
                }
            }
        }

        var assessmentId = await assessmentService.SubmitAssessmentAsync(userId, submitDto);
        return RedirectToAction(nameof(Recommendations), new { assessmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Recommendations(Guid assessmentId)
    {
        var recommendations = await recommendationService.GenerateRecommendationsAsync(assessmentId);
        return View(recommendations);
    }
}
