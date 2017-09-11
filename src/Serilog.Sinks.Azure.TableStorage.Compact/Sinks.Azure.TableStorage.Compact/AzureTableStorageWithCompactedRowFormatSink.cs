using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.Azure.TableStorage.Compact.Persistence;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public class AzureTableStorageWithCompactedRowFormatSink : PeriodicBatchingSink
    {
        private readonly ILogsTable m_logsTable;
        private readonly ISerializedClefLogFactory m_logFactory;

        public AzureTableStorageWithCompactedRowFormatSink(
            int batchSizeLimit,
            TimeSpan period,
            ISerializedClefLogFactory logFactory,
            ILogsTable logsTable) : base(batchSizeLimit, period)
        {
            m_logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            m_logsTable = logsTable ?? throw new ArgumentNullException(nameof(logsTable));
        }

        public AzureTableStorageWithCompactedRowFormatSink(
            int batchSizeLimit,
            TimeSpan period,
            int queueLimit,
            ISerializedClefLogFactory logFactory,
            ILogsTable logsTable) : base(batchSizeLimit, period, queueLimit)
        {
            m_logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            m_logsTable = logsTable ?? throw new ArgumentNullException(nameof(logsTable));
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            using (var serializedLog = m_logFactory.CreateFromEvents(events))
            {
                if (serializedLog.HasData)
                {
                    await m_logsTable.Save(serializedLog);
                }
            }
        }
    }
}
