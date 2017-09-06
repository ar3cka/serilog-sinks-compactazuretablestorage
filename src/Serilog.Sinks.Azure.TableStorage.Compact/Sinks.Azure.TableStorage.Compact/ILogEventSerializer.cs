using System.Collections.Generic;
using System.IO;
using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public interface ILogEventSerializer
    {
        void Serialize(TextWriter writer, IEnumerable<LogEvent> events, out LogEvent firstEvent, out LogEvent lastEvent);
    }
}
