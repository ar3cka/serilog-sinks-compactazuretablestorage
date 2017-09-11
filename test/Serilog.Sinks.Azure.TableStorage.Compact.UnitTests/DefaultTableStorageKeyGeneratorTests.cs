using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog.Sinks.Azure.TableStorage.Compact.Persistence;
using Xunit;

namespace Serilog.Sinks.Azure.TableStorage.Compact.UnitTests
{
    public class DefaultTableStorageKeyGeneratorTests
    {
        private readonly DefaultTableStorageKeyGenerator m_generator;

        public DefaultTableStorageKeyGeneratorTests()
        {
            m_generator = new DefaultTableStorageKeyGenerator();
        }

        [Fact]
        public void GeneratePartitionKey_ReturnsItemsOrderedByTime()
        {
            RunTest(m_generator.GeneratePartitionKey, 5, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GenerateRowKey_ReturnsItemsOrderedByTime()
        {
            RunTest(m_generator.GenerateRowKey, 100, TimeSpan.FromMilliseconds(15));
        }

        private static void RunTest(Func<DateTimeOffset, string> generateKey, int keyCount, TimeSpan sleepInterval)
        {
            var rowKeyActualPositions = new Dictionary<string, int>();
            var rowKeys = new SortedSet<string>();
            foreach (var i in Enumerable.Range(0, keyCount))
            {
                var key = generateKey(DateTimeOffset.UtcNow);
                Thread.Sleep(sleepInterval);

                rowKeyActualPositions[key] = i;
                Assert.True(rowKeys.Add(key));
            }

            var rowKeyExpectedPosition = 0;
            foreach (var rowKey in rowKeys)
            {
                Assert.Equal(rowKeyExpectedPosition, rowKeyActualPositions[rowKey]);
                rowKeyExpectedPosition++;
            }
        }
    }
}
