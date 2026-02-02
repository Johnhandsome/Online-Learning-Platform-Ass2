using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.LearningPath;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.LearningPath;

public class DetailsModel : PageModel
{
    private readonly ILearningPathService _learningPathService;

    public DetailsModel(ILearningPathService learningPathService)
    {
        _learningPathService = learningPathService;
    }

    public LearningPathDetailsWithProgressDto? LearningPath { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsEnrolled { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        IsAuthenticated = User.Identity?.IsAuthenticated == true;
        
        Guid? userId = null;
        if (IsAuthenticated && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId))
        {
            userId = parsedUserId;
        }

        LearningPath = await _learningPathService.GetPathDetailsWithProgressAsync(id, userId);
        
        if (LearningPath == null)
        {
            return NotFound();
        }

        IsEnrolled = LearningPath.IsEnrolled;

        return Page();
    }

    public async Task<IActionResult> OnPostEnrollAsync(Guid id)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/User/Login");
        }

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        // For demo purposes, simulate enrollment
        TempData["SuccessMessage"] = "Successfully enrolled in the learning path!";
        
        // In a real application, you would create an enrollment record here
        
        return RedirectToPage("/LearningPath/MyPaths");
    }
}
