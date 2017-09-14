using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class LogsTable : ILogsTable
    {
        private readonly ICloudTableFactory m_tableFactory;
        private readonly ITableStorageKeyGenerator m_keyGenerator;
        private readonly ITableEntityConverter m_tableEntityConverter;

        public LogsTable(ICloudTableFactory tableFactory, ITableStorageKeyGenerator keyGenerator, ITableEntityConverter tableEntityConverter)
        {
            m_tableFactory = tableFactory ?? throw new ArgumentNullException(nameof(tableFactory));
            m_keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
            m_tableEntityConverter = tableEntityConverter ?? throw new ArgumentNullException(nameof(tableEntityConverter));
        }

        public async Task Save(SerializedClefLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            var batch = new TableBatchOperation();
            var entity = m_tableEntityConverter.ConvertToDynamicEntity(log);
            entity.PartitionKey = m_keyGenerator.GeneratePartitionKey(log.LastEventTime);
            entity.RowKey = m_keyGenerator.GenerateRowKey(log.FirstEventTime);
            batch.Insert(entity);
            var table = await m_tableFactory.Create(log.LastEventTime);
            await table.ExecuteBatchAsync(batch);
        }
    }
}
