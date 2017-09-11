using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Reader
{
    public sealed class TableEntityReader
    {
        public List<LogEvent> ReadEvents(DynamicTableEntity row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            using (var data = ReadRawData(row))
            {
                var result = new List<LogEvent>();
                using (var zipArchive = new ZipArchive(data, ZipArchiveMode.Read, true))
                using (var zip = zipArchive.GetEntry("log.clef").Open())
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

        private MemoryStream ReadRawData(DynamicTableEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var properties = entity.Properties;
            var memory = PrepareMemoryStream(properties["PayloadSize"].Int64Value);
            var hasData = true;
            for (var i = 0; i < 15 && hasData; i++)
            {
                if (properties.TryGetValue(GetPropertyName(i), out var propertyValue))
                {
                    var buffer = new MemoryStream(propertyValue.BinaryValue);
                    buffer.CopyTo(memory);
                }
                else
                {
                    hasData = false;
                }
            }

            memory.Position = 0;

            return memory;
        }

        private static MemoryStream PrepareMemoryStream(long? size)
        {
            return size.HasValue ? new MemoryStream(new byte[size.Value]) : new MemoryStream();
        }

        private static string GetPropertyName(int i)
        {
            return "P" + i.ToString("D2");
        }
    }
}
