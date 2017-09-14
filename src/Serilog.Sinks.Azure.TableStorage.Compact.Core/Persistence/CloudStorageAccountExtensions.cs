using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public static class CloudStorageAccountExtensions
    {
        public static async Task<CloudTable> CreateTable(this CloudStorageAccount storageAccount, string tableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
