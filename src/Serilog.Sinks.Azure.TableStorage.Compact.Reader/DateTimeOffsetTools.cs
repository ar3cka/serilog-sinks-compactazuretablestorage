using System;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public static class DateTimeOffsetTools
    {
        public static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        public static string ToUnixTimeSecondsString(DateTimeOffset dateTimeOffset)
        {
#if NET45
         return Convert.ToInt64((dateTimeOffset - UnixEpoch).TotalSeconds).ToString("D19");
#else
         return dateTimeOffset.ToUnixTimeSeconds().ToString("D19");
#endif
        }

        public static string ToUnixTimeMillisecondsString(DateTimeOffset dateTimeOffset)
        {
#if NET45
         return Convert.ToInt64((dateTimeOffset - UnixEpoch).TotalMilliseconds).ToString("D19");
#else
         return dateTimeOffset.ToUnixTimeMilliseconds().ToString("D19");
#endif
        }
    }
}
