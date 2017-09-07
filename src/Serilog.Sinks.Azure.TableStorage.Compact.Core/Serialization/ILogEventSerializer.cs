using System.Collections.Generic;
using System.IO;
using Serilog.Events;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Serialization
{
    public interface ILogEventSerializer
    {
        MemoryStream Serialize(IEnumerable<LogEvent> events, out LogEvent firstEvent, out LogEvent lastEvent);

        List<LogEvent> Deserialize(Stream stream);
    }
}
