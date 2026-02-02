namespace OnlineLearningPlatformAss2.RazorWebApp.Pages.Course;

public class DetailsModel : PageModel
{
    private readonly ICourseService _courseService;

    public DetailsModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public CourseDetailViewModel? CourseDetails { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var parsedId))
        {
            userId = parsedId;
        }

        CourseDetails = await _courseService.GetCourseDetailsAsync(id, userId);
        if (CourseDetails == null)
            return NotFound();

        return Page();
    }
}
