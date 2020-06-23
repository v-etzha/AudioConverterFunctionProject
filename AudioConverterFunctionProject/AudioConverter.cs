using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace AudioConverterFunctionProject
{
    public static class AudioConverter
    {
        [FunctionName("AudioConverter")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var webmbasebyte = Convert.FromBase64String(requestBody);
            var temp = Path.GetTempFileName() + ".webm";
            var tempOut = Path.GetTempFileName() + ".wav";
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempPath);

            log.LogInformation("C# HTTP trigger function processed a request.");


            using (var ms = new MemoryStream())
            {
                File.WriteAllBytes(temp, webmbasebyte);
            }

            var bs = File.ReadAllBytes(temp);
            log.LogInformation($"Renc Length: {bs.Length}");

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"D:\home\FFmpeg\ffmpeg.exe";
            psi.Arguments = $"-i \"{temp}\" \"{tempOut}\"";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            Process ps = Process.Start(psi);
            ps.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);

            var bytes = File.ReadAllBytes(tempOut);
            log.LogInformation($"Renc Length: {bytes.Length}");

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(bytes));            
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            File.Delete(tempOut);
            File.Delete(temp);
            Directory.Delete(tempPath, true);           
            return response;
        }
    }
}
