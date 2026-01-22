using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Learning_Platform_Ass1.Service.DTOs.Course;
using Online_Learning_Platform_Ass1.Service.DTOs.Lesson;
using Online_Learning_Platform_Ass1.Service.DTOs.Order;
using Online_Learning_Platform_Ass1.Service.Services;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Controllers;

[Authorize]
public class CourseController(
    ICourseService courseService,
    IOrderService orderService,
    IAiLessonService aiLessonService) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var parsedId))
        {
            userId = parsedId;
        }

        var course = await courseService.GetCourseDetailsAsync(id, userId);
        if (course == null) return NotFound();
        return View(course);
    }

    // GET: Confirm Purchase
    public async Task<IActionResult> Checkout(Guid id)
    {
        var course = await courseService.GetCourseDetailsAsync(id);
        if (course == null) return NotFound();

        // Check if user is logged in (Authorize attribute handles it but double check logic if needed)
        // Check if already enrolled (Service logic usually, but here we just show checkout)

        return View(course);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessCheckout(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var order = await orderService.CreateOrderAsync(userId, new CreateOrderDto { CourseId = id });
        if (order == null) return BadRequest("Could not create order.");

        // Redirect to VNPay
        return RedirectToAction("CreatePaymentUrl", "Payment", new { orderId = order.OrderId });
    }

    public IActionResult Success(Guid id)
    {
        ViewBag.OrderId = id;
        return View();
    }
    public async Task<IActionResult> MyCourses()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToAction("Login", "User");
        }

        var courses = await courseService.GetEnrolledCoursesAsync(userId);
        return View(courses);
    }

    public async Task<IActionResult> Learn(Guid enrollmentId, Guid? lessonId)
    {
        var vm = await courseService.GetCourseLearnAsync(enrollmentId);
        if (vm == null) return NotFound();

        vm.CurrentLesson = lessonId == null
            ? vm.Modules.FirstOrDefault()?.Lessons.FirstOrDefault()
            : vm.Modules.SelectMany(m => m.Lessons)
                .FirstOrDefault(l => l.Id == lessonId);

        foreach (var lesson in vm.Modules.SelectMany(m => m.Lessons))
            lesson.IsCurrent = vm.CurrentLesson?.Id == lesson.Id;

        return View(vm);
    }

    public async Task<IActionResult> List()
    {
        Guid? currentUserId = null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var uid))
        {
            currentUserId = uid;
        }

        if (currentUserId == null)
        {
            return RedirectToAction("Login", "User");

        }
        var enrolledCourses = await courseService.GetEnrolledCoursesAsync(currentUserId.Value);
        return View(enrolledCourses);
    }

    [HttpPost]
    public async Task<IActionResult> AiSummary([FromBody] AiSummaryRequest req)
    {
        var result = await aiLessonService.GenerateSummaryAsync(
            req.EnrollmentId, req.LessonId
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AiAsk([FromBody] AiAskRequest req)
    {
        var result = await aiLessonService.AskAsync(
            req.EnrollmentId,
            req.LessonId,
            req.Question
        );

        return Ok(result);
    }

}
