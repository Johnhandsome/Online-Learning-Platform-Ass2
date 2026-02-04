using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using OnlineLearningPlatformAss2.Service.DTOs.Course;
using System.Security.Claims;

namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

[Authorize]
public class LearnModel : PageModel
{
    private readonly ICourseService _courseService;

    public LearnModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public CourseLearnViewModel? CourseLearn { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToPage("/User/Login");
        }

        try
        {
            var enrollmentId = await _courseService.GetEnrollmentIdAsync(userId, id);
            
            if (enrollmentId.HasValue)
            {
                CourseLearn = await _courseService.GetCourseLearnAsync(enrollmentId.Value);
            }
            
            if (CourseLearn == null)
            {
                ErrorMessage = "You are not enrolled in this course.";
                CourseLearn = CreateSampleLearnSession(id);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading course: {ex.Message}";
            CourseLearn = CreateSampleLearnSession(id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteLessonAsync([FromBody] CompleteLessonRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return new JsonResult(new { success = false, message = "User not found" });
        }

        var enrollmentId = await _courseService.GetEnrollmentIdAsync(userId, request.CourseId);
        if (!enrollmentId.HasValue)
        {
            return new JsonResult(new { success = false, message = "Enrollment not found" });
        }

        var success = await _courseService.UpdateLessonProgressAsync(enrollmentId.Value, request.LessonId, true);
        
        // Fetch updated progress
        var updatedLearn = await _courseService.GetCourseLearnAsync(enrollmentId.Value);

        return new JsonResult(new { 
            success, 
            progress = updatedLearn?.Progress ?? 0,
            isCourseCompleted = updatedLearn?.Progress >= 100
        });
    }

    public class CompleteLessonRequest
    {
        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
    }

    private CourseLearnViewModel CreateSampleLearnSession(Guid courseId)
    {
        return new CourseLearnViewModel
        {
            EnrollmentId = Guid.NewGuid(),
            CourseId = courseId,
            CourseTitle = "Complete Web Development Bootcamp",
            Progress = 25,
            CurrentLessonId = Guid.NewGuid(),
            CurrentLesson = new LessonViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Setting Up Your Development Environment",
                Content = "In this lesson, you'll learn how to set up your development environment with all the necessary tools for web development.",
                VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Demo video
                Duration = 15,
                OrderIndex = 2
            },
            Modules = new List<ModuleViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Introduction to Web Development",
                    Description = "Getting started with web development fundamentals",
                    OrderIndex = 1,
                    Lessons = new List<LessonViewModel>
                    {
                        new() { Id = Guid.NewGuid(), Title = "Welcome to the Course", Duration = 5, OrderIndex = 1, IsCompleted = true },
                        new() { Id = Guid.NewGuid(), Title = "Setting Up Your Development Environment", Duration = 15, OrderIndex = 2, IsCurrent = true },
                        new() { Id = Guid.NewGuid(), Title = "How the Internet Works", Duration = 20, OrderIndex = 3 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "HTML Fundamentals",
                    Description = "Master HTML5 and semantic markup",
                    OrderIndex = 2,
                    Lessons = new List<LessonViewModel>
                    {
                        new() { Id = Guid.NewGuid(), Title = "HTML Structure and Elements", Duration = 25, OrderIndex = 1 },
                        new() { Id = Guid.NewGuid(), Title = "Forms and Input Elements", Duration = 30, OrderIndex = 2 },
                        new() { Id = Guid.NewGuid(), Title = "Semantic HTML5 Elements", Duration = 20, OrderIndex = 3 }
                    }
                }
            }
        };
    }
}
