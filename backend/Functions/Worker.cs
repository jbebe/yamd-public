using Functions.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using YamdFunctions.Logic;
using YamdFunctions.Types;

namespace YamdFunctions
{
    public static class Worker
    {
        static Worker() 
        {
            
        }

        [FunctionName("process")]
        [return: Table("media", Connection = "AzureWebJobsStorage")]
        public static async Task<MediaTableEntity> RunAsync(
            [QueueTrigger("media-request", Connection = "AzureWebJobsStorage")] string media,
            ILogger log
        ){
            var entity = JsonConvert.DeserializeObject<MediaEntity>(media);

            entity = await ProcessMediaAsync(entity);

            var tableEntity = new MediaTableEntity(entity)
            {
                // This must be set to allow upsert entity
                ETag = "*",
            };

            return await Task.FromResult(tableEntity);
        }

        private async static Task<MediaEntity> ProcessMediaAsync(MediaEntity entity)
        {
            var manager = MediaManagerFactory.GetManager(entity.Type);

            return await manager.ProcessAsync(entity);
        }
    }
}
