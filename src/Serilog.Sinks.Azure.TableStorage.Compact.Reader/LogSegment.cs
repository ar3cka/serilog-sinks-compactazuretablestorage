using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public class LogSegment
    {
        public readonly TableContinuationToken ContinuationToken;
        public readonly List<PersistedLogEvent> LogEvents;
        public readonly DateTimeOffset? NextLogsSearchDate;
        public readonly bool HasNext;

        public LogSegment(List<PersistedLogEvent> logEvents, bool hasNext, DateTimeOffset? nextLogsSearchDate, TableContinuationToken continuationToken)
        {
            LogEvents = logEvents;
            NextLogsSearchDate = nextLogsSearchDate;
            ContinuationToken = continuationToken;
            HasNext = hasNext;
        }


    }
}
