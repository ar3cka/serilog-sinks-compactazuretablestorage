using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public class LogsTableReader
    {
        private readonly ITableEntityConverter m_converter = new TableEntityConverter();
        private readonly CloudTable m_cloudTable;

        public LogsTableReader(CloudTable cloudTable)
        {
            m_cloudTable = cloudTable ?? throw new ArgumentNullException(nameof(cloudTable));
        }

        public async Task<List<PersistedLogEvent>> ReadLogs(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var query = PrepareTableQuery(fromDate, toDate);
            var result = new List<PersistedLogEvent>();

            TableContinuationToken continuationToken = null;
            do
            {
                var (logEvents, token) = await FetchSegment(query, continuationToken);
                result.AddRange(logEvents);
                continuationToken = token;
            }
            while (continuationToken != null);

            return result;
        }

        private static TableQuery<DynamicTableEntity> PrepareTableQuery(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var fromFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    fromDate.ToUnixTimeSeconds().ToString("D19"));

            var toFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.LessThanOrEqual,
                    toDate.ToUnixTimeSeconds().ToString("D19"));

            return new TableQuery<DynamicTableEntity>().Where(
                TableQuery.CombineFilters(fromFilter, TableOperators.And, toFilter));
        }

        private async Task<(List<PersistedLogEvent> logEvents, TableContinuationToken token)> FetchSegment(TableQuery<DynamicTableEntity> query, TableContinuationToken token)
        {
            var result = new List<PersistedLogEvent>();

            var dataSegment = await m_cloudTable.ExecuteQuerySegmentedAsync(query, token);
            foreach (var dynamicTableEntity in dataSegment.Results)
            {
                var data = m_converter.ConvertFromDynamicEntity(dynamicTableEntity);
                using (var logEventReader = new LogEventReader(new StreamReader(data)))
                {
                    while (logEventReader.TryRead(out var logEvent))
                    {
                        result.Add(new PersistedLogEvent(dynamicTableEntity.PartitionKey, dynamicTableEntity.RowKey, logEvent));
                    }
                }
            }

            return (result, dataSegment.ContinuationToken);
        }
    }
}
