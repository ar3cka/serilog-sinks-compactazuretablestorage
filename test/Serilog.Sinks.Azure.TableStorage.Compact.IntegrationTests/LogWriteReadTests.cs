using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Serilog.Core;
using Serilog.Sinks.Azure.TableStorage.Compact.Reader;
using Xunit;

namespace Serilog.Sinks.Azure.TableStorage.Compact.IntegrationTests
{
    public class LogWriteReadTests
    {
        private readonly Logger m_logger;
        private readonly LogsTableReader m_logsTableReader;

        public LogWriteReadTests()
        {
            var tableName = "LogTable" + Guid.NewGuid().ToString("N");
            var tableClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExistsAsync().Wait();

            m_logsTableReader = new LogsTableReader(table);

            m_logger = new LoggerConfiguration()
                .WriteTo.AzureTableStorageWithCompactedRowFormat(CloudStorageAccount.DevelopmentStorageAccount, tableName)
                .CreateLogger();
        }

        [Fact]
        public async Task WrittenLogs_PreserveOrderOnRead()
        {
            const int itemCount = 500;
            var from = DateTimeOffset.UtcNow;
            
            WriteLogs(itemCount);

            var to = DateTimeOffset.UtcNow;

            var logs = await m_logsTableReader.ReadLogs(from, to);
            Assert.Equal(itemCount, logs.Count);

            var itemNumber = 0;
            foreach (var persistedLogEvent in logs)
            {
                Assert.Equal(itemNumber.ToString(), persistedLogEvent.Event.Properties["ItemNumber"].ToString());
                itemNumber++;
            }
        }

        private void WriteLogs(int itemCount)
        {
            foreach (var item in Enumerable.Range(0, itemCount))
            {
                m_logger.Information("test {ItemNumber}", item);
                Thread.Sleep(TimeSpan.FromMilliseconds(15));
            }

            m_logger.Dispose();
        }
    }
}
