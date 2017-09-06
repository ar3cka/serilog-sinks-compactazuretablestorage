using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public class AzureTableStorageWithCompactedRowFormatSink : PeriodicBatchingSink
    {
        private readonly ILogEventSerializer m_eventSerializer = new ClefLogEventSerializer();
        private readonly ITableEntityConverter m_converter = new TableEntityConverter();

        private readonly string m_tableName;
        private readonly CloudStorageAccount m_cloudStorageAccount;

        public AzureTableStorageWithCompactedRowFormatSink(
            CloudStorageAccount cloudStorageAccount,
            string tableName,
            int batchSizeLimit,
            TimeSpan period) : base(batchSizeLimit, period)
        {
            m_cloudStorageAccount = cloudStorageAccount ?? throw new ArgumentNullException(nameof(cloudStorageAccount));
            m_tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public AzureTableStorageWithCompactedRowFormatSink(
            CloudStorageAccount cloudStorageAccount,
            string tableName,
            int batchSizeLimit,
            TimeSpan period,
            int queueLimit) : base(batchSizeLimit, period, queueLimit)
        {
            m_cloudStorageAccount = cloudStorageAccount ?? throw new ArgumentNullException(nameof(cloudStorageAccount));
            m_tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            {
                m_eventSerializer.Serialize(writer, events, out var firstEvent, out var lastEvent);

                memory.Position = 0;

                if (lastEvent != null)
                {
                    var batch = new TableBatchOperation();
                    batch.Insert(m_converter.ConvertToDynamicEntity(firstEvent, lastEvent, memory));
                    var table = await GetOrCreateStorageTable();
                    await table.ExecuteBatchAsync(batch);
                }
            }
        }

        private async Task<CloudTable> GetOrCreateStorageTable()
        {
            var tableClient = m_cloudStorageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(m_tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
