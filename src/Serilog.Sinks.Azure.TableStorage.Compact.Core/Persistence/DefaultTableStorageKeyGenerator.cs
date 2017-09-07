using System;
using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class DefaultTableStorageKeyGenerator : ITableStorageKeyGenerator
    {
        public string GeneratePartitionKey(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var unixSeconds = logEvent.Timestamp.ToUnixTimeSeconds();
            return unixSeconds.ToString("D19");
        }

        public string GenerateRowKey(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var now = logEvent.Timestamp.ToUnixTimeMilliseconds();

            return now.ToString("D19") + "|" + Guid.NewGuid().ToString("N");
        }
    }
}
