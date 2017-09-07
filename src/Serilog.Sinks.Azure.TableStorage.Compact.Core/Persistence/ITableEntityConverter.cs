using System.IO;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ITableEntityConverter
    {
        DynamicTableEntity ConvertToDynamicEntity(LogEvent firstEvent, LogEvent lastEvent, MemoryStream data);

        MemoryStream ConvertFromDynamicEntity(DynamicTableEntity entity);
    }
}
