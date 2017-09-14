using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class RotatedCloudTableFactory : ICloudTableFactory
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<CloudTable>> m_tableCache = new ConcurrentDictionary<string, AsyncLazy<CloudTable>>();
        private readonly CloudStorageAccount m_account;
        private readonly string m_tableName;

        public RotatedCloudTableFactory(CloudStorageAccount account, string tableName)
        {
            m_account = account;
            m_tableName = tableName;
        }

        public Task<CloudTable> Create(DateTimeOffset time)
        {
            var tableName = m_tableName + time.Date.ToString("yyyyMMdd");

            if (!m_tableCache.TryGetValue(tableName, out var table))
            {
                if (m_tableCache.Count > 365)
                {
                    m_tableCache.Clear();
                }

                table = m_tableCache.GetOrAdd(tableName, name => new AsyncLazy<CloudTable>(() => m_account.CreateTable(name)));
            }
            
            return table.Value;
        }
    }
}
