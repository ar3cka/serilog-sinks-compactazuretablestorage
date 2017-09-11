using System;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ITableStorageKeyGenerator
    {
        string GeneratePartitionKey(DateTimeOffset time);

        string GenerateRowKey(DateTimeOffset time);
    }
}
