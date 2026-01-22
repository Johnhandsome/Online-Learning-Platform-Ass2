using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_Learning_Platform_Ass1.Service.DTOs.Lesson;
public class AiAskRequest
{

    public Guid EnrollmentId { get; set; }
    public Guid LessonId { get; set; }
    public string Question { get; set; } = "";


}
