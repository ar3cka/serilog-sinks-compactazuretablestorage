using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Configuration;
using Serilog.Sinks.Azure.TableStorage.Compact;

namespace Serilog
{
    public static class LoggerConfigurationForAzureTableStorageCompactExtensions
    {
        public static LoggerConfiguration AzureTableStorageWithCompactedRowFormat(
            this LoggerSinkConfiguration configuration,
            CloudStorageAccount cloudStorageAccount,
            string tableName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (cloudStorageAccount == null) throw new ArgumentNullException(nameof(cloudStorageAccount));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            return configuration.Sink(
                new AzureTableStorageWithCompactedRowFormatSink(
                    table: new AsyncLazy<CloudTable>(() => GetOrCreateTable(cloudStorageAccount, tableName)),
                    batchSizeLimit: 100,
                    period: TimeSpan.FromSeconds(5)));
        }

        public static LoggerConfiguration AzureTableStorageWithCompactedRowFormat(
            this LoggerSinkConfiguration configuration,
            CloudTable table)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return configuration.Sink(
                new AzureTableStorageWithCompactedRowFormatSink(
                    table: new AsyncLazy<CloudTable>(() => table),
                    batchSizeLimit: 100,
                    period: TimeSpan.FromSeconds(5)));
        }

        private static async Task<CloudTable> GetOrCreateTable(CloudStorageAccount cloudStorageAccount, string tableName)
        {
            var tableClient = cloudStorageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }
    }
}
