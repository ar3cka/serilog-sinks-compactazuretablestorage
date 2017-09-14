using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ICloudTableFactory
    {
        Task<CloudTable> Create(DateTimeOffset time);
    }
}
