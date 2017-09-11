using System.Threading.Tasks;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public interface ILogsTable
    {
        Task Save(SerializedClefLog log);
    }
}
