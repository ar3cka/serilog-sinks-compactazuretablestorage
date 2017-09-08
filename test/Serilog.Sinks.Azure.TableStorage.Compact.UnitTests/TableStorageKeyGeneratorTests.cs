using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog.Events;
using Serilog.Sinks.Azure.TableStorage.Compact.Persistence;
using Xunit;

namespace Serilog.Sinks.Azure.TableStorage.Compact.UnitTests
{
    public class TableStorageKeyGeneratorTests
    {
        [Fact]
        public void GenerateRowKey_ReturnsItemsOrderedByTime()
        {
            var generator = new DefaultTableStorageKeyGenerator();
            var rowKeyActualPositions = new Dictionary<string, int>();
            var rowKeys = new SortedSet<string>();
            foreach (var i in Enumerable.Range(0, 100))
            {
                var e = new LogEvent(
                    timestamp: DateTimeOffset.UtcNow,
                    level: LogEventLevel.Information,
                    exception: null,
                    messageTemplate: MessageTemplate.Empty,
                    properties: Enumerable.Empty<LogEventProperty>());

                var rowKey = generator.GenerateRowKey(e);
                Thread.Sleep(TimeSpan.FromMilliseconds(15));

                rowKeyActualPositions[rowKey] = i;
                Assert.True(rowKeys.Add(rowKey));
            }

            var rowKeyExpectedPosition = 0;
            foreach (var rowKey in rowKeys)
            {
                Assert.Equal(rowKeyExpectedPosition, rowKeyActualPositions[rowKey]);
                rowKeyExpectedPosition++;
            }
        }

        [Fact]
        public void Test()
        {
            var now = DateTimeOffset.UtcNow;
            var ms = DateTimeOffsetTools.ToUnixTimeMilliseconds(now);
            Assert.Equal(now.ToUnixTimeMilliseconds(), ms);
        }
    }
}
