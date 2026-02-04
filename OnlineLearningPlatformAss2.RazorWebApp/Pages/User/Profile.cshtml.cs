using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.User;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.User;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IUserService _userService;

    public ProfileModel(IUserService userService)
    {
        _userService = userService;
    }

    public UserProfileDto? UserProfile { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        UserProfile = await _userService.GetUserProfileAsync(userId);
        
        if (UserProfile == null)
        {
            return NotFound();
        }

        return Page();
    }
}
