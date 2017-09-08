using System;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public static class DateTimeOffsetTools
    {
        public static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        public static long ToUnixTimeSeconds(DateTimeOffset dateTimeOffset)
        {
#if NET45
         return Convert.ToInt64((dateTimeOffset - UnixEpoch).TotalSeconds);
#else
         return dateTimeOffset.ToUnixTimeSeconds();
#endif
        }

        public static long ToUnixTimeMilliseconds(DateTimeOffset dateTimeOffset)
        {
#if NET45
         return Convert.ToInt64((dateTimeOffset - UnixEpoch).TotalMilliseconds);
#else
         return dateTimeOffset.ToUnixTimeMilliseconds();
#endif
        }
    }
}
