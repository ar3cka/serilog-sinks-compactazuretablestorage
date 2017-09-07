using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ITableStorageKeyGenerator
    {
        string GeneratePartitionKey(LogEvent logEvent);

        string GenerateRowKey(LogEvent logEvent);
    }
}
