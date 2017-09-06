using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public interface ITableStorageKeyGenerator
    {
        string GeneratePartitionKey(LogEvent logEvent);

        string GenerateRowKey(LogEvent logEvent);
    }
}
