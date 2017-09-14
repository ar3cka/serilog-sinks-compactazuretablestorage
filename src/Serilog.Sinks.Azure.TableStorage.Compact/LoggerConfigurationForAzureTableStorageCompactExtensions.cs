using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Azure.TableStorage.Compact;
using Serilog.Sinks.Azure.TableStorage.Compact.Persistence;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;

namespace Serilog
{
    public static class LoggerConfigurationForAzureTableStorageCompactExtensions
    {
        private static readonly ITextFormatter s_defaultTextFormatter = new CompactJsonFormatter();
        private static readonly ISerializedClefLogFactory s_defaultLogFactory = new SerializedClefLogFactory(s_defaultTextFormatter);

        public static LoggerConfiguration AzureTableStorageWithCompactedRowFormat(
            this LoggerSinkConfiguration configuration,
            CloudStorageAccount cloudStorageAccount,
            string tableName,
            bool enableTableRotation = true)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (cloudStorageAccount == null) throw new ArgumentNullException(nameof(cloudStorageAccount));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            return configuration.Sink(
                new AzureTableStorageWithCompactedRowFormatSink(
                    batchSizeLimit: 100,
                    period: TimeSpan.FromSeconds(5),
                    logFactory: s_defaultLogFactory,
                    logsTable: enableTableRotation
                        ? PrepareRotatedLogsTable(cloudStorageAccount, tableName)
                        : PrepareLogsTable(() => GetOrCreateTable(cloudStorageAccount, tableName))));
        }

        public static LoggerConfiguration AzureTableStorageWithCompactedRowFormat(
            this LoggerSinkConfiguration configuration,
            CloudTable table)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return configuration.Sink(
                new AzureTableStorageWithCompactedRowFormatSink(
                    batchSizeLimit: 100,
                    period: TimeSpan.FromSeconds(5),
                    logFactory: s_defaultLogFactory,
                    logsTable: PrepareLogsTable(() => Task.FromResult(table))));
        }

        private static LogsTable PrepareRotatedLogsTable(CloudStorageAccount cloudStorageAccount, string tableName)
        {
            return new LogsTable(
                tableFactory: new RotatedCloudTableFactory(cloudStorageAccount, tableName), 
                keyGenerator: new DefaultTableStorageKeyGenerator(),
                tableEntityConverter: new TableEntityConverter());
        }

        private static LogsTable PrepareLogsTable(Func<Task<CloudTable>> prepareCloudTable)
        {
            return new LogsTable(
                tableFactory: new SingleInstanceCloudTableFactory(new AsyncLazy<CloudTable>(prepareCloudTable)),
                keyGenerator: new DefaultTableStorageKeyGenerator(),
                tableEntityConverter: new TableEntityConverter());
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
