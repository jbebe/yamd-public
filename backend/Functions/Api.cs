using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YamdFunctions.Types;
using System;
using Functions.Utils;
using Functions.Types;
using System.Web.Http;

namespace YamdFunctions
{
    public static class Api
    {
        public static string? YamdTableStorageConnectionString { get; set; } =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public static string GetYamdTableStorageConnectionString() =>
            YamdTableStorageConnectionString ?? throw new Exception($"Unable to get environment variable {"AzureWebJobsStorage"}");

        [FunctionName("submit")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest httpRequest,
            [Queue("media-request"), StorageAccount("AzureWebJobsStorage")] ICollector<string> queue,
            ILogger log
        ){
            try
            {
                string content;
                using (StreamReader stream = new StreamReader(httpRequest.Body))
                    content = await stream.ReadToEndAsync();

                var request = JsonConvert.DeserializeObject<MediaEntityRequest>(content);
                if (!request.IsValid)
                    return new BadRequestErrorMessageResult("Invalid media request");

                var entity = request.ToMediaEntity();

                var mediaStore = await MediaStore.GetInstanceAsync(GetYamdTableStorageConnectionString());
                var tableEntity = await mediaStore.GetAsync(entity.Id);

                if (tableEntity != null)
                    return new OkObjectResult(new SubmitResult(entity: tableEntity));

                queue.Add(content);
                var token = await mediaStore.CreateWithTokenAsync(entity);

                return new OkObjectResult(new SubmitResult(token: token));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                return new NotFoundObjectResult(ex);
            }
        }
    }
}
