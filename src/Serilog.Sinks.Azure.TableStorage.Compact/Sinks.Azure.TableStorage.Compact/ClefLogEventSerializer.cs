using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public class ClefLogEventSerializer : ILogEventSerializer
    {
        private readonly ITextFormatter m_compactJsonFormatter = new CompactJsonFormatter();

        public void Serialize(TextWriter writer, IEnumerable<LogEvent> events, out LogEvent firstEvent, out LogEvent lastEvent)
        {
            firstEvent = null;
            lastEvent = null;
            
            foreach (var logEvent in events)
            {
                if (firstEvent == null)
                {
                    firstEvent = logEvent;
                }

                m_compactJsonFormatter.Format(logEvent, writer);

                lastEvent = logEvent;
            }

            writer.Flush();
        }
    }
}
