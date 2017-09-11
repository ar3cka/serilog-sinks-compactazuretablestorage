using System;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class DefaultTableStorageKeyGenerator : ITableStorageKeyGenerator
    {
        public string GeneratePartitionKey(DateTimeOffset time)
        {
            var unixSeconds = DateTimeOffsetTools.ToUnixTimeSeconds(time);
            return unixSeconds.ToString("D19");
        }

        public string GenerateRowKey(DateTimeOffset time)
        {
            var now = DateTimeOffsetTools.ToUnixTimeMilliseconds(time);

            return now.ToString("D19") + "|" + Guid.NewGuid().ToString("N");
        }
    }
}
