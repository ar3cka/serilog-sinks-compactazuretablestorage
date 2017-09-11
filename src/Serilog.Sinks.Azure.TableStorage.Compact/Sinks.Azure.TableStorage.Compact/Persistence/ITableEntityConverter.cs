using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ITableEntityConverter
    {
        DynamicTableEntity ConvertToDynamicEntity(SerializedClefLog log);
    }
}
