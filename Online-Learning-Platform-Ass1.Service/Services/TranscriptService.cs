using System.Net.Http.Headers;
using Newtonsoft.Json;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Service.Services;
public class TranscriptService(HttpClient httpClient) : ITranscriptService
{
    private readonly HttpClient _http = httpClient;

    public async Task<string> GenerateTranscriptFromVideoAsync(string videoUrl)
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            //tai file ve tam
            await using (var stream = await _http.GetStreamAsync(videoUrl))
            await using (var fs = File.Create(tempFile))
            {
                await stream.CopyToAsync(fs);
            }

            using var form = new MultipartFormDataContent();

            await using var fs2 = File.OpenRead(tempFile);
            var fileContent = new StreamContent(fs2);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

            form.Add(fileContent, "file", Path.GetFileName(tempFile));

            var response = await _http.PostAsync(
                "http://192.168.1.25:8000/transcript",
                form
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json)!;

            return result.text;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
