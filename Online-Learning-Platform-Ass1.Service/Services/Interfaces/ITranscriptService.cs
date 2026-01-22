using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_Learning_Platform_Ass1.Service.Services.Interfaces;
public interface ITranscriptService
{
    Task<string> GenerateTranscriptFromVideoAsync(string videoUrl);
}
