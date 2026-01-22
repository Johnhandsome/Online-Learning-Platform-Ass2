using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Online_Learning_Platform_Ass1.Data.Database.Entities;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;
public interface ILessonProgressService
{
    Task<LessonProgress?> GetAsync(Guid enrollmentId, Guid lessonId);
    Task<IEnumerable<LessonProgress>> GetByEnrollmentAsync(Guid enrollmentId);

    Task UpdateProgressAsync(
        Guid enrollmentId,
        Guid lessonId,
        int watchedSeconds,
        bool isCompleted
    );
}
