using System;
using System.IO;

namespace Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog
{
    public sealed class SerializedClefLog : IDisposable
    {
        public static readonly SerializedClefLog Empty = new SerializedClefLog();

        public readonly DateTimeOffset FirstEventTime;
        public readonly DateTimeOffset LastEventTime;
        public readonly Stream Data;
        public readonly bool HasData;

        private SerializedClefLog()
        {
            FirstEventTime = default(DateTimeOffset);
            LastEventTime = default(DateTimeOffset);
            Data = Stream.Null;
            HasData = false;
        }

        public SerializedClefLog(DateTimeOffset firstEventTime, DateTimeOffset lastEventTime, Stream data)
        {
            FirstEventTime = firstEventTime;
            LastEventTime = lastEventTime;
            Data = data;
            HasData = true;
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}
