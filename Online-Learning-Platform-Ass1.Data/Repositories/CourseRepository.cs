using Microsoft.EntityFrameworkCore;
using Online_Learning_Platform_Ass1.Data.Database;
using Online_Learning_Platform_Ass1.Data.Database.Entities;
using Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

namespace Online_Learning_Platform_Ass1.Data.Repositories;

public class CourseRepository(OnlineLearningContext context) : ICourseRepository
{
    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesAsync(string? searchTerm = null, Guid? categoryId = null)
    {
        var query = context.Courses.AsNoTracking().Where(c => c.Status == "active");

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(term) || (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        return await query
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetFeaturableCoursesAsync(int count)
    {
        // Simple logic for "featured": just take top N latest active courses
        return await context.Courses
         .AsNoTracking()
         .Include(c => c.Instructor)
         .Include(c => c.Category)
         .Where(c => c.Status == "published")
         .OrderByDescending(c => c.CreatedAt)
         .Take(count)
         .ToListAsync();

    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}
