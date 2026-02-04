using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatformAss2.Service.DTOs.Admin;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UserManagementModel : PageModel
{
    private readonly IAdminService _adminService;

    public UserManagementModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IEnumerable<AdminUserDto> Users { get; set; } = new List<AdminUserDto>();

    public async Task OnGetAsync()
    {
        Users = await _adminService.GetAllUsersAsync();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
    {
        var success = await _adminService.ToggleUserStatusAsync(id);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(Guid id, string role)
    {
        var success = await _adminService.ChangeUserRoleAsync(id, role);
        return new JsonResult(new { success });
    }
}
