using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class SingleInstanceCloudTableFactory : ICloudTableFactory
    {
        private readonly AsyncLazy<CloudTable> m_table;

        public SingleInstanceCloudTableFactory(AsyncLazy<CloudTable> table)
        {
            m_table = table;
        }

        public Task<CloudTable> Create(DateTimeOffset time)
        {
            return m_table.Value;
        }
    }
}
