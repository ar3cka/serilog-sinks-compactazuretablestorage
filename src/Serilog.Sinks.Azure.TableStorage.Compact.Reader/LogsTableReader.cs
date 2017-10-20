using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Sinks.Azure.TableStorage.Compact.Persistence;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public class LogsTableReader
    {
        private readonly TableEntityReader m_reader = new TableEntityReader();
        private readonly ICloudTableFactory m_cloudTableFactory;

        public LogsTableReader(CloudTable cloudTable)
        {
            m_cloudTableFactory = new SingleInstanceCloudTableFactory(new AsyncLazy<CloudTable>(() => cloudTable));
        }

        public LogsTableReader(CloudStorageAccount storageAccount, string tableName, bool tableRotationEnabled)
        {
            if (tableRotationEnabled)
            {
                m_cloudTableFactory = new RotatedCloudTableFactory(storageAccount, tableName);
            }
            else
            {
                m_cloudTableFactory = new SingleInstanceCloudTableFactory(new AsyncLazy<CloudTable>(() => storageAccount.CreateTable(tableName)));
            }
        }

        public async Task<LogSegment> ReadLogsSegmented(DateTimeOffset fromTime, DateTimeOffset toTime, DateTimeOffset? logsDate, TableContinuationToken continuationToken)
        {
            var query = PrepareTableQuery(fromTime, toTime);

            var fromDate = logsDate ?? fromTime.Date;
            var table = await m_cloudTableFactory.Create(fromDate);
            var segment = await FetchSegment(table, query, continuationToken);

            if (segment.token != null)
            {
                return new LogSegment(segment.logEvents, true, fromDate, segment.token);
            }

            fromDate = fromDate.AddDays(1);
            var toDate = toTime.Date;
            if (fromDate <= toDate)
            {
                return new LogSegment(segment.logEvents, true, fromDate, null);
            }

            return new LogSegment(segment.logEvents, false, null, null);
        }

        public async Task<List<PersistedLogEvent>> ReadLogs(DateTimeOffset fromTime, DateTimeOffset toTime)
        {
            var query = PrepareTableQuery(fromTime, toTime);
            var result = new List<PersistedLogEvent>();

            var fromDate = fromTime.Date;
            var toDate = toTime.Date;
            TableContinuationToken continuationToken = null;
            do
            {
                var table = await m_cloudTableFactory.Create(fromDate);

                do
                {
                    var segment = await FetchSegment(table, query, continuationToken);
                    result.AddRange(segment.logEvents);
                    continuationToken = segment.token;
                }
                while (continuationToken != null);

                fromDate = fromDate.AddDays(1);
            }
            while (fromDate <= toDate);

            return result;
        }

        private static TableQuery<DynamicTableEntity> PrepareTableQuery(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var fromFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    DateTimeOffsetTools.ToUnixTimeSecondsString(fromDate));

            var toFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.LessThan,
                    DateTimeOffsetTools.ToUnixTimeSecondsString(toDate));

            return new TableQuery<DynamicTableEntity>().Where(
                TableQuery.CombineFilters(fromFilter, TableOperators.And, toFilter));
        }

        private async Task<(List<PersistedLogEvent> logEvents, TableContinuationToken token)> FetchSegment(CloudTable table, TableQuery<DynamicTableEntity> query, TableContinuationToken token)
        {
            var result = new List<PersistedLogEvent>();

            var dataSegment = await table.ExecuteQuerySegmentedAsync(query, token);
            foreach (var dynamicTableEntity in dataSegment.Results)
            {
                var events = m_reader.ReadEvents(dynamicTableEntity);

                foreach (var logEvent in events)
                {
                    result.Add(new PersistedLogEvent(dynamicTableEntity.PartitionKey, dynamicTableEntity.RowKey, logEvent));
                }
            }

            return (result, dataSegment.ContinuationToken);
        }
    }
}
