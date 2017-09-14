using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
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

            m_logsTableReader = new LogsTableReader(CloudStorageAccount.DevelopmentStorageAccount, tableName, true);

            m_logger = new LoggerConfiguration()
                .WriteTo.AzureTableStorageWithCompactedRowFormat(CloudStorageAccount.DevelopmentStorageAccount, tableName)
                .CreateLogger();
        }

        [Fact]
        public async Task WrittenLogs_PreserveOrderOnRead()
        {
            const int itemCount = 500;
            var from = DateTimeOffset.UtcNow;

            WriteLogs(itemCount, DateTimeOffset.UtcNow);
            m_logger.Dispose();
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

        [Fact]
        public async Task WrittenLogs_OnRotatedLogs_PreserveOrderOnRead()
        {
            const int itemCount = 100;
            var from = DateTimeOffset.UtcNow.AddDays(-5);

            WriteLogs(itemCount, from);
            WriteLogs(itemCount, from.AddDays(1));
            WriteLogs(itemCount, from.AddDays(2));
            WriteLogs(itemCount, from.AddDays(3));
            WriteLogs(itemCount, from.AddDays(4));
            m_logger.Dispose();

            var to = from.AddDays(5);

            var logs = await m_logsTableReader.ReadLogs(from, to);
            Assert.Equal(itemCount * 5, logs.Count);
        }

        private void WriteLogs(int itemCount, DateTimeOffset date)
        {

            foreach (var item in Enumerable.Range(0, itemCount))
            {
                var now = DateTimeOffset.UtcNow;

                m_logger.Write(new LogEvent(
                    new DateTimeOffset(date.Year, date.Month, date.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Offset),
                    LogEventLevel.Information,
                    null,
                    new MessageTemplate("test {ItemNumber}", new[] { new PropertyToken("ItemNumber", "{ItemNumber}") }),
                    new[] { new LogEventProperty("ItemNumber", new ScalarValue(item)) }));

                Thread.Sleep(TimeSpan.FromMilliseconds(15));
            }
        }
    }
}
