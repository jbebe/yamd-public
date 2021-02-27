using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net;
using System.Threading.Tasks;
using YamdFunctions.Types;

namespace Functions.Utils
{
    public class MediaStore
    {
        CloudStorageAccount StorageAccount { get; }
        CloudTableClient TableClient { get; }
        CloudTable Table { get; }

        private MediaStore(string connectionString)
        {
            StorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = StorageAccount.CreateCloudTableClient();
            Table = TableClient.GetTableReference("media");
        }

        public static async Task<MediaStore> GetInstanceAsync(string connectionString)
        {
            var store = new MediaStore(connectionString);
            await store.Table.CreateIfNotExistsAsync();

            return store;
        }

        public async Task<MediaEntity?> GetAsync(string mediaId)
        {
            var result = await Table.ExecuteAsync(
                TableOperation.Retrieve<MediaTableEntity>(mediaId, MediaTableEntity.RowKeyValue));

            return result.HttpStatusCode == (int)HttpStatusCode.NotFound
                ? null
                : (result.Result as MediaTableEntity)?.ToMediaEntity();
        }

        public async Task<string> CreateWithTokenAsync(MediaEntity entity)
        {
            var tableEntity = new MediaTableEntity(entity);
            await Table.ExecuteAsync(TableOperation.Insert(tableEntity));

            var policy = new SharedAccessTablePolicy()
            {
                Permissions = SharedAccessTablePermissions.Query,
                SharedAccessStartTime = DateTimeOffset.UtcNow,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(1),
            };
            var token = Table.GetSharedAccessSignature(
                policy, null, 
                tableEntity.PartitionKey, string.Empty, 
                tableEntity.PartitionKey, Constants.TableStorageLexicalMaxValue);
            var storageUrl = StorageAccount.TableStorageUri.PrimaryUri.ToString().TrimEnd('/');
            var tokenUrl = $"{storageUrl}/{Table.Name}{token}";

            return tokenUrl;
        }
    }
}
