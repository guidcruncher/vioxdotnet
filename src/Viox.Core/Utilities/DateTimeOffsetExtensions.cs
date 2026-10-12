namespace Viox.Core.Utilities;

public static class DateTimeOffsetExtensions
{
    public static string ToCron(this DateTimeOffset dateTimeOffset, bool excludeDate = false)
    {
        int minute = dateTimeOffset.Minute;
        int hour = dateTimeOffset.Hour;
        int day = dateTimeOffset.Day;
        int month = dateTimeOffset.Month;
        int second = dateTimeOffset.Second;

        // Cron format including seconds: Second Minute Hour Day Month DayOfWeek
        if (excludeDate)
        {
            return $"{second} {minute} {hour} * * *";
        }

        return $"{second} {minute} {hour} {day} {month} *";
    }

    public static DateTimeOffset FromEpoch(this long epoch, bool useMilliseconds = true)
    {
        return useMilliseconds
            ? DateTimeOffset.FromUnixTimeMilliseconds(epoch)
            : DateTimeOffset.FromUnixTimeSeconds(epoch);
    }

    public static DateTimeOffset FromEpoch(this double epoch, bool useMilliseconds = true)
    {
        long value = (long)Math.Round(epoch);
        return useMilliseconds
                    ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                    : DateTimeOffset.FromUnixTimeSeconds(value);
    }

}
