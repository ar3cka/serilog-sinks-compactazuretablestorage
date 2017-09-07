using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Serialization
{
    public class ClefLogEventSerializer : ILogEventSerializer
    {
        private readonly ITextFormatter m_compactJsonFormatter = new CompactJsonFormatter();

        public MemoryStream Serialize(IEnumerable<LogEvent> events, out LogEvent firstEvent, out LogEvent lastEvent)
        {
            using (var memory = new MemoryStream())
            using (var zipArchive = new ZipArchive(memory, ZipArchiveMode.Create, false))
            {
                using (var zip = zipArchive.CreateEntry("logs.clef").Open())
                using (var writer = new StreamWriter(zip, Encoding.UTF8, 1024, false))
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
                    zip.Flush();
                }

                return new MemoryStream(memory.GetBuffer(), false);
            }
        }

        public List<LogEvent> Deserialize(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var result = new List<LogEvent>();
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            using (var zip = zipArchive.GetEntry("logs.clef").Open())
            using (var reader = new StreamReader(zip, Encoding.UTF8, false, 1024, true))
            using (var logEventReader = new LogEventReader(reader))
            {
                while (logEventReader.TryRead(out var logEvent))
                {
                    result.Add(logEvent);
                }
            }

            return result;
        }
    }
}
