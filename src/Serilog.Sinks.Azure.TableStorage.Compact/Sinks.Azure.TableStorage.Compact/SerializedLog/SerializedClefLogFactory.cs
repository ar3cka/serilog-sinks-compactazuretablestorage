using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog
{
    public class SerializedClefLogFactory : ISerializedClefLogFactory
    {
        private readonly ITextFormatter m_formatter;

        public SerializedClefLogFactory(ITextFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public SerializedClefLog CreateFromEvents(IEnumerable<LogEvent> logEvents)
        {
            if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));

            using (var memory = new MemoryStream())
            {
                var result = SerializedClefLog.Empty;
                LogEvent firstEvent = null;
                LogEvent lastEvent = null;

                using (var zipArchive = new ZipArchive(memory, ZipArchiveMode.Create, false))
                using (var zip = zipArchive.CreateEntry("log.clef").Open())
                using (var writer = new StreamWriter(zip, Encoding.UTF8, 1024, false))
                {
                    foreach (var logEvent in logEvents)
                    {
                        if (firstEvent == null)
                        {
                            firstEvent = logEvent;
                        }

                        m_formatter.Format(logEvent, writer);

                        lastEvent = logEvent;
                    }

                    writer.Flush();
                    zip.Flush();
                }

                if (firstEvent != null)
                {
                    result = new SerializedClefLog(
                        firstEvent.Timestamp,
                        lastEvent.Timestamp,
                        new MemoryStream(memory.GetBuffer(), false));
                }

                return result;
            }
        }
    }
}
