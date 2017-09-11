using System.Collections.Generic;
using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog
{
    public interface ISerializedClefLogFactory
    {
        SerializedClefLog CreateFromEvents(IEnumerable<LogEvent> logEvents);
    }
}
