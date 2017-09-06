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
        private readonly AsyncLazy<CloudTable> m_table;

        public AzureTableStorageWithCompactedRowFormatSink(
            AsyncLazy<CloudTable> table,
            int batchSizeLimit,
            TimeSpan period) : base(batchSizeLimit, period)
        {
            m_table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public AzureTableStorageWithCompactedRowFormatSink(
            AsyncLazy<CloudTable> table,
            int batchSizeLimit,
            TimeSpan period,
            int queueLimit) : base(batchSizeLimit, period, queueLimit)
        {
            m_table = table ?? throw new ArgumentNullException(nameof(table));
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
                    var table = await m_table;
                    await table.ExecuteBatchAsync(batch);
                }
            }
        }

        private async Task<CloudTable> GetOrCreateStorageTable()
        {
            return await m_table;
        }
    }
}
