using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public class LogSegment
    {
        public readonly TableContinuationToken ContinuationToken;
        public readonly List<PersistedLogEvent> LogEvents;

        public LogSegment(List<PersistedLogEvent> logEvents, TableContinuationToken continuationToken)
        {
            LogEvents = logEvents;
            ContinuationToken = continuationToken;
        }
    }
}
