using Online_Learning_Platform_Ass1.Data.Database.Entities;


namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;

public interface ILessonRepository
{
    Task<IEnumerable<Lesson>> GetAllAsync();
    Task<Lesson?> GetByIdAsync(Guid lessonId);
    Task<IEnumerable<Lesson>> GetByModuleIdAsync(Guid moduleId);
    Task AddAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(Guid lessonId);
}
