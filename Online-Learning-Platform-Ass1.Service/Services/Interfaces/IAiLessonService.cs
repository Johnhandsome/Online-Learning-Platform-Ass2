using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;
public interface IAiLessonService
{
    Task<string> GenerateSummaryAsync(Guid enrollmentId, Guid lessonId);
    Task<string> AskAsync (Guid enrollmentId, Guid lessonId, string question);
}
