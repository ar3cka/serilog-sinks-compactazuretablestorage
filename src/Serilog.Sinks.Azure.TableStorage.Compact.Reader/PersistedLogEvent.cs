using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public class PersistedLogEvent
    {
        public string PartitionKey { get; }

        public string RowKey { get; }

        public LogEvent Event { get; }

        public PersistedLogEvent(string partitionKey, string rowKey, LogEvent @event)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Event = @event;
        }
    }
}
