using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Online_Learning_Platform_Ass1.Service.DTOs.User;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Controllers;

public class UserController(IUserService userService) : Controller
{
    // GET: User/Login
    [HttpGet]
    public IActionResult Login() => View();

    // POST: User/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginDto userLoginDto)
    {
        if (!ModelState.IsValid) return View(userLoginDto);

        var result = await userService.LoginAsync(userLoginDto);

        if (result.Success && result.Data is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Data.Id.ToString()),
                new(ClaimTypes.Name, result.Data.Username),
                new(ClaimTypes.Email, result.Data.Email),
                new(ClaimTypes.Role, result.Data.Role ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, result.Message ?? "Login failed");
        return View(userLoginDto);
    }

    // GET: User/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: User/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
    {
        if (!ModelState.IsValid) return View(userRegisterDto);

        var result = await userService.RegisterAsync(userRegisterDto);

        if (result.Success && result.Data != Guid.Empty)
        {
            // Auto-login the newly registered user
            var loginDto = new UserLoginDto(
                userRegisterDto.Username,
                userRegisterDto.Password
            );
            
            var loginResult = await userService.LoginAsync(loginDto);
            
            if (loginResult.Success && loginResult.Data is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loginResult.Data.Id.ToString()),
                    new(ClaimTypes.Name, loginResult.Data.Username),
                    new(ClaimTypes.Email, loginResult.Data.Email),
                    new(ClaimTypes.Role, loginResult.Data.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["WelcomeMessage"] = "Welcome! Let's personalize your learning journey.";
                return RedirectToAction("Start", "Assessment");
            }
        }

        ModelState.AddModelError(string.Empty, result.Message ?? "Registration failed");
        return View(userRegisterDto);
    }

    // GET: User/Logout
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "You have been logged out";
        return RedirectToAction("Index", "Home");
    }

    // GET: User/List
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List()
    {
        var users = await userService.GetAllUsersAsync();
        return View(users);
    }
}
