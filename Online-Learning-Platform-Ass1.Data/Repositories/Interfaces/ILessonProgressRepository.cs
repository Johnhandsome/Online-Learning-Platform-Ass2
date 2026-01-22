using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Data.Repositories.Interfaces;
public interface ILessonProgressRepository
{
    Task<LessonProgress?> GetAsync(Guid enrollmentId, Guid lessonId);
    Task<IEnumerable<LessonProgress>> GetByEnrollmentAsync(Guid enrollmentId);

    Task UpsertAsync(LessonProgress progress);
    Task UpdateAsync(LessonProgress progress);
    Task AddAsync(LessonProgress progress);
}
